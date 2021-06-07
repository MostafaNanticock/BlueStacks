using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct FilterInfo
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string achName;

		[MarshalAs(UnmanagedType.Interface)]
		public IFilterGraph pGraph;
	}
}
