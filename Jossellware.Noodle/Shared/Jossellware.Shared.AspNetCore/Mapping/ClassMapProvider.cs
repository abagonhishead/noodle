namespace Jossellware.Shared.AspNetCore.Mapping
{
	using System.Collections.Concurrent;
	using Jossellware.Shared.Mapping.Models;

	public class ClassMapProvider : IClassMapProvider
	{
		private readonly IClassMapFactory mapFactory;

		public ClassMapProvider(IClassMapFactory mapFactory)
		{
			this.mapFactory = mapFactory;
		}

		public TDestination? Map<TSource, TDestination>(TSource source)
			where TSource : class, IClassMapSource
			where TDestination : class, IClassMapDestination
		{
			var map = this.mapFactory.Get<TSource, TDestination>();
			return map.Build(source);
		}

		public TDestination? Map<TSource, TDestination>(TSource source, TDestination destination)
			where TSource : class, IClassMapSource
			where TDestination : class, IClassMapDestination
		{
			var map = this.mapFactory.Get<TSource, TDestination>();
			return map.Build(source, destination);
		}
	}
}
