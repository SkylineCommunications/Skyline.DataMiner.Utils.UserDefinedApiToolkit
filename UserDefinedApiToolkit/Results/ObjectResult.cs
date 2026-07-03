namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Linq;

	public class ObjectResult<T> : StatusCodeResult, IApiResult
	{
		public ObjectResult(int statusCode, T value) : base(statusCode)
		{
			Value = value;
		}

		public T Value { get; }

		internal IOutputConverter? Converter { get; set; }

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
