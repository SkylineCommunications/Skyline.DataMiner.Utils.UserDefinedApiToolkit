namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// Explicitly specifies that a parameter should be bound from a query string parameter. Query
	/// parameters can also be bound implicitly (without this attribute) when the parameter's name
	/// matches a query string key; use this attribute mainly to override the bound name via
	/// <see cref="Name"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
	public sealed class FromQueryAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the name of the query parameter to bind from. When not set, the
		/// parameter's own name is used.
		/// </summary>
		public string? Name { get; set; }
	}
}
