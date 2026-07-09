namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Microsoft.Extensions.DependencyInjection;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions;
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

		public ControllerBase CreateController(IEngine engine, IServiceProvider services)
		{
			var parameters = new object[ConstructorParameters.Length];
			for (int i = 0; i < ConstructorParameters.Length; i++)
			{
				var param = ConstructorParameters[i];

				// Framework provided parameters
				if (param.ParameterType == typeof(IEngine))
				{
					parameters[i] = engine;
					continue;
				}

				if (param.ParameterType == typeof(IConnection))
				{
					parameters[i] = engine.GetUserConnection();
					continue;
				}

				if (param.ParameterType == typeof(IServiceProvider))
				{
					parameters[i] = services;
					continue;
				}

				// Dependency injection
				var service = services.GetRequiredService(param.ParameterType);
				parameters[i] = service;
			}

			var result = (ControllerBase)ConstructorInfo.Invoke(parameters);
			return result;
		}

		public IApiResult Invoke(ApiContext context, ControllerBase controller, IServiceProvider services)
		{
			// Route was already matched by the RouteSelector (via GetRank), so this should always
			// succeed; TryMatchSegments is re-run here (stateless) to extract the placeholder values.
			TryMatchSegments(context.Request.Route, out _, out var routeValues);

			var parameters = new object[MethodParameters.Length];
			for (int i = 0; i < MethodParameters.Length; i++)
			{
				var param = MethodParameters[i];

				// Framework provided parameters
				if (param.ParameterType == typeof(ApiContext))
				{
					parameters[i] = context;
					continue;
				}

				if (param.ParameterType == typeof(IEngine))
				{
					parameters[i] = services.GetRequiredService<IAccessor<IEngine>>().Value;
					continue;
				}

				if (param.ParameterType == typeof(IEngine))
				{
					parameters[i] = services.GetRequiredService<IAccessor<IEngine>>().Value.GetUserConnection();
					continue;
				}

				if (param.ParameterType == typeof(IServiceProvider))
				{
					parameters[i] = services;
					continue;
				}

				// Handle explicit parameters
				var fromBodyAttribute = param.GetCustomAttribute<FromBodyAttribute>();
				if (fromBodyAttribute is not null)
				{
					parameters[i] = HandleBodyParam(context, param);
					continue;
				}

				var fromRouteAttribute = param.GetCustomAttribute<FromRouteAttribute>();
				if (fromRouteAttribute is not null)
				{
					var routeName = String.IsNullOrEmpty(fromRouteAttribute.Name) ? param.Name : fromRouteAttribute.Name;
					parameters[i] = HandleRouteParam(context, param, routeName, routeValues);
					continue;
				}

				// Implicit binding: an unattributed parameter whose name matches a placeholder.
				if (routeValues.ContainsKey(param.Name))
				{
					parameters[i] = HandleRouteParam(context, param, param.Name, routeValues);
					continue;
				}

				var fromQueryAttribute = param.GetCustomAttribute<FromQueryAttribute>();
				if (fromQueryAttribute is not null)
				{
					var queryName = String.IsNullOrEmpty(fromQueryAttribute.Name) ? param.Name : fromQueryAttribute.Name;
					parameters[i] = HandleQueryParam(context, param, queryName);
					continue;
				}

				// Dependency injection
				var service = services.GetService(param.ParameterType);
				if (service is not null)
				{
					parameters[i] = service;
					continue;
				}

				// TODO: if we still didn't find a match perhaps we should throw?
				// For now default to query parameter handling
				parameters[i] = HandleQueryParam(context, param, param.Name);
				continue;
			}

			var result = (IApiResult)MethodInfo.Invoke(controller, parameters);
			return result;
		}

		public int GetRank(ApiContext context)
		{
			if (Template.Segments.Count == 0 && String.IsNullOrEmpty(Template.Raw))
			{
				return -1; // No route defined on controller
			}

			if (context.Request.RequestMethod != HttpMethod)
			{
				return -1; // HTTP method doesn't match
			}

			if (!TryMatchSegments(context.Request.Route, out var literalMatches, out var routeValues))
			{
				return -1; // Route doesn't match (different segment count, or a literal segment mismatch)
			}

			// Literal segment matches must always outrank placeholder segment matches for the same
			// request (e.g. "items/count" beats "items/{id}"), regardless of query/body scoring
			// below, so they're weighted far above the maximum plausible query/body score.
			var score = literalMatches * 100;

			var hasBodyParam = MethodParameters.Any(p => p.GetCustomAttribute<FromBodyAttribute>() is not null);
			if (!hasBodyParam &&
				String.IsNullOrEmpty(context.Request.RawBody))
			{
				score += 1; // No body expected and no body provided
			}

			foreach (var p in MethodParameters)
			{
				if (p.GetCustomAttribute<FromBodyAttribute>() is not null)
				{
					// If there is a body and the route expects a body, give a point
					if (!String.IsNullOrEmpty(context.Request.RawBody))
					{
						score += 1;
					}

					continue;
				}

				var fromRouteAttribute = p.GetCustomAttribute<FromRouteAttribute>();
				if (fromRouteAttribute is not null)
				{
					var routeName = String.IsNullOrEmpty(fromRouteAttribute.Name) ? p.Name : fromRouteAttribute.Name;
					if (!routeValues.ContainsKey(routeName))
					{
						return -1; // Explicit [FromRoute] references a placeholder that isn't in this template
					}

					score += 2;
					continue;
				}

				// Implicit binding: an unattributed parameter whose name matches a placeholder.
				if (routeValues.ContainsKey(p.Name))
				{
					score += 2;
					continue;
				}

				var fromQueryAttribute = p.GetCustomAttribute<FromQueryAttribute>();
				var queryName = fromQueryAttribute is not null && !String.IsNullOrEmpty(fromQueryAttribute.Name) ? fromQueryAttribute.Name : p.Name;

				if (context.Request.QueryParameters?.ContainsKey(queryName) ?? false)
				{
					score += 2; // Exact matches are preferred
				}
				else if (p.HasDefaultValue)
				{
					score += 1; // Default values are weaker matches
				}
				else
				{
					return -1; // Required parameter missing
				}
			}

			return score;
		}

		/// <summary>
		/// Attempts to match <paramref name="requestRoute"/> against <see cref="Template"/>
		/// segment-by-segment. Literal segments must match exactly; placeholder segments match any
		/// value and are captured into <paramref name="routeValues"/> by placeholder name.
		/// </summary>
		private bool TryMatchSegments(string requestRoute, out int literalMatches, out IReadOnlyDictionary<string, string> routeValues)
		{
			var requestSegments = SplitSegments(requestRoute);
			var templateSegments = Template.Segments;

			if (requestSegments.Length != templateSegments.Count)
			{
				literalMatches = 0;
				routeValues = EmptyRouteValues;
				return false;
			}

			var values = new Dictionary<string, string>();
			var literalCount = 0;
			for (int i = 0; i < templateSegments.Count; i++)
			{
				var templateSegment = templateSegments[i];
				var requestSegment = requestSegments[i];

				if (templateSegment.IsPlaceholder)
				{
					values[templateSegment.Value] = requestSegment;
					continue;
				}

				if (!String.Equals(templateSegment.Value, requestSegment, StringComparison.Ordinal))
				{
					literalMatches = 0;
					routeValues = EmptyRouteValues;
					return false;
				}

				literalCount++;
			}

			literalMatches = literalCount;
			routeValues = values;
			return true;
		}

		private static string[] SplitSegments(string route)
		{
			var trimmed = route?.Trim('/') ?? String.Empty;
			return String.IsNullOrEmpty(trimmed) ? Array.Empty<string>() : trimmed.Split('/');
		}

		private static readonly IReadOnlyDictionary<string, string> EmptyRouteValues = new Dictionary<string, string>();

		private static object HandleBodyParam(ApiContext context, ParameterInfo param)
		{
			if (param.ParameterType == typeof(string))
			{
				return context.Request.RawBody;
			}

			var converter = context.InputConverters.Reverse().FirstOrDefault(c => c.CanConvertInput(param.ParameterType));
			if (converter is not null)
			{
				return converter.ConvertInput(context.Request.RawBody, param.ParameterType) ?? new object();
			}

			// None of the registered converters can handle this type (e.g. a custom converter
			// that only targets complex/DTO types). Fall back to the same primitive string
			// conversion used for query parameters, so simple types (int, bool, Guid, ...) still
			// work out of the box even without a converter that explicitly supports them.
			if (StringValueConverter.TryConvert(context.Request.RawBody, param.ParameterType, out var value))
			{
				return value!;
			}

			throw new InvalidParameterException(context, param.Name, context.Request.RawBody, param.ParameterType);
		}

		private static object HandleRouteParam(ApiContext context, ParameterInfo param, string routeName, IReadOnlyDictionary<string, string> routeValues)
		{
			if (!routeValues.TryGetValue(routeName, out var value))
			{
				throw new InvalidOperationException($"Could not find a route value for parameter '{param.Name}' (expected placeholder '{routeName}').");
			}

			if (param.ParameterType == typeof(string))
			{
				return value;
			}

			if (!StringValueConverter.TryConvert(value, param.ParameterType, out var converted))
			{
				throw new InvalidParameterException(context, param.Name, value, param.ParameterType);
			}

			return converted!;
		}

		private static object HandleQueryParam(ApiContext context, ParameterInfo param, string queryName)
		{
			if (context.Request.QueryParameters.TryGetValue(queryName, out var value))
			{
				if (param.ParameterType == typeof(string))
				{
					return value;
				}

				if (!StringValueConverter.TryConvert(value, param.ParameterType, out var converted))
				{
					throw new InvalidParameterException(context, param.Name, value, param.ParameterType);
				}

				return converted!;
			}
			else if (param.HasDefaultValue)
			{
				return param.DefaultValue;
			}
			else
			{
				throw new InvalidOperationException($"Could not handle the parameter '{param.Name}'.");
			}
		}
	}
}