namespace Jossellware.Shared.AspNetCore.Extensions.DependencyInjection.Mapping
{
	using System.Reflection;
	using Jossellware.Shared.AspNetCore.Mapping;
	using Jossellware.Shared.Mapping.Maps;
	using Jossellware.Shared.Mapping.Models;

	public static class ServiceCollectionExtensions
	{
		public static void AddClassMapProvider(this IServiceCollection services)
		{
			services.AddSingleton<IClassMapFactory, ClassMapFactory>();
			services.AddSingleton<IClassMapProvider, ClassMapProvider>();
		}

		public static void AddClassMap<TClassMap, TSource, TDestination>(this IServiceCollection services)
			where TClassMap : ClassMapBase<TSource, TDestination>
			where TSource : class, IClassMapSource
			where TDestination : class, IClassMapDestination, new()
		{
			services.AddSingleton<IClassMap<TSource, TDestination>, TClassMap>();
		}

		public static void AddAllClassMaps(this IServiceCollection services, params Assembly[] assemblies)
		{
			foreach (var assembly in assemblies)
			{
				var types = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IClassMap)));
				foreach (var type in types)
				{
					var genericInterfaces = type.GetInterfaces().Where(x => x.GetInterfaces().Contains(typeof(IClassMap)));
					foreach (var generic in genericInterfaces)
					{
						services.AddSingleton(generic, type);
					}
				}
			}
		}
	}
}
