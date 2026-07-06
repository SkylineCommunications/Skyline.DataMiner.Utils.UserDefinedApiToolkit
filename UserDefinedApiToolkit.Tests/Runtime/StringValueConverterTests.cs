namespace UserDefinedApiToolkit.Tests.Runtime
{
	using System;

	using FluentAssertions;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[TestClass]
	public sealed class StringValueConverterTests
	{
		[TestMethod]
		public void TryConvert_StringTarget_ReturnsRawValueUnchanged()
		{
			// Act
			var success = StringValueConverter.TryConvert("hello", typeof(string), out var value);

			// Assert
			success.Should().BeTrue();
			value.Should().Be("hello");
		}

		[TestMethod]
		[DataRow("42", 42)]
		[DataRow("-7", -7)]
		[DataRow("0", 0)]
		public void TryConvert_ValidInt_ReturnsConvertedValue(string rawValue, int expected)
		{
			// Act
			var success = StringValueConverter.TryConvert(rawValue, typeof(int), out var value);

			// Assert
			success.Should().BeTrue();
			value.Should().Be(expected);
		}

		[TestMethod]
		[DataRow("not-a-number")]
		[DataRow("")]
		[DataRow("4.2")]
		public void TryConvert_InvalidInt_ReturnsFalse(string rawValue)
		{
			// Act
			var success = StringValueConverter.TryConvert(rawValue, typeof(int), out var value);

			// Assert
			success.Should().BeFalse();
			value.Should().BeNull();
		}

		[TestMethod]
		[DataRow("true", true)]
		[DataRow("false", false)]
		public void TryConvert_ValidBool_ReturnsConvertedValue(string rawValue, bool expected)
		{
			// Act
			var success = StringValueConverter.TryConvert(rawValue, typeof(bool), out var value);

			// Assert
			success.Should().BeTrue();
			value.Should().Be(expected);
		}

		[TestMethod]
		public void TryConvert_ValidGuid_ReturnsConvertedValue()
		{
			// Arrange
			var guid = Guid.NewGuid();

			// Act
			var success = StringValueConverter.TryConvert(guid.ToString(), typeof(Guid), out var value);

			// Assert
			success.Should().BeTrue();
			value.Should().Be(guid);
		}

		[TestMethod]
		public void TryConvert_InvalidGuid_ReturnsFalse()
		{
			// Act
			var success = StringValueConverter.TryConvert("not-a-guid", typeof(Guid), out var value);

			// Assert
			success.Should().BeFalse();
			value.Should().BeNull();
		}

		[TestMethod]
		public void TryConvert_NullableIntWithValue_ReturnsConvertedValue()
		{
			// Act
			var success = StringValueConverter.TryConvert("13", typeof(int?), out var value);

			// Assert
			success.Should().BeTrue();
			value.Should().Be(13);
		}

		[TestMethod]
		public void TryConvert_NullableIntWithEmptyString_ReturnsNull()
		{
			// Act
			var success = StringValueConverter.TryConvert(String.Empty, typeof(int?), out var value);

			// Assert
			success.Should().BeTrue();
			value.Should().BeNull();
		}

		[TestMethod]
		public void TryConvert_ValidEnum_ReturnsConvertedValue()
		{
			// Act
			var success = StringValueConverter.TryConvert(nameof(DayOfWeek.Monday), typeof(DayOfWeek), out var value);

			// Assert
			success.Should().BeTrue();
			value.Should().Be(DayOfWeek.Monday);
		}

		[TestMethod]
		public void TryConvert_InvalidEnum_ReturnsFalse()
		{
			// Act
			var success = StringValueConverter.TryConvert("NotADay", typeof(DayOfWeek), out var value);

			// Assert
			success.Should().BeFalse();
			value.Should().BeNull();
		}

		[TestMethod]
		public void TryConvert_NullTargetType_ThrowsArgumentNullException()
		{
			// Act
			var act = () => StringValueConverter.TryConvert("value", null!, out _);

			// Assert
			act.Should().Throw<ArgumentNullException>();
		}
	}
}
