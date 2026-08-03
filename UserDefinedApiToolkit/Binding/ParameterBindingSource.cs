namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	/// <summary>
	/// Identifies where a controller action's parameter value should be resolved from.
	/// </summary>
	internal enum ParameterBindingSource
	{
		/// <summary>
		/// A framework-provided type (<see cref="ApiContext"/>, <c>IEngine</c>, <c>IConnection</c>,
		/// or <c>IServiceProvider</c>), always resolvable regardless of the request.
		/// </summary>
		Framework,

		/// <summary>
		/// The request body, bound via a <see cref="FromBodyAttribute"/> parameter.
		/// </summary>
		Body,

		/// <summary>
		/// A route placeholder value, bound via a <see cref="FromRouteAttribute"/> parameter or an
		/// unattributed parameter whose name matches a placeholder.
		/// </summary>
		Route,

		/// <summary>
		/// A query string value, bound via a <see cref="FromQueryAttribute"/> parameter or as the
		/// default binding for an otherwise-unclassified parameter.
		/// </summary>
		Query,

		/// <summary>
		/// A service resolved from the dependency injection container, for an unattributed,
		/// non-route-matching parameter whose type is registered with the container.
		/// </summary>
		DependencyInjection,
	}
}
