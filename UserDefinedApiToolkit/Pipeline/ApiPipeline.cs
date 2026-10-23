namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;

	internal sealed class ApiPipeline
	{
		private readonly IReadOnlyList<Func<IServiceProvider, IApiMiddleware>> _middlewareFactories;
		private readonly IServiceProvider _services;
		private readonly ApiRequestDelegate _endpoint;

		public ApiPipeline(
			IReadOnlyList<Func<IServiceProvider, IApiMiddleware>> middlewareFactories,
			IServiceProvider services,
			ApiRequestDelegate endpoint)
		{
			_middlewareFactories = middlewareFactories ?? throw new ArgumentNullException(nameof(middlewareFactories));
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
		}

		public void Invoke(ApiContext context)
		{
			Invoke(context, 0);
		}

		private void Invoke(ApiContext context, int middlewareIndex)
		{
			if (middlewareIndex == _middlewareFactories.Count)
			{
				_endpoint(context);
				return;
			}

			var middleware = _middlewareFactories[middlewareIndex](_services)
				?? throw new InvalidOperationException("The API middleware factory returned null.");
			middleware.Invoke(
				context,
				nextContext => Invoke(nextContext, middlewareIndex + 1));
		}
	}
}
