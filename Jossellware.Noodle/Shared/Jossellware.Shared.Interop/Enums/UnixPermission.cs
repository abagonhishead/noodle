namespace Jossellware.Shared.Interop.Enums
{
	using System;

	[Flags]
	public enum UnixPermission
	{
		OtherExec = 0x1,
		OtherWrite = 0x2,
		OtherRead = 0x4,
		GroupExec = 0x8,
		GroupWrite = 0x10,
		GroupRead = 0x20,
		UserExec = 0x40,
		UserWrite = 0x80,
		UserRead = 0x100,
	}
}
