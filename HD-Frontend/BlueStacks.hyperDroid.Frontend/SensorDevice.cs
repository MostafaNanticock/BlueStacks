using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
	public class SensorDevice
	{
		public enum Type
		{
			Accelerometer
		}

		private class State
		{
			public bool Enabled;

			public uint Period;

			public bool HasPhysical;

			public int ControllerCount;
		}

		private class AccelerometerState : State
		{
			public object Lock = new object();

			public float X;

			public float Y;

			public float Z;
		}

		private delegate void LoggerCallback(string msg);

		private delegate void AccelerometerCallback(float x, float y, float z);

		private delegate void EnableHandler(Type sensor, bool enable);

		private delegate void SetDelayHandler(Type sensor, uint msec);

		private const string NATIVE_DLL = "HD-Sensor-Native.dll";

		private Monitor mMonitor;

		private LoggerCallback mLogger;

		private bool mRunning;

		private Dictionary<Type, State> mStateMap;

		private Thread mAccelerometerThread;

		private bool mEmulatedPortraitMode;

		private bool mRotateGuest180;

		private AccelerometerCallback mAccelerometerCallback;

		private EnableHandler mEnableHandler;

		private SetDelayHandler mSetDelayHandler;

		private SerialWorkQueue mSerialQueue;

		[CompilerGenerated]
		private static LoggerCallback _003C_003E9__CachedAnonymousMethodDelegate2;

		[DllImport("HD-Sensor-Native.dll")]
		private static extern void LoggerSetCallback(LoggerCallback cb);

		[DllImport("HD-Sensor-Native.dll")]
		private static extern void MesgInit(EnableHandler enableHandler, SetDelayHandler setDelayHandler);

		[DllImport("HD-Sensor-Native.dll")]
		private static extern uint MesgGetDeviceClass();

		[DllImport("HD-Sensor-Native.dll")]
		private static extern void MesgHandleMessage(IntPtr msg);

		[DllImport("HD-Sensor-Native.dll")]
		private static extern void MesgSendReattach(Type sensor, Monitor.SendMessage handler);

		[DllImport("HD-Sensor-Native.dll")]
		private static extern void MesgSendAccelerometerEvent(float x, float y, float z, Monitor.SendMessage handler);

		[DllImport("HD-Sensor-Native.dll")]
		private static extern bool HostInit();

		[DllImport("HD-Sensor-Native.dll")]
		private static extern void HostSetOrientation(int orientation);

		[DllImport("HD-Sensor-Native.dll")]
		private static extern bool HostSetupAccelerometer(AccelerometerCallback callback);

		[DllImport("HD-Sensor-Native.dll")]
		private static extern void HostEnableSensor(Type sensor, bool enable);

		[DllImport("HD-Sensor-Native.dll")]
		private static extern void HostSetSensorPeriod(Type sensor, uint msec);

		public SensorDevice()
		{
			this.mStateMap = new Dictionary<Type, State>();
			this.mStateMap[Type.Accelerometer] = new AccelerometerState();
		}

		public void StartThreads()
		{
			this.mAccelerometerThread = new Thread(this.AccelerometerThreadEntry);
			this.mAccelerometerThread.IsBackground = true;
			this.mAccelerometerThread.Start();
		}

		public void Start(string vmName)
		{
			this.mRunning = true;
			this.mLogger = delegate(string msg)
			{
				Logger.Info("SensorDevice: " + msg);
			};
			SensorDevice.LoggerSetCallback(this.mLogger);
			this.UpdateOrientation(null, null);
			SystemEvents.DisplaySettingsChanged += this.UpdateOrientation;
			this.mSerialQueue = new SerialWorkQueue();
			this.mSerialQueue.Start();
			EventWaitHandle evt = new ManualResetEvent(false);
			this.mSerialQueue.Enqueue(delegate
			{
				this.SetupHostSensors();
				evt.Set();
			});
			evt.WaitOne();
			this.mEnableHandler = this.EnableHandlerImpl;
			this.mSetDelayHandler = this.SetDelayHandlerImpl;
			SensorDevice.MesgInit(this.mEnableHandler, this.mSetDelayHandler);
			this.mMonitor = Monitor.Connect(vmName, SensorDevice.MesgGetDeviceClass());
			this.mMonitor.StartReceiver(SensorDevice.MesgHandleMessage);
			SensorDevice.MesgSendReattach(Type.Accelerometer, this.SendMessage);
		}

		public void Stop()
		{
			this.mRunning = false;
			SystemEvents.DisplaySettingsChanged -= this.UpdateOrientation;
			this.mMonitor.StopReceiver();
			this.mMonitor.Close();
			this.mMonitor = null;
		}

		public void SetDisplay(bool emulatedPortraitMode, bool rotateGuest180)
		{
			this.mEmulatedPortraitMode = emulatedPortraitMode;
			this.mRotateGuest180 = rotateGuest180;
		}

		private State LookupState(Type sensor)
		{
			if (this.mStateMap == null)
			{
				return null;
			}
			if (!this.mStateMap.ContainsKey(sensor))
			{
				return null;
			}
			return this.mStateMap[sensor];
		}

		private void SetupHostSensors()
		{
			if (!SensorDevice.HostInit())
			{
				Logger.Warning("Cannot initialize host sensors");
			}
			else
			{
				Logger.Info("Setting up host accelerometer");
				this.mAccelerometerCallback = this.SetAccelerometerVector;
				if (SensorDevice.HostSetupAccelerometer(this.mAccelerometerCallback))
				{
					this.mStateMap[Type.Accelerometer].HasPhysical = true;
				}
				else
				{
					Logger.Warning("Cannot setup host accelerometer");
				}
			}
		}

		public void ControllerAttach(Type sensor)
		{
			this.ControllerAttachDetach(sensor, true);
		}

		public void ControllerDetach(Type sensor)
		{
			this.ControllerAttachDetach(sensor, false);
		}

		private void ControllerAttachDetach(Type sensor, bool attach)
		{
			State state = this.LookupState(sensor);
			if (state != null)
			{
				if (sensor != 0)
				{
					Logger.Warning("Don't know how to do controller override for sensor type " + sensor);
				}
				else
				{
					this.mSerialQueue.Enqueue(delegate
					{
						if (attach)
						{
							state.ControllerCount++;
						}
						else
						{
							state.ControllerCount--;
						}
						Logger.Info("Sensor device sees {0} controllers", state.ControllerCount);
						if (state.ControllerCount < 0)
						{
							Logger.Error("Bad sensor device controller count");
						}
						else if (attach && state.ControllerCount == 1)
						{
							Logger.Info("Switching from host accelerometer to controller accelerometer");
							if (state.HasPhysical)
							{
								SensorDevice.HostEnableSensor(sensor, false);
							}
						}
						else if (!attach && state.ControllerCount == 0)
						{
							Logger.Info("Switching from controller accelerometer to host accelerometer");
							if (state.HasPhysical)
							{
								SensorDevice.HostEnableSensor(sensor, true);
							}
						}
					});
				}
			}
		}

		public void SetAccelerometerVector(float origX, float origY, float origZ)
		{
			AccelerometerState accelerometerState = (AccelerometerState)this.LookupState(Type.Accelerometer);
			if (accelerometerState != null)
			{
				float num;
				float num2;
				if (!this.mEmulatedPortraitMode)
				{
					num = 0f - origX;
					num2 = 0f - origY;
				}
				else
				{
					num = 0f - origY;
					num2 = origX;
				}
				if (this.mRotateGuest180)
				{
					num = 0f - num;
					num2 = 0f - num2;
				}
				lock (accelerometerState.Lock)
				{
					accelerometerState.X = num;
					accelerometerState.Y = num2;
					accelerometerState.Z = origZ;
				}
			}
		}

		private void AccelerometerThreadEntry()
		{
			AccelerometerState accelerometerState = (AccelerometerState)this.LookupState(Type.Accelerometer);
			Logger.Info("Starting accelerometer sensor thread");
			while (true)
			{
				long ticks = DateTime.Now.Ticks;
				int num;
				if (this.mRunning && accelerometerState != null && accelerometerState.Enabled && accelerometerState.Period != 0)
				{
					float x = default(float);
					float y = default(float);
					float z = default(float);
					lock (accelerometerState.Lock)
					{
						x = accelerometerState.X;
						y = accelerometerState.Y;
						z = accelerometerState.Z;
					}
					this.SendAccelerometerVector(x, y, z);
					num = (int)accelerometerState.Period;
				}
				else
				{
					num = 200;
				}
				long ticks2 = DateTime.Now.Ticks;
				long num2 = num * 10000 - ticks2 + ticks;
				if (num2 > 0)
				{
					Thread.Sleep((int)(num2 / 10000));
				}
			}
		}

		private void SendAccelerometerVector(float x, float y, float z)
		{
			try
			{
				SensorDevice.MesgSendAccelerometerEvent(x, y, z, this.SendMessage);
			}
			catch (Exception arg)
			{
				Logger.Error("Cannot send accelerometer event: " + arg);
			}
		}

		private void EnableHandlerImpl(Type sensor, bool enable)
		{
			Logger.Info("SensorDevice.EnableHandlerImpl({0}, {1})", sensor, enable);
			State state = this.LookupState(sensor);
			if (state == null)
			{
				Logger.Error("Enable/disable for invalid sensor " + sensor);
			}
			else
			{
				state.Enabled = enable;
				this.mSerialQueue.Enqueue(delegate
				{
					if (state.HasPhysical && state.ControllerCount == 0)
					{
						SensorDevice.HostEnableSensor(sensor, enable);
					}
				});
			}
		}

		private void SetDelayHandlerImpl(Type sensor, uint msec)
		{
			Logger.Info("SensorDevice.SetDelayHandlerImpl({0}, {1})", sensor, msec);
			State state = this.LookupState(sensor);
			if (state == null)
			{
				Logger.Error("Set delay for invalid sensor " + sensor);
			}
			else
			{
				state.Period = msec;
				this.mSerialQueue.Enqueue(delegate
				{
					SensorDevice.HostSetSensorPeriod(sensor, msec);
				});
			}
		}

		private void SendMessage(IntPtr msg)
		{
			this.mMonitor.Send(msg);
		}

		private void UpdateOrientation(object obj, EventArgs evt)
		{
			switch (SystemInformation.ScreenOrientation)
			{
			case ScreenOrientation.Angle0:
				SensorDevice.HostSetOrientation(0);
				break;
			case ScreenOrientation.Angle90:
				SensorDevice.HostSetOrientation(1);
				break;
			case ScreenOrientation.Angle180:
				SensorDevice.HostSetOrientation(2);
				break;
			case ScreenOrientation.Angle270:
				SensorDevice.HostSetOrientation(3);
				break;
			}
		}
	}
}
