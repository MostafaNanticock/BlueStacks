using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Core.Shim
{
	public class Logger
	{
		[DllImport("HD-Logger-Native.dll", CharSet = CharSet.Unicode)]
		private static extern void LoggerDllInit(string prog, string file, bool toConsole);

		[DllImport("HD-Logger-Native.dll")]
		private static extern void LoggerDllReinit();

		[DllImport("HD-Logger-Native.dll", CharSet = CharSet.Unicode)]
		private static extern void LoggerDllPrint(string line);

		public static void Init(bool toConsole)
		{
			Logger.LoggerDllInit(Process.GetCurrentProcess().MainModule.FileName, null, toConsole);
		}

		public static void Reinit()
		{
			Logger.LoggerDllReinit();
		}

		public static void Info(string fmt, params object[] args)
		{
			string text = string.Format(fmt, args);
			char[] separator = new char[1]
			{
				'\n'
			};
			char[] trimChars = new char[1]
			{
				'\r'
			};
			string[] array = text.Split(separator);
			foreach (string text2 in array)
			{
				Logger.LoggerDllPrint(text2.Trim(trimChars));
			}
		}
	}
}
