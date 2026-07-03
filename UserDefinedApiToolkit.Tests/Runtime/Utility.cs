namespace UserDefinedApiToolkit.Tests.Runtime
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	internal class Utility
	{
		public static Url ParseUrl(string url)
		{
			if (String.IsNullOrEmpty(url))
			{
				return new Url();
			}

			var qIndex = url.IndexOf('?');
			if (qIndex < 0 || qIndex == url.Length - 1)
			{
				return new Url
				{
					Path = url,
				};
			}

			var result = new Url
			{
				Path = url.Substring(0, qIndex),
			};

			var i = qIndex + 1;
			var start = i;

			while (i <= url.Length)
			{
				if (i != url.Length && url[i] != '&')
				{
					i++;
					continue;
				}

				if (i <= start)
				{
					start = i + 1;
					i++;
					continue;
				}

				var segment = url.Substring(start, i - start);

				var key = default(string);
				var value = default(string);

				var eqIndex = segment.IndexOf('=');

				if (eqIndex >= 0)
				{
					key = Uri.UnescapeDataString(segment.Substring(0, eqIndex));
					value = Uri.UnescapeDataString(segment.Substring(eqIndex + 1));
				}
				else
				{
					key = Uri.UnescapeDataString(segment);
					value = String.Empty;
				}

				if (!result.QueryParameters.TryGetValue(key, out var list))
				{
					list = new List<string>();
					result.QueryParameters[key] = list;
				}

				list.Add(value);

				start = i + 1;
				i++;
			}

			return result;
		}
	}

	internal class Url
	{
		public string Path { get; set; } = String.Empty;

		public Dictionary<string, List<string>> QueryParameters { get; set; } = new Dictionary<string, List<string>>();
	}
}
