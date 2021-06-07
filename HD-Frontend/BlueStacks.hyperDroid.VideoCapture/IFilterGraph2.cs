using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[Guid("36b73882-c2c8-11cf-8b46-00805f6cef60")]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IFilterGraph2 : IGraphBuilder, IFilterGraph
	{
		new int AddFilter([In] IBaseFilter pFilter, [In] [MarshalAs(UnmanagedType.LPWStr)] string pName);

		new int RemoveFilter([In] IBaseFilter pFilter);

		new int EnumFilters(out IEnumFilters ppEnum);

		new int FindFilterByName([In] [MarshalAs(UnmanagedType.LPWStr)] string pName, out IBaseFilter ppFilter);

		new int ConnectDirect([In] IPin ppinOut, [In] IPin ppinIn, [In] [MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

		new int Reconnect([In] IPin ppin);

		new int Disconnect([In] IPin ppin);

		new int SetDefaultSyncSource();

		new int Connect([In] IPin ppinOut, [In] IPin ppinIn);

		new int Render([In] IPin ppinOut);

		new int RenderFile([In] [MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFile, [In] [MarshalAs(UnmanagedType.LPWStr)] string lpcwstrPlayList);

		new int AddSourceFilter([In] [MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFileName, [In] [MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName, out IBaseFilter ppFilter);

		new int SetLogFile(IntPtr hFile);

		new int Abort();

		new int ShouldOperationContinue();

		int AddSourceFilterForMoniker([In] IMoniker pMoniker, [In] IBindCtx pCtx, [In] [MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName, out IBaseFilter ppFilter);

		int ReconnectEx([In] IPin ppin, [In] AMMediaType pmt);

		int RenderEx([In] IPin pPinOut, [In] int dwFlags, [In] IntPtr pvContext);
	}
}
