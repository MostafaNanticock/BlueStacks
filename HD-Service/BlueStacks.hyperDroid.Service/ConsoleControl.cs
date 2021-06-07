using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Service
{
	public class ConsoleControl
	{
		public delegate bool Handler(CtrlType ctrlType);

		public enum CtrlType
		{
			CTRL_C_EVENT,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}

		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(Handler handler, bool Add);

		public static void SetHandler(Handler handler)
		{
			ConsoleControl.SetConsoleCtrlHandler(handler, true);
		}
	}
}
