namespace UserDefinedApiToolkit.Tests.Runtime.DI
{
	using FluentAssertions;

	using Microsoft.Extensions.DependencyInjection;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	using TestFiles;

	[TestClass]
	public sealed class DIParameterTests
	{
		[TestMethod]
		public void Get_WithUnattributedDIParameter_InstantiatesTransientOnlyOnce()
		{
			// Arrange
			TrackedTransientService.Reset();

			var engine = new EngineMock();
			var api = UserDefinedApi.CreateBuilder()
				.AddController<Controller_DI>()
				.ConfigureServices(services => services.AddTransient<TrackedTransientService>())
				.Build();

			// Act
			var act = () => api.Run(engine, new ApiTriggerInput
			{
				Route = "/v1/di-test",
				RequestMethod = RequestMethod.Get,
			});

			// Assert
			// Classify (used both by GetRank for scoring and by Invoke to determine the binding
			// source) must not itself resolve/instantiate the service via IServiceProvider.GetService;
			// only the single actual resolution during Invoke's binding switch should do so.
			act.Should().NotThrow();
			TrackedTransientService.InstantiationCount.Should().Be(1);
		}
	}
}
