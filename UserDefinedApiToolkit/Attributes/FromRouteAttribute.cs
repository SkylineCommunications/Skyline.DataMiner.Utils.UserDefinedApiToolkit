namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// Specifies that a parameter should be bound from a route placeholder (e.g. <c>{id}</c>) in the
	/// combined controller/method route template.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
	public sealed class FromRouteAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the name of the route placeholder to bind from. When not set, the
		/// parameter's own name is used.
		/// </summary>
		public string? Name { get; set; }
	}
}
