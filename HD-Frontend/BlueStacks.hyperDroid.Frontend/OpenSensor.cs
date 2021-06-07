using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlueStacks.hyperDroid.Frontend
{
	public class OpenSensor
	{
		private enum GestureEvent
		{
			Begin,
			Update,
			End
		}

		private delegate void ConnectHandler(IntPtr context, int identity, int connected, int clientCount);

		private delegate void TrackPadMoveHandler(IntPtr context, int identity, float x, float y, int absolute);

		private delegate void TrackPadClickHandler(IntPtr context, int identity, int down);

		private delegate void TrackPadScrollHandler(IntPtr context, int identity, float dx, float dy, GestureEvent evt);

		private delegate void TrackPadZoomHandler(IntPtr context, int identity, float dz, GestureEvent evt);

		private delegate void TouchHandler(IntPtr context, int identity, IntPtr list, int count);

		private delegate void KeyboardHandler(IntPtr context, int identity, char ch);

		private delegate void ControllerHandler(IntPtr context, int identity, int button, int down);

		private delegate void AccelerometerHandler(IntPtr context, int identity, float x, float y, float z);

		private delegate void SpecialHandler(IntPtr context, int identity, string cmd);

		private delegate void LoggerCallback(string msg);

		public const int IDENTITY_OFFSET = 16;

		private const string CFG_KEY = "Software\\BlueStacks\\Guests\\Android\\Config";

		private const string OPENSENSOR_DEVICEID_KEY = "OpenSensorDeviceId";

		private const string OPEN_SENSOR_DLL = "HD-OpenSensor-Native.dll";

		private InputMapper mInputMapper;

		private int mBeaconPort;

		private int mBeaconInterval;

		private string mDeviceType;

		private SensorDevice mSensorDevice;

		private Cursor mCursor;

		private bool mVerbose;

		private Console mConsole;

		private IControlHandler mControlHandler;

		private ConnectHandler mConnectHandler;

		private TrackPadMoveHandler mTrackPadMoveHandler;

		private TrackPadClickHandler mTrackPadClickHandler;

		private TrackPadScrollHandler mTrackPadScrollHandler;

		private TrackPadZoomHandler mTrackPadZoomHandler;

		private TouchHandler mTouchHandler;

		private KeyboardHandler mKeyboardHandler;

		private ControllerHandler mControllerHandler;

		private AccelerometerHandler mAccelerometerHandler;

		private SpecialHandler mSpecialHandler;

		private LoggerCallback mLoggerCallback;

		[CompilerGenerated]
		private static LoggerCallback _003C_003E9__CachedAnonymousMethodDelegate1;

		[CompilerGenerated]
		private static InputMapper.ModeHandlerNet _003C_003E9__CachedAnonymousMethodDelegate3;

		[DllImport("HD-OpenSensor-Native.dll")]
		private static extern int OpenSensorInit(LoggerCallback logger, int beaconPort, int beaconInterval, ConnectHandler connectHandler, IntPtr connectContext, TrackPadMoveHandler moveHandler, IntPtr moveContext, TrackPadClickHandler clickHandler, IntPtr clickContext, TrackPadScrollHandler scrollHandler, IntPtr scrollContext, TrackPadZoomHandler zoomHandler, IntPtr zoomContext, TouchHandler touchHandler, IntPtr touchContext, KeyboardHandler keyboardHandler, IntPtr keyboardContext, ControllerHandler ctrlHandler, IntPtr ctrlContext, AccelerometerHandler accelHandler, IntPtr accelContext, SpecialHandler specialHandler, IntPtr specialContext, string deviceType, string deviceId, int verbose);

		[DllImport("HD-OpenSensor-Native.dll")]
		private static extern void OpenSensorRunServerInet();

		[DllImport("HD-OpenSensor-Native.dll")]
		private static extern void OpenSensorRunServerBluetooth();

		[DllImport("HD-OpenSensor-Native.dll")]
		private static extern void OpenSensorRunBeacon();

		[DllImport("HD-OpenSensor-Native.dll")]
		private static extern void OpenSensorAdvertiseBeacon(string service, int port);

		[DllImport("HD-OpenSensor-Native.dll")]
		private static extern void OpenSensorSetMode(string mode);

		public OpenSensor(InputMapper inputMapper, int beaconPort, int beaconInterval, string deviceType, SensorDevice sensorDevice, Cursor cursor, bool verbose)
		{
			this.mInputMapper = inputMapper;
			this.mBeaconPort = beaconPort;
			this.mBeaconInterval = beaconInterval;
			this.mDeviceType = deviceType;
			this.mSensorDevice = sensorDevice;
			this.mCursor = cursor;
			this.mVerbose = verbose;
			this.mLoggerCallback = delegate(string msg)
			{
				Logger.Info("OpenSensor: " + msg);
			};
		}

		public void SetConsole(Console console)
		{
			this.mConsole = console;
		}

		public void SetControlHandler(IControlHandler handler)
		{
			this.mControlHandler = handler;
		}

		public void Start()
		{
			this.mConnectHandler = this.ConnectHandlerImpl;
			this.mTrackPadMoveHandler = this.TrackPadMoveHandlerImpl;
			this.mTrackPadClickHandler = this.TrackPadClickHandlerImpl;
			this.mTrackPadScrollHandler = this.TrackPadScrollHandlerImpl;
			this.mTrackPadZoomHandler = this.TrackPadZoomHandlerImpl;
			this.mTouchHandler = this.TouchHandlerImpl;
			this.mKeyboardHandler = this.KeyboardHandlerImpl;
			this.mControllerHandler = this.ControllerHandlerImpl;
			this.mAccelerometerHandler = this.AccelerometerHandlerImpl;
			this.mSpecialHandler = this.SpecialHandlerImpl;
			if (OpenSensor.OpenSensorInit(this.mLoggerCallback, this.mBeaconPort, this.mBeaconInterval, this.mConnectHandler, IntPtr.Zero, this.mTrackPadMoveHandler, IntPtr.Zero, this.mTrackPadClickHandler, IntPtr.Zero, this.mTrackPadScrollHandler, IntPtr.Zero, this.mTrackPadZoomHandler, IntPtr.Zero, this.mTouchHandler, IntPtr.Zero, this.mKeyboardHandler, IntPtr.Zero, this.mControllerHandler, IntPtr.Zero, this.mAccelerometerHandler, IntPtr.Zero, this.mSpecialHandler, IntPtr.Zero, this.mDeviceType, this.GetDeviceIdentifier(), this.mVerbose ? 1 : 0) != -1)
			{
				Thread thread = new Thread(OpenSensor.OpenSensorRunServerInet);
				thread.IsBackground = true;
				thread.Start();
				Thread thread2 = new Thread(OpenSensor.OpenSensorRunServerBluetooth);
				thread2.IsBackground = true;
				thread2.Start();
				Thread thread3 = new Thread(OpenSensor.OpenSensorRunBeacon);
				thread3.IsBackground = true;
				thread3.Start();
				this.mInputMapper.SetModeHandler(delegate(string mode)
				{
					OpenSensor.OpenSensorSetMode(mode);
				});
			}
		}

		private void ConnectHandlerImpl(IntPtr context, int identity, int connected, int clientCount)
		{
			Logger.Info("OpenSensor client {0} {1}", identity, (connected != 0) ? "connected" : "disconnected");
			Logger.Info("OpenSensor now has {0} clients", clientCount);
			if (connected != 0)
			{
				this.mCursor.Attach(identity + 16);
			}
			else
			{
				this.mCursor.Detach(identity + 16);
			}
			this.mConsole.HandleControllerAttach(connected != 0, identity + 16, "OpenSensor");
		}

		private void TrackPadMoveHandlerImpl(IntPtr context, int identity, float x, float y, int absolute)
		{
			this.mCursor.Move(identity + 16, x, y, absolute != 0);
		}

		private void TrackPadClickHandlerImpl(IntPtr context, int identity, int down)
		{
			this.mCursor.Click(identity + 16, down != 0);
		}

		private void TrackPadScrollHandlerImpl(IntPtr context, int identity, float dx, float dy, GestureEvent evt)
		{
			switch (evt)
			{
			case GestureEvent.Begin:
			{
				float x = 0f;
				float y = 0f;
				this.mCursor.GetNormalizedPosition(identity + 16, out x, out y);
				this.mInputMapper.ScrollBegin(x, y);
				break;
			}
			case GestureEvent.Update:
			{
				Size windowSize = this.GetWindowSize();
				this.mInputMapper.ScrollUpdate(dx / (float)windowSize.Width, dy / (float)windowSize.Height);
				break;
			}
			case GestureEvent.End:
				this.mInputMapper.ScrollEnd();
				break;
			}
		}

		private void TrackPadZoomHandlerImpl(IntPtr context, int identity, float dz, GestureEvent evt)
		{
			switch (evt)
			{
			case GestureEvent.Begin:
			{
				float x = 0f;
				float y = 0f;
				this.mCursor.GetNormalizedPosition(identity + 16, out x, out y);
				this.mInputMapper.ZoomBegin(x, y);
				break;
			}
			case GestureEvent.Update:
				this.mInputMapper.ZoomUpdate(dz);
				break;
			case GestureEvent.End:
				this.mInputMapper.ZoomEnd();
				break;
			}
		}

		private void TouchHandlerImpl(IntPtr context, int identity, IntPtr list, int count)
		{
			this.mInputMapper.TouchHandlerImpl(list, count, identity * count);
		}

		private void KeyboardHandlerImpl(IntPtr context, int identity, char ch)
		{
			this.mInputMapper.DispatchCharacter(ch);
		}

		private void ControllerHandlerImpl(IntPtr context, int identity, int button, int down)
		{
			this.mInputMapper.DispatchControllerEvent(identity, (uint)button, down);
		}

		private void AccelerometerHandlerImpl(IntPtr context, int identity, float x, float y, float z)
		{
			this.mSensorDevice.SetAccelerometerVector(x, y, z);
		}

		private void SpecialHandlerImpl(IntPtr context, int identity, string cmd)
		{
			Logger.Info("OpenSensor.SpecialHandlerImpl -> " + cmd);
			if (cmd == "Back")
			{
				this.mControlHandler.Back();
			}
			else if (cmd == "Menu")
			{
				this.mControlHandler.Menu();
			}
			else if (cmd == "Home")
			{
				this.mControlHandler.Home();
			}
		}

		private Size GetWindowSize()
		{
			Size size = default(Size);
			UIHelper.RunOnUIThread(this.mConsole, delegate
			{
				size = this.mConsole.Size;
			});
			return size;
		}

		private string GetDeviceIdentifier()
		{
			string storedDeviceIdentifier = this.GetStoredDeviceIdentifier();
			if (storedDeviceIdentifier != null)
			{
				Logger.Info("Using stored OpenSensor device identifier: " + storedDeviceIdentifier);
				return storedDeviceIdentifier;
			}
			try
			{
				ManagementClass managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
				ManagementObjectCollection instances = managementClass.GetInstances();
				foreach (ManagementObject item in instances)
				{
					string text = (string)((ManagementBaseObject)item)["Description"];
					string text2 = (string)((ManagementBaseObject)item)["MACAddress"];
					if (text.Contains("Bluetooth"))
					{
						Logger.Info("Bluetooth device: " + text);
						if (text2 != null)
						{
							Logger.Info("Bluetooth address: " + text2);
							storedDeviceIdentifier = text2.ToUpper().Replace(":", "");
							Logger.Info("Using OpenSensor device identifier: " + storedDeviceIdentifier);
							this.SetStoredDeviceIdentifier(storedDeviceIdentifier);
							return storedDeviceIdentifier;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
			Logger.Info("Using default OpenSensor device identifier: " + User.GUID);
			this.SetStoredDeviceIdentifier(User.GUID);
			return User.GUID;
		}

		private string GetStoredDeviceIdentifier()
		{
			string result = null;
			RegistryKey registryKey;
			using (registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config"))
			{
				object value = registryKey.GetValue("OpenSensorDeviceId");
				if (value != null)
				{
					return (string)value;
				}
				return result;
			}
		}

		private void SetStoredDeviceIdentifier(string deviceId)
		{
			RegistryKey registryKey;
			using (registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config", true))
			{
				registryKey.SetValue("OpenSensorDeviceId", deviceId);
			}
		}
	}
}
