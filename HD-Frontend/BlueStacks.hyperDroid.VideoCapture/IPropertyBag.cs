using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("55272A00-42CB-11CE-8135-00AA004BB851")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPropertyBag
	{
		int Read([In] [MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [MarshalAs(UnmanagedType.Struct)] out object pVar, [In] IErrorLog pErrorLog);

		int Write([In] [MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [In] [MarshalAs(UnmanagedType.Struct)] ref object pVar);
	}
}
