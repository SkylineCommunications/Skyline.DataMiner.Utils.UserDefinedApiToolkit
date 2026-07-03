namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	[AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
	public sealed class FromQueryAttribute : Attribute
	{
	}
}
