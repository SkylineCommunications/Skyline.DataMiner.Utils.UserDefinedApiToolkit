namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions
{
	using System;

	/// <summary>
	/// Thrown when no registered route handler matches the incoming request's method and path.
	/// </summary>
	[Serializable]
	public class NoRouteException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NoRouteException"/> class with a default
		/// message describing the unmatched request.
		/// </summary>
		/// <param name="context">The context of the request that could not be routed.</param>
		public NoRouteException(ApiContext context)
			: this(context, $"Could not find a matching route handler for route '{context.Request.RequestMethod} {context.Request.Route}'")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NoRouteException"/> class with a custom message.
		/// </summary>
		/// <param name="context">The context of the request that could not be routed.</param>
		/// <param name="message">The exception message.</param>
		public NoRouteException(ApiContext context, string message) : base(message)
		{
			Context = context;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NoRouteException"/> class with a custom
		/// message and inner exception.
		/// </summary>
		/// <param name="context">The context of the request that could not be routed.</param>
		/// <param name="message">The exception message.</param>
		/// <param name="innerException">The exception that caused this exception.</param>
		public NoRouteException(ApiContext context, string message, Exception innerException) : base(message, innerException)
		{
			Context = context;
		}

		/// <summary>
		/// Gets the context of the request that could not be routed.
		/// </summary>
		public ApiContext Context { get; }
	}
}
