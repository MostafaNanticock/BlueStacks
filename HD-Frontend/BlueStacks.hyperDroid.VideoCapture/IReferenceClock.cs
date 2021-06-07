using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("56a86897-0ad4-11ce-b03a-0020af0ba770")]
	[SuppressUnmanagedCodeSecurity]
	public interface IReferenceClock
	{
		int GetTime(out long pTime);

		int AdviseTime([In] long baseTime, [In] long streamTime, [In] IntPtr hEvent, out int pdwAdviseCookie);

		int AdvisePeriodic([In] long startTime, [In] long periodTime, [In] IntPtr hSemaphore, out int pdwAdviseCookie);

		int Unadvise([In] int dwAdviseCookie);
	}
}
