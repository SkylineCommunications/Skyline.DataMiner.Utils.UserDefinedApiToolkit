namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	/// <summary>
	/// Represents an action result that can only succeed with a value of type
	/// <typeparamref name="TSuccess"/>, or return a status-code-only result (e.g. an error).
	/// Enables strongly-typed action method return types (e.g.
	/// <c>ApiResult&lt;Item&gt; GetById(int id)</c>) while still allowing implicit conversion from
	/// the <c>Ok()</c>/<c>NotFound()</c> etc. helper methods on <see cref="ControllerBase"/>.
	/// </summary>
	/// <typeparam name="TSuccess">The type of the success value.</typeparam>
	public sealed class ApiResult<TSuccess> : IApiResult
	{
		private readonly IApiResult _inner;

		private ApiResult(IApiResult inner)
		{
			_inner = inner;
		}

		/// <summary>
		/// Implicitly wraps an <see cref="ObjectResult{T}"/> of type <typeparamref name="TSuccess"/>.
		/// </summary>
		/// <param name="result">The result to wrap.</param>
		/// <returns>An <see cref="ApiResult{TSuccess}"/> wrapping <paramref name="result"/>.</returns>
		public static implicit operator ApiResult<TSuccess>(ObjectResult<TSuccess> result) => new ApiResult<TSuccess>(result);

		/// <summary>
		/// Implicitly wraps a status-code-only <see cref="StatusCodeResult"/>.
		/// </summary>
		/// <param name="result">The result to wrap.</param>
		/// <returns>An <see cref="ApiResult{TSuccess}"/> wrapping <paramref name="result"/>.</returns>
		public static implicit operator ApiResult<TSuccess>(StatusCodeResult result) => new ApiResult<TSuccess>(result);

		/// <inheritdoc/>
		public void ExecuteResult(ApiContext context) => _inner.ExecuteResult(context);
	}

	/// <summary>
	/// Represents an action result that can succeed with a value of type
	/// <typeparamref name="TSuccess"/>, or fail with a value of type <typeparamref name="TError"/>,
	/// or return a status-code-only result. Enables strongly-typed action method return types (e.g.
	/// <c>ApiResult&lt;Item, ErrorDetails&gt; GetById(int id)</c>) while still allowing implicit
	/// conversion from the <c>Ok()</c>/<c>BadRequest()</c> etc. helper methods on
	/// <see cref="ControllerBase"/>.
	/// </summary>
	/// <typeparam name="TSuccess">The type of the success value.</typeparam>
	/// <typeparam name="TError">The type of the error value.</typeparam>
	public sealed class ApiResult<TSuccess, TError> : IApiResult
	{
		private readonly IApiResult _inner;

		/// <summary>
		/// Initializes a new instance of the <see cref="ApiResult{TSuccess, TError}"/> class,
		/// wrapping the given <paramref name="inner"/> result.
		/// </summary>
		/// <param name="inner">The result to wrap.</param>
		public ApiResult(IApiResult inner)
		{
			_inner = inner;
		}

		/// <summary>
		/// Implicitly wraps an <see cref="ObjectResult{T}"/> of type <typeparamref name="TSuccess"/>.
		/// </summary>
		/// <param name="result">The result to wrap.</param>
		/// <returns>An <see cref="ApiResult{TSuccess, TError}"/> wrapping <paramref name="result"/>.</returns>
		public static implicit operator ApiResult<TSuccess, TError>(ObjectResult<TSuccess> result)
			=> new ApiResult<TSuccess, TError>(result);

		/// <summary>
		/// Implicitly wraps an <see cref="ObjectResult{T}"/> of type <typeparamref name="TError"/>.
		/// </summary>
		/// <param name="result">The result to wrap.</param>
		/// <returns>An <see cref="ApiResult{TSuccess, TError}"/> wrapping <paramref name="result"/>.</returns>
		public static implicit operator ApiResult<TSuccess, TError>(ObjectResult<TError> result)
			=> new ApiResult<TSuccess, TError>(result);

		/// <summary>
		/// Implicitly wraps a status-code-only <see cref="StatusCodeResult"/>.
		/// </summary>
		/// <param name="result">The result to wrap.</param>
		/// <returns>An <see cref="ApiResult{TSuccess, TError}"/> wrapping <paramref name="result"/>.</returns>
		public static implicit operator ApiResult<TSuccess, TError>(StatusCodeResult result)
			=> new ApiResult<TSuccess, TError>(result);

		/// <inheritdoc/>
		public void ExecuteResult(ApiContext context) => _inner.ExecuteResult(context);
	}
}
