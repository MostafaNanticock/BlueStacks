using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Frontend.Interop
{
	internal class MouseHWheel
	{
		public delegate void MouseHWheelCallback(int x, int y, int keyState, int delta);

		private static MouseHWheelCallback s_MouseHWheelCallback;

		private int keyEnableSynaptic;

		[DllImport("HD-Frontend-Native.dll")]
		private static extern bool SetMouseHWheelCallback(MouseHWheelCallback func);

		public MouseHWheel()
		{
		}

		public MouseHWheel(MouseHWheelCallback cb)
		{
			this.setMousehWheelCallback(cb);
		}

		public bool setMousehWheelCallback(MouseHWheelCallback cb)
		{
			if (cb == null)
			{
				return false;
			}
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
			try
			{
				this.keyEnableSynaptic = (int)registryKey.GetValue("HScroll");
			}
			catch
			{
				this.keyEnableSynaptic = 0;
			}
			if (this.keyEnableSynaptic != 1)
			{
				Logger.Info("Horizontal Mouse Wheel support is Disabled");
				return false;
			}
			MouseHWheel.s_MouseHWheelCallback = cb.Invoke;
			try
			{
				if (!MouseHWheel.SetMouseHWheelCallback(MouseHWheel.s_MouseHWheelCallback))
				{
					Logger.Info("Horizontal scrolling disabled, no synaptic device found");
				}
				return true;
			}
			catch (Exception ex)
			{
				Logger.Error("Continue with MouseHWheel error:");
				Logger.Error(ex.ToString());
			}
			return false;
		}
	}
}
