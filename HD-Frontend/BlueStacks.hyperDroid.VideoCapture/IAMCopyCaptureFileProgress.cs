using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("670d1d20-a068-11d0-b3f0-00aa003761c5")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAMCopyCaptureFileProgress
	{
		int Progress(int iProgress);
	}
}
