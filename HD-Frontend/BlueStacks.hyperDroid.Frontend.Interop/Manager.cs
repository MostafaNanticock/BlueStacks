using System;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Frontend.Interop
{
	public class Manager
	{
		private IntPtr handle = IntPtr.Zero;

		[DllImport("HD-Frontend-Native.dll", SetLastError = true)]
		private static extern IntPtr ManagerOpen();

		[DllImport("HD-Frontend-Native.dll", SetLastError = true)]
		private static extern int ManagerList(IntPtr handle, uint[] list, int count);

		[DllImport("HD-Frontend-Native.dll", SetLastError = true)]
		private static extern bool ManagerAttach(IntPtr handle, uint id);

		[DllImport("HD-Frontend-Native.dll", SetLastError = true)]
		private static extern bool ManagerIsVmxActive();

		[DllImport("kernel32.dll")]
		private static extern bool CloseHandle(IntPtr handle);

		private Manager(IntPtr handle)
		{
			this.handle = handle;
		}

		public static Manager Open()
		{
			IntPtr value = Manager.ManagerOpen();
			if (value == IntPtr.Zero)
			{
				Common.ThrowLastWin32Error("Cannot open hyperDroid manager");
			}
			return new Manager(value);
		}

		public void Close()
		{
			Manager.CloseHandle(this.handle);
		}

		public uint[] List()
		{
			int num;
			uint[] array;
			int num2;
			do
			{
				num = Manager.ManagerList(this.handle, null, 0);
				if (num == -1)
				{
					Common.ThrowLastWin32Error("Cannot get monitor count");
				}
				array = new uint[num];
				num2 = Manager.ManagerList(this.handle, array, num);
				if (num2 == -1)
				{
					Common.ThrowLastWin32Error("Cannot get monitor list");
				}
			}
			while (num2 != num);
			return array;
		}

		public Monitor Attach(uint id, Monitor.ExitHandler exitHandler)
		{
			if (!Manager.ManagerAttach(this.handle, id))
			{
				Common.ThrowLastWin32Error("Cannot attach to monitor " + id);
			}
			return new Monitor(this.handle, id, exitHandler);
		}

		public static bool IsVmxActive()
		{
			return Manager.ManagerIsVmxActive();
		}
	}
}
