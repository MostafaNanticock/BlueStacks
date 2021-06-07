using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Frontend.Interop
{
	internal class Input
	{
		private struct HookData
		{
			public uint vkCode;

			public uint scanCode;

			public uint flags;

			public uint time;

			public IntPtr dwExtraInfo;
		}

		public delegate bool KeyboardCallback(bool pressed, uint key);

		private delegate int HookProc(int code, uint wparam, IntPtr lparam);

		private const uint MOUSEEVENTF_FROMTOUCH = 4283520896u;

		private const uint MOUSEEVENTF_FROMPEN = 4283520768u;

		private const uint MOUSEEVENTF_MASK = 4294967168u;

		private const int WM_KEYDOWN = 256;

		private const int WM_KEYUP = 256;

		private const int WM_SYSKEYDOWN = 260;

		private const int WM_SYSKEYUP = 261;

		private const int WH_KEYBOARD_LL = 13;

		private const int HC_ACTION = 0;

		public const int VK_LWIN = 91;

		private static int sHookHandle;

		private static HookProc sHookProc;

		[DllImport("user32.dll")]
		private static extern uint GetMessageExtraInfo();

		public static bool IsEventFromTouch()
		{
			return ((int)Input.GetMessageExtraInfo() & -128) == -11446400;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern ushort GlobalAddAtom(string str);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool SetProp(IntPtr wind, string str, IntPtr data);

		public static void DisablePressAndHold(IntPtr hWnd)
		{
			string str = "MicrosoftTabletPenServiceProperty";
			if (Input.GlobalAddAtom(str) == 0)
			{
				throw new SystemException("Cannot add global atom", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			if (Input.SetProp(hWnd, str, (IntPtr)1))
			{
				return;
			}
			throw new SystemException("Cannot set property", new Win32Exception(Marshal.GetLastWin32Error()));
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int SetWindowsHookEx(int type, HookProc callback, IntPtr module, uint threadId);

		[DllImport("user32.dll")]
		private static extern int CallNextHookEx(int handle, int code, uint wparam, IntPtr lparam);

		[DllImport("user32.dll")]
		private static extern bool UnhookWindowsHookEx(int handle);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetModuleHandle(IntPtr name);

		public static void HookKeyboard(KeyboardCallback cb)
		{
			Input.sHookProc = delegate(int code, uint wparam, IntPtr lparam)
			{
				if (code < 0)
				{
					return Input.CallNextHookEx(Input.sHookHandle, code, wparam, lparam);
				}
				if (wparam != 260 && wparam != 261)
				{
					HookData hookData = (HookData)Marshal.PtrToStructure(lparam, typeof(HookData));
					bool pressed = wparam == 256;
					if (cb(pressed, hookData.vkCode))
					{
						return Input.CallNextHookEx(Input.sHookHandle, code, wparam, lparam);
					}
					return 1;
				}
				return Input.CallNextHookEx(Input.sHookHandle, code, wparam, lparam);
			};
			if (Input.sHookHandle != 0)
			{
				throw new SystemException("Keyboard hook is already set");
			}
			IntPtr moduleHandle = Input.GetModuleHandle(IntPtr.Zero);
			Input.sHookHandle = Input.SetWindowsHookEx(13, Input.sHookProc, moduleHandle, 0u);
			if (Input.sHookHandle != 0)
			{
				return;
			}
			throw new SystemException("Cannot set hooks", new Win32Exception(Marshal.GetLastWin32Error()));
		}

		public static void UnhookKeyboard()
		{
			if (Input.sHookHandle != 0)
			{
				Input.UnhookWindowsHookEx(Input.sHookHandle);
				Input.sHookHandle = 0;
			}
		}
	}
}
