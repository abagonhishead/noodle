namespace Jossellware.Shared.Mapping.Maps
{
	using System;
	using Jossellware.Shared.Mapping.Models;

	public abstract class ClassMapBase<TSource, TDestination> : IClassMap<TSource, TDestination>
		where TSource : class, IClassMapSource
		where TDestination : class, IClassMapDestination
	{
		public TDestination Build(TSource source, TDestination? destination)
		{
			return this.BuildImplementation(source, destination);
		}

		public TDestination Build(TSource source)
		{
			return this.Build(source, this.DestinationFactory());
		}

		public object Build(object source, object destination)
		{
			if (!this.CanMap(source.GetType(), destination.GetType()))
			{
				throw new NotSupportedException($"Source and/or destination type not supported by this map");
			}

			return this.Build(source as TSource, destination as TDestination);
		}

		public object Build(object source)
		{
			if (source.GetType() != typeof(TSource))
			{
				throw new NotSupportedException($"Source type not supported by this map");
			}

			return this.Build(source as TSource);
		}

		public bool CanMap(Type sourceType, Type destinationType)
		{
			return sourceType == typeof(TSource) && destinationType == typeof(TDestination);
		}

		protected virtual TDestination? DestinationFactory(params object[] args)
		{
			return default(TDestination?);
		}

		protected abstract TDestination BuildImplementation(TSource source, TDestination? destination = null);
	}
}
