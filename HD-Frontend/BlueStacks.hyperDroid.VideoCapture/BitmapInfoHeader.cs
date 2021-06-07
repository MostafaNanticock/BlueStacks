using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class BitmapInfoHeader
	{
		public int Size;

		public int Width;

		public int Height;

		public short Planes;

		public short BitCount;

		public int Compression;

		public int ImageSize;

		public int XPelsPerMeter;

		public int YPelsPerMeter;

		public int ClrUsed;

		public int ClrImportant;
	}
}
