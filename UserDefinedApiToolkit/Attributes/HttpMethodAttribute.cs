namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;

	/// <summary>
	/// Base class for the HTTP method attributes (<see cref="HttpGetAttribute"/>,
	/// <see cref="HttpPostAttribute"/>, <see cref="HttpPutAttribute"/>,
	/// <see cref="HttpDeleteAttribute"/>) applied to controller action methods to register them as
	/// route handlers for a specific <see cref="RequestMethod"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public abstract class HttpMethodAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HttpMethodAttribute"/> class with no extra
		/// route segment.
		/// </summary>
		protected HttpMethodAttribute()
			: this(String.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpMethodAttribute"/> class with a route
		/// template that is appended to the controller's route.
		/// </summary>
		/// <param name="template">
		/// The route template to append to the controller's route, e.g. <c>"{id}"</c>. Can be
		/// <c>null</c> or an empty string when the method contributes no extra route segment.
		/// </param>
		protected HttpMethodAttribute(string template)
		{
			Template = template ?? String.Empty;
		}

		/// <summary>
		/// Gets the HTTP request method this attribute registers the action for.
		/// </summary>
		public abstract RequestMethod HttpMethod { get; }

		/// <summary>
		/// Gets the route template that is appended to the controller's route.
		/// </summary>
		public string Template { get; }
	}
}
