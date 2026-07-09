namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	/// <summary>
	/// Specifies that a parameter should be bound by deserializing the request body, using the
	/// registered <see cref="IInputConverter"/> that matches the parameter's type (see
	/// <see cref="UserDefinedApiBuilder.AddInputConverter"/>).
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
	public sealed class FromBodyAttribute : Attribute
	{
	}
}
