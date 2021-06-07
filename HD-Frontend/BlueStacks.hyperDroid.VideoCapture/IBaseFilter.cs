using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("56a86895-0ad4-11ce-b03a-0020af0ba770")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IBaseFilter : IMediaFilter, IPersist
	{
		new int GetClassID(out Guid pClassID);

		new int Stop();

		new int Pause();

		new int Run(long tStart);

		new int GetState([In] int dwMilliSecsTimeout, out FilterState filtState);

		new int SetSyncSource([In] IReferenceClock pClock);

		new int GetSyncSource(out IReferenceClock pClock);

		int EnumPins(out IEnumPins ppEnum);

		int FindPin([In] [MarshalAs(UnmanagedType.LPWStr)] string Id, out IPin ppPin);

		int QueryFilterInfo(out FilterInfo pInfo);

		int JoinFilterGraph([In] IFilterGraph pGraph, [In] [MarshalAs(UnmanagedType.LPWStr)] string pName);

		int QueryVendorInfo([MarshalAs(UnmanagedType.LPWStr)] out string pVendorInfo);
	}
}
