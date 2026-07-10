namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Routes
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	/// <summary>
	/// The result of matching a request route against a <see cref="RouteTemplate"/>.
	/// </summary>
	internal readonly struct RouteMatch
	{
		private static readonly IReadOnlyDictionary<string, string> EmptyRouteValues = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

		private RouteMatch(bool isMatch, int literalMatches, IReadOnlyDictionary<string, string> routeValues)
		{
			IsMatch = isMatch;
			LiteralMatches = literalMatches;
			RouteValues = routeValues;
		}

		/// <summary>
		/// Gets a <see cref="RouteMatch"/> representing a failed match, with no literal matches
		/// and no captured route values.
		/// </summary>
		public static RouteMatch NoMatch { get; } = new RouteMatch(false, 0, EmptyRouteValues);

		/// <summary>
		/// Gets a value indicating whether the request route matched the template.
		/// </summary>
		public bool IsMatch { get; }

		/// <summary>
		/// Gets the number of literal (non-placeholder) segments that matched.
		/// </summary>
		public int LiteralMatches { get; }

		/// <summary>
		/// Gets the placeholder values captured from the request route, keyed by placeholder name.
		/// </summary>
		public IReadOnlyDictionary<string, string> RouteValues { get; }

		/// <summary>
		/// Creates a <see cref="RouteMatch"/> representing a successful match.
		/// </summary>
		/// <param name="literalMatches">The number of literal (non-placeholder) segments that matched.</param>
		/// <param name="routeValues">The placeholder values captured from the request route, keyed by placeholder name.</param>
		/// <returns>A successful <see cref="RouteMatch"/>.</returns>
		public static RouteMatch Success(int literalMatches, IReadOnlyDictionary<string, string> routeValues)
		{
			return new RouteMatch(true, literalMatches, routeValues);
		}
	}
}
