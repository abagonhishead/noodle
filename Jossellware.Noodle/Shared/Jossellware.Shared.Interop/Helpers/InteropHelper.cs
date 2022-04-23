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

		public static bool Is64BitOS()
		{
			if (RuntimeInformation.OSArchitecture == Architecture.X64 ||
				RuntimeInformation.OSArchitecture == Architecture.Arm64)
			{
				return true;
			}

			return false;
		}

		public static bool Is64BitProcess()
		{
			if (RuntimeInformation.ProcessArchitecture == Architecture.X64 ||
				RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
			{
				return true;
			}

			return false;
		}

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
