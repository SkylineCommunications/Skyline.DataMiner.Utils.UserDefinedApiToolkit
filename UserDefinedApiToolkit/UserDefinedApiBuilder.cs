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

		public UserDefinedApiBuilder AddController<TController>()
			where TController : ControllerBase
		{
			var controllerType = typeof(TController);
			return AddController(controllerType);
		}

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
				var routeInfo = new RouteHandlerInfo(
					controllerType,
					httpMethodAttr.HttpMethod,
					controllerRoute,
					method,
					parameters);

				_handlers.Add(routeInfo);
			}

			return this;
		}

		public UserDefinedApiBuilder ConfigureServices(Action<IServiceCollection> configure)
		{
			if (configure is null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			_configureActions.Add(configure);
			return this;
		}

		public UserDefinedApiBuilder WithDefaultInputConverter(IInputConverter converter)
		{
			if (converter is null)
			{
				throw new ArgumentNullException(nameof(converter));
			}

			_inputConverters[0] = converter;
			return this;
		}

		public UserDefinedApiBuilder WithDefaultOutputConverter(IOutputConverter converter)
		{
			if (converter is null)
			{
				throw new ArgumentNullException(nameof(converter));
			}

			_outputConverters[0] = converter;
			return this;
		}

		public UserDefinedApiBuilder AddInputConverter(IInputConverter converter)
		{
			if (converter is null)
			{
				throw new ArgumentNullException(nameof(converter));
			}

			_inputConverters.Add(converter);
			return this;
		}

		public UserDefinedApiBuilder AddOutputConverter(IOutputConverter converter)
		{
			if (converter is null)
			{
				throw new ArgumentNullException(nameof(converter));
			}

			_outputConverters.Add(converter);
			return this;
		}

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
	}
}
