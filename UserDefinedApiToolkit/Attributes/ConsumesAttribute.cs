namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Documents the request content type(s) accepted by a controller or action, for use by the
	/// OpenAPI generator. Does not affect runtime request handling.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class ConsumesAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConsumesAttribute"/> class.
		/// </summary>
		/// <param name="contentType">The primary accepted content type, e.g. <c>"application/json"</c>.</param>
		/// <param name="additionalContentTypes">Any additional accepted content types.</param>
		public ConsumesAttribute(string contentType, params string[] additionalContentTypes)
		{
			ContentTypes = new[] { contentType }.Concat(additionalContentTypes).ToList();
		}

		/// <summary>
		/// Gets the accepted content types.
		/// </summary>
		public IReadOnlyList<string> ContentTypes { get; }
	}
}
