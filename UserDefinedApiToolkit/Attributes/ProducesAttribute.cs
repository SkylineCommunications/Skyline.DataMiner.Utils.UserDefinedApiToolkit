namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Documents the response content type(s) produced by a controller or action, for use by the
	/// OpenAPI generator. Does not affect runtime response serialization (see
	/// <see cref="IOutputConverter"/> for that).
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class ProducesAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ProducesAttribute"/> class.
		/// </summary>
		/// <param name="contentType">The primary produced content type, e.g. <c>"application/json"</c>.</param>
		/// <param name="additionalContentTypes">Any additional produced content types.</param>
		public ProducesAttribute(string contentType, params string[] additionalContentTypes)
		{
			ContentTypes = new[] { contentType }.Concat(additionalContentTypes).ToList();
		}

		/// <summary>
		/// Gets the produced content types.
		/// </summary>
		public IReadOnlyList<string> ContentTypes { get; }
	}
}
