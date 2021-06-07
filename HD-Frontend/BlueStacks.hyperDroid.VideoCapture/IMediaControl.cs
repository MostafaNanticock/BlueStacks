using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("56a868b1-0ad4-11ce-b03a-0020af0ba770")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IMediaControl
	{
		int Run();

		int Pause();

		int Stop();

		int GetState([In] int msTimeout, out FilterState pfs);

		int RenderFile([In] [MarshalAs(UnmanagedType.BStr)] string strFilename);

		[Obsolete("MSDN: Intended for Visual Basic 6.0; not documented here.", false)]
		int AddSourceFilter([In] [MarshalAs(UnmanagedType.BStr)] string strFilename, [MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);

		[Obsolete("MSDN: Intended for Visual Basic 6.0; not documented here.", false)]
		int get_FilterCollection([MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);

		[Obsolete("MSDN: Intended for Visual Basic 6.0; not documented here.", false)]
		int get_RegFilterCollection([MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);

		int StopWhenReady();
	}
}
