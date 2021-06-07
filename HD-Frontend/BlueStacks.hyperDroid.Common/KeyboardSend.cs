using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common
{
	public static class KeyboardSend
	{
		private const int KEYEVENTF_EXTENDEDKEY = 1;

		private const int KEYEVENTF_KEYUP = 2;

		[DllImport("user32.dll")]
		private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

		public static void KeyDown(Keys vKey)
		{
			KeyboardSend.keybd_event((byte)vKey, 0, 1, 0);
		}

		public static void KeyUp(Keys vKey)
		{
			KeyboardSend.keybd_event((byte)vKey, 0, 3, 0);
		}
	}
}
