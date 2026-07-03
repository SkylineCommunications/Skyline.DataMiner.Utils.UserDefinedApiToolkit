namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	public class StatusCodeResult : IApiResult
	{
		public StatusCodeResult(int statusCode)
		{
			StatusCode = statusCode;
		}

		public int StatusCode { get; }

		public virtual void ExecuteResult(ApiContext context)
		{
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			context.Response.ResponseCode = StatusCode;
		}
	}
}
