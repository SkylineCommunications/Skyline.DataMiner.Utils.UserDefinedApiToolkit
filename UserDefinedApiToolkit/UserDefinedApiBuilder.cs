namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Routes;

	/// <summary>
	/// Builds a <see cref="UserDefinedApi"/> by registering controllers, dependency injection
	/// services, and input/output converters. Use <see cref="UserDefinedApi.CreateBuilder"/> to
	/// create an instance, then call <see cref="Build"/> once configuration is complete.
	/// </summary>
	public class UserDefinedApiBuilder
	{
		private readonly List<RouteHandlerInfo> _handlers = new();
		private readonly List<Action<IServiceCollection>> _configureActions = new();

		private readonly List<IInputConverter> _inputConverters = new();
		private readonly List<IOutputConverter> _outputConverters = new();

		private readonly IServiceCollection _services;

		internal UserDefinedApiBuilder()
		{
			_services = new ServiceCollection();

			var defaultConverter = new NewtonsoftConverter();
			_inputConverters.Add(defaultConverter);
			_outputConverters.Add(defaultConverter);
		}

		/// <summary>
		/// Registers a controller type by generic type parameter. See
		/// <see cref="AddController(Type)"/> for the registration rules and exceptions.
		/// </summary>
		/// <typeparam name="TController">The controller type to register.</typeparam>
		/// <returns>The same builder instance, for chaining.</returns>
		public UserDefinedApiBuilder AddController<TController>()
			where TController : ControllerBase
		{
			var controllerType = typeof(TController);
			return AddController(controllerType);
		}

		/// <summary>
		/// Registers a controller type. The type must inherit from <see cref="ControllerBase"/> and
		/// be decorated with a non-empty <see cref="RouteAttribute"/>; every public instance method
		/// decorated with an <see cref="HttpMethodAttribute"/> derivative (e.g.
		/// <see cref="HttpGetAttribute"/>) is registered as a route handler.
		/// </summary>
		/// <param name="controllerType">The controller type to register.</param>
		/// <returns>The same builder instance, for chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="controllerType"/> is <c>null</c>.</exception>
		/// <exception cref="Exceptions.InvalidControllerException">
		/// Thrown when <paramref name="controllerType"/> does not inherit from <see cref="ControllerBase"/>,
		/// or does not have a valid <see cref="RouteAttribute"/>.
		/// </exception>
		public UserDefinedApiBuilder AddController(Type controllerType)
		{
			if (controllerType is null)
			{
				throw new ArgumentNullException(nameof(controllerType));
			}

			if (!typeof(ControllerBase).IsAssignableFrom(controllerType))
			{
				throw new InvalidControllerException($"Controller '{controllerType}' must inherit from {nameof(ControllerBase)}.");
			}

			var controllerRoute = controllerType
				.GetCustomAttribute<RouteAttribute>()?
				.Template?
				.Trim('/') ?? String.Empty;

			if (String.IsNullOrEmpty(controllerRoute))
			{
				throw new InvalidControllerException($"Controller '{controllerType}' does not have a valid Route attribute.");
			}

			_services.AddScoped(controllerType);

			var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			foreach (var method in methods)
			{
				var httpMethodAttr = method.GetCustomAttribute<HttpMethodAttribute>(true);
				if (httpMethodAttr is null)
				{
					continue;
				}

				var parameters = method.GetParameters();
				var combinedRoute = RouteTemplate.Combine(controllerRoute, httpMethodAttr.Template);
				var routeInfo = new RouteHandlerInfo(
					controllerType,
					httpMethodAttr.HttpMethod,
					combinedRoute,
					method,
					parameters);

				_handlers.Add(routeInfo);
			}

			return this;
		}

		/// <summary>
		/// Registers an action used to configure the dependency injection container, e.g. to
		/// register repositories or other services consumed by controller constructors.
		/// </summary>
		/// <param name="configure">The action that configures the <see cref="IServiceCollection"/>.</param>
		/// <returns>The same builder instance, for chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <c>null</c>.</exception>
		public UserDefinedApiBuilder ConfigureServices(Action<IServiceCollection> configure)
		{
			if (configure is null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			_configureActions.Add(configure);
			return this;
		}

		/// <summary>
		/// Replaces the default input converter (<see cref="NewtonsoftConverter"/>) used to
		/// deserialize <c>[FromBody]</c> parameters when no other registered converter can handle
		/// the parameter's type.
		/// </summary>
		/// <param name="converter">The converter to use as the default.</param>
		/// <returns>The same builder instance, for chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="converter"/> is <c>null</c>.</exception>
		public UserDefinedApiBuilder WithDefaultInputConverter(IInputConverter converter)
		{
			if (converter is null)
			{
				throw new ArgumentNullException(nameof(converter));
			}

			_inputConverters[0] = converter;
			return this;
		}

		/// <summary>
		/// Replaces the default output converter (<see cref="NewtonsoftConverter"/>) used to
		/// serialize action results when no other registered converter can handle the result's type.
		/// </summary>
		/// <param name="converter">The converter to use as the default.</param>
		/// <returns>The same builder instance, for chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="converter"/> is <c>null</c>.</exception>
		public UserDefinedApiBuilder WithDefaultOutputConverter(IOutputConverter converter)
		{
			if (converter is null)
			{
				throw new ArgumentNullException(nameof(converter));
			}

			_outputConverters[0] = converter;
			return this;
		}

		/// <summary>
		/// Registers an additional input converter, tried (most-recently-added first) before the
		/// default converter when deserializing <c>[FromBody]</c> parameters.
		/// </summary>
		/// <param name="converter">The converter to add.</param>
		/// <returns>The same builder instance, for chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="converter"/> is <c>null</c>.</exception>
		public UserDefinedApiBuilder AddInputConverter(IInputConverter converter)
		{
			if (converter is null)
			{
				throw new ArgumentNullException(nameof(converter));
			}

			_inputConverters.Add(converter);
			return this;
		}

		/// <summary>
		/// Registers an additional output converter, tried (most-recently-added first) before the
		/// default converter when serializing action results.
		/// </summary>
		/// <param name="converter">The converter to add.</param>
		/// <returns>The same builder instance, for chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="converter"/> is <c>null</c>.</exception>
		public UserDefinedApiBuilder AddOutputConverter(IOutputConverter converter)
		{
			if (converter is null)
			{
				throw new ArgumentNullException(nameof(converter));
			}

			_outputConverters.Add(converter);
			return this;
		}

		/// <summary>
		/// Validates the current configuration and builds the <see cref="IUserDefinedApi"/>.
		/// </summary>
		/// <returns>The built <see cref="IUserDefinedApi"/>, ready to handle requests via <see cref="IUserDefinedApi.Run"/>.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when a registered action has a <c>[FromBody]</c> parameter whose type cannot be
		/// deserialized by any registered <see cref="IInputConverter"/> (nor the built-in string
		/// conversion used for simple types).
		/// </exception>
		/// <exception cref="Exceptions.InvalidRouteException">
		/// Thrown when a registered action's route template and parameters are inconsistent: a
		/// <c>{placeholder}</c> has no bound parameter, or a <see cref="FromRouteAttribute"/>
		/// references a placeholder that doesn't exist in the combined route template.
		/// </exception>
		public IUserDefinedApi Build()
		{
			// Check if all the input arguments can be deserialized.
			var unhandledParameters = new List<ParameterInfo>();
			foreach (var handler in _handlers)
			{
				unhandledParameters.AddRange(handler.MethodParameters.Where(p =>
				{
					if (p.GetCustomAttribute<FromBodyAttribute>() is null)
					{
						return false;
					}

					var handledByConverter = _inputConverters.Reverse<IInputConverter>().Any(c => c.CanConvertInput(p.ParameterType));
					return !handledByConverter && !StringValueConverter.CanConvert(p.ParameterType);
				}));
			}

			if (unhandledParameters.DistinctBy(p => p.ParameterType.FullName).Any())
			{
				throw new InvalidOperationException($"No input converter found for the following parameters of types '{unhandledParameters.Select(p => p.ParameterType.FullName)}'");
			}

			// Check that every route template placeholder has a matching bound parameter, and that
			// every [FromRoute] parameter references a placeholder that actually exists.
			foreach (var handler in _handlers)
			{
				ValidateRouteParameters(handler);
			}

			// Register custom services
			foreach (var configure in _configureActions)
			{
				configure(_services);
			}

			// Register default services
			_services.AddScoped<IAccessor<IEngine>, EngineAccessor>();
			_services.AddScoped<IAccessor<IConnection>, ConnectionAccessor>();
			_services.AddScoped(typeof(ILogger<>), typeof(EngineLogger<>));
			_services.AddScoped<ILogger, EngineLogger>();

			// Build the api
			var api = new UserDefinedApi(
				_handlers,
				_inputConverters,
				_outputConverters,
				_services.BuildServiceProvider(
					new ServiceProviderOptions
					{
						ValidateScopes = true,
						ValidateOnBuild = true,
					}));

			var serviceProvider = _services.BuildServiceProvider();
			return new UserDefinedApi(_handlers, _inputConverters, _outputConverters, serviceProvider);
		}

		private static void ValidateRouteParameters(RouteHandlerInfo handler)
		{
			var duplicatePlaceholderName = handler.Template.PlaceholderNames
				.GroupBy(name => name, StringComparer.Ordinal)
				.FirstOrDefault(group => group.Count() > 1)?.Key;
			if (duplicatePlaceholderName is not null)
			{
				throw new InvalidRouteException($"Route template '{handler.Template.Raw}' on '{handler.ControllerType.Name}.{handler.MethodInfo.Name}' contains the '{{{duplicatePlaceholderName}}}' placeholder more than once, which would cause ambiguous binding.");
			}

			var placeholderNames = new HashSet<string>(handler.Template.PlaceholderNames, StringComparer.Ordinal);

			foreach (var param in handler.MethodParameters)
			{
				var fromRouteAttribute = param.GetCustomAttribute<FromRouteAttribute>();
				if (fromRouteAttribute is null)
				{
					continue;
				}

				var routeName = String.IsNullOrEmpty(fromRouteAttribute.Name) ? param.Name ?? String.Empty : fromRouteAttribute.Name;
				if (!placeholderNames.Contains(routeName))
				{
					throw new InvalidRouteException($"Parameter '{param.Name}' on '{handler.ControllerType.Name}.{handler.MethodInfo.Name}' is decorated with [FromRoute(Name = \"{routeName}\")], but the route template '{handler.Template.Raw}' does not contain a '{{{routeName}}}' placeholder.");
				}
			}

			foreach (var placeholderName in placeholderNames)
			{
				var isBound = handler.MethodParameters.Any(param =>
				{
					if (param.GetCustomAttribute<FromBodyAttribute>() is not null
						|| ParameterBinder.IsFrameworkProvidedType(param.ParameterType))
					{
						return false;
					}

					var fromRouteAttribute = param.GetCustomAttribute<FromRouteAttribute>();
					if (fromRouteAttribute is not null)
					{
						var routeName = String.IsNullOrEmpty(fromRouteAttribute.Name) ? param.Name : fromRouteAttribute.Name;
						return routeName == placeholderName;
					}

					if (param.GetCustomAttribute<FromQueryAttribute>() is not null)
					{
						return false;
					}

					// Implicit binding: an unattributed parameter whose name matches the placeholder.
					return param.Name == placeholderName;
				});

				if (!isBound)
				{
					throw new InvalidRouteException($"The route template '{handler.Template.Raw}' for '{handler.ControllerType.Name}.{handler.MethodInfo.Name}' contains a '{{{placeholderName}}}' placeholder that has no matching bound parameter (implicit name match or [FromRoute(Name = \"{placeholderName}\")]).");
				}
			}
		}
	}
}
