namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Routes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Exceptions;

	internal class RouteSelector
	{
		private readonly ICollection<RouteHandlerInfo> _routes;

		public RouteSelector(ICollection<RouteHandlerInfo> routes)
		{
			_routes = routes ?? throw new ArgumentNullException(nameof(routes));
		}

		public RouteHandlerInfo SelectRoute(ApiContext context, IServiceProvider services)
		{
			var potential = _routes
				.Select(route => new { Route = route, Rank = route.GetRank(context, services) })
				.Where(a => a.Rank >= 0)
				.GroupBy(a => a.Rank)
				.OrderByDescending(a => a.Key)
				.FirstOrDefault()?
				.ToList();

			if (potential is null || potential.Count == 0)
			{
				throw new NoRouteException(context);
			}

			if (potential.Count > 1)
			{
				throw new AmbiguousRouteException(context);
			}

			return potential[0].Route;
		}
	}
}
