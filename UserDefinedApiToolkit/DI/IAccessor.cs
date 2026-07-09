namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;

	/// <summary>
	/// Provides scoped access to a per-request value, such as <see cref="IEngine"/> or
	/// <see cref="IConnection"/>. Inject <c>IAccessor&lt;IEngine&gt;</c> or
	/// <c>IAccessor&lt;IConnection&gt;</c> into a controller or service constructor to access the
	/// engine/connection for the current request.
	/// </summary>
	/// <typeparam name="T">The type of the accessed value.</typeparam>
	public interface IAccessor<T>
	{
		/// <summary>
		/// Gets the current value for this request scope.
		/// </summary>
		T Value { get; }

		/// <summary>
		/// Sets the value for this request scope. Called by the framework; not intended to be
		/// called from user code.
		/// </summary>
		/// <param name="value">The value to store.</param>
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
