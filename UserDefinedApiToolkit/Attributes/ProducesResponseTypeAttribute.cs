namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// Documents a possible response status code (and optionally its body type) for an action, for
	/// use by the OpenAPI generator. Can be applied multiple times to document multiple possible
	/// responses. Does not affect runtime behavior.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class ProducesResponseTypeAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ProducesResponseTypeAttribute"/> class for
		/// a response with no body.
		/// </summary>
		/// <param name="statusCode">The documented HTTP status code.</param>
		public ProducesResponseTypeAttribute(int statusCode)
		{
			StatusCode = statusCode;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProducesResponseTypeAttribute"/> class for
		/// a response with the given body type.
		/// </summary>
		/// <param name="responseType">The type of the response body.</param>
		/// <param name="statusCode">The documented HTTP status code.</param>
		public ProducesResponseTypeAttribute(Type responseType, int statusCode)
		{
			ResponseType = responseType;
			StatusCode = statusCode;
		}

		/// <summary>
		/// Gets the documented HTTP status code.
		/// </summary>
		public int StatusCode { get; }

		/// <summary>
		/// Gets the type of the response body, or <c>null</c> if the response has no body.
		/// </summary>
		public Type? ResponseType { get; }
	}
}
