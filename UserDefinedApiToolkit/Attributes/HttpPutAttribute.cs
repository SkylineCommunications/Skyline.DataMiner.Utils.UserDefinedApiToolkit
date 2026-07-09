namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;

	/// <summary>
	/// Registers an action method as an HTTP PUT route handler.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class HttpPutAttribute : HttpMethodAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HttpPutAttribute"/> class with an optional
		/// route template that is appended to the controller's route.
		/// </summary>
		/// <param name="template">The route template to append to the controller's route, e.g. <c>"{id}"</c>.</param>
		public HttpPutAttribute(string template = "")
			: base(template)
		{
		}

		/// <inheritdoc/>
		public override RequestMethod HttpMethod => RequestMethod.Put;
	}
}
