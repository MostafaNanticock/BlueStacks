using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend.Interop;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlueStacks.hyperDroid.Frontend
{
	public class Monitor
	{
		public delegate void SendMessage(IntPtr msg);

		public delegate void ReceiverCallback(IntPtr msg);

		private SafeFileHandle mHandle;

		private ReceiverCallback mReceiverCallback;

		private Thread mReceiverThread;

		private EventWaitHandle mReceiverWakeup;

		[DllImport("HD-Frontend-Native.dll", SetLastError = true)]
		private static extern SafeFileHandle ManagerOpen();

		[DllImport("HD-Frontend-Native.dll", SetLastError = true)]
		private static extern bool ManagerAttachWithListener(SafeFileHandle handle, uint id, uint cls);

		[DllImport("HD-Frontend-Native.dll", SetLastError = true)]
		private static extern bool MonitorSendMesg(SafeFileHandle handle, IntPtr msg);

		[DllImport("HD-Frontend-Native.dll", SetLastError = true)]
		private static extern bool MonitorRecvMesg(SafeFileHandle handle, ReceiverCallback callback, SafeWaitHandle wakeupEvent);

		private Monitor(SafeFileHandle handle)
		{
			this.mHandle = handle;
		}

		public static Monitor Connect(string vmName, uint cls)
		{
			uint num = MonitorLocator.Lookup(vmName);
			if (num == 0)
			{
				throw new ApplicationException("Cannot lookup VM");
			}
			SafeFileHandle safeFileHandle = Monitor.ManagerOpen();
			if (safeFileHandle.IsInvalid)
			{
				BlueStacks.hyperDroid.Frontend.Interop.Common.ThrowLastWin32Error("Cannot open manager");
			}
			if (!Monitor.ManagerAttachWithListener(safeFileHandle, num, cls))
			{
				BlueStacks.hyperDroid.Frontend.Interop.Common.ThrowLastWin32Error("Cannot attach to guest");
			}
			return new Monitor(safeFileHandle);
		}

		public void Close()
		{
			this.mHandle.Close();
		}

		public void Send(IntPtr msg)
		{
			if (!Monitor.MonitorSendMesg(this.mHandle, msg))
			{
				BlueStacks.hyperDroid.Frontend.Interop.Common.ThrowLastWin32Error("Cannot send message to guest");
			}
		}

		public void StartReceiver(ReceiverCallback callback)
		{
			this.mReceiverCallback = callback;
			this.mReceiverWakeup = new ManualResetEvent(false);
			this.mReceiverThread = new Thread((ThreadStart)delegate
			{
				try
				{
					if (!Monitor.MonitorRecvMesg(this.mHandle, this.mReceiverCallback, this.mReceiverWakeup.SafeWaitHandle))
					{
						BlueStacks.hyperDroid.Frontend.Interop.Common.ThrowLastWin32Error("Cannot receive monitor message");
					}
				}
				catch (Exception arg)
				{
					Logger.Error("Receiver thread died: " + arg);
				}
			});
			this.mReceiverThread.IsBackground = true;
			this.mReceiverThread.Start();
		}

		public void StopReceiver()
		{
			this.mReceiverWakeup.Set();
		}
	}
}
