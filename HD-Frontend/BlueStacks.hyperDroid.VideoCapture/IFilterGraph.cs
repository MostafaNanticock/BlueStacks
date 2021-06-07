using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("56a8689f-0ad4-11ce-b03a-0020af0ba770")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IFilterGraph
	{
		int AddFilter([In] IBaseFilter pFilter, [In] [MarshalAs(UnmanagedType.LPWStr)] string pName);

		int RemoveFilter([In] IBaseFilter pFilter);

		int EnumFilters(out IEnumFilters ppEnum);

		int FindFilterByName([In] [MarshalAs(UnmanagedType.LPWStr)] string pName, out IBaseFilter ppFilter);

		int ConnectDirect([In] IPin ppinOut, [In] IPin ppinIn, [In] [MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

		int Reconnect([In] IPin ppin);

		int Disconnect([In] IPin ppin);

		int SetDefaultSyncSource();
	}
}
