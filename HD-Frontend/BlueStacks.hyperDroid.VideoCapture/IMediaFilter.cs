using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[Guid("56a86899-0ad4-11ce-b03a-0020af0ba770")]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMediaFilter : IPersist
	{
		new int GetClassID(out Guid pClassID);

		int Stop();

		int Pause();

		int Run([In] long tStart);

		int GetState([In] int dwMilliSecsTimeout, out FilterState State);

		int SetSyncSource([In] IReferenceClock pClock);

		int GetSyncSource(out IReferenceClock pClock);
	}
}
