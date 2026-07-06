namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions
{
	using System;

	/// <summary>
	/// Thrown when a request value (e.g. a query string or route segment value) could not be
	/// converted to the type expected by a controller action parameter.
	/// </summary>
	[Serializable]
	public class InvalidParameterException : Exception
	{
		public InvalidParameterException(ApiContext context, string parameterName, string rawValue, Type targetType)
			: this(context, parameterName, rawValue, targetType, $"Could not convert value '{rawValue}' for parameter '{parameterName}' to type '{targetType.Name}'.")
		{
		}

		public InvalidParameterException(ApiContext context, string parameterName, string rawValue, Type targetType, string message)
			: base(message)
		{
			Context = context;
			ParameterName = parameterName;
			RawValue = rawValue;
			TargetType = targetType;
		}

		public InvalidParameterException(ApiContext context, string parameterName, string rawValue, Type targetType, string message, Exception innerException)
			: base(message, innerException)
		{
			Context = context;
			ParameterName = parameterName;
			RawValue = rawValue;
			TargetType = targetType;
		}

		public ApiContext Context { get; }

		public string ParameterName { get; }

		public string RawValue { get; }

		public Type TargetType { get; }
	}
}
