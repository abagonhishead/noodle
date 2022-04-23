namespace Jossellware.Shared.AspNetCore.Mapping
{
	using Jossellware.Shared.Mapping.Maps;
	using Jossellware.Shared.Mapping.Models;

	public interface IClassMapFactory
	{
		public IClassMap<TSource, TDestination> Get<TSource, TDestination>()
			where TSource : class, IClassMapSource
			where TDestination : class, IClassMapDestination;
	}
}