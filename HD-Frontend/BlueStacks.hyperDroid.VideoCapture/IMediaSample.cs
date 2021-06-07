using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[SuppressUnmanagedCodeSecurity]
	[Guid("56a8689a-0ad4-11ce-b03a-0020af0ba770")]
	public interface IMediaSample
	{
		int GetPointer(out IntPtr ppBuffer);

		int GetSize();

		int GetTime(out long pTimeStart, out long pTimeEnd);

		int SetTime([In] long pTimeStart, [In] long pTimeEnd);

		int IsSyncPoint();

		int SetSyncPoint([In] [MarshalAs(UnmanagedType.Bool)] bool bIsSyncPoint);

		int IsPreroll();

		int SetPreroll([In] [MarshalAs(UnmanagedType.Bool)] bool bIsPreroll);

		int GetActualDataLength();

		int SetActualDataLength([In] int len);

		int GetMediaType([MarshalAs(UnmanagedType.LPStruct)] out AMMediaType ppMediaType);

		int SetMediaType([In] [MarshalAs(UnmanagedType.LPStruct)] AMMediaType pMediaType);

		int IsDiscontinuity();

		int SetDiscontinuity([In] [MarshalAs(UnmanagedType.Bool)] bool bDiscontinuity);

		int GetMediaTime(out long pTimeStart, out long pTimeEnd);

		int SetMediaTime([In] long pTimeStart, [In] long pTimeEnd);
	}
}
