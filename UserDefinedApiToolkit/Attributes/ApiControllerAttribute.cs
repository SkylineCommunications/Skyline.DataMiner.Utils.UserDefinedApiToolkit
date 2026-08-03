namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// Marks a class as an API controller. Currently informational only; controller discovery and
	/// validation is driven by <see cref="RouteAttribute"/> and inheritance from
	/// <see cref="ControllerBase"/> in <see cref="UserDefinedApiBuilder.AddController(System.Type)"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class ApiControllerAttribute : Attribute
	{
	}
}
