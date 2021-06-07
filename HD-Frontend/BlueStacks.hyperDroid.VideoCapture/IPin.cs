using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("56a86891-0ad4-11ce-b03a-0020af0ba770")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPin
	{
		int Connect([In] IPin pReceivePin, [In] [MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

		int ReceiveConnection([In] IPin pReceivePin, [In] [MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

		int Disconnect();

		int ConnectedTo(out IPin ppPin);

		int ConnectionMediaType([Out] [MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

		int QueryPinInfo(out PinInfo pInfo);

		int QueryDirection(out PinDirection pPinDir);

		int QueryId([MarshalAs(UnmanagedType.LPWStr)] out string Id);

		int QueryAccept([In] [MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

		int EnumMediaTypes(out IEnumMediaTypes ppEnum);

		int QueryInternalConnections([Out] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IPin[] ppPins, [In] [Out] ref int nPin);

		int EndOfStream();

		int BeginFlush();

		int EndFlush();

		int NewSegment([In] long tStart, [In] long tStop, [In] double dRate);
	}
}
