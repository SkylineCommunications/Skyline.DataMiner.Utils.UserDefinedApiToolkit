namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;

	using Microsoft.Extensions.DependencyInjection;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions;

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
			Route = route ?? throw new ArgumentNullException(nameof(route));
			MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			MethodParameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
		}

		public Type ControllerType { get; }

		public ConstructorInfo ConstructorInfo { get; }

		public ParameterInfo[] ConstructorParameters { get; }

		public RequestMethod HttpMethod { get; }

		public string Route { get; }

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

				var fromQueryAttribute = param.GetCustomAttribute<FromQueryAttribute>();
				if (fromQueryAttribute is not null)
				{
					parameters[i] = HandleQueryParam(context, param);
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
				parameters[i] = HandleQueryParam(context, param);
				continue;
			}

			var result = (IApiResult)MethodInfo.Invoke(controller, parameters);
			return result;
		}

		public int GetRank(ApiContext context)
		{
			if (String.IsNullOrEmpty(Route))
			{
				return -1; // No route defined on controller
			}

			if (context.Request.Route.Trim('/') != Route.Trim('/'))
			{
				return -1; // Route doesn't match
			}

			if (context.Request.RequestMethod != HttpMethod)
			{
				return -1; // HTTP method doesn't match
			}

			var score = 0;

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

				if (context.Request.QueryParameters?.ContainsKey(p.Name) ?? false)
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

		private static object HandleQueryParam(ApiContext context, ParameterInfo param)
		{
			if (context.Request.QueryParameters.TryGetValue(param.Name, out var value))
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