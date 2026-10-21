namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;

	using Microsoft.Extensions.DependencyInjection;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Routes;

	public class UserDefinedApi : IUserDefinedApi
	{
		private readonly IServiceProvider _rootProvider;
		private readonly RouteSelector _routeSelector;
		private readonly List<IInputConverter> _inputConverters;
		private readonly List<IOutputConverter> _outputConverters;

		internal UserDefinedApi(
			List<RouteHandlerInfo> handlers,
			List<IInputConverter> inputConverters,
			List<IOutputConverter> outputConverters,
			IServiceProvider rootProvider)
		{
			_rootProvider = rootProvider;
			_inputConverters = inputConverters;
			_outputConverters = outputConverters;
			_routeSelector = new RouteSelector(handlers);
		}

		/// <summary>
		/// Creates a new <see cref="UserDefinedApiBuilder"/> instance for building a <see cref="UserDefinedApi"/>.
		/// </summary>
		/// <returns>A new <see cref="UserDefinedApiBuilder"/> instance.</returns>
		public static UserDefinedApiBuilder CreateBuilder()
		{
			return new UserDefinedApiBuilder();
		}

		/// <inheritdoc />
		public ApiTriggerOutput Run(IEngine engine, ApiTriggerInput apiTriggerInput)
		{
			using (var scope = _rootProvider.CreateScope())
			{
				scope.ServiceProvider
					.GetRequiredService<IAccessor<IEngine>>()
					.SetValue(engine);
				scope.ServiceProvider
					.GetRequiredService<IAccessor<IConnection>>()
					.SetValue(engine.GetUserConnection());

				var apiContext = new ApiContext
				{
					Request = apiTriggerInput,
					Response = new ApiTriggerOutput(),
					InputConverters = _inputConverters,
					OutputConverters = _outputConverters,
				};

				BuildPipeline(scope.ServiceProvider).Invoke(apiContext);
				return apiContext.Response;
			}
		}

		private ApiPipeline BuildPipeline(IServiceProvider services)
		{
			var middleware = new List<IApiMiddleware>(services.GetServices<IApiMiddleware>());

			return new ApiPipeline(middleware, context => ExecuteEndpoint(context, services));
		}

		private void ExecuteEndpoint(ApiContext context, IServiceProvider services)
		{
			var route = _routeSelector.SelectRoute(context, services);
			var controller = route.CreateController(services);
			controller.ApiContext = context;

			var result = route.Invoke(context, controller, services);
			if (result is null)
			{
				throw new InvalidOperationException("The API action returned a null result.");
			}

			result.ExecuteResult(context);
		}
	}
}