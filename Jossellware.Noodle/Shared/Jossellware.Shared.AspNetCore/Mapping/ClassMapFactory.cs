namespace Jossellware.Shared.AspNetCore.Mapping
{
	using System.Collections.Concurrent;
	using Jossellware.Shared.Mapping.Maps;
	using Jossellware.Shared.Mapping.Models;

	public class ClassMapFactory : IClassMapFactory
	{
		private readonly IServiceProvider serviceProvider;

		private ConcurrentDictionary<string, IClassMap> mapCache;

		public ClassMapFactory(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
			this.mapCache = new ConcurrentDictionary<string, IClassMap>();
		}

		public IClassMap<TSource, TDestination> Get<TSource, TDestination>()
			where TSource : class, IClassMapSource
			where TDestination : class, IClassMapDestination
		{
			var key = ClassMapFactory.BuildCacheKey<TSource, TDestination>();
			if (this.mapCache.TryGetValue(key, out var cacheResult) &&
				cacheResult?.CanMap(typeof(TSource), typeof(TDestination)) == true)
			{
				return (IClassMap<TSource, TDestination>)cacheResult;
			}

			try
			{
				var result = this.serviceProvider.GetRequiredService<IClassMap<TSource, TDestination>>();
				if (result.CanMap(typeof(TSource), typeof(TDestination)))
				{
					this.mapCache[key] = result;
					return (IClassMap<TSource, TDestination>)result;
				}
			}
			catch (InvalidOperationException)
			{
			}

			throw new ApplicationException($"Couldn't find an IClassMap implementation for {typeof(TSource).FullName} -> {typeof(TDestination).FullName} (Source: {typeof(TSource).AssemblyQualifiedName}; Destination: {typeof(TDestination).AssemblyQualifiedName})");
		}

		private static string BuildCacheKey<TSource, TDestination>()
		{
			return $"{typeof(TSource).AssemblyQualifiedName.ToUpperInvariant()}->{typeof(TDestination).AssemblyQualifiedName.ToUpperInvariant()}";
		}
	}
}
