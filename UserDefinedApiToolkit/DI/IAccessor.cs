namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;

	public interface IAccessor<T>
	{
		T Value { get; }

		void SetValue(T value);
	}

	internal class EngineAccessor : IAccessor<IEngine>
	{
		public IEngine Value { get; private set; }

		public void SetValue(IEngine value)
		{
			if (value is null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			Value = value;
		}
	}

	internal class ConnectionAccessor : IAccessor<IConnection>
	{
		public IConnection Value { get; private set; }

		public void SetValue(IConnection value)
		{
			if (value is null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			Value = value;
		}
	}
}
