namespace UserDefinedApiToolkit.Tests.Build
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using FluentAssertions;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build;

	[TestClass]
	public sealed class TypeHelperTests
	{
		[TestMethod]
		public void Diagnostic_TypeHelperAssembly_ExposesGetResultTypesMethod()
		{
			// Diagnostic test: on some CI test hosts, GetResultTypes() has been observed to throw
			// MissingMethodException even though the source clearly defines it. typeof(TypeHelper)
			// and GetMethods() bind to whatever assembly the runtime actually loaded (without
			// invoking the method itself, which is what triggers the JIT resolution failure), so
			// this reports the real assembly location/version/method list for troubleshooting.
			var assembly = typeof(TypeHelper).Assembly;
			var methodSignatures = typeof(TypeHelper)
				.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)
				.Select(m => $"{m.ReturnType} {m.Name}({String.Join(",", m.GetParameters().Select(p => p.ParameterType.ToString()))})")
				.ToList();

			var info = $"Assembly location: {assembly.Location}{Environment.NewLine}" +
					   $"Assembly full name: {assembly.FullName}{Environment.NewLine}" +
					   $"Public/internal static methods:{Environment.NewLine}  " +
					   String.Join($"{Environment.NewLine}  ", methodSignatures);

			// Escape braces: FluentAssertions treats "because" as a composite format string.
			var because = info.Replace("{", "{{").Replace("}", "}}");

			methodSignatures.Should().Contain(
				m => m.Contains("GetResultTypes"),
				because);
		}

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
