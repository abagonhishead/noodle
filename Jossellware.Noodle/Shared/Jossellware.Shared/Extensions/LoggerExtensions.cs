namespace Jossellware.Shared.Extensions
{
    using System;
    using Microsoft.Extensions.Logging;

    public static class LoggerExtensions
	{
		public static IDisposable BeginMethodScope(this ILogger logger, string methodName)
		{
			return logger.BeginScope("Method:{MethodName}", methodName);
		}

		public static IDisposable BeginMethodScope(this ILogger logger, string typeName, string methodName)
		{
			return logger.BeginScope("Method:{TypeName}.{MethodName}", typeName, methodName);
		}

		public static IDisposable BeginMethodScope<T>(this ILogger logger, string methodName)
		{
			return logger.BeginMethodScope(typeof(T).Name, methodName);
		}
	}
}
