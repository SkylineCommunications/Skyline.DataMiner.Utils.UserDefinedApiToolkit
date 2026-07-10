namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Thrown when a controller type registered via <see cref="UserDefinedApiBuilder.AddController(System.Type)"/>
	/// does not inherit from <see cref="ControllerBase"/> or does not have a valid
	/// <see cref="RouteAttribute"/>.
	/// </summary>
	[Serializable]
	public class InvalidControllerException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidControllerException"/> class.
		/// </summary>
		public InvalidControllerException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidControllerException"/> class with a
		/// custom message.
		/// </summary>
		/// <param name="message">The exception message.</param>
		public InvalidControllerException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidControllerException"/> class with a
		/// custom message and inner exception.
		/// </summary>
		/// <param name="message">The exception message.</param>
		/// <param name="innerException">The exception that caused this exception.</param>
		public InvalidControllerException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidControllerException"/> class with
		/// serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		protected InvalidControllerException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
