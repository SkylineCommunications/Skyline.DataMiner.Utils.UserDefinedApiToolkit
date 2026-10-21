namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;

	internal sealed class ApiPipeline
	{
		private readonly IReadOnlyList<IApiMiddleware> _middleware;
		private readonly ApiRequestDelegate _endpoint;

		public ApiPipeline(
			IReadOnlyList<IApiMiddleware> middleware,
			ApiRequestDelegate endpoint)
		{
			_middleware = middleware ?? throw new ArgumentNullException(nameof(middleware));
			_endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
		}

		public void Invoke(ApiContext context)
		{
			Invoke(context, 0);
		}

		private void Invoke(ApiContext context, int middlewareIndex)
		{
			if (middlewareIndex == _middleware.Count)
			{
				_endpoint(context);
				return;
			}

			var middleware = _middleware[middlewareIndex];
			middleware.Invoke(
				context,
				nextContext => Invoke(nextContext, middlewareIndex + 1));
		}
	}
}
