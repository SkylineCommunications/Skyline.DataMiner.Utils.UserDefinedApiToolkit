namespace UserDefinedApiToolkit.Tests.Runtime.DI.TestFiles
{
	using System.Threading;

	public class TrackedTransientService
	{
		private static int instantiationCount;

		public TrackedTransientService()
		{
			Interlocked.Increment(ref instantiationCount);
		}

		public static int InstantiationCount => instantiationCount;

		public static void Reset()
		{
			instantiationCount = 0;
		}
	}
}
