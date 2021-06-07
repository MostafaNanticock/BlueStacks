using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Common.Interop
{
	public class Window
	{
		public struct RECT
		{
			public int left;

			public int top;

			public int right;

			public int bottom;
		}

		public const int WM_CLOSE = 16;

		public const int WM_USER = 1024;

		public const int WM_USER_SHOW_WINDOW = 1025;

		public const int WM_USER_SWITCH_TO_LAUNCHER = 1026;

		public const int WM_USER_RESIZE_WINDOW = 1027;

		public const int WM_LBUTTONDOWN = 513;

		public const int WM_LBUTTONUP = 514;

		public const int WM_DISPLAYCHANGE = 126;

		public const int SM_CXSCREEN = 0;

		public const int SM_CYSCREEN = 1;

		public const int SWP_SHOWWINDOW = 64;

		public const int SWP_NOZORDER = 4;

		public const int WS_OVERLAPPED = 0;

		public const int WS_CAPTION = 12582912;

		public const int WS_SYSMENU = 524288;

		public const int WS_THICKFRAME = 262144;

		public const int WS_MINIMIZEBOX = 131072;

		public const int WS_MAXIMIZEBOX = 65536;

		public const int WS_OVERLAPPEDWINDOW = 13565952;

		private const int LOGPIXELSX = 88;

		public const int SW_HIDE = 0;

		public const int SW_SHOWMAXIMIZED = 3;

		public const int SW_SHOW = 5;

		public const int SW_MINIMIZE = 6;

		public const int SW_SHOWNA = 8;

		public const int SW_RESTORE = 9;

		public static IntPtr HWND_TOP = IntPtr.Zero;

		public static int ScreenWidth
		{
			get
			{
				return Window.GetSystemMetrics(0);
			}
		}

		public static int ScreenHeight
		{
			get
			{
				return Window.GetSystemMetrics(1);
			}
		}

		[DllImport("user32.dll")]
		private static extern int GetSystemMetrics(int which);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int w, int h, uint flags);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool AdjustWindowRect(out RECT lpRect, int dwStyle, bool bMenu);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateDC(string driver, string name, string output, IntPtr mode);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern int GetDeviceCaps(IntPtr hdc, int index);

		public static void SetFullScreen(IntPtr hwnd)
		{
			Window.SetFullScreen(hwnd, 0, 0, Window.ScreenWidth, Window.ScreenHeight);
		}

		public static void SetFullScreen(IntPtr hwnd, int X, int Y, int cx, int cy)
		{
			if (Window.SetWindowPos(hwnd, Window.HWND_TOP, X, Y, cx, cy, 64u))
			{
				return;
			}
			throw new SystemException("Cannot call SetWindowPos()", new Win32Exception(Marshal.GetLastWin32Error()));
		}

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindow(string cls, string name);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetForegroundWindow(IntPtr handle);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr handle, int cmd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetParent(IntPtr handle);

		[DllImport("user32.dll")]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("kernel32.dll")]
		public static extern bool FreeConsole();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint ProcessId);

		[DllImport("kernel32.dll")]
		private static extern uint GetCurrentThreadId();

		[DllImport("user32.dll")]
		private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		public static IntPtr MinimizeWindow(string name)
		{
			IntPtr intPtr = Window.FindWindow(null, name);
			if (intPtr == IntPtr.Zero)
			{
				throw new SystemException("Cannot find window '" + name + "'", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			Window.ShowWindow(intPtr, 6);
			return intPtr;
		}

		public static IntPtr BringWindowToFront(string name, bool fullScreen)
		{
			IntPtr intPtr = Window.FindWindow(null, name);
			if (intPtr == IntPtr.Zero)
			{
				throw new SystemException("Cannot find window '" + name + "'", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			if (!Window.SetForegroundWindow(intPtr))
			{
				throw new SystemException("Cannot set foreground window", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			if (fullScreen)
			{
				Window.ShowWindow(intPtr, 5);
			}
			int num = 1;
			string name2 = "Software\\BlueStacks\\Guests\\Android\\FrameBuffer\\0";
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name2);
			if (registryKey != null)
			{
				num = (int)registryKey.GetValue("WindowState", 1);
			}
			if (num == 2)
			{
				Window.ShowWindow(intPtr, 3);
			}
			else
			{
				Window.ShowWindow(intPtr, 5);
			}
			return intPtr;
		}

		public static IntPtr GetWindowHandle(string name)
		{
			IntPtr intPtr = Window.FindWindow(null, name);
			if (intPtr == IntPtr.Zero)
			{
				throw new SystemException("Cannot find window '" + name + "'", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			return intPtr;
		}

		public static bool ForceSetForegroundWindow(IntPtr h)
		{
			if (h == IntPtr.Zero)
			{
				return false;
			}
			IntPtr foregroundWindow = Window.GetForegroundWindow();
			if (foregroundWindow == IntPtr.Zero)
			{
				return Window.SetForegroundWindow(h);
			}
			if (h == foregroundWindow)
			{
				return true;
			}
			uint num = 0u;
			uint windowThreadProcessId = Window.GetWindowThreadProcessId(foregroundWindow, ref num);
			uint currentThreadId = Window.GetCurrentThreadId();
			if (currentThreadId == windowThreadProcessId)
			{
				return Window.SetForegroundWindow(h);
			}
			if (windowThreadProcessId != 0)
			{
				if (!Window.AttachThreadInput(currentThreadId, windowThreadProcessId, true))
				{
					return false;
				}
				if (!Window.SetForegroundWindow(h))
				{
					Window.AttachThreadInput(currentThreadId, windowThreadProcessId, false);
					return false;
				}
				Window.AttachThreadInput(currentThreadId, windowThreadProcessId, false);
			}
			return true;
		}

		public static int GetScreenDpi()
		{
			IntPtr intPtr = Window.CreateDC("DISPLAY", null, null, IntPtr.Zero);
			if (intPtr == IntPtr.Zero)
			{
				return -1;
			}
			int num = Window.GetDeviceCaps(intPtr, 88);
			if (num == 0)
			{
				num = 96;
			}
			Window.DeleteDC(intPtr);
			return num;
		}
	}
}
