namespace UserDefinedApiToolkit.Tests.Runtime.Body.TestFiles
{
	using System;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	/// <summary>
	/// Test-only <see cref="IInputConverter"/> that only claims to support <see cref="string"/>,
	/// used to force <c>RouteHandlerInfo.HandleBodyParam</c> down its "no converter found" fallback
	/// path for non-string parameter types.
	/// </summary>
	internal class StringOnlyInputConverter : IInputConverter
	{
		public string InputMediaType => "text/plain";

		public bool CanConvertInput(Type type) => type == typeof(string);

		public object? ConvertInput(string input, Type type) => input;
	}
}
