namespace Jossellware.Shared.Mapping.Maps
{
	using System;
	using Jossellware.Shared.Mapping.Models;

	public interface IClassMap<TSource, TDestination> : IClassMap
		where TSource : IClassMapSource
		where TDestination : IClassMapDestination
	{
		public TDestination Build(TSource source, TDestination destination);

		public TDestination Build(TSource source);
	}

	public interface IClassMap
	{
		public object Build(object source, object destination);

		public object Build(object source);

		public bool CanMap(Type source, Type destination);
	}
}
