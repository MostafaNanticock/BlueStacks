using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[Guid("a2104830-7c70-11cf-8bce-00aa00a3f1a6")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[SuppressUnmanagedCodeSecurity]
	public interface IFileSinkFilter
	{
		int SetFileName([In] [MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [In] [MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

		int GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string pszFileName, [Out] [MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);
	}
}
