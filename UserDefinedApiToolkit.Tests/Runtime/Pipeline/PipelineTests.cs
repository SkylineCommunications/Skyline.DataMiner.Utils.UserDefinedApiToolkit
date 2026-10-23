namespace UserDefinedApiToolkit.Tests.Runtime.Pipeline
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;

	using Microsoft.Extensions.DependencyInjection;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	using UserDefinedApiToolkit.Tests.Runtime.Pipeline.TestFiles;

	[TestClass]
	public sealed class PipelineTests
	{
		[TestMethod]
		public void Run_WithMiddleware_ExecutesMiddlewareInRegistrationOrder()
		{
			// Arrange
			var recorder = new PipelineRecorder();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseMiddleware<FirstMiddleware>()
				.UseMiddleware<SecondMiddleware>()
				.ConfigureServices(services => services.AddSingleton(recorder))
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("success"));

			// Assert
			result.ResponseCode.Should().Be(200);
			recorder.Events.Should().Equal(
				"first-before",
				"second-before",
				"action",
				"second-after",
				"first-after");
		}

		[TestMethod]
		public void Run_WithShortCircuitingMiddleware_DoesNotInvokeEndpoint()
		{
			// Arrange
			var recorder = new PipelineRecorder();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseMiddleware<ShortCircuitMiddleware>()
				.ConfigureServices(services => services.AddSingleton(recorder))
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("success"));

			// Assert
			result.ResponseCode.Should().Be(204);
			recorder.Events.Should().Equal("short-circuit");
		}

		[TestMethod]
		public void Run_WithShortCircuitingMiddleware_DoesNotActivateLaterMiddleware()
		{
			// Arrange
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseMiddleware<ShortCircuitMiddleware>()
				.UseMiddleware<ConstructorThrowingMiddleware>()
				.ConfigureServices(services => services.AddSingleton(new PipelineRecorder()))
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("success"));

			// Assert
			result.ResponseCode.Should().Be(204);
		}

		[TestMethod]
		public void Build_SnapshotsMiddlewareRegistrations()
		{
			// Arrange
			var recorder = new PipelineRecorder();
			var builder = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.ConfigureServices(services => services.AddSingleton(recorder));
			var api = builder.Build();
			builder.UseMiddleware<RequestLoggingMiddleware>();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("success"));

			// Assert
			result.ResponseCode.Should().Be(200);
			recorder.Events.Should().Equal("action");
		}

		[TestMethod]
		public void Run_WithExceptionHandler_PassesExceptionAndContextToHandler()
		{
			// Arrange
			var recorder = new ExceptionRecorder();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<RecordingExceptionHandler>()
				.ConfigureServices(services =>
				{
					services.AddSingleton(recorder);
					services.AddSingleton(new PipelineRecorder());
				})
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("throw"));

			// Assert
			result.ResponseCode.Should().Be(418);
			result.ResponseBody.Should().Be("\"handled\"");
			recorder.Exception.Should().BeOfType<InvalidOperationException>();
			recorder.Exception!.Message.Should().Be("controller failure");
			recorder.Context.Should().NotBeNull();
			recorder.Context!.Request.Route.Should().Be("/v1/pipeline/throw");
		}

		[TestMethod]
		public void Run_WithExceptionHandler_CatchesMiddlewareExceptions()
		{
			// Arrange
			var exceptionRecorder = new ExceptionRecorder();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<RecordingExceptionHandler>()
				.UseMiddleware<ThrowingMiddleware>()
				.ConfigureServices(services =>
				{
					services.AddSingleton(exceptionRecorder);
					services.AddSingleton(new PipelineRecorder());
				})
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("success"));

			// Assert
			result.ResponseCode.Should().Be(418);
			exceptionRecorder.Exception.Should().BeOfType<InvalidOperationException>();
			exceptionRecorder.Exception!.Message.Should().Be("middleware failure");
		}

		[TestMethod]
		public void Build_SnapshotsExceptionHandlerRegistrations()
		{
			// Arrange
			var exceptionRecorder = new ExceptionRecorder();
			var handlerRecorder = new ExceptionHandlerRecorder();
			var builder = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<NonMatchingExceptionHandler>()
				.ConfigureServices(services =>
				{
					services.AddSingleton(exceptionRecorder);
					services.AddSingleton(handlerRecorder);
					services.AddSingleton(new PipelineRecorder());
					services.AddScoped<RecordingExceptionHandler>();
				});
			var api = builder.Build();
			builder.UseExceptionHandler<RecordingExceptionHandler>();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("throw"));

			// Assert
			result.ResponseCode.Should().Be(500);
			exceptionRecorder.Exception.Should().BeNull();
			handlerRecorder.Events.Should().Equal("non-matching");
		}

		[TestMethod]
		public void Run_WithExceptionHandler_CatchesMiddlewareActivationExceptions()
		{
			// Arrange
			var exceptionRecorder = new ExceptionRecorder();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<RecordingExceptionHandler>()
				.UseMiddleware<ConstructorThrowingMiddleware>()
				.ConfigureServices(services =>
				{
					services.AddSingleton(exceptionRecorder);
					services.AddSingleton(new PipelineRecorder());
				})
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("success"));

			// Assert
			result.ResponseCode.Should().Be(418);
			exceptionRecorder.Exception.Should().NotBeNull();
		}

		[TestMethod]
		public void Run_WithMultipleExceptionHandlers_UsesFirstHandlerThatHandlesException()
		{
			// Arrange
			var exceptionRecorder = new ExceptionRecorder();
			var handlerRecorder = new ExceptionHandlerRecorder();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<NonMatchingExceptionHandler>()
				.UseExceptionHandler<RecordingExceptionHandler>()
				.ConfigureServices(services =>
				{
					services.AddSingleton(exceptionRecorder);
					services.AddSingleton(handlerRecorder);
					services.AddSingleton(new PipelineRecorder());
				})
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("throw"));

			// Assert
			result.ResponseCode.Should().Be(418);
			handlerRecorder.Events.Should().Equal("non-matching");
			exceptionRecorder.Exception.Should().BeOfType<InvalidOperationException>();
		}

		[TestMethod]
		public void Run_WithMultipleExceptionHandlers_DoesNotActivateHandlersAfterHandledException()
		{
			// Arrange
			var exceptionRecorder = new ExceptionRecorder();
			var handlerRecorder = new ExceptionHandlerRecorder();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<NonMatchingExceptionHandler>()
				.UseExceptionHandler<RecordingExceptionHandler>()
				.UseExceptionHandler<ConstructorThrowingExceptionHandler>()
				.ConfigureServices(services =>
				{
					services.AddSingleton(exceptionRecorder);
					services.AddSingleton(handlerRecorder);
					services.AddSingleton(new PipelineRecorder());
				})
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("throw"));

			// Assert
			result.ResponseCode.Should().Be(418);
		}

		[TestMethod]
		public void Run_WithExceptionHandlers_WhenNoneHandlesException_ReturnsInternalServerError()
		{
			// Arrange
			var handlerRecorder = new ExceptionHandlerRecorder();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<NonMatchingExceptionHandler>()
				.ConfigureServices(services =>
				{
					services.AddSingleton(handlerRecorder);
					services.AddSingleton(new PipelineRecorder());
				})
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("throw"));

			// Assert
			result.ResponseCode.Should().Be(500);
			result.ResponseBody.Should().BeNull();
			handlerRecorder.Events.Should().Equal("non-matching");
		}

		[TestMethod]
		public void Run_WithExceptionHandler_WhenHandlerReturnsNullResult_ReturnsInternalServerError()
		{
			// Arrange
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<NullResultExceptionHandler>()
				.ConfigureServices(services => services.AddSingleton(new PipelineRecorder()))
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("throw"));

			// Assert
			result.ResponseCode.Should().Be(500);
			result.ResponseBody.Should().BeNull();
		}

		[TestMethod]
		public void Run_WithExceptionHandler_ClearsResponseWrittenBeforeMiddlewareFailure()
		{
			// Arrange
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<StatusOnlyExceptionHandler>()
				.UseMiddleware<WritesResponseAndThrowsMiddleware>()
				.ConfigureServices(services => services.AddSingleton(new PipelineRecorder()))
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("success"));

			// Assert
			result.ResponseCode.Should().Be(500);
			result.ResponseBody.Should().BeNull();
			result.ResponseBodyBytes.Should().BeNull();
		}

		[TestMethod]
		public void Run_WithExceptionHandler_PreservesHeadersSetByOuterMiddleware()
		{
			// Arrange
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseMiddleware<SetsHeaderMiddleware>()
				.UseExceptionHandler<StatusOnlyExceptionHandler>()
				.ConfigureServices(services => services.AddSingleton(new PipelineRecorder()))
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("throw"));

			// Assert
			result.ResponseCode.Should().Be(500);
			result.ResponseHeaders.AsDictionary().Should().ContainKey("X-Outer");
			result.ResponseHeaders.AsDictionary()["X-Outer"].Should().Equal("outer");
		}

		[TestMethod]
		public void Run_WithExceptionHandler_DiscardsHeadersSetByEndpointBeforeFailure()
		{
			// Arrange
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<StatusOnlyExceptionHandler>()
				.ConfigureServices(services => services.AddSingleton(new PipelineRecorder()))
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("header-throw"));

			// Assert
			result.ResponseCode.Should().Be(500);
			result.ResponseHeaders.AsDictionary().Should().NotContainKey("X-Endpoint");
		}

		[TestMethod]
		public void Run_WhenExceptionHandlerFails_ResetsExistingResponseForFallback()
		{
			// Arrange
			var exceptionRecorder = new PartialResponseExceptionRecorder();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<PartialResponseExceptionHandler>()
				.ConfigureServices(services =>
				{
					services.AddSingleton(exceptionRecorder);
					services.AddSingleton(new PipelineRecorder());
				})
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("throw"));

			// Assert
			result.ResponseCode.Should().Be(500);
			result.ResponseBody.Should().BeNull();
			result.ResponseBodyBytes.Should().BeNull();
			result.Should().BeSameAs(exceptionRecorder.PartialResponse);
		}

		[TestMethod]
		public void Run_WithLoggingMiddlewareBeforeExceptionHandler_LogsRequestAndHandledResponse()
		{
			// Arrange
			var recorder = new PipelineRecorder();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseMiddleware<RequestLoggingMiddleware>()
				.UseExceptionHandler<RecordingExceptionHandler>()
				.ConfigureServices(services =>
				{
					services.AddSingleton(recorder);
					services.AddSingleton(new ExceptionRecorder());
				})
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("throw"));

			// Assert
			result.ResponseCode.Should().Be(418);
			recorder.Events.Should().Equal(
				"request:/v1/pipeline/throw",
				"response:418");
		}

		[TestMethod]
		public void Run_WithExceptionMiddleware_HandlesControllerExceptions()
		{
			// Arrange
			var recorder = new ExceptionRecorder();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.UseExceptionHandler<RecordingExceptionHandler>()
				.ConfigureServices(services =>
				{
					services.AddSingleton(recorder);
					services.AddSingleton(new PipelineRecorder());
				})
				.Build();

			// Act
			var result = api.Run(new EngineMock(), CreateRequest("throw"));

			// Assert
			result.ResponseCode.Should().Be(418);
			recorder.Exception.Should().BeOfType<InvalidOperationException>();
		}

		[TestMethod]
		public void Run_WithoutExceptionHandler_PreservesExceptionBehavior()
		{
			// Arrange
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_Pipeline>()
				.ConfigureServices(services => services.AddSingleton(new PipelineRecorder()))
				.Build();

			// Act
			var act = () => api.Run(new EngineMock(), CreateRequest("throw"));

			// Assert
			act.Should().Throw<InvalidOperationException>()
				.Which.Message.Should().Be("controller failure");
		}

		private static ApiTriggerInput CreateRequest(string action)
		{
			return new ApiTriggerInput
			{
				Route = "/v1/pipeline/" + action,
				RequestMethod = RequestMethod.Get,
			};
		}
	}

	internal sealed class PipelineRecorder
	{
		public List<string> Events { get; } = new List<string>();
	}

	internal sealed class ExceptionRecorder
	{
		public Exception? Exception { get; set; }

		public ApiContext? Context { get; set; }
	}
}
