using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("0579154A-2B53-4994-B0D0-E773148EFF85")]
	[SuppressUnmanagedCodeSecurity]
	public interface ISampleGrabberCB
	{
		[PreserveSig]
		int SampleCB(double SampleTime, IMediaSample pSample);

		[PreserveSig]
		int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen);
	}
}
