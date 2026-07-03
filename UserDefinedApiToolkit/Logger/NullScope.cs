namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	internal sealed class NullScope : IDisposable
	{
		internal static NullScope Instance { get; } = new NullScope();

		public void Dispose()
		{
		}
	}
}
