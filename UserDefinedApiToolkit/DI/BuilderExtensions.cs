namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit
{
	using System;
	using System.Linq;
	using System.Reflection;

	public static class BuilderExtensions
	{
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
