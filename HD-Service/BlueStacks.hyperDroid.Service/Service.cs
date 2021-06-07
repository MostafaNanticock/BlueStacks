using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Core.Shim;
using BlueStacks.hyperDroid.Core.VMCommand;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace BlueStacks.hyperDroid.Service
{
	internal class Service : ServiceBase
	{
		private delegate void LoggerCallback(string msg);

		private const uint UHD_CORE_WM_INFO = 1024u;

		private const uint UHD_CORE_WM_REINIT = 1025u;

		private const uint UHD_CORE_WM_QUIT = 1026u;

		private const string INFO_EVENT = "Global\\BlueStacks_Core_Service_Info_Event";

		private const string LOGROTATE_EVENT = "Global\\BlueStacks_Core_Service_LogRotate_Event";

		private const string MON_PATH = "Software\\BlueStacks\\Monitors";

		private const string SERVICE_STOPPED_GRACEFULLY = "ServiceStoppedGracefully";

		private const string FORCE_VM_LEGACY_MODE = "ForceVMLegacyMode";

		private const string VIRT_TYPE_LEGACY = "legacy";

		private const string VIRT_TYPE_VTX = "vtx";

		private const string VIRT_TYPE_SVM = "svm";

		private const string SERVICE_NATIVE_DLL = "HD-Service-Native.dll";

		private const string FRONTEND_NATIVE_DLL = "HD-Frontend-Native.dll";

		private const string PUBLIC_KEY_XML = "<RSAKeyValue><Modulus>rHDWCLJnSXrhdVQjebrkpiPBDWVisgvbq0jVhzWdxAArCxNiDDFC3Ich1QGG+OXbEuPHIN2GKN3jzBIQ2OlKnmtmZaUmbWPE+lAUuPljvnOLCONYW/2Yi592jw8gnW1VGCDze6yuvOqZCE+vuiO8P2bKRfk0mlC5OBbn0bD6QpE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

		private const string ROOTFS_PATH = "Software\\BlueStacks\\Guests\\Android\\BlockDevice\\0\\Path";

		private bool consoleMode;

		private EventWaitHandle consoleModeEvent;

		private string vmName;

		private Thread vmThread;

		private Thread logThread;

		private IntPtr vmHandle = IntPtr.Zero;

		private volatile bool vmStopping;

		private EventWaitHandle vmStoppedEvent;

		private IntPtr ioHandle = IntPtr.Zero;

		private string runtimeDir;

		private Process netProc;

		private Process blockProc;

		private Process folderProc;

		private Thread eventThread;

		private Thread guidMgrThread;

		private bool isHVM;

		private bool integrityCheckDisabled;

		[CompilerGenerated]
		private static LoggerCallback _003C_003E9__CachedAnonymousMethodDelegate2;

		[CompilerGenerated]
		private static UnhandledExceptionEventHandler _003C_003E9__CachedAnonymousMethodDelegate3;

		[DllImport("Shell32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern int SHChangeNotify(int wEventID, int uFlags, int dwItem1, int dwItem2);

		[DllImport("HD-Service-Native.dll", SetLastError = true)]
		private static extern IntPtr VmCreate(string name, string kernel, ref uint vmId);

		[DllImport("HD-Service-Native.dll", SetLastError = true)]
		private static extern IntPtr VmCreateHvm(string name, int nrVCpus, string kernel, ref uint vmId);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmLoadInitrd(IntPtr vm, string initrd);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmStart(IntPtr vm);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmStartHvm(IntPtr vm);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmRun(IntPtr vm);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmRunHvm(IntPtr vm);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmCanReceivePowerMessages(IntPtr vm, ref bool canReceivePowerMessages);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmCanReceivePowerMessagesHvm(uint monitorId, ref bool canReceivePowerMessages);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmStop(IntPtr vm);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmStopHvm(uint monitorId);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmAbortHvm(uint monitorId);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmHvmCheck(ref bool isHvm);

		[DllImport("HD-Service-Native.dll")]
		private static extern int VmHvmGetVirtType(ref int virtType);

		[DllImport("HD-Service-Native.dll", SetLastError = true)]
		private static extern IntPtr IoAttach(uint monitorId);

		[DllImport("HD-Service-Native.dll")]
		private static extern int IoProcessMessages(IntPtr ioHandle);

		[DllImport("HD-Service-Native.dll")]
		private static extern int LogSetCallback(LoggerCallback func);

		[DllImport("HD-Frontend-Native.dll")]
		private static extern int IsHVmodloaded(StringBuilder check_hv_name);

		[DllImport("HD-Frontend-Native.dll")]
		private static extern bool IsWindows8Point1OrGreater();

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr handle);

		[DllImport("User32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string className, string windowName);

		[DllImport("User32.dll", SetLastError = true)]
		private static extern uint SendMessage(IntPtr window, uint msg, uint wParam, uint lParam);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool IsWow64Process(IntPtr proc, ref bool isWow);

		public static void Main(string[] args)
		{
			if (args.Length != 2)
				Service.Usage();

			string text = args[0];
			string text2 = args[1];
			Service.Setup(text, text2);
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
			string strA = (string)registryKey.GetValue("InstallType");
			if (string.Compare(strA, "split", true) == 0)
			{
				Logger.Info("Installation type is split. Not starting service");
			}
			else if (text == "debug")
			{
				Service service = new Service(true, text, text2);
				service.RunDebug();
			}
			else
			{
				ServiceBase.Run(new Service(false, text, text2));
			}
		}

		private static void Usage()
		{
			string processName = Process.GetCurrentProcess().ProcessName;
			Console.Error.WriteLine("Usage: {0} <svc name> <vm name>", processName);
			Console.Error.WriteLine("       {0} debug      <vm name>", processName);
			Environment.Exit(1);
		}

		private Service(bool consoleMode, string svcName, string vmName)
		{
			this.consoleMode = consoleMode;
			this.consoleModeEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
			this.vmName = vmName;
			this.vmThread = null;
			this.logThread = null;
			this.vmStoppedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
			base.AutoLog = true;
			base.ServiceName = svcName;
			base.CanShutdown = true;
			base.CanStop = true;
			base.CanHandlePowerEvent = true;
		}

		private static void Setup(string svcName, string vmName)
		{
			try
			{
				Logger.Init(svcName == "debug");
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Cannot open log file");
				Console.Error.WriteLine(ex.ToString());
			}
			Service.LogSetCallback(delegate(string msg)
			{
				Logger.Info("{0}", msg);
			});
			AppDomain.CurrentDomain.UnhandledException += delegate(object obj, UnhandledExceptionEventArgs evt)
			{
				Logger.Info("Unhandled Exception:");
				Logger.Info(evt.ExceptionObject.ToString());
				Environment.Exit(1);
			};
		}

		private void RunDebug()
		{
			ConsoleControl.Handler handler = delegate(ConsoleControl.CtrlType ctrl)
			{
				if (ctrl == ConsoleControl.CtrlType.CTRL_C_EVENT)
				{
					this.consoleModeEvent.Set();
					return true;
				}
				return false;
			};
			ConsoleControl.SetHandler(handler);
			this.OnStart(new string[0]);
			this.consoleModeEvent.WaitOne();
			this.OnStop();
		}

		protected override void OnStart(string[] args)
		{
			try
			{
				Logger.Info("OnStart");
				Thread thread = new Thread(this.DisableIntegrityCheckThreadEntry);
				thread.IsBackground = true;
				thread.Start();
				thread.Join(500);
				if (thread.IsAlive)
				{
					thread.Abort();
				}
				if (this.integrityCheckDisabled)
				{
					Logger.Info("Skipping integrity check");
				}
				else
				{
					this.IntegrityCheck();
				}
				if (!Service.IsOs64Bit())
				{
					Logger.Info("Virtualization type Legacy");
					this.OnStartInternal();
				}
				else if (this.ForceVMLegacyMode())
				{
					Logger.Info("Virtualization type Legacy");
					this.OnStartInternal();
				}
				else
				{
					int num = Service.VmHvmCheck(ref this.isHVM);
					if (num != 0)
					{
						this.isHVM = false;
						Logger.Info("VmHvmCheck failed. error = " + num);
					}
					if (this.isHVM)
					{
						Logger.Info("Virtualization type HVM");
						this.OnStartInternalHvm();
					}
					else
					{
						Logger.Info("Virtualization type Legacy");
						this.OnStartInternal();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Info("Unhandled exception in OnStart()");
				Logger.Info(ex.ToString());
				base.ExitCode = 1064;
				throw ex;
			}
		}

		private void OnStartInternal()
		{
			uint num = 0u;
			if (this.consoleMode)
			{
				Logger.Info("Starting {0} VM in debug mode", this.vmName);
			}
			else
			{
				Logger.Info("Starting {0} VM as service {1}", this.vmName, base.ServiceName);
			}
			this.GracefulShutdownCheck();
			Logger.Info("Preparing VM");
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
			this.runtimeDir = (string)registryKey.GetValue("InstallDir");
			Logger.Info("Using runtime directory {0}", this.runtimeDir);
			Logger.Info("Using guest configuration key {0}\\Guests\\{1}", registryKey, this.vmName);
			RegistryKey registryKey2 = registryKey.OpenSubKey("Guests\\"+this.vmName, true);
			string kernel = (string)registryKey2.GetValue("Kernel", null);
			string text = (string)registryKey2.GetValue("Initrd", null);
			registryKey2.SetValue("VirtType", "legacy");
			registryKey.Close();
			registryKey2.Close();
			try
			{
				RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
				string text2 = (string)registryKey3.GetValue("CurrentBuildNumber", "9300");
				Logger.Info("winBuildNumber: " + text2);
				int num2 = Convert.ToInt32(text2);
				if (num2 >= 9200)
				{
					this.SetWin8TileRegistries();
				}
			}
			catch (Exception ex)
			{
				Logger.Info(ex.ToString());
			}
			this.vmHandle = Service.VmCreate(this.vmName, kernel, ref num);
			if (this.vmHandle == IntPtr.Zero)
			{
				throw new SystemException("Cannot create VM", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			Logger.Info("Created monitor with ID {0}", num);
			int num3;
			if (text != null)
			{
				num3 = Service.VmLoadInitrd(this.vmHandle, text);
				if (num3 != 0)
				{
					throw new SystemException("Cannot load initrd", new Win32Exception(num3));
				}
			}
			this.ioHandle = Service.IoAttach(num);
			if (this.ioHandle == IntPtr.Zero)
			{
				throw new SystemException("Cannot attach for I/O", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			MonitorLocator.Publish(this.vmName, num);
			this.netProc = this.StartHelperProcess("HD-Network.exe", "network");
			this.blockProc = this.StartHelperProcess("HD-BlockDevice.exe", "block device");
			this.folderProc = this.StartHelperProcess("HD-SharedFolder.exe", "shared folder");
			this.eventThread = new Thread(this.EventThreadEntry);
			this.eventThread.IsBackground = true;
			this.eventThread.Start();
			num3 = Service.IsHVmodloaded(new StringBuilder("avchv.sys"));
			if (num3 == 1)
			{
				throw new SystemException("Bitdefender antivirus HV.sys loaded", new Win32Exception(num3));
			}
			num3 = Service.VmStart(this.vmHandle);
			if (num3 != 0)
			{
				throw new SystemException("Cannot start VM", new Win32Exception(num3));
			}
			Logger.Info("Spawning VM thread");
			this.vmThread = new Thread(this.VmThreadEntry);
			this.vmThread.Start();
			this.guidMgrThread = new Thread(this.IdManagerEntry);
			this.guidMgrThread.IsBackground = true;
			this.guidMgrThread.Start();
		}

		private void OnStartInternalHvm()
		{
			uint num = 0u;
			if (this.consoleMode)
			{
				Logger.Info("Starting {0} VM in debug mode", this.vmName);
			}
			else
			{
				Logger.Info("Starting {0} VM as service {1}", this.vmName, base.ServiceName);
			}
			this.GracefulShutdownCheck();
			Logger.Info("Preparing VM");
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
			this.runtimeDir = (string)registryKey.GetValue("InstallDir");
			Logger.Info("Using runtime directory {0}", this.runtimeDir);
			Logger.Info("Using guest configuration key {0}\\Guests\\{1}", registryKey, this.vmName);
			RegistryKey registryKey2 = registryKey.OpenSubKey("Guests\\"+this.vmName, true);
			string kernel = (string)registryKey2.GetValue("Kernel", null);
			string text = (string)registryKey2.GetValue("Initrd", null);
			int num2 = 1;
			int num3 = Service.VmHvmGetVirtType(ref num2);
			if (num3 != 0)
			{
				Logger.Info("VmHvmGetVirtType failed. error = " + num3);
			}
			if (num2 == 1)
			{
				Logger.Info("VmHvmGetVirtType virtType = vtx");
				registryKey2.SetValue("VirtType", "vtx");
			}
			else
			{
				Logger.Info("VmHvmGetVirtType virtType = svm");
				registryKey2.SetValue("VirtType", "svm");
			}
			registryKey.Close();
			registryKey2.Close();
			int nrVCpus = 1;
			this.vmHandle = Service.VmCreateHvm(this.vmName, nrVCpus, kernel, ref num);
			if (this.vmHandle == IntPtr.Zero)
			{
				throw new SystemException("Cannot create VM", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			Logger.Info("Created monitor with ID {0}", num);
			if (text != null)
			{
				num3 = Service.VmLoadInitrd(this.vmHandle, text);
				if (num3 != 0)
				{
					throw new SystemException("Cannot load initrd", new Win32Exception(num3));
				}
			}
			this.ioHandle = Service.IoAttach(num);
			if (this.ioHandle == IntPtr.Zero)
			{
				throw new SystemException("Cannot attach for I/O", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			MonitorLocator.Publish(this.vmName, num);
			this.netProc = this.StartHelperProcess("HD-Network.exe", "network");
			this.blockProc = this.StartHelperProcess("HD-BlockDevice.exe", "block device");
			this.folderProc = this.StartHelperProcess("HD-SharedFolder.exe", "shared folder");
			this.eventThread = new Thread(this.EventThreadEntry);
			this.eventThread.IsBackground = true;
			this.eventThread.Start();
			num3 = Service.IsHVmodloaded(new StringBuilder("avchv.sys"));
			if (num3 == 1)
			{
				throw new SystemException("Bitdefender antivirus HV.sys loaded", new Win32Exception(num3));
			}
			num3 = Service.VmStartHvm(this.vmHandle);
			if (num3 != 0)
			{
				throw new SystemException("Cannot start VM", new Win32Exception(num3));
			}
			this.logThread = new Thread(this.LogThreadEntry);
			this.logThread.Start();
			Logger.Info("Spawning VM thread");
			this.vmThread = new Thread(this.VmThreadEntryHvm);
			this.vmThread.Start();
			this.guidMgrThread = new Thread(this.IdManagerEntry);
			this.guidMgrThread.IsBackground = true;
			this.guidMgrThread.Start();
		}

		private void VmThreadEntry()
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			this.GracefulShutdownSet(0);
			while (!flag3)
			{
				int num;
				if (this.vmStopping)
				{
					num = Service.VmCanReceivePowerMessages(this.vmHandle, ref flag2);
					if (num != 0)
					{
						Logger.Info("Cannot get VM state: {0}", num);
						break;
					}
					if (flag2)
					{
						if (!flag)
						{
							Logger.Info("Graceful shutdown");
							Service.VmStop(this.vmHandle);
							flag = true;
						}
					}
					else
					{
						Logger.Info("Hard shutdown");
						flag3 = true;
					}
				}
				if (!this.AreCriticalHelperProcessesAlive())
				{
					Logger.Info("A critical helper process has died");
					break;
				}
				num = Service.VmRun(this.vmHandle);
				if (num == 1291 && this.vmStopping)
				{
					flag3 = true;
				}
				else if (num != 0)
				{
					Logger.Info("Cannot run VM: {0}", num);
					break;
				}
				num = Service.IoProcessMessages(this.ioHandle);
				if (num != 0)
				{
					Logger.Info("Cannot handle I/O: {0}", num);
					break;
				}
			}
			if (this.consoleMode)
			{
				this.consoleModeEvent.Set();
				this.vmStoppedEvent.Set();
			}
			else
			{
				this.vmStoppedEvent.Set();
				if (!this.vmStopping)
				{
					Logger.Info("Internal VM shutdown");
					base.Stop();
				}
			}
		}

		private void VmThreadEntryHvm()
		{
			this.GracefulShutdownSet(0);
			int num = Service.VmRunHvm(this.vmHandle);
			Logger.Info("VmRunHvm returned. error {0}", num);
			if (this.consoleMode)
			{
				this.consoleModeEvent.Set();
				this.vmStoppedEvent.Set();
			}
			else
			{
				this.vmStoppedEvent.Set();
				if (!this.vmStopping)
				{
					Logger.Info("Internal VM shutdown");
					base.Stop();
				}
			}
			Logger.Info("Exiting VmThreadEntryHvm");
		}

		private void LogThreadEntry()
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			Logger.Info("Entered LogThreadEntry");
			while (!flag3)
			{
				int num;
				if (this.vmStopping)
				{
					num = Service.VmCanReceivePowerMessagesHvm(MonitorLocator.Lookup(this.vmName), ref flag2);
					if (num != 0)
					{
						Logger.Info("Cannot get VM state: {0}", num);
						break;
					}
					if (flag2)
					{
						if (!flag)
						{
							Logger.Info("Started Graceful shutdown");
							if (Service.VmStopHvm(MonitorLocator.Lookup(this.vmName)) == 0)
							{
								Logger.Info("VmStopHvm returned successfully");
								flag = true;
								break;
							}
						}
					}
					else if (!flag)
					{
						Logger.Info("Hard shutdown");
						if (Service.VmAbortHvm(MonitorLocator.Lookup(this.vmName)) == 0)
						{
							Logger.Info("VmAbortHvm returned successfully");
							flag = true;
							break;
						}
					}
				}
				num = Service.IoProcessMessages(this.ioHandle);
				if (num != 0)
				{
					Logger.Info("Cannot handle I/O: {0}", num);
					break;
				}
				Thread.Sleep(10);
			}
			Logger.Info("Exiting LogThreadEntry");
		}

		protected override void OnStop()
		{
			Logger.Info("Stopping {0} VM", this.vmName);
			this.GracefulShutdownSet(1);
			this.vmStopping = true;
			while (!this.vmStoppedEvent.WaitOne(1000, false))
			{
				if (!this.consoleMode)
				{
					base.RequestAdditionalTime(5000);
				}
			}
			MonitorLocator.Publish(this.vmName, 0u);
			Logger.Info("{0} VM is Stopped", this.vmName);
			try
			{
				this.CleanupHelperProcess(this.netProc, "network");
				this.CleanupHelperProcess(this.blockProc, "block device");
				this.CleanupHelperProcess(this.folderProc, "shared folder");
			}
			catch (Exception)
			{
			}
			Logger.Info("Stopped {0} VM", this.vmName);
		}

		protected override void OnShutdown()
		{
			Logger.Info("Got operating system shutdown event");
			this.OnStop();
		}

		protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
		{
			Logger.Info("OnPowerEvent({0})", powerStatus);
			string text = "bsthdandroidsvc";
			if (powerStatus == PowerBroadcastStatus.Suspend && this.isHVM)
			{
				try
				{
					ServiceController serviceController = new ServiceController(text);
					if (serviceController.Status == ServiceControllerStatus.Running)
					{
						Logger.Info("Stopping " + text);
						serviceController.Stop();
						serviceController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 10));
					}
				}
				catch (Exception ex)
				{
					Logger.Info("Failed to stop service {0}", text);
					Logger.Info(ex.ToString());
				}
			}
			return base.OnPowerEvent(powerStatus);
		}

		private Process StartHelperProcess(string prog, string desc)
		{
			Logger.Info("Starting {0} helper process", desc);
			string fileName = this.runtimeDir+prog;
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = fileName;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.UseShellExecute = false;
			Process process = new Process();
			process.StartInfo = processStartInfo;
			process.Start();
			char[] array = new char[3];
			if (process.StandardOutput.Read(array, 0, 3) == 0)
			{
				throw new SystemException("Helper process exited prematurely");
			}
			if (array[0] != 'O' && array[1] != 'K' && array[2] != 0)
			{
				throw new SystemException("Helper process sent bad ready message");
			}
			Logger.Info(desc + ": " + new string(array));
			return process;
		}

		private bool AreCriticalHelperProcessesAlive()
		{
			if (this.netProc.HasExited)
			{
				Logger.Info("Network helper process is dead");
				return false;
			}
			if (this.blockProc.HasExited)
			{
				Logger.Info("Block device helper process is dead");
				return false;
			}
			if (this.folderProc.HasExited)
			{
				Logger.Info("Shared folder helper process is dead");
				return false;
			}
			return true;
		}

		private void CleanupHelperProcess(Process proc, string name)
		{
			if (proc != null)
			{
				try
				{
					this.SendHelperCommand(proc.Id, 1026u);
				}
				catch (Exception ex)
				{
					Logger.Info("Cannot ask {0} helper process to quit", name);
					Logger.Info(ex.ToString());
				}
				Logger.Info("Waiting for {0} helper process to terminate", name);
				if (!this.consoleMode)
				{
					base.RequestAdditionalTime(5000);
				}
				if (!proc.WaitForExit(4000))
				{
					Logger.Info("Killing {0} helper process", name);
					try
					{
						proc.Kill();
					}
					catch (Exception ex2)
					{
						Logger.Info("Cannot kill {0} helper process", name);
						Logger.Info(ex2.ToString());
					}
					if (!this.consoleMode)
					{
						base.RequestAdditionalTime(5000);
					}
					if (!proc.WaitForExit(4000))
					{
						Logger.Info("Timeout waiting for {0} helper process", name);
					}
				}
			}
		}

		public static byte[] SHA1Hash(Stream stream)
		{
			SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider();
			return sHA1CryptoServiceProvider.ComputeHash(stream);
		}

		public static string ReadString(string filePath)
		{
			StringBuilder stringBuilder = new StringBuilder();
			using (StreamReader streamReader = new StreamReader(filePath))
			{
				string value;
				while ((value = streamReader.ReadLine()) != null)
				{
					stringBuilder.Append(value);
				}
			}
			return stringBuilder.ToString();
		}

		public static string BytesToHex(byte[] bytes)
		{
			StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
			foreach (byte b in bytes)
			{
				stringBuilder.AppendFormat("{0:x2}", b);
			}
			return stringBuilder.ToString();
		}

		public static byte[] HexToBytes(string hex)
		{
			byte[] array = new byte[hex.Length / 2];
			for (int i = 0; i < hex.Length; i += 2)
			{
				array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}
			return array;
		}

		private bool IntegrityCheckFile(string filePath, string fileSignaturePath)
		{
			if (!File.Exists(fileSignaturePath))
			{
				string text = "Cannot start service. " + fileSignaturePath + " doesn't exist";
				Logger.Info(text);
				throw new ApplicationException(text);
			}
			byte[] array = default(byte[]);
			using (Stream stream = File.OpenRead(filePath))
			{
				array = Service.SHA1Hash(stream);
				string str = Service.BytesToHex(array);
				Logger.Info("SHA1 of " + filePath + " is " + str);
			}
			string hex = Service.ReadString(fileSignaturePath);
			byte[] rgbSignature = Service.HexToBytes(hex);
			RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
			rSACryptoServiceProvider.FromXmlString("<RSAKeyValue><Modulus>rHDWCLJnSXrhdVQjebrkpiPBDWVisgvbq0jVhzWdxAArCxNiDDFC3Ich1QGG+OXbEuPHIN2GKN3jzBIQ2OlKnmtmZaUmbWPE+lAUuPljvnOLCONYW/2Yi592jw8gnW1VGCDze6yuvOqZCE+vuiO8P2bKRfk0mlC5OBbn0bD6QpE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
			bool flag = rSACryptoServiceProvider.VerifyHash(array, CryptoConfig.MapNameToOID("SHA1"), rgbSignature);
			if (!flag)
			{
				string text2 = "Cannot start service. " + filePath + " integrity check failed";
				Logger.Info(text2);
				throw new ApplicationException(text2);
			}
			return flag;
		}

		private void IntegrityCheck()
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
			RegistryKey registryKey2 = registryKey.OpenSubKey("Guests\\"+this.vmName+"\\BlockDevice\\0", true);
			string text = (string)registryKey2.GetValue("Path", null);
			string text2 = text + ".signature";
			RegistryKey registryKey3 = registryKey.OpenSubKey("Guests\\"+this.vmName, true);
			string text3 = (string)registryKey3.GetValue("Kernel", null);
			string text4 = text3 + ".signature";
			string text5 = (string)registryKey3.GetValue("Initrd", null);
			string text6 = text5 + ".signature";
			registryKey.Close();
			registryKey2.Close();
			this.IntegrityCheckFile(text3, text4);
			Logger.Info("Signature " + text4 + " verified successfully for " + text3);
			this.IntegrityCheckFile(text5, text6);
			Logger.Info("Signature " + text6 + " verified successfully for " + text5);
			this.IntegrityCheckFile(text, text2);
			Logger.Info("Signature " + text2 + " verified successfully for " + text);
		}

		private void DisableIntegrityCheckThreadEntry()
		{
			string name = "Software\\BlueStacks\\Guests\\"+this.vmName+"\\Config";
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
			while (true)
			{
				string[] valueNames = registryKey.GetValueNames();
				foreach (string text in valueNames)
				{
					string b = "DisableIntegrityCheck";
					if (!(text != b))
					{
						string text2 = (string)registryKey.GetValue(text);
						string a = this.ComputeSha1Digest(text2);
						this.integrityCheckDisabled = (a == "1E52EDBA5E4711ADE83F3A3B28E396BF7031A3D3");
					}
				}
				Thread.Sleep(1000);
			}
		}

		private string ComputeSha1Digest(string text)
		{
			SHA1 sHA = new SHA1CryptoServiceProvider();
			Encoding encoding = new UTF8Encoding(true, true);
			string text2 = "";
			try
			{
				byte[] bytes = encoding.GetBytes(text);
				byte[] array = sHA.ComputeHash(bytes);
				for (int i = 0; i < array.Length; i++)
				{
					text2 += array[i];
				}
				return text2;
			}
			catch (Exception ex)
			{
				Logger.Info("Cannot compute digest");
				Logger.Info(ex.ToString());
				return "";
			}
		}

		private void GracefulShutdownCheck()
		{
			string name = "Software\\BlueStacks\\Guests\\"+this.vmName+"\\Config";
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name))
			{
				object value = registryKey.GetValue("ServiceStoppedGracefully");
				if (value != null && (int)value == 0)
				{
					string text = "Cannot start service.  Service did not stop gracefully the last time it was run.";
					Logger.Info(text);
					throw new ApplicationException(text);
				}
			}
		}

		private void GracefulShutdownSet(int val)
		{
			string name = "Software\\BlueStacks\\Guests\\"+this.vmName+"\\Config";
			RegistryKey registryKey;
			using (registryKey = Registry.LocalMachine.OpenSubKey(name, true))
			{
				registryKey.SetValue("ServiceStoppedGracefully", val, RegistryValueKind.DWord);
				registryKey.Flush();
			}
		}

		private void SetWin8TileRegistries()
		{
			Logger.Info("Setting registries required for win8 tiles");
			try
			{
				string subkey = "Software\\BlueStacks\\Guests\\Android\\Config";
				RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(subkey);
				string text = (string)registryKey.GetValue("ProgId", "");
				Logger.Info("progId: " + text);
				if (!(text == ""))
				{
					string subkey2 = "Software\\Classes\\"+text;
					RegistryKey registryKey2 = Registry.LocalMachine.CreateSubKey(subkey2);
					registryKey2.SetValue("AppUserModelId", "Microsoft.InternetExplorer.Default");
					registryKey2.Close();
					RegistryKey registryKey3 = Registry.ClassesRoot.CreateSubKey(text);
					registryKey3.SetValue("AppUserModelId", "Microsoft.InternetExplorer.Default");
					registryKey3.Close();
					Service.RefreshExplorer();
				}
			}
			catch (Exception ex)
			{
				Logger.Info(ex.ToString());
			}
		}

		public static void RefreshExplorer()
		{
			Service.SHChangeNotify(134217728, 0, 0, 0);
		}

		private bool ForceVMLegacyMode()
		{
			bool result = false;
			string name = "Software\\BlueStacks\\Guests\\"+this.vmName+"\\Config";
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name))
			{
				object value = registryKey.GetValue("ForceVMLegacyMode");
				if (value != null)
				{
					if ((int)value != 0)
					{
						Logger.Info("ForceVMLegacyMode is set");
						return true;
					}
					return result;
				}
				return result;
			}
		}

		private void SendHelperCommand(int pid, uint cmd)
		{
			string windowName = "uHD-Core-Window-"+pid;
			IntPtr intPtr = Service.FindWindow(null, windowName);
			if (intPtr == IntPtr.Zero)
			{
				throw new SystemException("Cannot find window for helper PID " + pid, new Win32Exception(Marshal.GetLastWin32Error()));
			}
			Service.SendMessage(intPtr, cmd, 0u, 0u);
		}

		private void EventThreadEntry()
		{
			EventWaitHandleSecurity eventWaitHandleSecurity = new EventWaitHandleSecurity();
			EventWaitHandleAccessRule rule = new EventWaitHandleAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), EventWaitHandleRights.Modify, AccessControlType.Allow);
			eventWaitHandleSecurity.AddAccessRule(rule);
			bool flag = default(bool);
			EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, "Global\\BlueStacks_Core_Service_Info_Event", out flag, eventWaitHandleSecurity);
			if (!flag)
			{
				throw new SystemException("Info request event already exists");
			}
			EventWaitHandle eventWaitHandle2 = new EventWaitHandle(false, EventResetMode.ManualReset, "Global\\BlueStacks_Core_Service_LogRotate_Event", out flag, eventWaitHandleSecurity);
			if (!flag)
			{
				throw new SystemException("Log rotate event already exists");
			}
			WaitHandle[] waitHandles = new WaitHandle[2]
			{
				eventWaitHandle,
				eventWaitHandle2
			};
			while (true)
			{
				WaitHandle.WaitAny(waitHandles);
				if (eventWaitHandle.WaitOne(0, false))
				{
					this.EventThreadHandleInfoRequest();
					eventWaitHandle.Reset();
				}
				if (eventWaitHandle2.WaitOne(0, false))
				{
					this.EventThreadHandleLogRotateRequest();
					eventWaitHandle2.Reset();
				}
			}
		}

		private void IdManagerEntry()
		{
			Logger.Info("Starting id manager");
			bool flag = true;
			string text = Id.GenerateID();
			do
			{
				Thread.Sleep(50);
				flag = false;
				try
				{
					Command command = new Command();
					command.Attach(this.vmName);
					int num = command.Run(new string[2]
					{
						"iSetId",
						text
					});
					if (num != 0)
					{
						Logger.Info("Failed to set Id:" + num);
						flag = true;
					}
				}
				catch
				{
					Logger.Info("Retrying to set Id");
					flag = true;
				}
			}
			while (flag);
			Logger.Info("Set Id success!");
		}

		private void EventThreadHandleInfoRequest()
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				Logger.Info("Asking helper processes to dump info");
				this.SendHelperCommand(this.blockProc.Id, 1024u);
				Thread.Sleep(250);
				this.SendHelperCommand(this.netProc.Id, 1024u);
				Thread.Sleep(250);
				this.SendHelperCommand(this.folderProc.Id, 1024u);
				Thread.Sleep(250);
			});
			thread.IsBackground = true;
			thread.Start();
		}

		private void EventThreadHandleLogRotateRequest()
		{
			Logger.Info("Handling log rotate request");
			this.SendHelperCommand(this.blockProc.Id, 1025u);
			this.SendHelperCommand(this.netProc.Id, 1025u);
			this.SendHelperCommand(this.folderProc.Id, 1025u);
			Logger.Reinit();
		}

		public static bool IsOs64Bit()
		{
			Process currentProcess = Process.GetCurrentProcess();
			bool result = false;
			if (!Service.IsWow64Process(currentProcess.Handle, ref result))
			{
				throw new Exception("Could not get os arch info.");
			}
			return result;
		}
	}
}
