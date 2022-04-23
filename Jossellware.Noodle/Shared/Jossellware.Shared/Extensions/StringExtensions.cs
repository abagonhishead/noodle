namespace Jossellware.Shared.Extensions
{
	public static class StringExtensions
    {
		public static bool IsNullOrWhitespace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}
    }
}
