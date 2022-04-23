namespace Jossellware.Shared.Interop.Helpers
{
	using System;
	using System.Runtime.InteropServices;
	using Jossellware.Shared.Interop.Enums;

	public static class InteropHelper
	{
		public static Platform GetOSPlatform()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				// Android also lands here
				return Platform.Linux;
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return Platform.Windows;
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return Platform.OSX;
			}
			else
			{
				throw new PlatformNotSupportedException($"Unsupported platform: {RuntimeInformation.OSDescription}/arch:{RuntimeInformation.OSArchitecture}");
			}
		}

        public static bool Is64BitOS() => RuntimeInformation.OSArchitecture.ToString().EndsWith("64");

        public static bool Is64BitProcess() => RuntimeInformation.ProcessArchitecture.ToString().EndsWith("64");

		public static bool IsUnixLike()
		{
			switch (InteropHelper.GetOSPlatform())
			{
				case Platform.Linux:
				case Platform.FreeBSD:
				case Platform.OSX:
					return true;
				default:
					return false;
			}
		}
	}
}
