namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;
	using Newtonsoft.Json.Linq;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;

	/// <summary>
	/// Provides helpers for creating DataMiner API trigger inputs.
	/// </summary>
	public static class ApiTriggerInputHelper
	{
		private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			Converters = { new StringEnumConverter() },
		};

		/// <summary>
		/// Deserializes a JSON object into an <see cref="ApiTriggerInput"/>.
		/// </summary>
		/// <param name="json">The JSON object to deserialize.</param>
		/// <returns>The deserialized API trigger input.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
		/// <exception cref="JsonException">
		/// Thrown when the JSON is invalid or the query parameters do not contain an object.
		/// </exception>
		public static ApiTriggerInput Deserialize(string json)
		{
			if (json is null)
			{
				throw new ArgumentNullException(nameof(json));
			}

			var jsonObject = JObject.Parse(json);
			var queryParametersProperty = jsonObject.Properties()
				.FirstOrDefault(property => String.Equals(
					property.Name,
					nameof(ApiTriggerInput.QueryParameters),
					StringComparison.OrdinalIgnoreCase));
			var queryParametersToken = queryParametersProperty?.Value;
			queryParametersProperty?.Remove();

			var input = jsonObject.ToObject<ApiTriggerInput>(JsonSerializer.Create(Settings))
				?? throw new JsonSerializationException("The API trigger input JSON resolved to null.");

			if (queryParametersToken is null || queryParametersToken.Type == JTokenType.Null)
			{
				return input;
			}

			if (queryParametersToken.Type != JTokenType.Object)
			{
				throw new JsonSerializationException(
					$"The '{nameof(ApiTriggerInput.QueryParameters)}' property must contain a JSON object.");
			}

			var queryParameters = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			foreach (var property in queryParametersToken.Children<JProperty>())
			{
				queryParameters[property.Name] = DeserializeQueryValues(property.Value, property.Name);
			}

			input.QueryParameters = new QueryParameters(queryParameters);
			return input;
		}

		private static List<string> DeserializeQueryValues(JToken token, string parameterName)
		{
			if (token.Type == JTokenType.Array)
			{
				var values = new List<string>();
				foreach (var value in token.Children())
				{
					values.Add(DeserializeQueryValue(value, parameterName));
				}

				return values;
			}

			return new List<string> { DeserializeQueryValue(token, parameterName) };
		}

		private static string DeserializeQueryValue(JToken token, string parameterName)
		{
			if (token.Type == JTokenType.Null)
			{
				throw new JsonSerializationException(
					$"The query parameter '{parameterName}' contains a null value.");
			}

			if (token.Type == JTokenType.Object || token.Type == JTokenType.Array)
			{
				throw new JsonSerializationException(
					$"The query parameter '{parameterName}' contains a value that is not a string.");
			}

			return token.ToObject<string>() ?? throw new JsonSerializationException(
				$"The query parameter '{parameterName}' contains a null value.");
		}
	}
}
