namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Routes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// A parsed route template, split into literal and <c>{placeholder}</c> segments.
	/// </summary>
	internal sealed class RouteTemplate
	{
		private RouteTemplate(string raw, IReadOnlyList<RouteSegment> segments)
		{
			Raw = raw;
			Segments = segments;
		}

		/// <summary>
		/// Gets the raw route template this instance was parsed from.
		/// </summary>
		public string Raw { get; }

		/// <summary>
		/// Gets the parsed segments of the route template, in order.
		/// </summary>
		public IReadOnlyList<RouteSegment> Segments { get; }

		/// <summary>
		/// Gets the names of all <c>{placeholder}</c> segments in this template.
		/// </summary>
		public IEnumerable<string> PlaceholderNames => Segments.Where(s => s.IsPlaceholder).Select(s => s.Value);

		/// <summary>
		/// Combines a controller-level route template with a method-level route template, e.g.
		/// <c>"items"</c> + <c>"{id}"</c> → <c>"items/{id}"</c>. Either side may be empty.
		/// </summary>
		/// <param name="controllerTemplate">The controller-level route template, e.g. from a <see cref="RouteAttribute"/>.</param>
		/// <param name="methodTemplate">The method-level route template, e.g. from an <see cref="HttpMethodAttribute"/>.</param>
		/// <returns>The combined route template, with leading/trailing slashes trimmed.</returns>
		public static string Combine(string? controllerTemplate, string? methodTemplate)
		{
			var left = controllerTemplate?.Trim('/') ?? String.Empty;
			var right = methodTemplate?.Trim('/') ?? String.Empty;

			if (String.IsNullOrEmpty(right))
			{
				return left;
			}

			if (String.IsNullOrEmpty(left))
			{
				return right;
			}

			return $"{left}/{right}";
		}

		/// <summary>
		/// Parses a combined route template string into its literal and placeholder segments.
		/// </summary>
		/// <param name="template">The raw, combined route template to parse, e.g. <c>"items/{id}"</c>.</param>
		/// <returns>The parsed <see cref="RouteTemplate"/>.</returns>
		public static RouteTemplate Parse(string? template)
		{
			var trimmed = template?.Trim('/') ?? String.Empty;

			if (String.IsNullOrEmpty(trimmed))
			{
				return new RouteTemplate(trimmed, Array.Empty<RouteSegment>());
			}

			var segments = trimmed
				.Split('/')
				.Select(ParseSegment)
				.ToList();

			return new RouteTemplate(trimmed, segments);
		}

		private static RouteSegment ParseSegment(string segment)
		{
			if (segment.Length > 2 && segment[0] == '{' && segment[segment.Length - 1] == '}')
			{
				return RouteSegment.Placeholder(segment.Substring(1, segment.Length - 2));
			}

			return RouteSegment.Literal(segment);
		}

		/// <summary>
		/// Attempts to match <paramref name="requestRoute"/> against this template segment-by-
		/// segment. Literal segments must match exactly; placeholder segments match any value and
		/// are captured into the result by placeholder name.
		/// </summary>
		/// <param name="requestRoute">The incoming request route to match, e.g. <c>"items/5"</c>.</param>
		/// <returns>A <see cref="RouteMatch"/> describing whether and how <paramref name="requestRoute"/> matches this template.</returns>
		public RouteMatch Match(string requestRoute)
		{
			var requestSegments = SplitSegments(requestRoute);

			if (requestSegments.Length != Segments.Count)
			{
				return RouteMatch.NoMatch;
			}

			var values = new Dictionary<string, string>();
			var literalCount = 0;
			for (int i = 0; i < Segments.Count; i++)
			{
				var templateSegment = Segments[i];
				var requestSegment = requestSegments[i];

				if (templateSegment.IsPlaceholder)
				{
					values[templateSegment.Value] = requestSegment;
					continue;
				}

				if (!String.Equals(templateSegment.Value, requestSegment, StringComparison.Ordinal))
				{
					return RouteMatch.NoMatch;
				}

				literalCount++;
			}

			return RouteMatch.Success(literalCount, values);
		}

		private static string[] SplitSegments(string route)
		{
			var trimmed = route?.Trim('/') ?? String.Empty;
			return String.IsNullOrEmpty(trimmed) ? Array.Empty<string>() : trimmed.Split('/');
		}
	}
}
