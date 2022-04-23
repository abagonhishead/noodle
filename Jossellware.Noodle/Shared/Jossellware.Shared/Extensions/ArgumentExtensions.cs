namespace Jossellware.Shared.Extensions
{
	using System;

	public static class ArgumentExtensions
	{
		public static void ThrowIfNull<T>(this T source, string parameterName)
		{
			if (source is null)
			{
				throw new ArgumentNullException(parameterName);
			}
		}

		public static void ThrowIfNullOrEmpty(this string source, string parameterName)
		{
			if (parameterName.IsNullOrEmpty())
			{
				throw new ArgumentException("Cannot be null or empty", parameterName);
			}
		}

		public static void ThrowIfNullOrWhitespace(this string source, string parameterName)
		{
			if (parameterName.IsNullOrWhitespace())
			{
				throw new ArgumentException("Cannot be null, empty or whitespace", parameterName);
			}
		}
	}
}
