namespace UserDefinedApiToolkit.Tests.Runtime
{
	using System;

	using FluentAssertions;

	using Microsoft.Extensions.Logging;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[TestClass]
	public sealed class EngineLoggerTests
	{
		[TestMethod]
		public void Log_WithException_IncludesExceptionDetailsAndStackTrace()
		{
			// Arrange
			var engine = new EngineMock();
			var accessor = new EngineAccessor();
			accessor.SetValue(engine);

			Exception exception;
			try
			{
				throw new InvalidOperationException("failure");
			}
			catch (Exception caughtException)
			{
				exception = caughtException;
			}

			// Act
			new EngineLogger<EngineLoggerTests>(accessor)
				.Log(LogLevel.Error, default(EventId), "request failed", exception, (state, error) => state);
			new EngineLogger(accessor)
				.Log(LogLevel.Error, default(EventId), "request failed", exception, (state, error) => state);

			// Assert
			engine.Logs.Should().HaveCount(2);
			engine.Logs.Should().OnlyContain(log => log.Contains(exception.ToString()));
		}
	}
}
