namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Microsoft.Extensions.DependencyInjection;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions;

	/// <summary>
	/// Classifies controller action parameters (<see cref="Classify"/>) and resolves their actual
	/// values (<see cref="ResolveFrameworkParam"/>, <see cref="HandleBodyParam"/>,
	/// <see cref="HandleRouteParam"/>, <see cref="HandleQueryParam"/>), shared between
	/// <see cref="RouteHandlerInfo.GetRank"/> (scoring) and <see cref="RouteHandlerInfo.Invoke"/>
	/// (binding) so both always agree on how a parameter is resolved.
	/// </summary>
	internal static class ParameterBinder
	{
		/// <summary>
		/// Classifies where a method parameter's value should come from (framework, body, route,
		/// query, or DI).
		/// </summary>
		/// <param name="param">The controller action parameter to classify.</param>
		/// <param name="routeValues">The route placeholder values captured for the current request.</param>
		/// <param name="services">The service provider used to detect dependency-injectable parameters.</param>
		/// <returns>The <see cref="ParameterBinding"/> describing how <paramref name="param"/> should be resolved.</returns>
		public static ParameterBinding Classify(ParameterInfo param, IReadOnlyDictionary<string, string> routeValues, IServiceProvider services)
		{
			if (IsFrameworkProvidedType(param.ParameterType))
			{
				return new ParameterBinding(ParameterBindingSource.Framework, null);
			}

			if (param.GetCustomAttribute<FromBodyAttribute>() is not null)
			{
				return new ParameterBinding(ParameterBindingSource.Body, null);
			}

			var fromRouteAttribute = param.GetCustomAttribute<FromRouteAttribute>();
			if (fromRouteAttribute is not null)
			{
				var routeName = String.IsNullOrEmpty(fromRouteAttribute.Name) ? param.Name : fromRouteAttribute.Name;
				return new ParameterBinding(ParameterBindingSource.Route, routeName);
			}

			var fromQueryAttribute = param.GetCustomAttribute<FromQueryAttribute>();
			if (fromQueryAttribute is not null)
			{
				var queryName = String.IsNullOrEmpty(fromQueryAttribute.Name) ? param.Name : fromQueryAttribute.Name;
				return new ParameterBinding(ParameterBindingSource.Query, queryName);
			}

			// Implicit binding: an unattributed parameter whose name matches a placeholder.
			if (routeValues.ContainsKey(param.Name))
			{
				return new ParameterBinding(ParameterBindingSource.Route, param.Name);
			}

			// Dependency injection: an unattributed, non-route-matching parameter that the DI
			// container can resolve (e.g. a registered repository/service). Uses
			// IServiceProviderIsService (a lookup-only check) rather than GetService, so
			// classification never triggers instantiation of transient/scoped services as a
			// side effect of ranking or binding-source detection.
			var serviceCheck = services.GetService<IServiceProviderIsService>();
			if (serviceCheck?.IsService(param.ParameterType) == true)
			{
				return new ParameterBinding(ParameterBindingSource.DependencyInjection, null);
			}

			// Nothing else matched; fall back to query parameter handling by the parameter's own name.
			return new ParameterBinding(ParameterBindingSource.Query, param.Name);
		}

		/// <summary>
		/// Determines whether <paramref name="type"/> is a framework-provided type (<see cref="ApiContext"/>,
		/// <c>IEngine</c>, <c>IConnection</c>, or <c>IServiceProvider</c>) that is always
		/// resolvable, regardless of the request or the DI container's registrations.
		/// </summary>
		/// <param name="type">The parameter type to check.</param>
		/// <returns><see langword="true"/> if <paramref name="type"/> is a framework-provided type; otherwise, <see langword="false"/>.</returns>
		public static bool IsFrameworkProvidedType(Type type)
		{
			return type == typeof(ApiContext)
				|| type == typeof(IEngine)
				|| type == typeof(IConnection)
				|| type == typeof(IServiceProvider);
		}

		/// <summary>
		/// Resolves the actual value for a framework-provided parameter (<see cref="ApiContext"/>,
		/// <c>IEngine</c>, <c>IConnection</c>, or <c>IServiceProvider</c>). Used for both controller
		/// action parameters and controller constructor parameters.
		/// </summary>
		/// <param name="param">The parameter to resolve.</param>
		/// <param name="context">
		/// The current API context, or <see langword="null"/> when resolving a controller
		/// constructor parameter (the <see cref="ApiContext"/> isn't available yet at that point).
		/// </param>
		/// <param name="services">The service provider used to resolve the framework-provided instance.</param>
		/// <returns>The resolved framework-provided value.</returns>
		public static object ResolveFrameworkParam(ParameterInfo param, ApiContext? context, IServiceProvider services)
		{
			if (param.ParameterType == typeof(ApiContext))
			{
				return context ?? throw new InvalidOperationException($"'{nameof(ApiContext)}' cannot be injected into a controller constructor (parameter '{param.Name}'); it is only available on controller action methods, or via the 'ControllerBase.ApiContext' property.");
			}

			if (param.ParameterType == typeof(IEngine))
			{
				return services.GetRequiredService<IAccessor<IEngine>>().Value;
			}

			if (param.ParameterType == typeof(IConnection))
			{
				return services.GetRequiredService<IAccessor<IConnection>>().Value;
			}

			// IServiceProvider
			return services;
		}

		/// <summary>
		/// Resolves the actual value for a <see cref="ParameterBindingSource.Body"/> parameter.
		/// </summary>
		/// <param name="context">The current API context.</param>
		/// <param name="param">The controller action parameter to resolve.</param>
		/// <returns>The resolved value, converted to <paramref name="param"/>'s type.</returns>
		public static object HandleBodyParam(ApiContext context, ParameterInfo param)
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

		/// <summary>
		/// Resolves the actual value for a <see cref="ParameterBindingSource.Route"/> parameter.
		/// </summary>
		/// <param name="context">The current API context.</param>
		/// <param name="param">The controller action parameter to resolve.</param>
		/// <param name="routeName">The placeholder name to look the value up by.</param>
		/// <param name="routeValues">The route placeholder values captured for the current request.</param>
		/// <returns>The resolved value, converted to <paramref name="param"/>'s type.</returns>
		public static object HandleRouteParam(ApiContext context, ParameterInfo param, string routeName, IReadOnlyDictionary<string, string> routeValues)
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

		/// <summary>
		/// Resolves the actual value for a <see cref="ParameterBindingSource.Query"/> parameter.
		/// </summary>
		/// <param name="context">The current API context.</param>
		/// <param name="param">The controller action parameter to resolve.</param>
		/// <param name="queryName">The query string key to look the value up by.</param>
		/// <returns>The resolved value, converted to <paramref name="param"/>'s type, or the parameter's default value when the query key is absent.</returns>
		public static object HandleQueryParam(ApiContext context, ParameterInfo param, string queryName)
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
