namespace Jossellware.Shared.AspNetCore.Mapping
{
	using Jossellware.Shared.Mapping.Models;

	public interface IClassMapProvider
	{
		TDestination? Map<TSource, TDestination>(TSource source)
			where TSource : class, IClassMapSource
			where TDestination : class, IClassMapDestination;

		TDestination? Map<TSource, TDestination>(TSource source, TDestination destination)
			where TSource : class, IClassMapSource
			where TDestination : class, IClassMapDestination;
	}
}