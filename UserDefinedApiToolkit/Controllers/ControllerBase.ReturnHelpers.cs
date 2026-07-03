namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	public abstract partial class ControllerBase
	{
		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object with the specified status code.
		/// </summary>
		/// <param name="statusCode">The HTTP status code.</param>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult StatusCode(int statusCode)
		{
			return new StatusCodeResult(statusCode);
		}

		/// <summary>
		/// Creates a <see cref="ObjectResult{T}"/> object with the specified status code and value.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="statusCode">The HTTP status code.</param>
		/// <param name="value">The value to format in the response body.</param>
		/// <returns>The created <see cref="ObjectResult{T}"/> object.</returns>
		public ObjectResult<T> StatusCode<T>(int statusCode, T value)
		{
			return new ObjectResult<T>(statusCode, value)
			{
				Converter = DefaultOutputConverter,
			};
		}

		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object that produces a status 200 OK response.
		/// </summary>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult Ok()
		{
			return new StatusCodeResult(200);
		}

		/// <summary>
		/// Creates a <see cref="ObjectResult{T}"/> object that produces a status 200 OK response.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to format in the response body.</param>
		/// <returns>The created <see cref="ObjectResult{T}"/> object.</returns>
		public ObjectResult<T> Ok<T>(T value)
		{
			return new ObjectResult<T>(200, value)
			{
				Converter = DefaultOutputConverter,
			};
		}

		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object that produces a status 404 Not Found response.
		/// </summary>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult NotFound()
		{
			return new StatusCodeResult(404);
		}

		/// <summary>
		/// Creates a <see cref="ObjectResult{T}"/> object that produces a status 404 Not Found response.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to format in the response body.</param>
		/// <returns>The created <see cref="ObjectResult{T}"/> object.</returns>
		public ObjectResult<T> NotFound<T>(T value)
		{
			return new ObjectResult<T>(404, value)
			{
				Converter = DefaultOutputConverter,
			};
		}

		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object that produces a status 401 Unauthorized response.
		/// </summary>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult Unauthorized()
		{
			return new StatusCodeResult(401);
		}

		/// <summary>
		/// Creates a <see cref="ObjectResult{T}"/> object that produces a status 401 Unauthorized response.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to format in the response body.</param>
		/// <returns>The created <see cref="ObjectResult{T}"/> object.</returns>
		public ObjectResult<T> Unauthorized<T>(T value)
		{
			return new ObjectResult<T>(401, value)
			{
				Converter = DefaultOutputConverter,
			};
		}

		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object that produces a status 400 Bad Request response.
		/// </summary>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult BadRequest()
		{
			return new StatusCodeResult(400);
		}

		/// <summary>
		/// Creates a <see cref="ObjectResult{T}"/> object that produces a status 400 Bad Request response.
		/// </summary>
		/// <typeparam name="T">The type of the error value.</typeparam>
		/// <param name="error">The error to format in the response body.</param>
		/// <returns>The created <see cref="ObjectResult{T}"/> object.</returns>
		public ObjectResult<T> BadRequest<T>(T error)
		{
			return new ObjectResult<T>(400, error)
			{
				Converter = DefaultOutputConverter,
			};
		}

		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object that produces a status 201 Created response.
		/// </summary>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult Created()
		{
			return new StatusCodeResult(201);
		}

		/// <summary>
		/// Creates a <see cref="ObjectResult{T}"/> object that produces a status 201 Created response.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to format in the response body.</param>
		/// <returns>The created <see cref="ObjectResult{T}"/> object.</returns>
		public ObjectResult<T> Created<T>(T value)
		{
			return new ObjectResult<T>(201, value)
			{
				Converter = DefaultOutputConverter,
			};
		}

		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object that produces a status 204 No Content response.
		/// </summary>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult NoContent()
		{
			return new StatusCodeResult(204);
		}

		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object that produces a status 403 Forbidden response.
		/// </summary>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult Forbid()
		{
			return new StatusCodeResult(403);
		}

		/// <summary>
		/// Creates a <see cref="ObjectResult{T}"/> object that produces a status 403 Forbidden response.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to format in the response body.</param>
		/// <returns>The created <see cref="ObjectResult{T}"/> object.</returns>
		public ObjectResult<T> Forbid<T>(T value)
		{
			return new ObjectResult<T>(403, value)
			{
				Converter = DefaultOutputConverter,
			};
		}

		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object that produces a status 409 Conflict response.
		/// </summary>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult Conflict()
		{
			return new StatusCodeResult(409);
		}

		/// <summary>
		/// Creates a <see cref="ObjectResult{T}"/> object that produces a status 409 Conflict response.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to format in the response body.</param>
		/// <returns>The created <see cref="ObjectResult{T}"/> object.</returns>
		public ObjectResult<T> Conflict<T>(T value)
		{
			return new ObjectResult<T>(409, value)
			{
				Converter = DefaultOutputConverter,
			};
		}

		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object that produces a status 422 Unprocessable Entity response.
		/// </summary>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult UnprocessableEntity()
		{
			return new StatusCodeResult(422);
		}

		/// <summary>
		/// Creates a <see cref="ObjectResult{T}"/> object that produces a status 422 Unprocessable Entity response.
		/// </summary>
		/// <typeparam name="T">The type of the error value.</typeparam>
		/// <param name="error">The error to format in the response body.</param>
		/// <returns>The created <see cref="ObjectResult{T}"/> object.</returns>
		public ObjectResult<T> UnprocessableEntity<T>(T error)
		{
			return new ObjectResult<T>(422, error)
			{
				Converter = DefaultOutputConverter,
			};
		}

		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object that produces a status 500 Internal Server Error response.
		/// </summary>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult InternalServerError()
		{
			return new StatusCodeResult(500);
		}

		/// <summary>
		/// Creates a <see cref="ObjectResult{T}"/> object that produces a status 500 Internal Server Error response.
		/// </summary>
		/// <typeparam name="T">The type of the error value.</typeparam>
		/// <param name="error">The error to format in the response body.</param>
		/// <returns>The created <see cref="ObjectResult{T}"/> object.</returns>
		public ObjectResult<T> InternalServerError<T>(T error)
		{
			return new ObjectResult<T>(500, error)
			{
				Converter = DefaultOutputConverter,
			};
		}

		/// <summary>
		/// Creates a <see cref="StatusCodeResult"/> object that produces a status 503 Service Unavailable response.
		/// </summary>
		/// <returns>The created <see cref="StatusCodeResult"/> object.</returns>
		public StatusCodeResult ServiceUnavailable()
		{
			return new StatusCodeResult(503);
		}

		/// <summary>
		/// Creates a <see cref="ObjectResult{T}"/> object that produces a status 503 Service Unavailable response.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to format in the response body.</param>
		/// <returns>The created <see cref="ObjectResult{T}"/> object.</returns>
		public ObjectResult<T> ServiceUnavailable<T>(T value)
		{
			return new ObjectResult<T>(503, value)
			{
				Converter = DefaultOutputConverter,
			};
		}
	}
}
