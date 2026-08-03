namespace UserDefinedApiToolkit.Tests
{
	using System;

	using DiffEngine;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public static class VerifyConfig
	{
		[AssemblyInitialize]
		public static void Initialize(TestContext context)
		{
			// Configure Verify to use VS Code for diff comparisons
			DiffTools.UseOrder(DiffTool.VisualStudioCode);

			// Only launch a diff tool automatically on local developer machines; CI agents (and
			// machines without VS Code installed/configured) should never have a diff tool pop up
			// or hang the test run.
			bool isCi = !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
			DiffRunner.Disabled = isCi;
		}
	}
}
