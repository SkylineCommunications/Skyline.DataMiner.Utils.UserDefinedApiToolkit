namespace UserDefinedApiToolkit.Tests.Runtime
{
	using System;

	using FluentAssertions;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[TestClass]
	public sealed class ControllerTests
	{
		[TestMethod]
		[DataRow("UserDefinedApiToolkit.Tests.Runtime.GET.TestFiles.Empty_Controller", true)]
		[DataRow("UserDefinedApiToolkit.Tests.Runtime.GET.TestFiles.Empty_Controller_Missing_ApiController", true)]
		[DataRow("UserDefinedApiToolkit.Tests.Runtime.GET.TestFiles.Empty_Controller_Missing_Route", false)]
		[DataRow("UserDefinedApiToolkit.Tests.Runtime.GET.TestFiles.Empty_Controller_Missing_ControllerBase", false)]
		public void ValidateController(string controller_fullname, bool isValidController)
		{
			// Arrange
			var controllerType = Type.GetType(controller_fullname);

			// Act
			var act = () => UserDefinedApi.CreateBuilder()
				.AddController(Type.GetType(controller_fullname))
				.Build();

			// Assert
			if (isValidController)
			{
				act.Should().NotThrow();
			}
			else
			{
				act.Should().Throw<Exception>();
			}
		}
	}
}
