using BlueStacks.hyperDroid.Common;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlueStacks.hyperDroid.Gps
{
	public class Manager
	{
		private const string NATIVE_DLL = "HD-Gps-Native.dll";

		private static IntPtr s_IoHandle = IntPtr.Zero;

		private static object s_IoHandleLock = new object();

		[CompilerGenerated]
		private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate1;

		[DllImport("kernel32.dll")]
		private static extern bool CloseHandle(IntPtr handle);

		[DllImport("HD-Gps-Native.dll", SetLastError = true)]
		private static extern IntPtr GpsIoAttach(uint vmId);

		[DllImport("HD-Gps-Native.dll", SetLastError = true)]
		private static extern int GpsIoProcessMessages(IntPtr ioHandle);

		public static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Logger.Error("Gps: Invalid invocation. Argument missing.");
			}
			else
			{
				string vmName = args[0];
				uint num = MonitorLocator.Lookup(vmName);
				lock (Manager.s_IoHandleLock)
				{
					if (Manager.s_IoHandle != IntPtr.Zero)
					{
						throw new SystemException("I/O handle is already open");
					}
					Logger.Debug("Attaching to monitor ID {0}", num);
					Manager.s_IoHandle = Manager.GpsIoAttach(num);
					if (Manager.s_IoHandle == IntPtr.Zero)
					{
						throw new SystemException("Cannot attach for I/O", new Win32Exception(Marshal.GetLastWin32Error()));
					}
				}
				Logger.Debug("Waiting for Gps messages...");
				Thread thread = new Thread((ThreadStart)delegate
				{
					while (true)
					{
						try
						{
							int num2 = Manager.GpsIoProcessMessages(Manager.s_IoHandle);
							if (num2 != 0)
							{
								throw new SystemException("Cannot process VM messages", new Win32Exception(num2));
							}
						}
						catch (Exception ex)
						{
							Logger.Error(ex.ToString());
							Logger.Error("GPS: Exiting thread.");
							return;
						}
						Thread.Sleep(1000);
					}
				});
				thread.IsBackground = true;
				thread.Start();
			}
		}

		public static void Shutdown()
		{
			lock (Manager.s_IoHandleLock)
			{
				if (Manager.s_IoHandle != IntPtr.Zero)
				{
					Logger.Debug("Shutting down gps...\n");
					Manager.CloseHandle(Manager.s_IoHandle);
					Manager.s_IoHandle = IntPtr.Zero;
				}
			}
		}
	}
}
