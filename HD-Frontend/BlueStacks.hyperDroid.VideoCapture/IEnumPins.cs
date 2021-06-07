using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[Guid("56a86892-0ad4-11ce-b03a-0020af0ba770")]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumPins
	{
		int Next([In] int cPins, [Out] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IPin[] ppPins, [In] IntPtr pcFetched);

		int Skip([In] int cPins);

		int Reset();

		int Clone(out IEnumPins ppEnum);
	}
}
