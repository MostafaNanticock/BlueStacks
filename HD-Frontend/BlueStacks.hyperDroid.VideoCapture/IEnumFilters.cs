using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("56a86893-0ad4-11ce-b03a-0020af0ba770")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumFilters
	{
		int Next([In] int cFilters, [Out] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IBaseFilter[] ppFilter, [In] IntPtr pcFetched);

		int Skip([In] int cFilters);

		int Reset();

		int Clone(out IEnumFilters ppEnum);
	}
}
