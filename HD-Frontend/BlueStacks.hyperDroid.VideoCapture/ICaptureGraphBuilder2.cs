using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[SuppressUnmanagedCodeSecurity]
	[Guid("93E5A4E0-2D50-11d2-ABFA-00A0C9C6E38D")]
	public interface ICaptureGraphBuilder2
	{
		int SetFiltergraph([In] IGraphBuilder pfg);

		int GetFiltergraph(out IGraphBuilder ppfg);

		int SetOutputFileName([In] [MarshalAs(UnmanagedType.LPStruct)] Guid pType, [In] [MarshalAs(UnmanagedType.LPWStr)] string lpstrFile, out IBaseFilter ppbf, out IFileSinkFilter ppSink);

		int FindInterface([In] [MarshalAs(UnmanagedType.LPStruct)] Guid pCategory, [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pType, [In] IBaseFilter pf, [In] [MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppint);

		int RenderStream([In] [MarshalAs(UnmanagedType.LPStruct)] Guid pCategory, [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pType, [In] [MarshalAs(UnmanagedType.IUnknown)] object pSource, [In] IBaseFilter pCompressor, [In] IBaseFilter pRenderer);

		int ControlStream([In] [MarshalAs(UnmanagedType.LPStruct)] Guid pCategory, [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pType, [In] [MarshalAs(UnmanagedType.Interface)] IBaseFilter pFilter, [In] long pstart, [In] long pstop, [In] short wStartCookie, [In] short wStopCookie);

		int AllocCapFile([In] [MarshalAs(UnmanagedType.LPWStr)] string lpstr, [In] long dwlSize);

		int CopyCaptureFile([In] [MarshalAs(UnmanagedType.LPWStr)] string lpwstrOld, [In] [MarshalAs(UnmanagedType.LPWStr)] string lpwstrNew, [In] [MarshalAs(UnmanagedType.Bool)] bool fAllowEscAbort, [In] IAMCopyCaptureFileProgress pCallback);

		int FindPin([In] [MarshalAs(UnmanagedType.IUnknown)] object pSource, [In] PinDirection pindir, [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pCategory, [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pType, [In] [MarshalAs(UnmanagedType.Bool)] bool fUnconnected, [In] int num, [MarshalAs(UnmanagedType.Interface)] out IPin ppPin);
	}
}
