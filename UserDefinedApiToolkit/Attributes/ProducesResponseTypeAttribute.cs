namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// Documents a possible response status code (and optionally its body type and description) for
	/// an action, for use by the OpenAPI generator. Can be applied multiple times to document
	/// multiple possible responses. Does not affect runtime behavior.
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
		/// a response with no body and the given description.
		/// </summary>
		/// <param name="statusCode">The documented HTTP status code.</param>
		/// <param name="description">The description of the documented response.</param>
		public ProducesResponseTypeAttribute(int statusCode, string? description)
			: this(statusCode)
		{
			Description = description;
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
		/// Initializes a new instance of the <see cref="ProducesResponseTypeAttribute"/> class for
		/// a response with the given body type and description.
		/// </summary>
		/// <param name="responseType">The type of the response body.</param>
		/// <param name="statusCode">The documented HTTP status code.</param>
		/// <param name="description">The description of the documented response.</param>
		public ProducesResponseTypeAttribute(Type responseType, int statusCode, string? description)
			: this(responseType, statusCode)
		{
			Description = description;
		}

		/// <summary>
		/// Gets the documented HTTP status code.
		/// </summary>
		public int StatusCode { get; }

		/// <summary>
		/// Gets the description of the documented response, or <c>null</c> if no description was provided.
		/// </summary>
		public string? Description { get; }

		/// <summary>
		/// Gets the type of the response body, or <c>null</c> if the response has no body.
		/// </summary>
		public Type? ResponseType { get; }
	}
}
