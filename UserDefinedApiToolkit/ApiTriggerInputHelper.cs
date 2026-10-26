namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Collections.Generic;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;
	using Newtonsoft.Json.Linq;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

	/// <summary>
	/// Provides helpers for creating DataMiner API trigger inputs.
	/// </summary>
	public static class ApiTriggerInputHelper
	{
		private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			Converters =
			{
				new StringEnumConverter(),
				new QueryParametersConverter(),
			},
			DateParseHandling = DateParseHandling.None,
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

			return SecureNewtonsoftDeserialization.DeserializeObject<ApiTriggerInput>(json, Settings)
				?? throw new JsonSerializationException("The API trigger input JSON resolved to null.");
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

		private sealed class QueryParametersConverter : JsonConverter
		{
			public override bool CanWrite => false;

			public override bool CanConvert(Type objectType)
			{
				return objectType == typeof(IQueryParameters);
			}

			public override object? ReadJson(
				JsonReader reader,
				Type objectType,
				object? existingValue,
				JsonSerializer serializer)
			{
				if (reader.TokenType == JsonToken.Null)
				{
					return null;
				}

				if (reader.TokenType != JsonToken.StartObject)
				{
					throw new JsonSerializationException(
						$"The '{nameof(ApiTriggerInput.QueryParameters)}' property must contain a JSON object.");
				}

				var queryParametersToken = JObject.Load(reader);
				var queryParameters = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
				foreach (var property in queryParametersToken.Properties())
				{
					queryParameters[property.Name] = DeserializeQueryValues(property.Value, property.Name);
				}

				return new QueryParameters(queryParameters);
			}

			public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
			{
				throw new NotSupportedException();
			}
		}
	}
}
