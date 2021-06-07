using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("0000010c-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPersist
	{
		int GetClassID(out Guid pClassID);
	}
}
