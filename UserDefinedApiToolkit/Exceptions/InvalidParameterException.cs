namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Thrown when a request value (e.g. a query string or route segment value) could not be
	/// converted to the type expected by a controller action parameter.
	/// </summary>
	[Serializable]
	public class InvalidParameterException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidParameterException"/> class with a
		/// default message describing the failed conversion.
		/// </summary>
		/// <param name="context">The context of the request that triggered the failed conversion.</param>
		/// <param name="parameterName">The name of the parameter that could not be bound.</param>
		/// <param name="rawValue">The raw string value that could not be converted.</param>
		/// <param name="targetType">The parameter type the value could not be converted to.</param>
		public InvalidParameterException(ApiContext context, string parameterName, string rawValue, Type targetType)
			: this(context, parameterName, rawValue, targetType, $"Could not convert value '{rawValue}' for parameter '{parameterName}' to type '{targetType.Name}'.")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidParameterException"/> class with a
		/// custom message.
		/// </summary>
		/// <param name="context">The context of the request that triggered the failed conversion.</param>
		/// <param name="parameterName">The name of the parameter that could not be bound.</param>
		/// <param name="rawValue">The raw string value that could not be converted.</param>
		/// <param name="targetType">The parameter type the value could not be converted to.</param>
		/// <param name="message">The exception message.</param>
		public InvalidParameterException(ApiContext context, string parameterName, string rawValue, Type targetType, string message)
			: base(message)
		{
			Context = context;
			ParameterName = parameterName;
			RawValue = rawValue;
			TargetType = targetType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidParameterException"/> class with a
		/// custom message and inner exception.
		/// </summary>
		/// <param name="context">The context of the request that triggered the failed conversion.</param>
		/// <param name="parameterName">The name of the parameter that could not be bound.</param>
		/// <param name="rawValue">The raw string value that could not be converted.</param>
		/// <param name="targetType">The parameter type the value could not be converted to.</param>
		/// <param name="message">The exception message.</param>
		/// <param name="innerException">The exception that caused this exception.</param>
		public InvalidParameterException(ApiContext context, string parameterName, string rawValue, Type targetType, string message, Exception innerException)
			: base(message, innerException)
		{
			Context = context;
			ParameterName = parameterName;
			RawValue = rawValue;
			TargetType = targetType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidParameterException"/> class with
		/// serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		protected InvalidParameterException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			// ApiContext is not serializable (it wraps live request/converter state), so it can't
			// be preserved across a serialization boundary.
			Context = null!;
			ParameterName = info.GetString(nameof(ParameterName)) ?? String.Empty;
			RawValue = info.GetString(nameof(RawValue)) ?? String.Empty;

			var targetTypeName = info.GetString(nameof(TargetType));
			TargetType = targetTypeName is null ? typeof(object) : Type.GetType(targetTypeName) ?? typeof(object);
		}

		/// <summary>
		/// Gets the context of the request that triggered the failed conversion.
		/// </summary>
		public ApiContext Context { get; }

		/// <summary>
		/// Gets the name of the parameter that could not be bound.
		/// </summary>
		public string ParameterName { get; }

		/// <summary>
		/// Gets the raw string value that could not be converted.
		/// </summary>
		public string RawValue { get; }

		/// <summary>
		/// Gets the parameter type the value could not be converted to.
		/// </summary>
		public Type TargetType { get; }

		/// <inheritdoc/>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			// ApiContext is deliberately not included; it isn't serializable (see the
			// deserialization constructor).
			info.AddValue(nameof(ParameterName), ParameterName);
			info.AddValue(nameof(RawValue), RawValue);
			info.AddValue(nameof(TargetType), TargetType.AssemblyQualifiedName);
		}
	}
}
