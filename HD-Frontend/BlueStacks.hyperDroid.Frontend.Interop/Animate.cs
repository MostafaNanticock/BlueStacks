using System;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Frontend.Interop
{
	public class Animate
	{
		public const int AW_HOR_POSITIVE = 1;

		public const int AW_VER_POSITIVE = 4;

		public const int AW_VER_NEGATIVE = 8;

		public const int AW_CENTER = 16;

		public const int AW_HIDE = 65536;

		public const int AW_ACTIVATE = 131072;

		public const int AW_SLIDE = 262144;

		public const int AW_BLEND = 524288;

		[DllImport("User32.dll")]
		public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);
	}
}
