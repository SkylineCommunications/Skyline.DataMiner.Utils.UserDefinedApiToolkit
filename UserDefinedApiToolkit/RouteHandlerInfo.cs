namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Linq;
	using System.Reflection;

	using Microsoft.Extensions.DependencyInjection;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Routes;

	internal class RouteHandlerInfo
	{
		public RouteHandlerInfo(
			Type controllerType,
			RequestMethod httpMethod,
			string route,
			MethodInfo methodInfo,
			ParameterInfo[] parameters)
		{
			ControllerType = controllerType ?? throw new ArgumentNullException(nameof(controllerType));
			ConstructorInfo = controllerType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault() ?? throw new InvalidOperationException($"No public constructors found for controller type '{controllerType.FullName}'.");
			ConstructorParameters = ConstructorInfo.GetParameters();
			HttpMethod = httpMethod;
			Template = RouteTemplate.Parse(route ?? throw new ArgumentNullException(nameof(route)));
			MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			MethodParameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
		}

		public Type ControllerType { get; }

		public ConstructorInfo ConstructorInfo { get; }

		public ParameterInfo[] ConstructorParameters { get; }

		public RequestMethod HttpMethod { get; }

		/// <summary>
		/// Gets the parsed route template (literal and <c>{placeholder}</c> segments) for this
		/// route handler.
		/// </summary>
		public RouteTemplate Template { get; }

		public MethodInfo MethodInfo { get; }

		public ParameterInfo[] MethodParameters { get; }

		public ControllerBase CreateController(IServiceProvider services)
		{
			var parameters = new object[ConstructorParameters.Length];
			for (int i = 0; i < ConstructorParameters.Length; i++)
			{
				var param = ConstructorParameters[i];

				parameters[i] = ParameterBinder.IsFrameworkProvidedType(param.ParameterType)
					? ParameterBinder.ResolveFrameworkParam(param, null, services)
					: services.GetRequiredService(param.ParameterType);
			}

			var result = (ControllerBase)ConstructorInfo.Invoke(parameters);
			return result;
		}

		public IApiResult Invoke(ApiContext context, ControllerBase controller, IServiceProvider services)
		{
			// Route was already matched by the RouteSelector (via GetRank), so this should always
			// succeed; Match is re-run here (stateless) to extract the placeholder values.
			var routeValues = Template.Match(context.Request.Route).RouteValues;

			var parameters = new object[MethodParameters.Length];
			for (int i = 0; i < MethodParameters.Length; i++)
			{
				var param = MethodParameters[i];
				var binding = ParameterBinder.Classify(param, routeValues, services);

				// binding.Name is only ever null for the Framework/DependencyInjection sources
				// (see ParameterBinder.Classify), so the null-forgiving operator below is safe.
				parameters[i] = binding.Source switch
				{
					ParameterBindingSource.Framework => ParameterBinder.ResolveFrameworkParam(param, context, services),
					ParameterBindingSource.Body => ParameterBinder.HandleBodyParam(context, param)!,
					ParameterBindingSource.Route => ParameterBinder.HandleRouteParam(context, param, binding.Name!, routeValues),
					ParameterBindingSource.DependencyInjection => services.GetRequiredService(param.ParameterType),
					_ => ParameterBinder.HandleQueryParam(context, param, binding.Name!),
				};
			}

			var result = (IApiResult)MethodInfo.Invoke(controller, parameters);
			return result;
		}

		public int GetRank(ApiContext context, IServiceProvider services)
		{
			if (Template.Segments.Count == 0 && String.IsNullOrEmpty(Template.Raw))
			{
				return -1; // No route defined on controller
			}

			if (context.Request.RequestMethod != HttpMethod)
			{
				return -1; // HTTP method doesn't match
			}

			var match = Template.Match(context.Request.Route);
			if (!match.IsMatch)
			{
				return -1; // Route doesn't match (different segment count, or a literal segment mismatch)
			}

			var routeValues = match.RouteValues;

			// Literal segment matches must always outrank placeholder segment matches for the same
			// request (e.g. "items/count" beats "items/{id}"), regardless of query/body scoring
			// below, so they're weighted far above the maximum plausible query/body score.
			var score = match.LiteralMatches * 100;

			var bindings = MethodParameters.Select(p => (Param: p, Binding: ParameterBinder.Classify(p, routeValues, services))).ToList();

			var hasBodyParam = bindings.Any(b => b.Binding.Source == ParameterBindingSource.Body);
			if (!hasBodyParam &&
				String.IsNullOrEmpty(context.Request.RawBody))
			{
				score += 1; // No body expected and no body provided
			}

			foreach (var (param, binding) in bindings)
			{
				switch (binding.Source)
				{
					case ParameterBindingSource.Framework:
						// Framework-provided parameters (ApiContext, IEngine, IConnection,
						// IServiceProvider) are always resolvable and don't affect scoring.
						break;

					case ParameterBindingSource.Body:
						// If there is a body and the route expects a body, give a point
						if (!String.IsNullOrEmpty(context.Request.RawBody))
						{
							score += 1;
						}

						break;

					case ParameterBindingSource.Route:
						// binding.Name is guaranteed non-null for the Route source (see
						// ParameterBinder.Classify).
						if (!routeValues.ContainsKey(binding.Name!))
						{
							return -1; // Explicit [FromRoute] references a placeholder that isn't in this template
						}

						score += 2;
						break;

					case ParameterBindingSource.DependencyInjection:
						score += 1; // Resolvable via DI, but not backed by request data
						break;

					case ParameterBindingSource.Query:
					default:
						// binding.Name is guaranteed non-null for the Query source (see
						// ParameterBinder.Classify).
						if (context.Request.QueryParameters?.ContainsKey(binding.Name!) ?? false)
						{
							score += 2; // Exact matches are preferred
						}
						else if (param.HasDefaultValue)
						{
							score += 1; // Default values are weaker matches
						}
						else
						{
							return -1; // Required parameter missing
						}

						break;
				}
			}

			return score;
		}
	}
}
