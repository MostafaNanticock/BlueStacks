using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend.Interop;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlueStacks.hyperDroid.VideoCapture
{
	public class Manager
	{
		private delegate void fpStartStopCamera(int startStop, int unit, int width, int height, int framerate);

		private const string NATIVE_DLL = "HD-Camera-Native.dll";

		private fpStartStopCamera s_fpStartStopCamera;

		private static BlueStacks.hyperDroid.Frontend.Interop.Monitor s_Monitor;

		private static IntPtr s_IoHandle = IntPtr.Zero;

		private static object s_IoHandleLock = new object();

		private IntPtr overWrite;

		private Camera camera;

		private Camera.getFrameCB cb;

		private bool bShutDown;

		private int unit;

		private int framerate = 30;

		private int width = 640;

		private int height = 480;

		private int jpegQuality = 100;

		private int keyEnableCam;

		private bool cameraStoped = true;

		private SupportedColorFormat m_color;

		private IntPtr m_buffer = IntPtr.Zero;

		public static BlueStacks.hyperDroid.Frontend.Interop.Monitor Monitor
		{
			get
			{
				return Manager.s_Monitor;
			}
			set
			{
				Manager.s_Monitor = value;
			}
		}

		[DllImport("kernel32.dll")]
		private static extern bool CloseHandle(IntPtr handle);

		[DllImport("HD-Camera-Native.dll")]
		private static extern void SetStartStopCamerCB(fpStartStopCamera func);

		[DllImport("HD-Camera-Native.dll")]
		private static extern int CameraIoProcessMessages(IntPtr ioHandle);

		[DllImport("HD-Camera-Native.dll")]
		private static extern bool MonitorSendCaptureStream(IntPtr handle, IntPtr stream, int size, IntPtr over, int width, int height, int stride);

		[DllImport("HD-Camera-Native.dll", SetLastError = true)]
		private static extern IntPtr CameraIoAttach(uint vmId);

		[DllImport("HD-Camera-Native.dll", SetLastError = true)]
		private static extern IntPtr MonitorCreateOverWrite();

		[DllImport("HD-Camera-Native.dll", SetLastError = true)]
		private static extern bool convertRGB24toYUV422(IntPtr src, int w, int h, IntPtr dst);

		private void BstStartStopCamera(int startStop, int unit, int width, int height, int framerate)
		{
			lock (Manager.s_IoHandleLock)
			{
				if (this.unit != unit && startStop == 1)
				{
					this.camStop();
				}
				if (this.unit == unit && startStop == 0)
				{
					this.camStop();
				}
				if (startStop == 1)
				{
					this.camStart(unit, width, height, framerate);
					this.unit = unit;
				}
			}
		}

		public void InitCamera(string[] args)
		{
			if (args.Length != 1)
			{
				throw new SystemException("InitCamera: Should have vmName as one arg");
			}
			string vmName = args[0];
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
			try
			{
				this.keyEnableCam = (int)registryKey.GetValue("Camera");
			}
			catch
			{
				this.keyEnableCam = 0;
			}
			if (this.keyEnableCam != 1)
			{
				Logger.Info("Camera is Disabled");
			}
			else
			{
				uint num = MonitorLocator.Lookup(vmName);
				lock (Manager.s_IoHandleLock)
				{
					if (Manager.s_IoHandle != IntPtr.Zero)
					{
						throw new SystemException("I/O handle is already open");
					}
					Logger.Info("Attaching to monitor ID {0}", num);
					Manager.s_IoHandle = Manager.CameraIoAttach(num);
					if (Manager.s_IoHandle == IntPtr.Zero)
					{
						throw new SystemException("Cannot attach for I/O", new Win32Exception(Marshal.GetLastWin32Error()));
					}
				}
				this.overWrite = Manager.MonitorCreateOverWrite();
				if (this.overWrite == IntPtr.Zero)
				{
					throw new SystemException("Cannot create overlapped structure", new Win32Exception(Marshal.GetLastWin32Error()));
				}
				this.s_fpStartStopCamera = this.BstStartStopCamera;
				Manager.SetStartStopCamerCB(this.s_fpStartStopCamera);
				Logger.Info("Waiting for Camera messages...");
				Thread thread = new Thread((ThreadStart)delegate
				{
					while (!this.bShutDown)
					{
						if (Manager.CameraIoProcessMessages(Manager.s_IoHandle) != 0)
						{
							Logger.Error("Camera: Cannot process VM messages");
							this.Shutdown();
						}
					}
				});
				thread.IsBackground = true;
				thread.Start();
			}
		}

		public void Shutdown()
		{
			lock (Manager.s_IoHandleLock)
			{
				if (this.keyEnableCam == 1)
				{
					this.bShutDown = true;
					if (this.camera != null || !this.cameraStoped)
					{
						this.camera.StopCamera();
					}
					this.camera = null;
					if (Manager.s_IoHandle != IntPtr.Zero)
					{
						Manager.CloseHandle(Manager.s_IoHandle);
						Manager.s_IoHandle = IntPtr.Zero;
					}
				}
			}
		}

		public void getFrame(IntPtr ip, int width, int height, int stride)
		{
			if (!(ip == IntPtr.Zero) && this.camera != null && !(Manager.s_IoHandle == IntPtr.Zero) && !(this.overWrite == IntPtr.Zero) && !this.cameraStoped)
			{
				IntPtr stream = ip;
				if (this.m_color == SupportedColorFormat.RGB24)
				{
					if (this.m_buffer == IntPtr.Zero)
					{
						this.m_buffer = Marshal.AllocCoTaskMem(width * height * 2);
					}
					Manager.convertRGB24toYUV422(ip, width, height, this.m_buffer);
					stream = this.m_buffer;
				}
				Manager.MonitorSendCaptureStream(Manager.s_IoHandle, stream, width * height * 2, this.overWrite, width, height, stride);
			}
		}

		public void camStart(int unit, int w, int h, int f)
		{
			if (this.camera == null && this.keyEnableCam == 1 && this.cameraStoped)
			{
				if (w > 0)
				{
					this.width = w;
				}
				if (h > 0)
				{
					this.height = h;
				}
				if (f > 0)
				{
					this.framerate = f;
				}
				Logger.Info("Starting Camera {0}. Frame width: {1}, height: {2}, framerate: {3}", unit, this.width, this.height, this.framerate);
				this.cameraStoped = false;
				this.cb = this.getFrame;
				for (int i = 0; i < 2; i++)
				{
					if (this.camera != null)
					{
						break;
					}
					try
					{
						this.m_color = (SupportedColorFormat)i;
						this.camera = new Camera(unit, this.width, this.height, this.framerate, this.jpegQuality, this.m_color);
					}
					catch (ColorFormatNotSupported colorFormatNotSupported)
					{
						Logger.Info("Trying with other color." + colorFormatNotSupported.ToString());
					}
					catch (Exception ex)
					{
						Logger.Error("Failed to initialize the camera", ex.ToString());
					}
				}
				if (this.camera == null)
				{
					Logger.Error("Cannot start the host camera.");
				}
				else
				{
					this.camera.registerFrameCB(this.cb);
					this.camera.StartCamera();
				}
			}
		}

		public void camStop()
		{
			if (this.camera != null && !this.cameraStoped)
			{
				Logger.Info("Stoping Camera.");
				this.cameraStoped = true;
				this.camera.StopCamera();
				this.camera = null;
				if (this.m_buffer != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(this.m_buffer);
					this.m_buffer = IntPtr.Zero;
				}
			}
		}

		public void resumeCamera()
		{
			lock (Manager.s_IoHandleLock)
			{
				if (this.keyEnableCam == 1 && this.cameraStoped && this.camera != null)
				{
					Logger.Info("Resuming Camera");
					this.camStart(this.unit, this.width, this.height, this.framerate);
				}
			}
		}

		public void pauseCamera()
		{
			lock (Manager.s_IoHandleLock)
			{
				if (this.keyEnableCam == 1 && this.camera != null && !this.cameraStoped)
				{
					Logger.Info("Pausing Camera");
					this.camStop();
				}
			}
		}
	}
}
