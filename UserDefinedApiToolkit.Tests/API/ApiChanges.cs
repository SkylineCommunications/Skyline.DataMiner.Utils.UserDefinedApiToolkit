namespace UserDefinedApiToolkit.Tests.API
{
	using System.Threading.Tasks;

	using PublicApiGenerator;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

	[TestClass]
	[UsesVerify]
	public partial class ApiChanges
	{
		[TestMethod]
		public Task PublicChanges()
		{
			var assembly = typeof(IUserDefinedApi).Assembly;
			var publicApi = assembly.GeneratePublicApi();

			return Verify(publicApi)
				.UseFileName("Skyline.DataMiner.Utils.UserDefinedApiToolkit");
		}
	}
}
