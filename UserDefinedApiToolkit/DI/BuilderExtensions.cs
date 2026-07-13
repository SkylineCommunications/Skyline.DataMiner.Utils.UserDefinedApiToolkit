namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Linq;
	using System.Reflection;

	/// <summary>
	/// Extension methods for <see cref="UserDefinedApiBuilder"/> that discover and register
	/// controllers automatically, instead of registering each controller type individually via
	/// <see cref="UserDefinedApiBuilder.AddController(Type)"/>.
	/// </summary>
	public static class BuilderExtensions
	{
		/// <summary>
		/// Registers every public, non-abstract class in <paramref name="assembly"/> that inherits
		/// from <see cref="ControllerBase"/> and has a <see cref="RouteAttribute"/>.
		/// </summary>
		/// <param name="builder">The builder to register controllers on.</param>
		/// <param name="assembly">The assembly to scan for controller types.</param>
		/// <returns>The same builder instance, for chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
		public static UserDefinedApiBuilder AddControllersFromAssembly(
					this UserDefinedApiBuilder builder,
					Assembly assembly)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			// Find all public, non-abstract classes that inherit ControllerBase
			var controllerTypes = assembly.GetTypes()
			.Where(t => t.IsClass &&
						!t.IsAbstract &&
						typeof(ControllerBase).IsAssignableFrom(t) &&
						t.GetCustomAttribute<RouteAttribute>() != null);

			foreach (var controllerType in controllerTypes)
			{
				builder.AddController(controllerType);
			}

			return builder;
		}

		/// <summary>
		/// Registers every public, non-abstract class in the calling assembly that inherits from
		/// <see cref="ControllerBase"/> and has a <see cref="RouteAttribute"/>. See
		/// <see cref="AddControllersFromAssembly"/> for details.
		/// </summary>
		/// <param name="builder">The builder to register controllers on.</param>
		/// <returns>The same builder instance, for chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
		public static UserDefinedApiBuilder AddControllers(
			this UserDefinedApiBuilder builder)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			var currentAssembly = Assembly.GetCallingAssembly();
			return builder.AddControllersFromAssembly(currentAssembly);
		}
	}
}
