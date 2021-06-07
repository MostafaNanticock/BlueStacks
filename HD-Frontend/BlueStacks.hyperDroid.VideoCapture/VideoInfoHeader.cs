using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[StructLayout(LayoutKind.Sequential)]
	public class VideoInfoHeader
	{
		public RECT SrcRect;

		public RECT TargetRect;

		public int BitRate;

		public int BitErrorRate;

		public long AvgTimePerFrame;

		public BitmapInfoHeader BmiHeader;
	}
}
