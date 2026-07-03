namespace UserDefinedApiToolkit.Tests.Build
{
	using System.Collections.Generic;

	using FluentAssertions;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build;

	[TestClass]
	public sealed class TypeHelperTests
	{
		[TestMethod]
		public void GetElementType_Array_ReturnsElementType()
		{
			TypeHelper.GetElementType(typeof(int[])).Should().Be(typeof(int));
		}

		[TestMethod]
		public void GetElementType_GenericList_ReturnsElementType()
		{
			TypeHelper.GetElementType(typeof(List<string>)).Should().Be(typeof(string));
		}

		[TestMethod]
		public void GetElementType_NonCollection_ReturnsSameType()
		{
			TypeHelper.GetElementType(typeof(int)).Should().Be(typeof(int));
		}

		[TestMethod]
		public void GetResultTypes_ApiResultOfOne_ReturnsSuccessTypeOnly()
		{
			var (success, error) = TypeHelper.GetResultTypes(typeof(ApiResult<string>));

			success.Should().Be(typeof(string));
			error.Should().BeNull();
		}

		[TestMethod]
		public void GetResultTypes_ApiResultOfTwo_ReturnsSuccessAndErrorType()
		{
			var (success, error) = TypeHelper.GetResultTypes(typeof(ApiResult<string, int>));

			success.Should().Be(typeof(string));
			error.Should().Be(typeof(int));
		}

		[TestMethod]
		public void GetResultTypes_NonGenericType_ReturnsNulls()
		{
			var (success, error) = TypeHelper.GetResultTypes(typeof(object));

			success.Should().BeNull();
			error.Should().BeNull();
		}

		[TestMethod]
		public void HasAttribute_Member_WithMatchingAttribute_ReturnsTrue()
		{
			var method = typeof(TestFiles.SampleController).GetMethod(nameof(TestFiles.SampleController.GetById));

			TypeHelper.HasAttribute(method!, "HttpGet").Should().BeTrue();
		}

		[TestMethod]
		public void HasAttribute_Member_WithoutMatchingAttribute_ReturnsFalse()
		{
			var method = typeof(TestFiles.SampleController).GetMethod(nameof(TestFiles.SampleController.GetById));

			TypeHelper.HasAttribute(method!, "HttpPost").Should().BeFalse();
		}

		[TestMethod]
		public void HasAttribute_Parameter_WithMatchingAttribute_ReturnsTrue()
		{
			var method = typeof(TestFiles.SampleController).GetMethod(nameof(TestFiles.SampleController.Create));
			var parameter = method!.GetParameters()[0];

			TypeHelper.HasAttribute(parameter, "FromBody").Should().BeTrue();
		}

		[TestMethod]
		public void HasAttribute_Parameter_WithoutMatchingAttribute_ReturnsFalse()
		{
			var method = typeof(TestFiles.SampleController).GetMethod(nameof(TestFiles.SampleController.GetById));
			var parameter = method!.GetParameters()[0];

			TypeHelper.HasAttribute(parameter, "FromBody").Should().BeFalse();
		}
	}
}
