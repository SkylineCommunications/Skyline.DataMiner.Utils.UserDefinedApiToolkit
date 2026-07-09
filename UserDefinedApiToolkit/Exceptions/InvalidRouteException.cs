namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Thrown when a controller's route template and its action parameters are inconsistent, e.g. a
	/// <c>{placeholder}</c> in the template has no bound parameter, or a <see cref="FromRouteAttribute"/>
	/// references a placeholder that doesn't exist in the template.
	/// </summary>
	[Serializable]
	public class InvalidRouteException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidRouteException"/> class.
		/// </summary>
		public InvalidRouteException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidRouteException"/> class with a
		/// custom message.
		/// </summary>
		/// <param name="message">The exception message.</param>
		public InvalidRouteException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidRouteException"/> class with a
		/// custom message and inner exception.
		/// </summary>
		/// <param name="message">The exception message.</param>
		/// <param name="innerException">The exception that caused this exception.</param>
		public InvalidRouteException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <inheritdoc/>
		protected InvalidRouteException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
