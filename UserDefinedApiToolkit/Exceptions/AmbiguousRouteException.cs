namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions
{
	using System;

	/// <summary>
	/// Thrown when more than one registered route handler matches an incoming request's method
	/// and path with the same rank, making it impossible to determine which one should handle it.
	/// </summary>
	[Serializable]
	public class AmbiguousRouteException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AmbiguousRouteException"/> class with a
		/// default message describing the ambiguous request.
		/// </summary>
		/// <param name="context">The context of the ambiguous request.</param>
		public AmbiguousRouteException(ApiContext context)
			: this(context, $"Ambiguous route detected for request '{context.Request.RequestMethod} {context.Request.Route}'. Multiple routes match the request.")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AmbiguousRouteException"/> class with a
		/// custom message.
		/// </summary>
		/// <param name="context">The context of the ambiguous request.</param>
		/// <param name="message">The exception message.</param>
		public AmbiguousRouteException(ApiContext context, string message) : base(message)
		{
			Context = context;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AmbiguousRouteException"/> class with a
		/// custom message and inner exception.
		/// </summary>
		/// <param name="context">The context of the ambiguous request.</param>
		/// <param name="message">The exception message.</param>
		/// <param name="innerException">The exception that caused this exception.</param>
		public AmbiguousRouteException(ApiContext context, string message, Exception innerException) : base(message, innerException)
		{
			Context = context;
		}

		/// <summary>
		/// Gets the context of the ambiguous request.
		/// </summary>
		public ApiContext Context { get; }
	}
}
