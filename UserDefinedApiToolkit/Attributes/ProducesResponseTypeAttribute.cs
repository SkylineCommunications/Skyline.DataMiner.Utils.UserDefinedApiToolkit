namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class ProducesResponseTypeAttribute : Attribute
	{
		public ProducesResponseTypeAttribute(int statusCode)
		{
			StatusCode = statusCode;
		}

		public ProducesResponseTypeAttribute(Type responseType, int statusCode)
		{
			ResponseType = responseType;
			StatusCode = statusCode;
		}

		public int StatusCode { get; }

		public Type? ResponseType { get; }
	}
}
