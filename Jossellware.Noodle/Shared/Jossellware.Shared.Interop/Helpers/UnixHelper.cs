namespace Jossellware.Shared.Interop.Helpers
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Runtime.InteropServices;
	using Jossellware.Shared.Extensions;
	using Jossellware.Shared.Interop.Enums;

	public static class UnixHelper
	{
		/* These are here mostly for documentation, as we have them in an enum */
		// user permissions
		private const int S_IRUSR = 0x100;
		private const int S_IWUSR = 0x80;
		private const int S_IXUSR = 0x40;

		// group permission
		private const int S_IRGRP = 0x20;
		private const int S_IWGRP = 0x10;
		private const int S_IXGRP = 0x8;

		// other permissions
		private const int S_IROTH = 0x4;
		private const int S_IWOTH = 0x2;
		private const int S_IXOTH = 0x1;

		public static void SetFilePermissions(string path, UnixPermission mode)
		{
			if (!InteropHelper.IsUnixLike())
			{
				throw new PlatformNotSupportedException("chmod can only be used on UNIX-like platforms");
			}

			path.ThrowIfNullOrWhitespace(nameof(path));

			if (!File.Exists(path) || string.IsNullOrWhiteSpace(path = Path.GetFullPath(path)))
			{
				throw new FileNotFoundException("Couldn't set permissions - file not found or invalid path", path);
			}

			var result = NativeMethods.Chmod(path, mode.GetHashCode());
			if (result != 0)
			{
				throw new ExternalException($"Couldn't set UNIX permissions {mode.GetHashCode()} on file at {path}", result);
			}
		}

		private static class NativeMethods
		{
			[DllImport("libc", EntryPoint = "chmod", SetLastError = true, CharSet = CharSet.Ansi)]
			[SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "The operation fails if we specify Unicode. Ansi seems to work fine.")]
			internal static extern int Chmod(string pathname, int mode);
		}
	}
}
