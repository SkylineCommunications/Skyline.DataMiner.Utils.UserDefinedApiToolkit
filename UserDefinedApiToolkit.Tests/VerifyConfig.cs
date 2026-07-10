namespace UserDefinedApiToolkit.Tests
{
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

			// Ensure diff tool launches automatically on test failure
			DiffRunner.Disabled = false;
		}
	}
}
