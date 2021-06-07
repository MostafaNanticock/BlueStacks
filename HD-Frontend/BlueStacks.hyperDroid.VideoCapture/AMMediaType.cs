using System;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.VideoCapture
{
	[StructLayout(LayoutKind.Sequential)]
	public class AMMediaType
	{
		public Guid majorType;

		public Guid subType;

		[MarshalAs(UnmanagedType.Bool)]
		public bool fixedSizeSamples;

		[MarshalAs(UnmanagedType.Bool)]
		public bool temporalCompression;

		public int sampleSize;

		public Guid formatType;

		public IntPtr pUnk;

		public int cbFormat;

		public IntPtr pbFormat;
	}
}
