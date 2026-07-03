namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions
{
	using System;

	[Serializable]
	public class NoRouteException : Exception
	{
		public NoRouteException(ApiContext context)
			: this(context, $"Could not find a matching route handler for route '{context.Request.RequestMethod} {context.Request.Route}'")
		{
		}

		public NoRouteException(ApiContext context, string message) : base(message)
		{
			Context = context;
		}

		public NoRouteException(ApiContext context, string message, Exception innerException) : base(message, innerException)
		{
			Context = context;
		}

		public ApiContext Context { get; }
	}
}
