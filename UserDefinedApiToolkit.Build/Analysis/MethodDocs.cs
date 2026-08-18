namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.Analysis
{
	using System.Collections.Generic;

	internal class MethodDocs
	{
		public string? Summary { get; set; }
		public string? Example { get; set; }
		public Dictionary<string, string>? Parameters { get; set; }
	}
}
