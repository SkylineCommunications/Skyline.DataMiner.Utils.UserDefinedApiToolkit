namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	/// <summary>
	/// The result of classifying a controller action's parameter: where its value should come
	/// from, and (when applicable) the route/query key name to look it up by.
	/// </summary>
	internal readonly struct ParameterBinding
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ParameterBinding"/> struct.
		/// </summary>
		/// <param name="source">Where the parameter's value should be resolved from.</param>
		/// <param name="name">The route/query key name to look the value up by, or <see langword="null"/> when not applicable (e.g. <see cref="ParameterBindingSource.Framework"/> or <see cref="ParameterBindingSource.DependencyInjection"/>).</param>
		public ParameterBinding(ParameterBindingSource source, string? name)
		{
			Source = source;
			Name = name;
		}

		/// <summary>
		/// Gets where the parameter's value should be resolved from.
		/// </summary>
		public ParameterBindingSource Source { get; }

		/// <summary>
		/// Gets the route/query key name to look the value up by, or <see langword="null"/> when
		/// not applicable.
		/// </summary>
		public string? Name { get; }
	}
}
