using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("56a868a9-0ad4-11ce-b03a-0020af0ba770")]
	[SuppressUnmanagedCodeSecurity]
	public interface IGraphBuilder : IFilterGraph
	{
		new int AddFilter([In] IBaseFilter pFilter, [In] [MarshalAs(UnmanagedType.LPWStr)] string pName);

		new int RemoveFilter([In] IBaseFilter pFilter);

		new int EnumFilters(out IEnumFilters ppEnum);

		new int FindFilterByName([In] [MarshalAs(UnmanagedType.LPWStr)] string pName, out IBaseFilter ppFilter);

		new int ConnectDirect([In] IPin ppinOut, [In] IPin ppinIn, [In] [MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

		new int Reconnect([In] IPin ppin);

		new int Disconnect([In] IPin ppin);

		new int SetDefaultSyncSource();

		int Connect([In] IPin ppinOut, [In] IPin ppinIn);

		int Render([In] IPin ppinOut);

		int RenderFile([In] [MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFile, [In] [MarshalAs(UnmanagedType.LPWStr)] string lpcwstrPlayList);

		int AddSourceFilter([In] [MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFileName, [In] [MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName, out IBaseFilter ppFilter);

		int SetLogFile(IntPtr hFile);

		int Abort();

		int ShouldOperationContinue();
	}
}
