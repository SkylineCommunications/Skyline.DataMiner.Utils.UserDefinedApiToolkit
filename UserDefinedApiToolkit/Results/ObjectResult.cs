namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Linq;

	/// <summary>
	/// An <see cref="IApiResult"/> that writes both a status code and a serialized body to the
	/// response. Typically created via helper methods on <see cref="ControllerBase"/>
	/// such as <c>Ok(value)</c>.
	/// </summary>
	/// <typeparam name="T">The type of the value to serialize as the response body.</typeparam>
	public class ObjectResult<T> : StatusCodeResult, IApiResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectResult{T}"/> class.
		/// </summary>
		/// <param name="statusCode">The HTTP status code to write to the response.</param>
		/// <param name="value">The value to serialize as the response body.</param>
		public ObjectResult(int statusCode, T value) : base(statusCode)
		{
			Value = value;
		}

		/// <summary>
		/// Gets the value that will be serialized as the response body.
		/// </summary>
		public T Value { get; }

		internal IOutputConverter? Converter { get; set; }

		/// <inheritdoc/>
		public override void ExecuteResult(ApiContext context)
		{
			base.ExecuteResult(context);

			var converter = Converter ?? context.OutputConverters.Reverse().FirstOrDefault(c => c.CanConvertOutput(typeof(T)));
			if (converter is null)
			{
				throw new NotSupportedException($"Could not convert result of type '{typeof(T).FullName}', because no valid converter was found.");
			}

			context.Response.ResponseBody = converter.ConvertOutput(Value, typeof(T));
		}
	}
}
