namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Routes
{
	/// <summary>
	/// A single segment of a parsed <see cref="RouteTemplate"/> either a literal path segment
	/// (e.g. <c>"items"</c>) or a placeholder segment (e.g. <c>"{id}"</c>, stored without braces
	/// as <c>"id"</c>).
	/// </summary>
	internal readonly struct RouteSegment
	{
		private RouteSegment(bool isPlaceholder, string value)
		{
			IsPlaceholder = isPlaceholder;
			Value = value;
		}

		/// <summary>
		/// Gets a value indicating whether this segment is a <c>{placeholder}</c> segment.
		/// </summary>
		public bool IsPlaceholder { get; }

		/// <summary>
		/// Gets the literal text of this segment, or the placeholder name (without braces) when
		/// <see cref="IsPlaceholder"/> is <c>true</c>.
		/// </summary>
		public string Value { get; }

		public static RouteSegment Literal(string value) => new RouteSegment(false, value);

		public static RouteSegment Placeholder(string name) => new RouteSegment(true, name);
	}
}
