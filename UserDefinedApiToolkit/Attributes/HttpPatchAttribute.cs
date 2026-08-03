namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;

	/// <summary>
	/// Registers an action method as an HTTP PATCH route handler.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class HttpPatchAttribute : HttpMethodAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HttpPatchAttribute"/> class with no extra
		/// route segment.
		/// </summary>
		public HttpPatchAttribute()
			: base(String.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpPatchAttribute"/> class with a route
		/// template that is appended to the controller's route.
		/// </summary>
		/// <param name="template">The route template to append to the controller's route, e.g. <c>"{id}"</c>.</param>
		public HttpPatchAttribute(string template)
			: base(template)
		{
		}

		/// <inheritdoc/>
		public override RequestMethod HttpMethod => RequestMethod.Patch;
	}
}
