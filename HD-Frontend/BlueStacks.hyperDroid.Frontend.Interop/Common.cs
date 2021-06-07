using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Frontend.Interop
{
	public class Common
	{
		public const string FRONTEND_DLL = "HD-Frontend-Native.dll";

		public static void ThrowLastWin32Error(string msg)
		{
			throw new SystemException(msg, new Win32Exception(Marshal.GetLastWin32Error()));
		}
	}
}
