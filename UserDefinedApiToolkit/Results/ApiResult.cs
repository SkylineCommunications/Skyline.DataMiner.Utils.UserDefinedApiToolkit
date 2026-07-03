namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	public sealed class ApiResult<TSuccess> : IApiResult
	{
		private readonly IApiResult _inner;

		private ApiResult(IApiResult inner)
		{
			_inner = inner;
		}

		public static implicit operator ApiResult<TSuccess>(ObjectResult<TSuccess> result) => new ApiResult<TSuccess>(result);

		public static implicit operator ApiResult<TSuccess>(StatusCodeResult result) => new ApiResult<TSuccess>(result);

		public void ExecuteResult(ApiContext context) => _inner.ExecuteResult(context);
	}

	public sealed class ApiResult<TSuccess, TError> : IApiResult
	{
		private readonly IApiResult _inner;

		public ApiResult(IApiResult inner)
		{
			_inner = inner;
		}

		public static implicit operator ApiResult<TSuccess, TError>(ObjectResult<TSuccess> result)
			=> new ApiResult<TSuccess, TError>(result);

		public static implicit operator ApiResult<TSuccess, TError>(ObjectResult<TError> result)
			=> new ApiResult<TSuccess, TError>(result);

		public static implicit operator ApiResult<TSuccess, TError>(StatusCodeResult result)
			=> new ApiResult<TSuccess, TError>(result);

		public void ExecuteResult(ApiContext context) => _inner.ExecuteResult(context);
	}
}
