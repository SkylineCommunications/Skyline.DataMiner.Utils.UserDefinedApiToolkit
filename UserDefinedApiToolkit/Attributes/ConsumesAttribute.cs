namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class ConsumesAttribute : Attribute
	{
		public ConsumesAttribute(string contentType, params string[] additionalContentTypes)
		{
			ContentTypes = new[] { contentType }.Concat(additionalContentTypes).ToList();
		}

		public IReadOnlyList<string> ContentTypes { get; }
	}
}
