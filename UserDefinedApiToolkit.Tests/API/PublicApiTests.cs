namespace UserDefinedApiToolkit.Tests.API
{
	using System;
	using System.IO;
	using System.Linq;

	[TestClass]
	public class PublicApiTests
	{
		[TestMethod]
		public void PublicApiShouldNotContainUnshippedEntries()
		{
			using Stream? stream = typeof(PublicApiTests).Assembly.GetManifestResourceStream("PublicAPI.Unshipped.txt");

			if (stream is null)
			{
				Assert.Fail("The public API baseline resource was not found.");
				return;
			}

			using var reader = new StreamReader(stream);
			var unshippedEntries = reader.ReadToEnd()
				.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
				.Where(line => !String.IsNullOrWhiteSpace(line))
				.Where(line => !line.TrimStart().StartsWith("#", StringComparison.Ordinal))
				.ToList();

			Assert.AreEqual(
				0,
				unshippedEntries.Count,
				$"The public API contains unshipped entries:{Environment.NewLine}{String.Join(Environment.NewLine, unshippedEntries)}");
		}
	}
}
