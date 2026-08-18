namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.Analysis
{
	using System.Net.Http;
	using System.Reflection;

	internal static class MethodInfoExtensions
	{
		public static bool TryGetHttpMethod(this MethodInfo method, out HttpMethod httpMethod)
		{
			httpMethod = HttpMethod.Get;

			foreach (var attr in method.GetCustomAttributesData())
			{
				switch (TypeHelper.GetAttributeName(attr))
				{
					case "HttpGetAttribute": httpMethod = HttpMethod.Get; return true;
					case "HttpPostAttribute": httpMethod = HttpMethod.Post; return true;
					case "HttpPutAttribute": httpMethod = HttpMethod.Put; return true;
					case "HttpDeleteAttribute": httpMethod = HttpMethod.Delete; return true;
					case "HttpPatchAttribute": httpMethod = new HttpMethod("PATCH"); return true;
					case "HttpHeadAttribute": httpMethod = HttpMethod.Head; return true;
					case "HttpOptionsAttribute": httpMethod = HttpMethod.Options; return true;
				}
			}

			return false;
		}
	}
}
