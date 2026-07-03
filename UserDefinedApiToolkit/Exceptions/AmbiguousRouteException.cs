namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions
{
	using System;

	[Serializable]
	public class AmbiguousRouteException : Exception
	{
		public AmbiguousRouteException(ApiContext context)
			: this(context, $"Ambiguous route detected for request '{context.Request.RequestMethod} {context.Request.Route}'. Multiple routes match the request.")
		{
		}

		public AmbiguousRouteException(ApiContext context, string message) : base(message)
		{
			Context = context;
		}

		public AmbiguousRouteException(ApiContext context, string message, Exception innerException) : base(message, innerException)
		{
			Context = context;
		}

		public ApiContext Context { get; }
	}
}
