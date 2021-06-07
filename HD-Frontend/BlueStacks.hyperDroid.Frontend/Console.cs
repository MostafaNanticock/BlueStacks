using BlueStacks.hyperDroid.Audio;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using BlueStacks.hyperDroid.Common.Interop;
using BlueStacks.hyperDroid.Common.UI;
using BlueStacks.hyperDroid.Core.VMCommand;
using BlueStacks.hyperDroid.Frontend.Interop;
using BlueStacks.hyperDroid.Gps;
using BlueStacks.hyperDroid.Locale;
using BlueStacks.hyperDroid.ThinInstaller;
using BlueStacks.hyperDroid.VideoCapture;
using CodeTitans.JSon;
using Microsoft.Samples.TabletPC.MTScratchpad.WMTouch;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
    public class Console : WMTouchForm
    {
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;

            public int cbData;

            public IntPtr lpData;
        }

        public enum State
        {
            Initial,
            DownloadData,
            Stopped,
            Starting,
            CannotStart,
            ConnectingBlank,
            Connecting,
            Connected
        }

        private enum GlWindowAction
        {
            None,
            Show,
            Hide
        }

        private class ControlHandler : IControlHandler
        {
            private Console mConsole;

            public ControlHandler(Console console)
            {
                this.mConsole = console;
            }

            public void Back()
            {
                if (this.mConsole.mFrontendState == State.Connected)
                {
                    Logger.Info("Back Button Clicked");
                    this.mConsole.HandleKeyEvent(Keys.Escape, true);
                    Thread.Sleep(100);
                    this.mConsole.HandleKeyEvent(Keys.Escape, false);
                }
            }

            public void InputMapper()
            {
                if (this.mConsole.mFrontendState == State.Connected)
                {
                    Logger.Info("InputMapper Button Clicked");
                    InputMapperTool inputMapperTool = new InputMapperTool(this.mConsole.Left, this.mConsole.Top, this.mConsole.Width, this.mConsole.Height - 48);
                    if (!InputMapperTool.sCurrentAppPackage.StartsWith("com.bluestacks"))
                    {
                        inputMapperTool.ShowDialog();
                    }
                }
            }

            public void Menu()
            {
                if (this.mConsole.mFrontendState == State.Connected)
                {
                    Logger.Info("Menu Button Clicked");
                    this.mConsole.HandleKeyEvent(Keys.Apps, true);
                    this.mConsole.HandleKeyEvent(Keys.Apps, false);
                }
            }

            public void Home()
            {
                if (this.mConsole.mFrontendState == State.Connected)
                {
                    Logger.Info("Home Button Clicked");
                    VmCmdHandler.RunCommand("home");
                }
            }

            public void Share()
            {
                if (this.mConsole.mFrontendState == State.Connected)
                {
                    Logger.Info("Share Button Clicked");
                    this.mConsole.HandleShareButtonClicked();
                }
            }

            public void Settings()
            {
                if (this.mConsole.mFrontendState == State.Connected)
                {
                    Logger.Info("Settings Button Clicked");
                    string cmd = string.Format("runex {0}/{1}", "com.bluestacks.settings", "com.bluestacks.settings.SettingsActivity");
                    VmCmdHandler.RunCommand(cmd);
                }
            }

            public void FullScreen()
            {
                Logger.Info("Full Screen Button Clicked");
                this.mConsole.ToggleFullScreen();
            }

            public void Close()
            {
                Logger.Info("Close Button Clicked");
                this.mConsole.Close();
            }
        }

        private const string REG_ROOT = "Software\\BlueStacks";

        private const string REG_ANDROID = "Software\\BlueStacks\\Guests\\Android";

        private const string REG_CONFIG = "Software\\BlueStacks\\Guests\\Android\\Config";

        private const int VM_EVENT_PORT = 9998;

        public const int GUEST_ABS_MAX_X = 32768;

        public const int GUEST_ABS_MAX_Y = 32768;

        public const int TOUCH_POINTS_MAX = 16;

        private const int SWIPE_TOUCH_POINTS_MAX = 1;

        private const int WM_QUERYENDSESSION = 17;

        private const int WM_SYSCOMMAND = 274;

        private const int WM_COPYDATA = 74;

        private const int SC_MAXIMIZE = 61488;

        private const int SC_MAXIMIZE2 = 61490;

        private const int SC_RESTORE = 61728;

        private const int SC_RESTORE2 = 61730;

        private const long LWIN_TIMEOUT_TICKS = 1000000L;

        private const int mTimeoutSecs = 60;

        private Size mConfiguredDisplaySize;

        private Size mConfiguredGuestSize;

        private Size mCurrentDisplaySize;

        private Rectangle mScaledDisplayArea;

        private bool mEmulatedPortraitMode;

        private bool mRotateGuest180;

        private bool mFullScreen = true;

        private bool originalFullScreenState;

        private FullScreenToast mFullScreenToast;

        private bool mControlBarVisible = true;

        private bool mControlBarEnabled = true;

        private bool mControlBarAlwaysShow;

        private ControlBar mControlBar;

        public State mFrontendState;

        private static Mutex sFrontendLock;

        private string vmName;

        private EventWaitHandle glReadyEvent;

        private string mUpdaterServiceName = "BstHdUpdaterSvc";

        private DateTime mLastActivityEndedSendMsgTime;

        private BlueStacks.hyperDroid.Frontend.Interop.Manager manager;

        private BlueStacks.hyperDroid.Frontend.Interop.Monitor monitor;

        private Video video;

        private Keyboard keyboard;

        private Mouse mouse;

        private Cursor mCursor;

        private InputMapper mInputMapper;

        private OpenSensor mOpenSensor;

        private GamePad mGamePad;

        private BlueStacks.hyperDroid.Frontend.Interop.Monitor.TouchPoint[] touchPoints;

        private SensorDevice mSensorDevice = new SensorDevice();

        private Dictionary<int, string> mControllerMap;

        private System.Windows.Forms.Timer afterSleepTimer;

        private static bool sDontStartVm = false;

        private bool cannotStartVm;

        private bool guestFinishedBooting;

        private bool appLaunched;

        private bool guestHasStopped;

        private bool glInitFailed;

        private bool mDownloadingAmi;

        private System.Windows.Forms.Timer timer;

        private bool grabKeyboard = true;

        private bool frontendNoClose;

        private bool stopZygoteOnClose;

        private bool forceVideoModeChange;

        private bool isMinimized;

        private static bool sFrontendActive = true;

        private long lastLWinTimestamp;

        public static string sInstallDir;

        private LoadingScreen loadingScreen;

        private bool atLoadingScreen;

        private Toast snapshotErrorToast;

        private bool snapshotErrorShown;

        private bool lockdownDisabled;

        private bool disableDwm;

        private bool userInteracted;

        private bool firstActivated;

        private bool frontendMinimized;

        private bool sessionEnding;

        private bool checkingIfBooted;

        private SynchronizationContext mUiContext;

        public static Console s_Console;

        public static ThinInstallerUi thinInstallerUiForm;

        private UsageTime mUsageTime;

        private static bool s_KeyMapTeachMode = false;

        private static ToolTip s_KeyMapToolTip = new ToolTip();

        private string mCurrentAppPackage;

        private string mCurrentAppActivity;

        private object lockObject = new object();

        private GlWindowAction glWindowAction;

        private static string sAppName = "";

        private static Image sAppIconImage;

        private static string sAppIconName = "";

        private static string sAppPackage = "";

        private static string sApkUrl;

        private static string sDriverUpdateUrl = "";

        private static string s_AmiDebugUrl = BlueStacks.hyperDroid.Common.Strings.ChannelsUrl + "/kkamiapkurl";

        private static string s_AmiDebug = "AmiDebug";

        private static int sFrontendPort = 2862;

        private static bool sNormalMode = false;

        private static bool sAppLaunch = false;

        private static bool sHideMode = false;

        private static bool sIsSpawnApps = false;

        private BlueStacks.hyperDroid.VideoCapture.Manager camManager;

        public static bool sPgaInitDone = false;

        [CompilerGenerated]
        private static LoggerCallback _003C_003E9__CachedAnonymousMethodDelegate1;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate1c;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate2c;

        [CompilerGenerated]
        private static ThreadExceptionEventHandler _003C_003E9__CachedAnonymousMethodDelegate2d;

        [CompilerGenerated]
        private static UnhandledExceptionEventHandler _003C_003E9__CachedAnonymousMethodDelegate2e;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate43;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate44;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate45;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDPIAware();

        [DllImport("kernel32.dll")]
        private static extern void OutputDebugString(string lpOutputString);

        private static void SetInstallDir()
        {
            string name = "Software\\BlueStacks";
            RegistryKey registryKey;
            using (registryKey = Registry.LocalMachine.OpenSubKey(name))
            {
                Console.sInstallDir = (string)registryKey.GetValue("InstallDir");
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Console.ValidateArgs(args);
            Console.InitLog(args[0]);
            Console.SetInstallDir();
            Console.ParseArgs(args);
            if (args.Length == 2 && args[1] == "dbgwait")
            {
                Console.WaitForDebugger();
            }
            if (args.Length == 2 && args[1] == "dontstartvm")
            {
                Console.sDontStartVm = true;
            }
            if (args.Length == 2 && args[1] == "hidemode")
            {
                Console.sFrontendActive = false;
                Console.sHideMode = true;
            }
            BlueStacks.hyperDroid.Locale.Strings.InitLocalization();
            if (!BlueStacks.hyperDroid.Common.Utils.IsOSWinXP())
            {
                Console.SetProcessDPIAware();
            }
            ServicePointManager.DefaultConnectionLimit = 10;
            try
            {
                if (BlueStacks.hyperDroid.Common.Utils.IsOEMBlueStacks() || BlueStacks.hyperDroid.Common.Utils.IsOEM("China") || BlueStacks.hyperDroid.Common.Utils.IsOEM("anqu") || BlueStacks.hyperDroid.Common.Utils.IsOEM("anquicafe") || BlueStacks.hyperDroid.Common.Utils.IsOEM("msft") || BlueStacks.hyperDroid.Common.Utils.IsOEM("ucweb"))
                {
                    BlueStacks.hyperDroid.Common.Strings.AppTitle = "BlueStacks App Player";
                }
                else if (BlueStacks.hyperDroid.Common.Utils.IsOEM("Lenovo"))
                {
                    BlueStacks.hyperDroid.Common.Strings.AppTitle = "Lenovo App Player";
                }
                else if (BlueStacks.hyperDroid.Common.Utils.IsOEM("MSI") || BlueStacks.hyperDroid.Common.Utils.IsOEM("360") || BlueStacks.hyperDroid.Common.Utils.IsOEM("bigfish"))
                {
                    BlueStacks.hyperDroid.Common.Strings.AppTitle = "BlueStacks App Player";
                }
                else if (BlueStacks.hyperDroid.Common.Utils.IsOEM("wildtangent"))
                {
                    BlueStacks.hyperDroid.Common.Strings.AppTitle = "WildTangent Games";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Could not set app player title. err: " + ex.ToString());
            }
            if (BlueStacks.hyperDroid.Common.Utils.IsRunningInSpawner())
            {
                Console.sIsSpawnApps = true;
            }
            try
            {
                if (BlueStacks.hyperDroid.Common.Utils.IsAlreadyRunning(BlueStacks.hyperDroid.Common.Strings.FrontendLockName, out Console.sFrontendLock))
                {
                    Logger.Info("Frontend already running");
                    IntPtr intPtr = IntPtr.Zero;
                    if (!Console.sHideMode)
                    {
                        intPtr = Console.BringToFront(args[0]);
                    }
                    if (Console.sNormalMode)
                    {
                        Logger.Info("Sending WM_USER_SHOW_WINDOW to Frontend handle {0}", intPtr);
                        int num = Window.SendMessage(intPtr, 1025u, IntPtr.Zero, IntPtr.Zero);
                        if (intPtr != IntPtr.Zero && num != 1)
                        {
                            string text = "http://127.0.0.1:" + Console.sFrontendPort + "/" + BlueStacks.hyperDroid.Common.Strings.ShowWindowUrl;
                            Dictionary<string, string> dictionary = new Dictionary<string, string>();
                            Logger.Info("Sending request to: " + text);
                            try
                            {
                                string str = Client.Post(text, dictionary, null, false);
                                Logger.Info("showwindow result: " + str);
                            }
                            catch (Exception ex2)
                            {
                                Logger.Error(ex2.ToString());
                                Logger.Error("Post failed. url = {0}, data = {1}", text, dictionary);
                            }
                        }
                    }
                    if (Console.sApkUrl != null)
                    {
                        string text2 = "http://127.0.0.1:" + Console.sFrontendPort + "/" + BlueStacks.hyperDroid.Common.Strings.CommandLineArgsUrl;
                        Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
                        Logger.Info("Sending request to: " + text2);
                        int num2 = 0;
                        foreach (string value in args)
                        {
                            dictionary2.Add(num2.ToString(), value);
                            num2++;
                        }
                        try
                        {
                            string str2 = Client.Post(text2, dictionary2, null, false);
                            Logger.Info("showwindow result: " + str2);
                        }
                        catch (Exception ex3)
                        {
                            Logger.Error(ex3.ToString());
                            Logger.Error("Post failed. url = {0}, data = {1}", text2, dictionary2);
                        }
                    }
                    Environment.Exit(0);
                }
                Application.EnableVisualStyles();
                ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback(Console.ValidateRemoteCertificate));
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
                string InstallType = (string)registryKey.GetValue("InstallType");
                if (string.Compare(InstallType, "uninstalled", true) == 0)
                {
                    System.Windows.Forms.MessageBox.Show("BlueStacks App Player is not installed on this machine. Please install it and try again. You can download the latest version from www.bluestacks.com", "BlueStacks is not installed.");
                    Application.Exit();
                }
                if (string.Compare(InstallType, "nconly", true) != 0 && string.Compare(InstallType, "split", true) != 0 && !Console.CheckAndroidFilesIntegrity())
                {
                    System.Windows.Forms.MessageBox.Show("The BlueStacks App Player installation is corrupt. Please download and install the latest version from www.bluestacks.com", "BlueStacks installation is corrupt.");
                    Environment.Exit(1);
                }
                Application.Run(new Console(args[0]));
                Logger.Info("console message loop done");
            }
            catch (Exception ex4)
            {
                Logger.Error("Unhandled Exception:");
                Logger.Error(ex4.ToString());
                Environment.Exit(1);
            }
        }

        public static void ValidateArgs(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Usage();
            }
        }

        public static void ParseArgs(string[] args)
        {
            Logger.Info("Arg list:");
            foreach (string msg in args)
            {
                Logger.Info(msg);
            }
            if (args.Length >= 3)
            {
                Console.sNormalMode = true;
                Console.sAppName = args[1];
                if (Console.sAppName != "")
                {
                    Console.sAppIconName = args[2];
                    try
                    {
                        Console.sAppIconImage = new Bitmap(Path.Combine(BlueStacks.hyperDroid.Common.Strings.GadgetDir, args[2]));
                    }
                    catch (Exception)
                    {
                        Console.sAppIconImage = new Bitmap(Path.Combine(Console.sInstallDir, "ProductLogo.png"));
                    }
                    Console.sAppLaunch = true;
                }
                if (args.Length == 5)
                {
                    Console.sAppPackage = args[3];
                    Console.sApkUrl = args[4];
                    Logger.Info("Got sAppPackage = {0} and sApkUrl = {1}", Console.sAppPackage, Console.sApkUrl);
                    Console.sAppLaunch = true;
                }
            }
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }

        private static bool CheckAndroidFilesIntegrity()
        {
            string bstAndroidDir = BlueStacks.hyperDroid.Common.Strings.BstAndroidDir;
            string Prebundled = Path.Combine(bstAndroidDir, "Prebundled.fs");
            string Root = Path.Combine(bstAndroidDir, "Root.fs");
            string Kernel = Path.Combine(bstAndroidDir, "kernel.elf");
            string initrd = Path.Combine(bstAndroidDir, "initrd.img");
            string Data = Path.Combine(bstAndroidDir, "Data.sparsefs");
            string SDCard = Path.Combine(bstAndroidDir, "SDCard.sparsefs");
            try
            {
                if (!BlueStacks.hyperDroid.Common.Utils.IsFileNullOrMissing(Prebundled) && !BlueStacks.hyperDroid.Common.Utils.IsFileNullOrMissing(Root) && !BlueStacks.hyperDroid.Common.Utils.IsFileNullOrMissing(Kernel) && !BlueStacks.hyperDroid.Common.Utils.IsFileNullOrMissing(initrd) && !BlueStacks.hyperDroid.Common.Utils.IsFileNullOrMissing(Path.Combine(Data, "Map")) && File.Exists(Path.Combine(Data, "Store")) && !BlueStacks.hyperDroid.Common.Utils.IsFileNullOrMissing(Path.Combine(SDCard, "Map")) && File.Exists(Path.Combine(SDCard, "Store")))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return true;
            }
        }

        private static void UpdateDownloadProgress()
        {
            int num = 0;
            while (true)
            {
                if (Console.thinInstallerUiForm == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (num != 10)
                {
                    try
                    {
                        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
                        int amount = (int)registryKey.GetValue("DownloadProgress", 0);
                        Console.thinInstallerUiForm.UpdateProgressBar(amount);
                    }
                    catch (Exception ex)
                    {
                        num++;
                        Logger.Error("Failed to update download progress. Err: " + ex.ToString());
                    }
                    Thread.Sleep(1000);
                    continue;
                }
                break;
            }
            Logger.Error("Too many exceptions while updating download progress. Exiting thread.");
        }

        private void SetupHTTPServer()
        {
            Dictionary<string, Server.RequestHandler> dictionary = new Dictionary<string, Server.RequestHandler>();
            dictionary.Add("/quitfrontend", HTTPHandler.QuitFrontend);
            dictionary.Add("/updatetitle", HTTPHandler.UpdateWindowTitle);
            dictionary.Add("/switchtolauncher", HTTPHandler.SwitchToLauncher);
            dictionary.Add("/switchtowindows", HTTPHandler.SwitchToWindows);
            dictionary.Add("/switchorientation", HTTPHandler.SwitchOrientation);
            dictionary.Add("/showwindow", HTTPHandler.ShowWindow);
            dictionary.Add("/sharescreenshot", HTTPHandler.ShareScreenshot);
            dictionary.Add("/togglescreen", HTTPHandler.ToggleScreen);
            dictionary.Add("/goback", HTTPHandler.GoBack);
            dictionary.Add("/closescreen", HTTPHandler.CloseScreen);
            dictionary.Add("/softControlBarEvent", HTTPHandler.HandleSoftControlBarEvent);
            dictionary.Add("/ping", HTTPHandler.PingHandler);
            dictionary.Add("/showtileinterface", HTTPHandler.ShowTileInterface);
            dictionary.Add("/commandlineargs", HTTPHandler.CommandLineArgs);
            dictionary.Add("/copyfiles", HTTPHandler.SendFilesToWindows);
            dictionary.Add("/getwindowsfiles", HTTPHandler.PickFilesFromWindows);
            dictionary.Add("/updategraphicsdrivers", HTTPHandler.UpdateGraphicsDrivers);
            dictionary.Add("/gpscoordinates", HTTPHandler.UpdateGpsCoordinates);
            dictionary.Add("/" + BlueStacks.hyperDroid.Common.Strings.AppDataFEUrl, this.SetCurrentAppData);
            Server server = new Server(Console.sFrontendPort, dictionary, null);
            server.Start();
            Logger.Info("Frontend server listening on port: " + server.Port);
            Console.sFrontendPort = server.Port;
            Thread thread = new Thread(this.SendFqdn);
            thread.IsBackground = true;
            thread.Start();
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
            registryKey.SetValue("FrontendServerPort", server.Port, RegistryValueKind.DWord);
            registryKey.Close();
            server.Run();
        }

        private void SendFqdn()
        {
            Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
            while (VmCmdHandler.FqdnSend(Console.sFrontendPort, "Frontend") == null)
            {
                Thread.Sleep(2000);
            }
        }

        private void SetCurrentAppData(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("SetCurrentAppData");
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                this.mCurrentAppPackage = requestData.data["package"];
                this.mCurrentAppActivity = requestData.data["activity"];
                Logger.Info("SetCurrentAppData mCurrentAppPackage = " + this.mCurrentAppPackage);
                Logger.Info("SetCurrentAppData mCurrentAppActivity = " + this.mCurrentAppActivity);
                Logger.Info("Looking for: " + Console.sAppPackage);
                if (!this.guestFinishedBooting && this.mCurrentAppPackage != "com.bluestacks.gamepophome" && this.mCurrentAppPackage != "com.bluestacks.appmart" && (Console.sAppIconName.Contains(this.mCurrentAppActivity) || Console.sAppPackage == this.mCurrentAppPackage))
                {
                    Logger.Info("Moved away from home");
                    this.guestFinishedBooting = true;
                    this.appLaunched = true;
                }
                if (Features.ExitOnHome() && this.appLaunched && (this.mCurrentAppPackage == "com.bluestacks.gamepophome" || this.mCurrentAppPackage == "com.bluestacks.appmart"))
                {
                    Logger.Info("Reached home app. Closing frontend.");
                    base.Close();
                }
                Opengl.HandleAppActivity(this.mCurrentAppPackage, this.mCurrentAppActivity);
                InputMapper.Instance().SetPackage(this.mCurrentAppPackage);
                InputMapperTool.sCurrentAppPackage = this.mCurrentAppPackage;
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server SetCurrentAppData: " + ex.ToString());
            }
        }

        private static void Quit()
        {
            Logger.Info("Exiting BlueStacks. Killing all running processes...");
            BlueStacks.hyperDroid.Common.Utils.StopServiceNoWait("bsthdandroidsvc");
            BlueStacks.hyperDroid.Common.Utils.KillProcessesByName(new string[4]
			{
				"HD-ApkHandler",
				"HD-Adb",
				"HD-Restart",
				"HD-RunApp"
			});
            Environment.Exit(0);
        }

        private static void Usage()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
            string caption = "BlueStacks Frontend";
            string str = "";
            str += string.Format("Usage:\n");
            str += "    " + processName + " <vm name>\n";
            System.Windows.Forms.MessageBox.Show(str, caption);
            Environment.Exit(1);
        }

        private static Image ResizeImage(string imagePath, int height, int width)
        {
            Image image = Image.FromFile(imagePath);
            Image image2 = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(image2);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(image, 0, 0, image2.Width, image2.Height);
            return image2;
        }

        private static void WaitForDebugger()
        {
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(1000);
            }
            Debugger.Break();
        }

        private static void PromptUserForUpdates()
        {
            string title = "Check for Updates";
            string message = string.Format("Please check whether a new version of BlueStacks is available at {0}", "http://updates.bluestacks.com/check");
            DialogResult dialogResult = BlueStacks.hyperDroid.Common.UI.MessageBox.ShowMessageBox(title, message, "Check Now and Launch", "Remind Me Later", null);
            if (dialogResult == DialogResult.OK)
            {
                string fileName = string.Format("{0}?version={1}&user_guid={2}", "http://updates.bluestacks.com/check", "0.9.4.4078", User.GUID);
                try
                {
                    Process.Start(fileName);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            }
        }

        private static IntPtr BringToFront(string vmName)
        {
            string name = string.Format("{0}\\{1}\\FrameBuffer\\0", "Software\\BlueStacks\\Guests", vmName);
            RegistryKey registryKey;
            bool fullScreen = default(bool);
            using (registryKey = Registry.LocalMachine.OpenSubKey(name))
            {
                fullScreen = ((int)registryKey.GetValue("FullScreen", 0) != 0);
            }
            Logger.Info("Starting BlueStacks " + vmName + " Frontend");
            string appTitle = BlueStacks.hyperDroid.Common.Strings.AppTitle;
            IntPtr zero = IntPtr.Zero;
            try
            {
                return Window.BringWindowToFront(appTitle, fullScreen);
            }
            catch (Exception ex)
            {
                Logger.Error("Cannot bring existing frontend window for VM {0} to the foreground", vmName);
                Logger.Error(ex.ToString());
                return Window.FindWindow(null, appTitle);
            }
        }

        protected override void OnLoad(EventArgs evt)
        {
            Animate.AnimateWindow(base.Handle, 500, 524288);
            base.OnLoad(evt);
        }

        private Console(string vmName)
            : base(16, delegate(string msg)
            {
                Logger.Info("Touch: " + msg);
            })
        {
            Input.DisablePressAndHold(base.Handle);
            Input.HookKeyboard(this.HandleKeyboardHook);
            Console.s_Console = this;
            this.mUsageTime = new UsageTime();
            this.mUiContext = SynchronizationContext.Current;
            this.vmName = vmName;
            this.keyboard = new Keyboard();
            this.mouse = new Mouse();
            this.mControllerMap = new Dictionary<int, string>();
            this.AllowDrop = true;
            base.DragEnter += FileImporter.HandleDragEnter;
            base.DragDrop += FileImporter.MakeDragDropHandler(this);
            this.touchPoints = new BlueStacks.hyperDroid.Frontend.Interop.Monitor.TouchPoint[16];
            for (int i = 0; i < this.touchPoints.Length; i++)
            {
                this.touchPoints[i] = new BlueStacks.hyperDroid.Frontend.Interop.Monitor.TouchPoint(65535, 65535);
            }
            this.mFrontendState = State.Initial;
            this.InitConfig(vmName);
            this.InitForm();
            this.InitScreen();
            this.mLastActivityEndedSendMsgTime = DateTime.Now;
            this.mFullScreenToast = new FullScreenToast(this);
            base.KeyPreview = true;
            base.FormClosing += this.HandleCloseEvent;
            base.LostFocus += this.HandleLostFocusEvent;
            base.Layout += this.HandleLayoutEvent;
            ServiceController serviceController = new ServiceController("BstHdAndroidSvc");
            if (serviceController.Status == ServiceControllerStatus.Running && Console.sNormalMode)
            {
                base.Paint += this.HandlePaintEvent;
            }
            if (this.disableDwm)
            {
                try
                {
                    DWM.DisableComposition();
                }
                catch (Exception ex)
                {
                    Logger.Error("Cannot disable DWM composition");
                    Logger.Error(ex.ToString());
                }
            }
            Logger.Info("Opengl.Init({0}, {1}, {2}, {3}, {4})", base.Handle, this.mScaledDisplayArea.X, this.mScaledDisplayArea.Y, this.mConfiguredGuestSize.Width, this.mConfiguredGuestSize.Height);
            Opengl.Init(vmName, base.Handle, this.mScaledDisplayArea.X, this.mScaledDisplayArea.Y, this.mConfiguredGuestSize.Width, this.mConfiguredGuestSize.Height, this.GlInitSuccess, this.GlInitFailed);
            Logger.Info("Done Opengl.Init");
            this.mCursor = new Cursor(this, Console.sInstallDir);
            this.SetupInputMapper();
            Logger.Info("Done InputMapper");
            this.SetupOpenSensor();
            Logger.Info("Done OpenSensor");
            this.mGamePad = new GamePad();
            this.mGamePad.Setup(this.mInputMapper, base.Handle);
            this.mCursor.SetInputMapper(this.mInputMapper);
            this.SetupSoftControlBar();
            this.mSensorDevice.StartThreads();
            Thread thread = new Thread(this.SetupHTTPServer);
            thread.IsBackground = true;
            thread.Start();
            if (BlueStacks.hyperDroid.Common.Utils.ForceVMLegacyMode == 1)
            {
                this.SetupVmxChecker();
            }
            this.SetupGraphicsDriverVersionChecker();
            SystemEvents.DisplaySettingsChanged += this.HandleDisplaySettingsChanged;
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
            string strA = (string)registryKey.GetValue("InstallType");
            if (string.Compare(strA, "nconly", true) == 0 || string.Compare(strA, "split", true) == 0)
            {
                if (string.Compare(strA, "nconly", true) == 0)
                {
                    Logger.Info("Restarting agent and setting registry to split");
                    BlueStacks.hyperDroid.Common.Utils.KillProcessByName("HD-Agent");
                    registryKey.SetValue("InstallType", "split");
                    registryKey.Flush();
                }
                string path = (string)registryKey.GetValue("InstallDir");
                Process.Start(Path.Combine(path, "HD-Agent"));
                this.StateExitInitial();
                this.StateEnterDownloadData("Runtime");
            }
            else if (!this.IsAppInstalled(Console.sAppPackage) && Console.sApkUrl != null)
            {
                this.StateExitInitial();
                this.StateEnterDownloadData("App");
            }
            else
            {
                this.StateExitInitial();
                this.ContinueStateMachine();
            }
        }

        public void CheckUserActive()
        {
            try
            {
                Logger.Debug("In CheckUserActive, last msg time = " + this.mLastActivityEndedSendMsgTime);
                if (TimeSpan.Compare(DateTime.Now.Subtract(this.mLastActivityEndedSendMsgTime), new TimeSpan(0, 0, 60)) > 0)
                {
                    Stats.SendUserActiveStats("true");
                    this.mLastActivityEndedSendMsgTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error Occured, Err : " + ex.ToString());
            }
        }

        public void ActionOnRelaunch(string[] args)
        {
            Console.ValidateArgs(args);
            Console.ParseArgs(args);
            if (this.mFrontendState != State.DownloadData && this.IsAppInstalled(Console.sAppPackage))
            {
                this.RunApp(Console.sAppPackage, ".Main");
            }
        }

        private void ContinueStateMachine()
        {
            Logger.Info("Continuing state machine");
            ServiceController serviceController = new ServiceController("bsthdandroidsvc");
            Logger.Info("bsthdandroidsvc state is " + serviceController.Status);
            if (serviceController.Status == ServiceControllerStatus.Stopped || serviceController.Status == ServiceControllerStatus.StopPending)
            {
                this.StartVmServiceAsync();
            }
            serviceController = new ServiceController(this.mUpdaterServiceName);
            Logger.Info(this.mUpdaterServiceName + " state is " + serviceController.Status);
            if (serviceController.Status == ServiceControllerStatus.Stopped || serviceController.Status == ServiceControllerStatus.StopPending)
            {
                this.StartUpdaterServiceAsync();
            }
            Logger.Info("Attaching VM");
            if (this.AttachVM(this.vmName))
            {
                Logger.Info("Attached");
                if (this.HideBootProgress())
                {
                    if (this.ConnectingBlankEnabled())
                    {
                        this.StateEnterConnectingBlank();
                    }
                    else
                    {
                        this.StateEnterConnecting();
                    }
                }
                else
                {
                    this.StateEnterConnected();
                }
            }
            else
            {
                Logger.Info("Attach failed");
                this.StateEnterStopped();
            }
            Thread thread = new Thread((ThreadStart)delegate
            {
                if (this.IsAppInstalled(Console.sAppPackage))
                {
                    this.RunApp(Console.sAppPackage, ".Main");
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private bool IsAppInstalled(string appPackage)
        {
            try
            {
                string url = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + VmCmdHandler.s_PingPath;
                Client.Get(url, null, false, 3000);
                Logger.Info("Guest booted. Will send request.");
                url = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + BlueStacks.hyperDroid.Common.Strings.IsPackageInstalledUrl;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("package", appPackage);
                string input = Client.Post(url, dictionary, null, false);
                JSonReader jSonReader = new JSonReader();
                IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
                string a = iJSonObject["result"].StringValue.Trim();
                if (a == "ok")
                {
                    Logger.Info("App installed");
                    return true;
                }
                Logger.Info("App not installed");
                return false;
            }
            catch (Exception)
            {
                Logger.Info("Guest not booted. Will read from apps.json");
                string text = default(string);
                if (JsonParser.IsAppInstalled(appPackage, out text))
                {
                    Logger.Info("Found in apps.json");
                    return true;
                }
                Logger.Info("Not found in apps.json");
                return false;
            }
        }

        private void DownloadApk(string appName, string apkUrl, bool runAfterInstall, int retries)
        {
            if (apkUrl == null)
            {
                SendOrPostCallback d = delegate
                {
                    if (this.mFrontendState == State.DownloadData)
                    {
                        this.StateExitDownloadData();
                    }
                };
                try
                {
                    this.mUiContext.Send(d, null);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string path = Path.Combine(folderPath, "BlueStacksSetup");
                string path2 = appName + ".apk";
                string apkFilePath = Path.Combine(path, path2);
                Logger.Info("Downloading Apk Prebundled to: " + apkFilePath);
                Thread thread = new Thread((ThreadStart)delegate
                {
                    bool mFileDownloaded = false;
                    for (int i = 1; i <= retries; i++)
                    {
                        if (mFileDownloaded)
                        {
                            break;
                        }
                        Downloader.Download(3, apkUrl, apkFilePath, delegate(int percent)
                        {
                            SendOrPostCallback d2 = delegate
                            {
                                this.loadingScreen.UpdateProgressBar(percent);
                            };
                            try
                            {
                                this.mUiContext.Send(d2, null);
                            }
                            catch (Exception)
                            {
                            }
                        }, delegate(string filePath)
                        {
                            if (apkUrl == Console.s_AmiDebugUrl)
                            {
                                this.mDownloadingAmi = false;
                                mFileDownloaded = true;
                            }
                            this.CallApkInstaller(filePath, runAfterInstall);
                        }, delegate(Exception ex)
                        {
                            Logger.Error("Failed to download Prebundled: {0}. err: {1}", apkFilePath, ex.Message);
                            if (apkUrl == Console.s_AmiDebugUrl)
                            {
                                this.mDownloadingAmi = false;
                            }
                        });
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void CallApkInstaller(string apkPath, bool runAfterInstall)
        {
            Logger.Info("Console: Installing apk: {0}", apkPath);
            Thread thread = new Thread((ThreadStart)delegate
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
                string path = (string)registryKey.GetValue("InstallDir");
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = Path.Combine(path, "HD-ApkHandler.exe");
                processStartInfo.Arguments = "-apk \"" + apkPath + "\" -s";
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;
                Logger.Info("Console: installer Data {0}", processStartInfo.FileName);
                Process process = Process.Start(processStartInfo);
                process.WaitForExit();
                Logger.Info("Console: apk installer exit code: {0}", process.ExitCode);
                if (runAfterInstall)
                {
                    this.RunApp(Console.sAppPackage, ".Main");
                }
            });
            thread.IsBackground = true;
            thread.Start();
            SendOrPostCallback d = delegate
            {
                this.StateExitDownloadData();
            };
            try
            {
                this.mUiContext.Send(d, null);
            }
            catch (Exception)
            {
            }
        }

        public void RunApp(string package, string activity)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("package", package);
            dictionary.Add("activity", activity);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMConfigRegKeyPath);
            int num = (int)registryKey.GetValue("AgentServerPort", 2861);
            string text = string.Format("http://127.0.0.1:{0}/{1}", num, "runapp");
            Logger.Info("RunApp: Sending post request to {0}", text);
            Client.PostWithRetries(text, dictionary, null, false, 10, 500);
        }

        private void UpdateDownloadProgressFromReg()
        {
            int progressPercent = 0;
            while (true)
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
                progressPercent = (int)registryKey.GetValue("DownloadProgress", 0);
                SendOrPostCallback d = delegate
                {
                    this.loadingScreen.UpdateProgressBar(progressPercent);
                };
                try
                {
                    this.mUiContext.Send(d, null);
                }
                catch (Exception)
                {
                }
                if (progressPercent != 100)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                break;
            }
            Logger.Info("Download completed 100%");
        }

        private void CheckIfAgentDone()
        {
            Logger.Info("Checking if agent done");
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
            while (true)
            {
                try
                {
                    string strA = (string)registryKey.GetValue("InstallType");
                    if (string.Compare(strA, "complete", true) == 0)
                    {
                        Logger.Info("Agent done");
                        return;
                    }
                }
                catch
                {
                }
                Thread.Sleep(1000);
            }
        }

        private void GlInitSuccess()
        {
            Logger.Info("Gl Init success");
            this.FixupGuestDisplay();
            if (!this.checkingIfBooted)
            {
                this.CheckIfGuestFinishedBooting();
            }
        }

        private void GlInitFailed()
        {
            Logger.Error("Gl Init failed");
            this.glInitFailed = true;
        }

        public void SetupGraphicsDriverVersionChecker()
        {
            Logger.Info("in SetupGraphicsDriverVersionChecker");
            Thread thread = new Thread((ThreadStart)delegate
            {
                while (!Console.sPgaInitDone)
                {
                    Thread.Sleep(100);
                }
                string text = default(string);
                if (!BlueStacks.hyperDroid.Common.Utils.IsGraphicsDriverUptodate(out Console.sDriverUpdateUrl, out text, (string)null))
                {
                    string graphicsDriverOutdatedWarning = BlueStacks.hyperDroid.Locale.Strings.GraphicsDriverOutdatedWarning;
                    string a;
                    if ((a = text) != null && !(a == "warning") && a == "ignore")
                    {
                        return;
                    }
                    if (Features.IsGraphicsDriverReminderEnabled())
                    {
                        HTTPHandler.SendSysTrayNotification("Graphics Driver Checker", "error", graphicsDriverOutdatedWarning);
                    }
                }
                Logger.Info("driverVersionChecker done");
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public void UpdateGraphicsDrivers()
        {
            Logger.Info("User chose to update graphics drivers");
            if (Console.sDriverUpdateUrl.EndsWith("zip"))
            {
                Thread thread = new Thread((ThreadStart)delegate
                {
                    Opengl.StopZygote(this.vmName);
                });
                thread.IsBackground = true;
                thread.Start();
                GraphicsDriverUpdater graphicsDriverUpdater = new GraphicsDriverUpdater(Console.s_Console);
                graphicsDriverUpdater.Update(Console.sDriverUpdateUrl);
                UIHelper.RunOnUIThread(this, delegate
                {
                    graphicsDriverUpdater.ShowDialog();
                });
            }
            else
            {
                Process.Start(Console.sDriverUpdateUrl);
                Logger.Info("Exiting frontend");
                Environment.Exit(0);
            }
        }

        public void SetupInputMapper()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
            this.mInputMapper = InputMapper.Instance();
            int num = (int)registryKey.GetValue("InputMapperVerbose", 0);
            this.mInputMapper.Init(BlueStacks.hyperDroid.Common.Strings.InputMapperFolder, num != 0, this.mSensorDevice, this.mCursor);
            this.mInputMapper.SetConsole(this);
            this.mInputMapper.SetControlHandler(new ControlHandler(this));
            int num2 = (int)registryKey.GetValue("InputMapperEmulatedSwipeLength", 20);
            int duration = (int)registryKey.GetValue("InputMapperEmulatedSwipeDuration", 100);
            this.mInputMapper.SetEmulatedSwipeKnobs((float)num2 / 100f, duration);
            int num3 = (int)registryKey.GetValue("InputMapperEmulatedPinchSplit", 20);
            int num4 = (int)registryKey.GetValue("InputMapperEmulatedPinchLengthIn", 20);
            int num5 = (int)registryKey.GetValue("InputMapperEmulatedPinchLengthOut", 10);
            int duration2 = (int)registryKey.GetValue("InputMapperEmulatedPinchDuration", 50);
            this.mInputMapper.SetEmulatedPinchKnobs((float)num3 / 100f, (float)num4 / 100f, (float)num5 / 100f, duration2);
            this.mInputMapper.SetDisplay(this.mEmulatedPortraitMode, this.mRotateGuest180);
            string text = (string)registryKey.GetValue("InputMapperLocale", "");
            if (text != "")
            {
                this.mInputMapper.OverrideLocale(text);
            }
            registryKey.Close();
        }

        private void SetupOpenSensor()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
            int num = (int)registryKey.GetValue("OpenSensorVerbose", 0);
            int beaconPort = (int)registryKey.GetValue("OpenSensorBeaconPort", 10505);
            int beaconInterval = (int)registryKey.GetValue("OpenSensorBeaconInterval", 2000);
            string deviceType = (string)registryKey.GetValue("OpenSensorDeviceType", "WindowsPC");
            this.mOpenSensor = new OpenSensor(this.mInputMapper, beaconPort, beaconInterval, deviceType, this.mSensorDevice, this.mCursor, num != 0);
            this.mOpenSensor.SetConsole(this);
            this.mOpenSensor.SetControlHandler(new ControlHandler(this));
            this.mOpenSensor.Start();
            registryKey.Close();
        }

        private void SetupSoftControlBar()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
            if ((int)registryKey.GetValue("SoftControlBarEnabled", 0) != 0)
            {
                Logger.Info("Soft Control Bar Enabled");
                this.SoftControlBarVisible(true);
            }
            registryKey.Close();
        }

        public void SoftControlBarVisible(bool visible)
        {
            float landscape = 0f;
            float portrait = 0f;
            if (visible)
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
                int num = (int)registryKey.GetValue("SoftControlBarHeightLandscape", 0);
                int num2 = (int)registryKey.GetValue("SoftControlBarHeightPortrait", 0);
                landscape = (float)num / (float)this.mConfiguredDisplaySize.Height;
                portrait = (float)num2 / (float)this.mConfiguredDisplaySize.Width;
                registryKey.Close();
            }
            this.mInputMapper.SetSoftControlBarHeight(landscape, portrait);
        }

        public void HandleControllerAttach(bool attach, int identity, string type)
        {
            UIHelper.RunOnUIThread(this, delegate
            {
                if (this.mFrontendState == State.Connected)
                {
                    Logger.Info("Sending controller attach event {0} {1} {2}", identity, attach, type);
                    if (attach)
                    {
                        this.mSensorDevice.ControllerAttach(SensorDevice.Type.Accelerometer);
                    }
                    else
                    {
                        this.mSensorDevice.ControllerDetach(SensorDevice.Type.Accelerometer);
                    }
                    this.SendControllerEvent(attach ? "attach" : "detach", identity, type);
                }
                else
                {
                    Logger.Info("Queueing controller attach event {0} {1} {2}", identity, attach, type);
                    if (attach)
                    {
                        this.mControllerMap[identity] = type;
                    }
                    else
                    {
                        this.mControllerMap.Remove(identity);
                    }
                }
            });
        }

        public void HandleControllerGuidance(bool pressed, int identity, string type)
        {
            if (this.mFrontendState == State.Connected)
            {
                this.SendControllerEvent(pressed ? "guidance_pressed" : "guidance_released", identity, type);
            }
        }

        private void SendControllerEvent(string name, int identity, string type)
        {
            string cmd = "controller_" + name + " " + identity + " " + type;
            this.SendControllerEventInternal(cmd, null);
        }

        private void SendControllerEventInternal(string cmd, UIHelper.Action continuation)
        {
            Logger.Info("Sending controller event " + cmd);
            VmCmdHandler.RunCommandAsync(cmd, continuation, this);
        }

        private void SetupVmxChecker()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
            if ((int)registryKey.GetValue("CheckVMX", 1) != 0)
            {
                string title;
                string text;
                if (BlueStacks.hyperDroid.Common.Utils.IsOEM("Lenovo"))
                {
                    title = "Closing Lenovo app Player";
                    text = BlueStacks.hyperDroid.Common.Strings.LenovoVMXError;
                }
                else
                {
                    title = "Closing BlueStacks app Player";
                    text = BlueStacks.hyperDroid.Common.Strings.VMXError;
                }
                VmxChecker vmxChecker = new VmxChecker(this, title, text);
                vmxChecker.Start();
            }
            else
            {
                Logger.Info("Skipping VMX check...");
            }
            registryKey.Close();
        }

        public void OrientationHandler(int orientation)
        {
            Logger.Info("Got orientation change notification for {0}", orientation);
            bool flag = this.ShouldEmulatePortraitMode();
            Logger.Info("ShouldEmulatePortraitMode => " + flag);
            if (flag)
            {
                this.mEmulatedPortraitMode = (orientation == 1 || orientation == 3);
                this.mRotateGuest180 = (orientation == 2 || orientation == 3);
            }
            else
            {
                this.mEmulatedPortraitMode = false;
            }
            this.mInputMapper.SetDisplay(this.mEmulatedPortraitMode, this.mRotateGuest180);
            this.mSensorDevice.SetDisplay(this.mEmulatedPortraitMode, this.mRotateGuest180);
            SendOrPostCallback d = delegate
            {
                this.ResizeFrontendWindow();
                this.FixupGuestDisplay();
                Console.OutputDebugString("SpawnApps:executeJavascript(\"orientationChanged();\")");
            };
            try
            {
                this.mUiContext.Send(d, null);
            }
            catch (Exception)
            {
            }
        }

        private void SendOrientationToGuest()
        {
            Logger.Info("Sending screen orientation to guest: {0}", SystemInformation.ScreenOrientation);
            string url = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + BlueStacks.hyperDroid.Common.Strings.HostOrientationUrl;
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("data", SystemInformation.ScreenOrientation.ToString());
            Thread thread = new Thread((ThreadStart)delegate
            {
                try
                {
                    Client.Post(url, data, null, false);
                }
                catch (Exception ex)
                {
                    Logger.Error("Cannot send orientation to guest: " + ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void InitConfig(string vmName)
        {
            string name = "Software\\BlueStacks\\Guests\\" + vmName + "\\Config";
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
            this.mControlBarEnabled = ((int)registryKey.GetValue("FEControlBar", 1) != 0);
            this.mControlBarAlwaysShow = ((int)registryKey.GetValue("ControlBarAlwaysShow", 0) != 0);
            this.grabKeyboard = ((int)registryKey.GetValue("GrabKeyboard", 1) != 0);
            this.frontendNoClose = ((int)registryKey.GetValue("FrontendNoClose", 0) != 0);
            this.stopZygoteOnClose = ((int)registryKey.GetValue("StopZygoteOnClose", 0) != 0);
            this.disableDwm = ((int)registryKey.GetValue("DisableDWM", 0) != 0);
            registryKey.Close();
            Thread thread = new Thread(this.EnableConsoleAccessThreadEntry);
            thread.IsBackground = true;
            thread.Start();
        }

        private void EnableConsoleAccessThreadEntry()
        {
            string name = "Software\\BlueStacks\\Guests\\" + this.vmName + "\\Config";
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
            while (true)
            {
                string[] valueNames = registryKey.GetValueNames();
                foreach (string text in valueNames)
                {
                    string b = "EnableConsoleAccess";
                    if (!(text != b))
                    {
                        string text2 = (string)registryKey.GetValue(text);
                        string a = this.ComputeSha1Digest(text2);
                        this.lockdownDisabled = (a == "B4003D4A30C230EB82380DE4AA9697B967FC239F");
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
                    text2 += array[i];//Modified
                }
                return text2;
            }
            catch (Exception ex)
            {
                Logger.Error("Cannot compute digest");
                Logger.Error(ex.ToString());
                return "";
            }
        }

        private void InitScreen()
        {
            Logger.Info("InitScreen()");
            string name = string.Format("{0}\\{1}\\FrameBuffer\\0", "Software\\BlueStacks\\Guests", this.vmName);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
            if (Console.sHideMode)
            {
                this.originalFullScreenState = ((int)registryKey.GetValue("FullScreen", 0) != 0);
                this.mFullScreen = false;
            }
            else
            {
                this.mFullScreen = ((int)registryKey.GetValue("FullScreen", 0) != 0);
            }
            registryKey.Close();
            this.mConfiguredDisplaySize = this.GetConfiguredDisplaySize();
            this.mConfiguredGuestSize = this.GetConfiguredGuestSize();
            this.ResizeFrontendWindow();
            Console.OutputDebugString("SpawnApps:executeJavascript(\"initScreenDone();\")");
        }

        public static void UpdateTitle(string title)
        {
            if (Features.UpdateFrontendAppTitle())
            {
                string arg = default(string);
                string text = default(string);
                string arg2 = default(string);
                JsonParser.GetAppInfoFromAppName(title, out arg, out text, out arg2);
                string text2 = Path.Combine(BlueStacks.hyperDroid.Common.Strings.LibraryDir, "Icons\\" + arg + "." + arg2 + ".ico");
                BlueStacks.hyperDroid.Common.Strings.AppTitle = title;
                Logger.Info("Setting new icon: " + text2);
                Icon icon = new Icon(text2);
                Console.s_Console.Text = title;
                Console.s_Console.Icon = icon;
                Console.BringToFront("Android");
            }
        }

        private void InitForm()
        {
            if (Features.UseBlueStacksFrontendIcon())
            {
                base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            else if (BlueStacks.hyperDroid.Common.Utils.IsOEM("wildtangent"))
            {
                string fileName = Path.Combine(Console.sInstallDir, "wildtangent.ico");
                base.Icon = new Icon(fileName);
            }
            this.Text = BlueStacks.hyperDroid.Common.Strings.AppTitle;
            base.MinimizeBox = true;
            base.MaximizeBox = false;
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;
            this.ForeColor = Color.LightGray;
        }

        private static void InitLog(string vmName)
        {
            Logger.InitLog(null, "Frontend");
            System.Console.SetOut(Logger.GetWriter());
            System.Console.SetError(Logger.GetWriter());
            AppDomain.CurrentDomain.ProcessExit += delegate
            {
                Logger.Info("Exiting frontend PID {0}", Process.GetCurrentProcess().Id);
            };
            Logger.Info("Starting frontend PID {0}", Process.GetCurrentProcess().Id);
            Logger.Info("CLR version {0}", Environment.Version);
            Logger.Info("IsAdministrator: {0}", User.IsAdministrator());
            Application.ThreadException += delegate(object obj, ThreadExceptionEventArgs evt)
            {
                Logger.Error("Unhandled Exception:");
                Logger.Error(evt.Exception.ToString());
                Environment.Exit(1);
            };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += delegate(object obj, UnhandledExceptionEventArgs evt)
            {
                Logger.Error("Unhandled Exception:");
                Logger.Error(evt.ExceptionObject.ToString());
                Environment.Exit(1);
            };
        }

        private bool AttachVM(string vmName)
        {
            if (this.manager != null)
            {
                throw new SystemException("A connection to the manager is already open");
            }
            if (this.monitor != null)
            {
                throw new SystemException("Another monitor is already attached");
            }
            if (this.video != null)
            {
                throw new SystemException("Another frame buffer is already attached");
            }
            uint num = MonitorLocator.Lookup(vmName);
            try
            {
                this.manager = BlueStacks.hyperDroid.Frontend.Interop.Manager.Open();
                this.monitor = this.manager.Attach(num, delegate
                {
                    this.guestHasStopped = true;
                });
                this.mInputMapper.SetMonitor(this.monitor);
            }
            catch (Exception ex)
            {
                if (!this.IsExceptionFileNotFound(ex))
                {
                    Logger.Error(ex.ToString());
                }
                if (this.manager != null)
                {
                    this.manager.Close();
                    this.manager = null;
                }
            }
            if (this.monitor == null)
            {
                return false;
            }
            Logger.Info("Attached to VM {0}, ID {1}", vmName, num);
            Logger.Info("Attaching to framebuffer");
            this.video = this.monitor.VideoAttach();
            this.forceVideoModeChange = true;
            this.DumpFrameBufferInfo();
            return true;
        }

        private bool IsExceptionFileNotFound(Exception exc)
        {
            Exception innerException = exc.InnerException;
            if (innerException != null && innerException.GetType() == typeof(Win32Exception))
            {
                Win32Exception ex = (Win32Exception)innerException;
                if (ex.NativeErrorCode != 2)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private void DumpFrameBufferInfo()
        {
            Video.Mode mode = this.video.GetMode();
            uint stride = this.video.GetStride();
            IntPtr bufferAddr = this.video.GetBufferAddr();
            IntPtr bufferEnd = this.video.GetBufferEnd();
            uint bufferSize = this.video.GetBufferSize();
            Logger.Info("mode    = {0}x{1}x{2}", mode.Width, mode.Height, mode.Depth);
            Logger.Info("stride  = {0}", stride);
            Logger.Info("addr    = 0x{0}", bufferAddr.ToString("x"));
            Logger.Info("end     = 0x{0}", bufferEnd.ToString("x"));
            Logger.Info("size    = 0x{0}", bufferSize.ToString("x"));
        }

        private bool HideBootProgress()
        {
            string name = string.Format("{0}\\{1}\\FrameBuffer\\0", "Software\\BlueStacks\\Guests", this.vmName);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
            bool result = (int)registryKey.GetValue("HideBootProgress", 0) != 0;
            registryKey.Close();
            return result;
        }

        private bool ConnectingBlankEnabled()
        {
            string name = string.Format("{0}\\{1}\\FrameBuffer\\0", "Software\\BlueStacks\\Guests", this.vmName);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
            bool result = (int)registryKey.GetValue("ConnectingBlankEnabled", 0) != 0;
            registryKey.Close();
            return result;
        }

        public void StateExitCurrent()
        {
            string text = "StateExit" + this.mFrontendState.ToString();
            Logger.Info("Invoking: " + text);
            MethodInfo method = base.GetType().GetMethod(text, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(this, null);
        }

        private void StateExitInitial()
        {
            Logger.Info("Exiting state Initial");
            if (this.mFrontendState == State.Initial)
            {
                return;
            }
            throw new SystemException("Illegal state " + this.mFrontendState);
        }

        private void StateEnterDownloadData(string toDownload)
        {
            Logger.Info("Entering state DownloadData");
            this.AddLoadingScreen("Progress");
            this.loadingScreen.SetStatusText(BlueStacks.hyperDroid.Locale.Strings.DownloadingGameData);
            if (toDownload == "Runtime")
            {
                Thread t = new Thread((ThreadStart)delegate
                {
                    this.UpdateDownloadProgressFromReg();
                });
                t.IsBackground = true;
                t.Start();
                Thread thread = new Thread((ThreadStart)delegate
                {
                    this.CheckIfAgentDone();
                    t.Abort();
                    if (!BlueStacks.hyperDroid.Common.Utils.IsOEM("360") && BlueStacks.hyperDroid.Common.Utils.IsP2DMEnabled())
                    {
                        this.mDownloadingAmi = true;
                        this.DownloadApk(Console.s_AmiDebug, Console.s_AmiDebugUrl, false, 3);
                    }
                    while (this.mDownloadingAmi)
                    {
                        Thread.Sleep(500);
                    }
                    this.DownloadApk(Console.sAppName, Console.sApkUrl, true, 1);
                });
                thread.IsBackground = true;
                thread.Start();
            }
            else if (toDownload == "App")
            {
                this.DownloadApk(Console.sAppName, Console.sApkUrl, true, 1);
            }
            this.mFrontendState = State.DownloadData;
        }

        private void StateExitDownloadData()
        {
            Logger.Info("Exiting state DownloadData");
            this.ClearControls();
            if (this.mFrontendState != State.DownloadData)
            {
                throw new SystemException("Illegal state " + this.mFrontendState);
            }
            this.ContinueStateMachine();
        }

        private void StateEnterStopped()
        {
            Logger.Info("Entering state Stopped");
            if (this.mFrontendState != 0 && this.mFrontendState != State.DownloadData)
            {
                Logger.Info("Frontend unexpectedly reaached the stopped state. Exiting.");
                Environment.Exit(1);
            }
            this.ClearControls();
            int num = this.mScaledDisplayArea.Width / 2;
            int num2 = this.mScaledDisplayArea.Height / 2;
            this.mInputMapper.SetMonitor(null);
            if (this.monitor != null)
            {
                this.monitor.Close();
                this.monitor = null;
            }
            if (this.manager != null)
            {
                this.manager.Close();
                this.manager = null;
            }
            this.video = null;
            this.cannotStartVm = false;
            this.guestFinishedBooting = false;
            this.guestHasStopped = false;
            this.timer = new System.Windows.Forms.Timer();
            this.timer.Interval = 1000;
            this.timer.Tick += delegate
            {
                Logger.Info("sHideMode = " + Console.sHideMode);
                if (Console.sHideMode)
                {
                    Console.sHideMode = false;
                }
                if (this.AttachVM(this.vmName))
                {
                    this.StateExitStopped();
                    if (this.HideBootProgress())
                    {
                        this.StateEnterConnecting();
                    }
                    else
                    {
                        this.StateEnterConnected();
                    }
                }
            };
            this.timer.Start();
            this.mFrontendState = State.Stopped;
            if (Console.sNormalMode)
            {
                try
                {
                    Process.Start(Console.sInstallDir + "HD-Agent.exe");
                }
                catch (Exception)
                {
                }
                if (!this.userInteracted)
                {
                    base.Paint += this.HandlePaintEvent;
                    this.userInteracted = true;
                }
                ServiceController serviceController = new ServiceController("bsthdandroidsvc");
                Logger.Info("bsthdandroidsvc state is " + serviceController.Status);
                if (serviceController.Status == ServiceControllerStatus.Stopped || serviceController.Status == ServiceControllerStatus.StopPending)
                {
                    Logger.Info("Starting Service in normal mode");
                    this.StartVmServiceAsync();
                }
                this.StateExitStopped();
                this.StateEnterStarting();
            }
        }

        private void StateExitStopped()
        {
            Logger.Info("Exiting state Stopped");
            if (this.mFrontendState != State.Stopped)
            {
                throw new SystemException("Illegal state " + this.mFrontendState);
            }
            this.timer.Stop();
            this.timer.Dispose();
            this.timer = null;
            this.ClearControls();
        }

        private void ClearControls()
        {
            Logger.Info("Clearing controls");
            for (int num = base.Controls.Count - 1; num >= 0; num--)
            {
                Control control = base.Controls[num];
                if (this.atLoadingScreen && control == this.loadingScreen)
                {
                    Logger.Info("Not clearing " + control.ToString());
                }
                else
                {
                    base.Controls.Remove(control);
                    control = null;
                }
            }
        }

        private void RemoveLoadingScreen()
        {
            if (this.loadingScreen != null)
            {
                base.Controls.Remove(this.loadingScreen);
                this.loadingScreen.Dispose();
                this.loadingScreen = null;
            }
        }

        private void AddLoadingScreen(string barType)
        {
            Logger.Info("AddLoadingScreen: " + barType);
            if (base.Controls.Contains(this.loadingScreen))
            {
                Logger.Info("Already added");
            }
            else
            {
                Size loadingScreenSize = default(Size);
                if (this.mControlBarVisible)
                {
                    loadingScreenSize.Height = base.ClientSize.Height - 48;
                }
                else
                {
                    loadingScreenSize.Height = base.ClientSize.Height;
                }
                loadingScreenSize.Width = base.ClientSize.Width;
                if (BlueStacks.hyperDroid.Common.Utils.IsOEM("ucweb"))
                {
                    this.loadingScreen = new LoadingScreen(loadingScreenSize, Console.sAppIconImage, Console.sAppName, barType, null);
                }
                else
                {
                    this.loadingScreen = new LoadingScreen(loadingScreenSize, Console.sAppIconImage, Console.sAppName, barType, this.ToggleFullScreen);
                }
                base.SuspendLayout();
                base.Controls.Add(this.loadingScreen);
                if (this.mFrontendState == State.DownloadData)
                {
                    this.loadingScreen.SetStatusText(BlueStacks.hyperDroid.Locale.Strings.DownloadingGameData);
                }
                else if (Console.sAppName == "")
                {
                    this.loadingScreen.SetStatusText(BlueStacks.hyperDroid.Locale.Strings.Initializing);
                }
                else
                {
                    this.loadingScreen.SetStatusText(BlueStacks.hyperDroid.Locale.Strings.InitializingGame);
                }
                base.ResumeLayout();
            }
        }

        private void StateEnterStarting()
        {
            Logger.Info("Entering state Starting");
            this.AddLoadingScreen("Marquee");
            if (this.mControlBarEnabled)
            {
                this.UpdateControlBar();
            }
            this.timer = new System.Windows.Forms.Timer();
            this.timer.Interval = 1000;
            this.timer.Tick += delegate
            {
                if (this.cannotStartVm || this.glInitFailed)
                {
                    this.StateExitStarting();
                    this.StateEnterCannotStart();
                }
                else if (this.AttachVM(this.vmName))
                {
                    if (this.HideBootProgress())
                    {
                        this.atLoadingScreen = true;
                    }
                    this.StateExitStarting();
                    if (this.HideBootProgress())
                    {
                        this.StateEnterConnecting();
                    }
                    else
                    {
                        this.StateEnterConnected();
                    }
                }
            };
            this.timer.Start();
            this.mFrontendState = State.Starting;
        }

        private void StateExitStarting()
        {
            Logger.Info("Exiting state Starting");
            if (this.mFrontendState != State.Starting)
            {
                throw new SystemException("Illegal state " + this.mFrontendState);
            }
            this.timer.Stop();
            this.timer.Dispose();
            this.timer = null;
            this.ClearControls();
        }

        private void StateEnterCannotStart()
        {
            Logger.Info("Entering state CannotStart");
            int num = this.mScaledDisplayArea.Width / 2;
            int num2 = this.mScaledDisplayArea.Height / 2;
            Image original = new Bitmap(Console.sInstallDir + "ProductLogo.png");
            original = new Bitmap(original, new Size(128, 128));
            Label label = new Label();
            label.BackgroundImage = original;
            label.Width = label.BackgroundImage.Width;
            label.Height = label.BackgroundImage.Height;
            label.Location = new Point(num - label.Width / 2, num2 - label.Height + 70);
            Label label2 = new StatusMessage();
            label2.Text = BlueStacks.hyperDroid.Locale.Strings.CanNotStart;
            label2.TextAlign = ContentAlignment.MiddleCenter;
            label2.Width = this.mScaledDisplayArea.Width;
            label2.Location = new Point(0, num2 + 90);
            Button button = new FrontendButton();
            button.Text = "Exit";
            button.Location = new Point(num - button.Width / 2, num2 + 180);
            button.Click += delegate
            {
                base.Close();
            };
            base.Controls.Add(label);
            base.Controls.Add(label2);
            base.Controls.Add(button);
            this.mFrontendState = State.CannotStart;
        }

        private void StateEnterConnectingBlank()
        {
            Logger.Info("Entering state ConnectingBlank");
            int retryCount = 10;
            this.timer = new System.Windows.Forms.Timer();
            this.timer.Interval = 100;
            this.timer.Tick += delegate
            {
                Logger.Info("sHideMode = " + Console.sHideMode);
                if (Console.sHideMode)
                {
                    Console.sHideMode = false;
                }
                if (this.guestFinishedBooting)
                {
                    this.StateExitConnectingBlank();
                    this.StateEnterConnected();
                }
                if (--retryCount == 0)
                {
                    this.StateExitConnectingBlank();
                    this.StateEnterConnecting();
                }
            };
            this.guestFinishedBooting = false;
            this.timer.Start();
            this.mFrontendState = State.ConnectingBlank;
        }

        private void StateExitConnectingBlank()
        {
            Logger.Info("Exiting state ConnectingBlank");
            if (this.mFrontendState != State.ConnectingBlank)
            {
                throw new SystemException("Illegal state " + this.mFrontendState);
            }
            this.timer.Stop();
            this.timer.Dispose();
            this.timer = null;
            this.ClearControls();
        }

        private void StateEnterConnecting()
        {
            Logger.Info("Entering state Connecting");
            this.AddLoadingScreen("Marquee");
            if (this.mControlBarEnabled)
            {
                this.UpdateControlBar();
            }
            this.timer = new System.Windows.Forms.Timer();
            this.timer.Interval = 250;
            this.timer.Tick += delegate
            {
                if (this.glInitFailed)
                {
                    this.StateExitConnecting();
                    this.StateEnterCannotStart();
                }
                else if (this.guestHasStopped)
                {
                    this.StateExitConnecting();
                    this.StateEnterStopped();
                }
                else if (this.guestFinishedBooting)
                {
                    this.StateExitConnecting();
                    Stats.SendBootStats("frontend", true, false);
                    Stats.SendVirtStats();
                    this.StateEnterConnected();
                }
            };
            this.guestFinishedBooting = false;
            this.timer.Start();
            this.UnstickKeyboardModifiers();
            this.mFrontendState = State.Connecting;
        }

        private void StateExitConnecting()
        {
            Logger.Info("Exiting state Connecting");
            if (this.mFrontendState != State.Connecting)
            {
                throw new SystemException("Illegal state " + this.mFrontendState);
            }
            this.timer.Stop();
            this.timer.Dispose();
            this.timer = null;
            this.atLoadingScreen = false;
            this.ClearControls();
        }

        private void StateEnterConnected()
        {
            Logger.Info("Entering state Connected");
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMConfigRegKeyPath);
            if ((int)registryKey.GetValue("ConfigSynced", 0) == 0)
            {
                Logger.Info("Config not synced. Syncing now.");
                Thread thread = new Thread((ThreadStart)delegate
                {
                    VmCmdHandler.SyncConfig();
                });
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                Logger.Info("Config already synced.");
            }
            Thread thread2 = new Thread((ThreadStart)delegate
            {
                Logger.Info("Started fqdnSender thread");
                VmCmdHandler.FqdnSend(0, "Agent");
                Logger.Info("fqdnSender thread exiting");
            });
            thread2.IsBackground = true;
            thread2.Start();
            this.RemoveLoadingScreen();
            Logger.Debug("Raising Layout event");
            this.OnLayout(new LayoutEventArgs(this, ""));
            this.SendOrientationToGuest();
            Thread thread3 = new Thread((ThreadStart)delegate
            {
                VmCmdHandler.SetKeyboard(Console.IsDesktop());
            });
            thread3.IsBackground = true;
            thread3.Start();
            if (Console.sNormalMode)
            {
                this.userInteracted = true;
            }
            if (!Opengl.IsSubWindowVisible())
            {
                Logger.Info("showing window");
                this.glWindowAction = GlWindowAction.Show;
                this.userInteracted = false;
            }
            if (this.mControlBarEnabled)
            {
                this.UpdateControlBar();
            }
            this.FixupGuestDisplay();
            this.timer = new System.Windows.Forms.Timer();
            this.timer.Interval = 33;
            this.timer.Tick += delegate
            {
                if (this.guestHasStopped)
                {
                    this.StateExitConnected();
                    this.StateEnterStopped();
                }
                else
                {
                    this.HandleDisplayTimeout();
                }
            };
            this.guestHasStopped = false;
            this.timer.Start();
            this.AudioAttach();
            this.GpsAttach();
            this.mSensorDevice.Start(this.vmName);
            this.CameraAttach();
            base.Activated += this.HandleActivatedEvent;
            base.Deactivate += this.HandleDeactivateEvent;
            base.MouseMove += this.HandleMouseMove;
            base.MouseDown += this.HandleMouseDown;
            base.MouseUp += this.HandleMouseUp;
            base.MouseWheel += this.HandleMouseWheel;
            SystemEvents.PowerModeChanged += this.SystemEvents_PowerModeChanged;
            base.TouchEvent += this.HandleTouchEvent;
            base.KeyDown += this.HandleKeyDown;
            base.KeyUp += this.HandleKeyUp;
            if (!this.checkingIfBooted)
            {
                this.CheckIfGuestFinishedBooting();
            }
            Logger.Info("sHideMode = " + Console.sHideMode);
            if (Console.sHideMode)
            {
                Console.sHideMode = false;
            }
            this.SendControllerEventInternal("controller_flush", delegate
            {
                foreach (int key in this.mControllerMap.Keys)
                {
                    this.mSensorDevice.ControllerAttach(SensorDevice.Type.Accelerometer);
                    this.SendControllerEvent("attach", key, this.mControllerMap[key]);
                }
                this.mControllerMap.Clear();
            });
            this.mFrontendState = State.Connected;
            Console.OutputDebugString("SpawnApps:executeJavascript(\"frontendFinishedBooting();\")");
            Thread thread4 = new Thread((ThreadStart)delegate
            {
                Logger.Info("Checking for Black Screen Error");
                while (this.CheckBlackScreen())
                {
                    Thread.Sleep(1000);
                }
                Logger.Info("Frontend launched Successfully");
                Stats.SendHomeScreenDisplayedStats();
            });
            thread4.IsBackground = true;
            thread4.Start();
        }

        public bool CheckBlackScreen()
        {
            try
            {
                Logger.Debug("Inside CheckBlackScreen");
                if (!Console.sFrontendActive)
                {
                    Logger.Debug("Frontend Inactive");
                    return true;
                }
                Logger.Info("Frontend active");
                Bitmap bitmap = new Bitmap(base.ClientSize.Width, base.ClientSize.Height);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(new Point(base.Left, base.Top), Point.Empty, new Size(base.ClientSize.Width, base.ClientSize.Height));
                for (int i = base.Width / 4; i < base.Width * 3 / 4; i++)
                {
                    int num = base.Height / 4;
                    while (num < base.Height * 3 / 4)
                    {
                        Color pixel = bitmap.GetPixel(i, num);
                        if (pixel.A == Color.Black.A && pixel.R == Color.Black.R && pixel.G == Color.Black.G && pixel.B == Color.Black.B)
                        {
                            num++;
                            continue;
                        }
                        Logger.Info("Pixel " + i + "," + num + " is not black");
                        return false;
                    }
                }
                Logger.Error("Black Screen Detected");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error Occured, Err: " + ex.ToString());
                return false;
            }
        }

        public void CheckIfGuestFinishedBooting()
        {
            Logger.Info("in CheckIfGuestFinishedBooting");
            Thread thread = new Thread((ThreadStart)delegate
            {
                this.checkingIfBooted = true;
                int num = 90;
                while (num > 0)
                {
                    num--;
                    try
                    {
                        string url = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + VmCmdHandler.s_PingPath;
                        Client.Get(url, null, false, 1000);
                        Logger.Info("Guest finished booting");
                        Thread.Sleep(2000);
                        this.SignalReady(this.vmName);
                        if (!Console.sAppLaunch)
                        {
                            this.guestFinishedBooting = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Guest not booted yet. err: " + ex.Message);
                        if (num == 0)
                        {
                            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMConfigRegKeyPath);
                            if (registryKey.GetValue("EnableConsoleAccess") != null)
                            {
                                Logger.Info("EnableConsoleAccess present. Not Aborting.");
                                num = 150;
                            }
                            else
                            {
                                Logger.Error("Android could not be started. Collecting dump state.");
                                num = 150;
                                Stats.SendBootStats("frontend", false, true);
                                Thread thread2 = new Thread((ThreadStart)delegate
                                {
                                    this.CollectDumpState("NoLoad");
                                });
                                thread2.IsBackground = true;
                                thread2.Start();
                                if (!thread2.Join(15000))
                                {
                                    thread2.Abort();
                                }
                            }
                        }
                        Thread.Sleep(1000);
                        continue;
                    }
                    break;
                }
                this.checkingIfBooted = false;
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void SignalReady(string vmName)
        {
            string name = "Global\\BlueStacks_Frontend_Ready_" + vmName;
            bool flag = default(bool);
            this.glReadyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, name, out flag);
            Logger.Info("Event created: " + flag);
            bool flag2 = this.glReadyEvent.Set();
            Logger.Info("Event set: " + flag2);
        }

        private void CollectDumpState(string state)
        {
            string path = Path.Combine(BlueStacks.hyperDroid.Common.Strings.BstCommonAppData, "Logs");
            string path2 = "Frontend-" + state + "-Android-DumpState.log";
            string prog = Path.Combine(Console.sInstallDir, "HD-Adb.exe");
            BlueStacks.hyperDroid.Common.Utils.RunCmdAsync(prog, "start-server");
            Thread.Sleep(5000);
            BlueStacks.hyperDroid.Common.Utils.RunCmd(prog, "connect localhost:5555", null);
            BlueStacks.hyperDroid.Common.Utils.RunCmdNoLog(prog, "-s localhost:5555 shell dumpstate", Path.Combine(path, path2));
        }

        private void StateExitConnected()
        {
            Logger.Info("Exiting state Connected");
            this.glWindowAction = GlWindowAction.Hide;
            if (this.mFrontendState != State.Connected)
            {
                throw new SystemException("Illegal state " + this.mFrontendState);
            }
            this.AudioDetach();
            this.GpsDetach();
            this.mSensorDevice.Stop();
            this.CameraDetach();
            base.FormClosing -= this.HandleCloseEvent;
            base.Activated -= this.HandleActivatedEvent;
            base.Deactivate -= this.HandleDeactivateEvent;
            base.LostFocus -= this.HandleLostFocusEvent;
            base.Paint -= this.HandlePaintEvent;
            base.MouseMove -= this.HandleMouseMove;
            base.MouseDown -= this.HandleMouseDown;
            base.MouseUp -= this.HandleMouseUp;
            base.MouseWheel -= this.HandleMouseWheel;
            base.TouchEvent -= this.HandleTouchEvent;
            base.KeyDown -= this.HandleKeyDown;
            base.KeyUp -= this.HandleKeyUp;
            this.timer.Stop();
            this.timer.Dispose();
            this.timer = null;
            this.monitor.Close();
            this.monitor = null;
            this.video = null;
            this.manager.Close();
            this.manager = null;
            this.ClearControls();
            base.Invalidate();
        }

        private void StartVmServiceAsync()
        {
            if (!Console.sDontStartVm)
            {
                Thread thread = new Thread((ThreadStart)delegate
                {
                    lock (this.lockObject)
                    {
                        Logger.Info("Starting VM service for {0}", this.vmName);
                        try
                        {
                            this.StartVmService();
                        }
                        catch (InvalidOperationException ex)
                        {
                            Logger.Error("Caught InvalidOperationException");
                            Logger.Error(ex.ToString());
                            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show(BlueStacks.hyperDroid.Common.Strings.SystemUpgradedError, "BlueStacks Installer", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                            if (dialogResult == DialogResult.OK)
                            {
                                Process process = new Process();
                                process.StartInfo.UseShellExecute = true;
                                process.StartInfo.CreateNoWindow = true;
                                process.StartInfo.FileName = Path.Combine(Console.sInstallDir, "HD-CreateSymlink.exe");
                                process.StartInfo.Arguments = "BlueStacks";
                                if (!BlueStacks.hyperDroid.Common.Utils.IsOSWinXP())
                                {
                                    process.StartInfo.Verb = "runas";
                                }
                                Logger.Info("Starting {0} with args {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
                                process.Start();
                                process.WaitForExit();
                                Logger.Info("HD-CreateSymlink done");
                                this.StartVmServiceAsync();
                            }
                            else
                            {
                                Logger.Info("User chose not to fix installation. Exiting.");
                                Environment.Exit(-1);
                            }
                        }
                        catch (Exception ex2)
                        {
                            Logger.Error("Cannot start VM service for {0}", this.vmName);
                            Logger.Error(ex2.ToString());
                            this.cannotStartVm = true;
                        }
                    }
                });
                this.cannotStartVm = false;
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void StartUpdaterServiceAsync()
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                Logger.Info("Starting Updater service");
                try
                {
                    this.StartUpdaterService();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to start updater service");
                    Logger.Error(ex.ToString());
                }
            });
            this.cannotStartVm = false;
            thread.IsBackground = true;
            thread.Start();
        }

        private void StartVmService()
        {
            string serviceName = "BstHd" + this.vmName + "Svc";
            this.StartService(serviceName);
        }

        private void StartUpdaterService()
        {
            this.StartService(this.mUpdaterServiceName);
        }

        private void StartService(string serviceName)
        {
            ServiceController serviceController = new ServiceController(serviceName);
            if (serviceController.Status == ServiceControllerStatus.StopPending)
            {
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            try
            {
                BlueStacks.hyperDroid.Common.Utils.EnableService(serviceName);
                serviceController.Start();
            }
            catch (Exception ex)
            {
                serviceController.Refresh();
                if (serviceController.Status != ServiceControllerStatus.Running && serviceController.Status != ServiceControllerStatus.StartPending)
                {
                    Logger.Error("Failed to start {0}", serviceName);
                    Logger.Error("{0} status = {1}", serviceName, serviceController.Status);
                    Logger.Error(ex.ToString());
                    throw ex;
                }
                Logger.Info("{0} is already running", serviceName);
            }
        }

        private void StopService(string serviceName)
        {
            ServiceController serviceController = new ServiceController(serviceName);
            if (serviceController.Status == ServiceControllerStatus.StartPending)
            {
                serviceController.WaitForStatus(ServiceControllerStatus.Running);
            }
            try
            {
                serviceController.Stop();
            }
            catch (Exception ex)
            {
                serviceController.Refresh();
                if (serviceController.Status != ServiceControllerStatus.Stopped && serviceController.Status != ServiceControllerStatus.StopPending)
                {
                    Logger.Error("Failed to stop {0}", serviceName);
                    Logger.Error("{0} status = {1}", serviceName, serviceController.Status);
                    Logger.Error(ex.ToString());
                }
                else
                {
                    Logger.Info("{0} is already stopped", serviceName);
                }
            }
        }

        private void AudioAttach()
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                Logger.Info("AudioAttach");
                try
                {
                    BlueStacks.hyperDroid.Audio.Manager.Monitor = this.monitor;
                    BlueStacks.hyperDroid.Audio.Manager.Main(new string[1]
					{
						this.vmName
					});
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.Priority = ThreadPriority.Highest;
            thread.IsBackground = true;
            thread.Start();
        }

        private void AudioDetach()
        {
            Logger.Info("AudioDetach");
            BlueStacks.hyperDroid.Audio.Manager.Shutdown();
        }

        private void GpsAttach()
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                Logger.Info("GpsAttach");
                try
                {
                    BlueStacks.hyperDroid.Gps.Manager.Main(new string[1]
					{
						this.vmName
					});
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void GpsDetach()
        {
            Logger.Info("GpsDetach");
            BlueStacks.hyperDroid.Gps.Manager.Shutdown();
        }

        private void CameraAttach()
        {
            if (this.camManager != null)
            {
                Logger.Info("cam Manager is already attached");
            }
            else
            {
                this.camManager = new BlueStacks.hyperDroid.VideoCapture.Manager();
                Logger.Info("CameraAttach");
                try
                {
                    BlueStacks.hyperDroid.VideoCapture.Manager.Monitor = this.monitor;
                    this.camManager.InitCamera(new string[1]
					{
						this.vmName
					});
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            }
        }

        private void CameraDetach()
        {
            if (this.camManager == null)
            {
                Logger.Info("Cannot detach camera, which is not yet attached");
            }
            else
            {
                Logger.Info("CameraDetach");
                this.camManager.Shutdown();
                this.camManager = null;
            }
        }

        private void HandleDisplayTimeout()
        {
            if (this.glWindowAction == GlWindowAction.Show)
            {
                if (Opengl.ShowSubWindow())
                {
                    this.glWindowAction = GlWindowAction.None;
                }
            }
            else if (this.glWindowAction == GlWindowAction.Hide && Opengl.HideSubWindow())
            {
                this.glWindowAction = GlWindowAction.None;
            }
            Video.Mode mode = this.video.GetMode();
            if (mode.Width != 0 && mode.Height != 0)
            {
                bool flag = mode.Width != this.mConfiguredGuestSize.Width || mode.Height != this.mConfiguredGuestSize.Height || this.forceVideoModeChange;
                if (flag)
                {
                    Logger.Info("mode changed to ({0},{1})", mode.Width, mode.Height);
                }
                this.forceVideoModeChange = false;
                if (flag)
                {
                    this.mConfiguredGuestSize.Width = mode.Width;
                    this.mConfiguredGuestSize.Height = mode.Height;
                }
                if (!this.video.GetAndClearDirty() && !flag)
                {
                    return;
                }
                base.Invalidate();
                base.Update();
            }
        }

        private void HandleLayoutEvent(object o, EventArgs e)
        {
            Logger.Info("HandleLayoutEvent()");
            Logger.Info("New client size is {0}x{1}", base.ClientSize.Width, base.ClientSize.Height);
            if (base.ClientSize.Width != 0 && base.ClientSize.Height != 0)
            {
                this.mCurrentDisplaySize.Width = base.ClientSize.Width;
                if (this.mControlBarVisible)
                {
                    this.mCurrentDisplaySize.Height = base.ClientSize.Height - 48;
                }
                else
                {
                    this.mCurrentDisplaySize.Height = base.ClientSize.Height;
                }
            }
            this.UpdateControlBar();
            this.CheckUserActive();
            if (base.WindowState == FormWindowState.Minimized)
            {
                this.frontendMinimized = true;
                this.isMinimized = true;
            }
            else
            {
                this.FixupGuestDisplay();
                this.FrontendActivated();
                this.isMinimized = false;
            }
            if (base.Controls.Contains(this.loadingScreen))
            {
                if (this.mFrontendState == State.Starting || this.mFrontendState == State.Connecting)
                {
                    this.RemoveLoadingScreen();
                    this.AddLoadingScreen("Marquee");
                }
                else if (this.mFrontendState == State.DownloadData)
                {
                    this.RemoveLoadingScreen();
                    this.AddLoadingScreen("Progress");
                }
            }
            if (Console.sHideMode)
            {
                Logger.Info("Hiding window");
                base.Hide();
            }
        }

        private void HandleLostFocusEvent(object sender, EventArgs e)
        {
            this.mUsageTime.Update();
        }

        private void HandleCloseEvent(object o, CancelEventArgs e)
        {
            Logger.Info("HandleCloseEvent sessionEnding = " + this.sessionEnding);
            Stats.SendUserActiveStats("false");
            if (BlueStacks.hyperDroid.Common.Utils.IsOEM("bigfish"))
            {
                string name = "Big Fish";
                IntPtr zero = IntPtr.Zero;
                try
                {
                    zero = Window.BringWindowToFront(name, false);
                    Logger.Info("Got big fish handle: " + zero);
                }
                catch (Exception ex)
                {
                    Logger.Error("Cannot bring existing big fish window to the foreground");
                    Logger.Error(ex.ToString());
                }
            }
            if (BlueStacks.hyperDroid.Common.Utils.IsOEM("360"))
            {
                BlueStacks.hyperDroid.Common.Utils.StopServiceNoWait("bsthdandroidsvc");
            }
            BlueStacks.hyperDroid.Common.Utils.KillProcessByName("HD-RunApp");
            this.mUsageTime.Update();
            if (this.sessionEnding)
            {
                Environment.Exit(1);
            }
            BlueStacks.hyperDroid.Audio.Manager.Mute();
            if (this.camManager != null)
            {
                this.camManager.pauseCamera();
            }
            base.Paint -= this.HandlePaintEvent;
            if (this.frontendNoClose)
            {
                Animate.AnimateWindow(base.Handle, 500, 589824);
                base.Hide();
                e.Cancel = true;
            }
            else if (this.stopZygoteOnClose)
            {
                Logger.Info("Stopping Zygote");
                try
                {
                    Command command = new Command();
                    command.Attach(this.vmName);
                    int num = command.Run(new string[1]
					{
						"/system/bin/stop"
					});
                    if (num != 0)
                    {
                        Logger.Error("Cannot stop Zygote: " + num);
                    }
                }
                catch (Exception ex2)
                {
                    Logger.Error("Cannot run command to stop Zygote: " + ex2.ToString());
                }
            }
            if (!Console.sHideMode)
            {
                this.firstActivated = true;
            }
        }

        private void HandleActivatedEvent(object o, EventArgs e)
        {
            Logger.Info("HandleActivatedEvent");
            Console.sFrontendActive = true;
            this.mLastActivityEndedSendMsgTime = DateTime.Now;
            Stats.SendUserActiveStats("true");
            this.FrontendActivated();
            this.mCursor.RaiseFocusChange();
        }

        private void HandleDeactivateEvent(object o, EventArgs e)
        {
            Logger.Info("HandleDeactivateEvent");
            Console.sFrontendActive = false;
            this.mLastActivityEndedSendMsgTime = DateTime.Now;
            Stats.SendUserActiveStats("false");
            if (this.mFullScreen)
            {
                this.mFullScreenToast.Hide();
            }
            this.mCursor.RaiseFocusChange();
        }

        private void FrontendActivated()
        {
            if (this.firstActivated && !this.frontendMinimized)
            {
                this.firstActivated = false;
            }
            if (this.frontendMinimized)
            {
                base.Paint += this.HandlePaintEvent;
                this.frontendMinimized = false;
                this.firstActivated = false;
            }
            BlueStacks.hyperDroid.Audio.Manager.Unmute();
            if (this.camManager != null)
            {
                this.camManager.resumeCamera();
            }
            this.mUsageTime = new UsageTime();
            if (this.keyboard != null && this.monitor != null)
            {
                this.UnstickKeyboardModifiers();
            }
        }

        private void HandlePaintEvent(object obj, PaintEventArgs evt)
        {
            Rectangle clipRectangle = evt.ClipRectangle;
            Graphics graphics = evt.Graphics;
            this.CheckUserActive();
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMConfigRegKeyPath);
            if (this.mFrontendState != State.Connected && registryKey.GetValue("EnableConsoleAccess") == null)
            {
                return;
            }
            if (this.video != null)
            {
                Video.Mode mode = this.video.GetMode();
                if (mode.Width != 0 && mode.Height != 0)
                {
                    PixelFormat format = (PixelFormat)((mode.Depth != 24) ? ((mode.Depth == 16) ? 135174 : 0) : 137224);
                    if (!Opengl.DrawFB(mode.Width, mode.Height, this.video.GetBufferAddr(), this.lockdownDisabled))
                    {
                        Bitmap image = new Bitmap(mode.Width, mode.Height, (int)this.video.GetStride(), format, this.video.GetBufferAddr());
                        if (!this.IsPortrait())
                        {
                            Point[] destPoints = new Point[3]
							{
								new Point(this.mScaledDisplayArea.X, this.mScaledDisplayArea.Y),
								new Point(this.mScaledDisplayArea.X + this.mScaledDisplayArea.Width, this.mScaledDisplayArea.Y),
								new Point(this.mScaledDisplayArea.X, this.mScaledDisplayArea.Y + this.mScaledDisplayArea.Height)
							};
                            graphics.DrawImage(image, destPoints);
                        }
                        else
                        {
                            Point[] destPoints2 = new Point[3]
							{
								new Point(this.mScaledDisplayArea.X, this.mScaledDisplayArea.Y + this.mScaledDisplayArea.Height),
								new Point(this.mScaledDisplayArea.X, this.mScaledDisplayArea.Y),
								new Point(this.mScaledDisplayArea.X + this.mScaledDisplayArea.Width, this.mScaledDisplayArea.Y + this.mScaledDisplayArea.Height)
							};
                            graphics.DrawImage(image, destPoints2);
                        }
                    }
                }
            }
        }

        private void HandleMouseMove(object obj, MouseEventArgs evt)
        {
            int x = evt.X;
            int y = evt.Y;
            this.CheckUserActive();
            if (!Input.IsEventFromTouch() && this.monitor != null)
            {
                this.mouse.UpdateCursor((uint)this.GetGuestX(x, y), (uint)this.GetGuestY(x, y));
                this.monitor.SendMouseState(this.mouse.X, this.mouse.Y, this.mouse.Mask);
                if (Console.s_KeyMapTeachMode)
                {
                    double value = 100.0 * (double)this.GetGuestX(x, y) / 32768.0;
                    double value2 = 100.0 * (double)this.GetGuestY(x, y) / 32768.0;
                    string text = "  [ x=" + Math.Round(value, 2) + "%, y=" + Math.Round(value2, 2) + "% - " + this.mCurrentAppPackage + "]";
                    Console.s_KeyMapToolTip.Show(text, this, x, y, 100000);
                }
                else
                {
                    Console.s_KeyMapToolTip.Hide(this);
                }
            }
        }

        private void HandleMouseDown(object obj, MouseEventArgs evt)
        {
            this.CheckUserActive();
            if (evt.Button == MouseButtons.Left)
            {
                Logger.Debug("left button");
                Logger.Debug("{0},{1}", evt.X, evt.Y);
                Logger.Debug("{0},{1}", base.ClientSize.Width, base.ClientSize.Height);
            }
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control && evt.Button == MouseButtons.Left)
            {
                this.mInputMapper.EmulatePinch((float)evt.X / (float)base.ClientSize.Width, (float)evt.Y / (float)base.ClientSize.Height, false);
            }
            else if ((Control.ModifierKeys & Keys.Control) == Keys.Control && evt.Button == MouseButtons.Right)
            {
                this.mInputMapper.EmulatePinch((float)evt.X / (float)base.ClientSize.Width, (float)evt.Y / (float)base.ClientSize.Height, true);
            }
            else
            {
                this.HandleMouseButton(evt.X, evt.Y, evt.Button, true);
            }
        }

        private void HandleMouseUp(object obj, MouseEventArgs evt)
        {
            this.CheckUserActive();
            this.HandleMouseButton(evt.X, evt.Y, evt.Button, false);
        }

        private void HandleMouseButton(int x, int y, MouseButtons button, bool pressed)
        {
            if (!Input.IsEventFromTouch() && this.monitor != null)
            {
                this.mouse.UpdateButton((uint)this.GetGuestX(x, y), (uint)this.GetGuestY(x, y), button, pressed);
                this.monitor.SendMouseState(this.mouse.X, this.mouse.Y, this.mouse.Mask);
            }
        }

        private void HandleMouseWheel(object obj, MouseEventArgs evt)
        {
            this.CheckUserActive();
            if (!Input.IsEventFromTouch())
            {
                float x = (float)this.GetGuestX(evt.X, evt.Y) / 32768f;
                float y = (float)this.GetGuestY(evt.X, evt.Y) / 32768f;
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    this.mInputMapper.EmulatePinch(x, y, evt.Delta > 0);
                }
                else
                {
                    InputMapper.Direction direction = (InputMapper.Direction)((evt.Delta < 0) ? 1 : 2);
                    this.mInputMapper.EmulateSwipe(x, y, direction);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            bool flag = false;
            switch (m.Msg)
            {
                case 1025:
                    Logger.Info("Received message WM_USER_SHOW_WINDOW");
                    this.HandleWMUserShowWindow();
                    flag = true;
                    break;
                case 1026:
                    Logger.Info("Received message WM_USER_SWITCH_TO_LAUNCHER");
                    break;
                case 17:
                    Logger.Info("Received message WM_QUERYENDSESSION");
                    this.sessionEnding = true;
                    break;
                case 274:
                    {
                        Logger.Info("Received message WM_SYSCOMMAND");
                        int command = m.WParam.ToInt32();
                        if (this.HandleWMSysCommand(command))
                        {
                            break;
                        }
                        return;
                    }
                case 1027:
                    {
                        Logger.Info("Received message WM_USER_RESIZE_WINDOW");
                        int num = m.WParam.ToInt32();
                        int num2 = m.LParam.ToInt32();
                        Logger.Info("WParam: " + num);
                        Logger.Info("LParam: " + num2);
                        this.HandleWMUserResizeWindow(num, num2);
                        break;
                    }
                case 74:
                    {
                        Logger.Info("Received message WM_COPYDATA");
                        COPYDATASTRUCT cOPYDATASTRUCT = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
                        string text = Marshal.PtrToStringUni(cOPYDATASTRUCT.lpData);
                        Logger.Info("got data: " + text);
                        if (!(text == ""))
                        {
                            try
                            {
                                this.HandleWMCopyData(text);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex.ToString());
                            }
                            break;
                        }
                        return;
                    }
                case 80:
                    Logger.Info("Received message WM_INPUTLANGCHANGEREQUEST");
                    return;
                default:
                    flag = false;
                    break;
            }
            base.WndProc(ref m);
            if (flag)
            {
                try
                {
                    m.Result = new IntPtr(1);
                }
                catch (Exception)
                {
                }
            }
        }

        public void HandleWMUserShowWindow()
        {
            string name = string.Format("{0}\\{1}\\FrameBuffer\\0", "Software\\BlueStacks\\Guests", this.vmName);
            if (!this.userInteracted)
            {
                Logger.Info("attaching paint event handler");
                base.Paint += this.HandlePaintEvent;
                this.userInteracted = true;
            }
            if (this.mFrontendState == State.Stopped)
            {
                Logger.Info("Starting Service");
                this.StartVmServiceAsync();
                this.StateExitStopped();
                this.StateEnterStarting();
            }
            if (this.mFrontendState == State.Connected)
            {
                Logger.Info("at connected state");
                this.appLaunched = true;
                this.userInteracted = false;
            }
            Animate.AnimateWindow(base.Handle, 500, 524288);
            base.Show();
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
            int num = (int)registryKey.GetValue("WindowState", 1);
            if (num == 2)
            {
                base.WindowState = FormWindowState.Maximized;
            }
            else
            {
                base.WindowState = FormWindowState.Normal;
            }
            if (this.originalFullScreenState && !this.mFullScreen && !Console.sIsSpawnApps)
            {
                this.ToggleFullScreen();
                this.originalFullScreenState = false;
            }
        }

        private bool HandleWMSysCommand(int command)
        {
            string subkey = string.Format("{0}\\{1}\\FrameBuffer\\0", "Software\\BlueStacks\\Guests", this.vmName);
            if (command == 61488 || command == 61490 || command == 61728 || command == 61730)
            {
                Logger.Info("Received MAXIMIZE/RESTORE command");
                if (this.isMinimized)
                {
                    return true;
                }
                if (BlueStacks.hyperDroid.Common.Utils.GlMode == 0)
                {
                    DialogResult dialogResult = BlueStacks.hyperDroid.Common.UI.MessageBox.ShowMessageBox(BlueStacks.hyperDroid.Locale.Strings.ResizeMessageBoxCaption, BlueStacks.hyperDroid.Locale.Strings.ResizeMessageBoxText, BlueStacks.hyperDroid.Locale.Strings.OKButtonText, BlueStacks.hyperDroid.Locale.Strings.CancelButtonText, null);
                    if (dialogResult != DialogResult.OK)
                    {
                        return false;
                    }
                }
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(subkey);
                switch (command)
                {
                    case 61728:
                    case 61730:
                        registryKey.SetValue("WindowState", 1);
                        if (BlueStacks.hyperDroid.Common.Utils.GlMode == 0)
                        {
                            int width = Screen.PrimaryScreen.Bounds.Width;
                            int height = Screen.PrimaryScreen.Bounds.Height;
                            int num = default(int);
                            int num2 = default(int);
                            BlueStacks.hyperDroid.Common.Utils.GetWindowWidthAndHeight(width, height, out num, out num2);
                            registryKey.SetValue("Width", num);
                            registryKey.SetValue("Height", num2);
                        }
                        break;
                    case 61488:
                    case 61490:
                        registryKey.SetValue("WindowState", 2);
                        if (BlueStacks.hyperDroid.Common.Utils.GlMode == 0)
                        {
                            registryKey.SetValue("Width", Screen.PrimaryScreen.WorkingArea.Width);
                            registryKey.SetValue("Height", Screen.PrimaryScreen.WorkingArea.Height - 48 - SystemInformation.CaptionHeight);
                        }
                        break;
                }
                registryKey.Close();
                if (BlueStacks.hyperDroid.Common.Utils.GlMode == 0)
                {
                    Process.Start(Path.Combine(Console.sInstallDir, "HD-Restart.exe"), "Android");
                    return false;
                }
            }
            return true;
        }

        private void HandleWMUserResizeWindow(int wParam, int lParam)
        {
            if (this.mFullScreen)
            {
                Logger.Info("In fullscreen mode. Not resizing.");
            }
            else
            {
                int w;
                int h;
                if (wParam != 0 && lParam != 0)
                {
                    w = wParam;
                    h = lParam;
                }
                else
                {
                    w = this.mConfiguredDisplaySize.Width;
                    h = this.mConfiguredDisplaySize.Height;
                }
                Window.SetWindowPos(base.Handle, Window.HWND_TOP, 0, 0, w, h, 96u);
            }
        }

        private void HandleWMCopyData(string messageData)
        {
            string[] array = messageData.Split(' ');
            string text = array[0];
            string text2 = ".Main";
            string text3 = array[1];
            Logger.Info("package = " + text);
            Logger.Info("apkUrl = " + text3);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("package", text);
            dictionary.Add("activity", text2);
            dictionary.Add("apkUrl", text3);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMConfigRegKeyPath);
            int num = (int)registryKey.GetValue("AgentServerPort", 2861);
            string text4 = string.Format("http://127.0.0.1:{0}/{1}", num, "runapp");
            Logger.Info("Console: Sending post request to {0}", text4);
            GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("runapp", text, text2, 1), BlueStacks.hyperDroid.Common.Strings.GAUserAccountAppClicks);
            Client.PostWithRetries(text4, dictionary, null, false, 10, 500);
        }

        private int GetBorderWidth(int width, int height)
        {
            Window.RECT rECT = default(Window.RECT);
            rECT.left = 0;
            rECT.top = 0;
            rECT.right = width;
            rECT.bottom = height;
            int dwStyle = 13565952;
            if (!Window.AdjustWindowRect(out rECT, dwStyle, false))
            {
                return 18;
            }
            int num = rECT.right - rECT.left - width;
            Logger.Info("Got border = " + num);
            return num;
        }

        private Size GetConfiguredDisplaySize()
        {
            string name = string.Format("{0}\\{1}\\FrameBuffer\\0", "Software\\BlueStacks\\Guests", this.vmName);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
            int width = (int)registryKey.GetValue("WindowWidth");
            int height = (int)registryKey.GetValue("WindowHeight");
            registryKey.Close();
            return new Size(width, height);
        }

        private Size GetConfiguredGuestSize()
        {
            string name = string.Format("{0}\\{1}\\FrameBuffer\\0", "Software\\BlueStacks\\Guests", this.vmName);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
            int width = (int)registryKey.GetValue("GuestWidth");
            int height = (int)registryKey.GetValue("GuestHeight");
            registryKey.Close();
            return new Size(width, height);
        }

        public Rectangle GetScaledGuestDisplayArea()
        {
            return this.mScaledDisplayArea;
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                this.afterSleepTimer = new System.Windows.Forms.Timer();
                this.afterSleepTimer.Interval = 3000;
                this.afterSleepTimer.Tick += delegate
                {
                    this.afterSleepTimer.Stop();
                };
                this.afterSleepTimer.Start();
            }
        }

        private void HandleTouchEvent(object obj, WMTouchEventArgs evt)
        {
            this.CheckUserActive();
            for (int i = 0; i < this.touchPoints.Length; i++)
            {
                TouchPoint point = evt.GetPoint(i);
                if (point.Id != -1)
                {
                    this.touchPoints[i].PosX = (ushort)this.GetGuestX(point.X, point.Y);
                    this.touchPoints[i].PosY = (ushort)this.GetGuestY(point.X, point.Y);
                }
                else
                {
                    this.touchPoints[i].PosX = 65535;
                    this.touchPoints[i].PosY = 65535;
                }
            }
            this.monitor.SendTouchState(this.touchPoints);
        }

        private bool HandleKeyboardHook(bool pressed, uint key)
        {
            this.CheckUserActive();
            if (this.mFrontendState == State.Connected && this.Focused)
            {
                if (this.grabKeyboard && (key == 91 || key == 92))
                {
                    this.lastLWinTimestamp = DateTime.Now.Ticks;
                    this.HandleKeyEvent(Keys.LWin, pressed);
                    return false;
                }
                switch (key)
                {
                    case 68u:
                        {
                            long ticks = DateTime.Now.Ticks;
                            if (ticks - this.lastLWinTimestamp < 1000000)
                            {
                                return false;
                            }
                            return true;
                        }
                    case 166u:
                        this.HandleKeyEvent(Keys.Escape, pressed);
                        return false;
                    case 172u:
                        this.HandleKeyEvent(Keys.F8, pressed);
                        return false;
                    case 255u:
                        this.HandleKeyEvent(Keys.Apps, pressed);
                        return false;
                    default:
                        return true;
                }
            }
            return true;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            return false;
        }

        private bool IgnoreKey(KeyEventArgs evt)
        {
            bool result = false;
            if (evt.KeyCode == Keys.VolumeDown || evt.KeyCode == Keys.VolumeUp || evt.KeyCode == Keys.VolumeMute)
            {
                result = true;
            }
            return result;
        }

        private void HandleKeyDown(object obj, KeyEventArgs evt)
        {
            this.CheckUserActive();
            if (evt.Alt && evt.Control)
            {
                if (evt.KeyCode == Keys.V || evt.KeyCode == Keys.O || evt.KeyCode == Keys.T)
                {
                    uint scancode = this.keyboard.NativeToScanCodes(evt.KeyCode);
                    Opengl.HandleCommand((int)scancode);
                }
                else if (evt.KeyCode == Keys.K)
                {
                    this.ToggleKeyMapTeachMode();
                }
                else if (evt.KeyCode == Keys.I)
                {
                    this.mInputMapper.ShowConfigDialog();
                    return;
                }
            }
            if (!this.IgnoreKey(evt) && this.LockdownIsKeyAllowed(evt.KeyCode))
            {
                if (this.lockdownDisabled)
                {
                    if (evt.KeyCode == Keys.F1 && evt.Alt)
                    {
                        Opengl.HideSubWindow();
                    }
                    if (evt.KeyCode == Keys.F7 && evt.Alt)
                    {
                        Opengl.ShowSubWindow();
                    }
                }
                if (evt.Control)
                {
                    if (evt.KeyCode == Keys.Up)
                    {
                        this.mInputMapper.EmulateSwipe(0.5f, 0.5f, InputMapper.Direction.Up);
                        return;
                    }
                    if (evt.KeyCode == Keys.Down)
                    {
                        this.mInputMapper.EmulateSwipe(0.5f, 0.5f, InputMapper.Direction.Down);
                        return;
                    }
                    if (evt.KeyCode == Keys.Left)
                    {
                        this.mInputMapper.EmulateSwipe(0.5f, 0.5f, InputMapper.Direction.Left);
                        return;
                    }
                    if (evt.KeyCode == Keys.Right)
                    {
                        this.mInputMapper.EmulateSwipe(0.5f, 0.5f, InputMapper.Direction.Right);
                        return;
                    }
                    if (evt.KeyCode == Keys.Oemplus)
                    {
                        this.mInputMapper.EmulatePinch(0.5f, 0.5f, true);
                        return;
                    }
                    if (evt.KeyCode == Keys.OemMinus)
                    {
                        this.mInputMapper.EmulatePinch(0.5f, 0.5f, false);
                        return;
                    }
                }
                if (evt.KeyCode == Keys.F11 && Features.IsFullScreenToggleEnabled())
                {
                    this.ToggleFullScreen();
                }
                this.HandleKeyEvent(evt.KeyCode, true);
            }
        }

        private void HandleKeyUp(object obj, KeyEventArgs evt)
        {
            this.CheckUserActive();
            if (!this.IgnoreKey(evt))
            {
                this.HandleKeyEvent(evt.KeyCode, false);
            }
        }

        public void HandleKeyEvent(Keys key, bool pressed)
        {
            this.CheckUserActive();
            this.mInputMapper.DispatchKeyboardEvent(this.keyboard.NativeToScanCodes(key), pressed);
        }

        private bool LockdownIsKeyAllowed(Keys key)
        {
            if (!this.lockdownDisabled && this.keyboard.IsAltDepressed())
            {
                if (key != Keys.Left && key != Keys.Right && key != Keys.F1 && key != Keys.F2 && key != Keys.F3 && key != Keys.F4)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private void UnstickKeyboardModifiers()
        {
            this.HandleKeyEvent(Keys.LWin, false);
            this.HandleKeyEvent(Keys.RWin, false);
            this.HandleKeyEvent(Keys.Apps, false);
            this.HandleKeyEvent(Keys.Menu, false);
            this.HandleKeyEvent(Keys.LMenu, false);
            this.HandleKeyEvent(Keys.RShiftKey, false);
            this.HandleKeyEvent(Keys.LControlKey, false);
            this.HandleKeyEvent(Keys.RControlKey, false);
        }

        private void HandleDisplaySettingsChanged(object sender, EventArgs evt)
        {
            Logger.Info("HandleDisplaySettingsChanged()");
            this.ResizeFrontendWindow();
            this.SendOrientationToGuest();
        }

        public void ToggleFullScreen()
        {
            string name = string.Format("{0}\\{1}\\FrameBuffer\\0", "Software\\BlueStacks\\Guests", this.vmName);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name, true);
            if (!this.mFullScreen)
            {
                Console.OutputDebugString("SpawnApps:detachWindow()");
                this.mFullScreen = true;
                this.ResizeFrontendWindow();
                if (!Console.sIsSpawnApps)
                {
                    this.mFullScreenToast.Show();
                }
                Console.OutputDebugString("SpawnApps:executeJavascript(\"goneFullScreen();\")");
            }
            else
            {
                Console.OutputDebugString("SpawnApps:executeJavascript(\"exitingFullScreen();\")");
                Console.OutputDebugString("SpawnApps:attachWindow()");
                this.mFullScreen = false;
                this.ResizeFrontendWindow();
                this.mFullScreenToast.Hide();
            }
            registryKey.SetValue("FullScreen", this.mFullScreen ? 1 : 0);
            registryKey.Close();
        }

        private void ToggleKeyMapTeachMode()
        {
            Console.s_KeyMapTeachMode = !Console.s_KeyMapTeachMode;
        }

        private void DrawPaneLine(object obj, PaintEventArgs evt)
        {
            evt.Graphics.DrawLine(Pens.Gray, 0, 0, 0, base.Height);
        }

        private void ResizeFrontendWindow()
        {
            Logger.Info("ResizeFrontendWindow()");
            Logger.Info("Suspending Layout");
            base.SuspendLayout();
            if (this.mFullScreen)
            {
                this.mControlBarVisible = (this.mControlBarEnabled && this.mControlBarAlwaysShow);
                this.ResizeFrontendWindow_FullScreen();
            }
            else
            {
                this.mControlBarVisible = this.mControlBarEnabled;
                if (!Console.sIsSpawnApps || this.mFrontendState == State.Initial)
                {
                    this.ResizeFrontendWindow_Windowed();
                }
                if (Console.sIsSpawnApps)
                {
                    Console.OutputDebugString("SpawnApps:executeJavascript(\"sendResize();\")");
                }
            }
            if (this.mControlBarEnabled)
            {
                this.UpdateControlBar();
            }
            if (Console.sHideMode)
            {
                base.WindowState = FormWindowState.Minimized;
                this.isMinimized = true;
            }
            else if (base.WindowState == FormWindowState.Minimized)
            {
                base.WindowState = FormWindowState.Normal;
            }
            Logger.Info("Resuming Layout");
            base.ResumeLayout();
            this.FixupGuestDisplay();
            Logger.Info("ResizeFrontendWindow DONE");
        }

        private void ResizeFrontendWindow_FullScreen()
        {
            Logger.Info("ResizeFrontendWindow_FullScreen()");
            Logger.Info("Screen size is {0}x{1}", Window.ScreenWidth, Window.ScreenHeight);
            this.mCurrentDisplaySize.Width = Window.ScreenWidth;
            this.mCurrentDisplaySize.Height = Window.ScreenHeight;
            if (this.mControlBarVisible)
            {
                this.mCurrentDisplaySize.Height -= 48;
            }
            Logger.Info("Guest display area is {0}x{1}", this.mCurrentDisplaySize.Width, this.mCurrentDisplaySize.Height);
            base.FormBorderStyle = FormBorderStyle.None;
            if (this.mEmulatedPortraitMode && !Console.sIsSpawnApps && Features.IsFeatureEnabled(2048u))
            {
                float num = (float)this.mConfiguredDisplaySize.Width / (float)this.mConfiguredDisplaySize.Height;
                Size size = default(Size);
                size.Height = Screen.PrimaryScreen.WorkingArea.Height;
                size.Width = (int)((float)size.Height / num);
                int x = Window.ScreenWidth - size.Width;
                int y = 0;
                int width = size.Width;
                int height = size.Height;
                Window.SetFullScreen(base.Handle, x, y, width, height);
            }
            else
            {
                Window.SetFullScreen(base.Handle);
            }
            Logger.Info("New client size is {0}x{1}", base.ClientSize.Width, base.ClientSize.Height);
            Logger.Info("ResizeFrontendWindow_FullScreen DONE");
        }

        private void ResizeFrontendWindow_Windowed()
        {
            Logger.Info("ResizeFrontendWindow_Windowed()");
            Logger.Info("mEmulatedPortraitMode: " + this.mEmulatedPortraitMode);
            Size clientSize = default(Size);
            int num = 20;
            int num2 = 20;
            int height = Screen.PrimaryScreen.WorkingArea.Height - SystemInformation.CaptionHeight - this.GetBorderWidth(100, 100);
            if (this.mEmulatedPortraitMode && !Console.sIsSpawnApps && Features.IsFeatureEnabled(2048u))
            {
                clientSize.Height = height;
                float num3 = (float)this.mConfiguredDisplaySize.Width / (float)this.mConfiguredDisplaySize.Height;
                clientSize.Width = (int)((float)clientSize.Height / num3);
                num = Screen.PrimaryScreen.WorkingArea.Width - clientSize.Width - this.GetBorderWidth(100, 100) / 2;
                num2 = this.GetBorderWidth(100, 100) / 2;
                Logger.Info("location: ({0}x{1})", num, num2);
            }
            else if (!this.IsPortrait())
            {
                clientSize.Width = this.mConfiguredDisplaySize.Width;
                clientSize.Height = this.mConfiguredDisplaySize.Height;
            }
            else
            {
                clientSize.Width = this.mConfiguredDisplaySize.Height;
                clientSize.Height = this.mConfiguredDisplaySize.Width;
            }
            this.mCurrentDisplaySize = clientSize;
            Logger.Info("Guest display area is {0}x{1}", this.mCurrentDisplaySize.Width, this.mCurrentDisplaySize.Height);
            if (this.mControlBarVisible)
            {
                clientSize.Height += 48;
            }
            Logger.Info("New window size is {0}x{1}", clientSize.Width, clientSize.Height);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.StartPosition = FormStartPosition.Manual;
            base.Location = new Point(num, num2);
            base.ClientSize = clientSize;
            Logger.Info("New client size is {0}x{1}", base.ClientSize.Width, base.ClientSize.Height);
            Logger.Info("ResizeFrontendWindow_Windowed DONE");
        }

        private void FixupGuestDisplay()
        {
            Logger.Info("FixupGuestDisplay()");
            this.FixupGuestDisplay_FixAspectRatio();
            this.FixupGuestDisplay_FixOpenGLSubwindow();
            Logger.Info("FixupGuestDisplay DONE");
        }

        private void FixupGuestDisplay_FixAspectRatio()
        {
            Logger.Info("FixupGuestDisplay_FixAspectRatio()");
            float num = (this.IsPortrait() || this.mEmulatedPortraitMode) ? ((float)this.mConfiguredDisplaySize.Height / (float)this.mConfiguredDisplaySize.Width) : ((float)this.mConfiguredDisplaySize.Width / (float)this.mConfiguredDisplaySize.Height);
            float num2 = (float)this.mCurrentDisplaySize.Width / (float)this.mCurrentDisplaySize.Height;
            Logger.Info("Current aspect ratio {0}, desired {1}", num2, num);
            if (num2 > num)
            {
                Logger.Info("Decreasing guest display width");
                float num3 = (float)this.mCurrentDisplaySize.Width / num2 * num;
                this.mScaledDisplayArea.X = (this.mCurrentDisplaySize.Width - (int)num3) / 2;
                this.mScaledDisplayArea.Y = 0;
                this.mScaledDisplayArea.Width = (int)num3;
                this.mScaledDisplayArea.Height = this.mCurrentDisplaySize.Height;
            }
            else
            {
                Logger.Info("Decreasing guest display height");
                float num4 = (float)this.mCurrentDisplaySize.Height * num2 / num;
                this.mScaledDisplayArea.X = 0;
                this.mScaledDisplayArea.Y = (this.mCurrentDisplaySize.Height - (int)num4) / 2;
                this.mScaledDisplayArea.Width = this.mCurrentDisplaySize.Width;
                this.mScaledDisplayArea.Height = (int)num4;
            }
            Logger.Info("FixupGuestDisplay_FixAspectRatio DONE");
        }

        private void FixupGuestDisplay_FixOpenGLSubwindow()
        {
            Logger.Info("FixupGuestDisplay_FixOpenGLSubwindow()");
            int num = this.IsPortrait() ? 1 : ((!this.mEmulatedPortraitMode) ? (this.mRotateGuest180 ? 2 : 0) : ((!this.mRotateGuest180) ? 1 : 3));
            Logger.Info("OpenGL at ({0},{1}) size ({2},{3}) mode {4}", this.mScaledDisplayArea.X, this.mScaledDisplayArea.Y, this.mScaledDisplayArea.Width, this.mScaledDisplayArea.Height, num);
            int num2 = this.mScaledDisplayArea.Width;
            int num3 = this.mScaledDisplayArea.Height;
            if (this.mFullScreen)
            {
                num2--;
                num3--;
            }
            Opengl.ResizeSubWindow(this.mScaledDisplayArea.X, this.mScaledDisplayArea.Y, num2, num3);
            Opengl.HandleOrientation(1f, 1f, num);
            Logger.Info("FixupGuestDisplay_FixOpenGLSubwindow DONE");
        }

        private bool IsPortrait()
        {
            ScreenOrientation screenOrientation = SystemInformation.ScreenOrientation;
            if (screenOrientation != ScreenOrientation.Angle90)
            {
                return screenOrientation == ScreenOrientation.Angle270;
            }
            return true;
        }

        private bool ShouldEmulatePortraitMode()
        {
            string name = "Software\\BlueStacks\\Guests\\" + this.vmName + "\\FrameBuffer\\0";
            RegistryKey registryKey;
            using (registryKey = Registry.LocalMachine.OpenSubKey(name))
            {
                object value = registryKey.GetValue("EmulatePortraitMode");
                if (value != null)
                {
                    return (int)value != 0;
                }
            }
            return Console.IsDesktop();
        }

        private static bool IsDesktop()
        {
            if (BlueStacks.hyperDroid.Common.Utils.IsDesktopPC())
            {
                return true;
            }
            try
            {
                List<DeviceEnumerator> list = DeviceEnumerator.ListDevices(Guids.VideoInputDeviceCategory);
                bool result = list.Count != 2;
                foreach (DeviceEnumerator item in list)
                {
                    item.Dispose();
                }
                return result;
            }
            catch (Exception arg)
            {
                Logger.Info("Cannot enumerate camera devices: " + arg);
                return false;
            }
        }

        private void UpdateControlBar()
        {
            Logger.Info("UpdateControlBar()");
            base.SuspendLayout();
            if (this.mControlBar != null)
            {
                base.Controls.Remove(this.mControlBar);
                this.mControlBar = null;
            }
            if (!this.mControlBarVisible)
            {
                base.ResumeLayout();
            }
            else
            {
                Size parent = new Size(base.ClientSize.Width, base.ClientSize.Height - 48);
                this.mControlBar = new ControlBar(Console.sInstallDir, parent, new ControlHandler(this), Features.IsHomeButtonEnabled(), Features.IsShareButtonEnabled(), Features.IsSettingsButtonEnabled(), Features.IsFullScreenToggleEnabled(), this.mFullScreen);
                base.Controls.Add(this.mControlBar);
                base.ResumeLayout();
            }
        }

        public void HandleShareButtonClicked()
        {
            this.CheckUserActive();
            GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("ControlBar", "ShareButton", "Click", 1));
            int num = (base.Width - base.ClientSize.Width) / 2;
            int num2 = base.Height - base.ClientSize.Height - 2 * num;
            int width = base.Width + 2 * num;
            int height = base.Height + 2 * num + num2;
            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(new Point(base.Left, base.Top), Point.Empty, new Size(base.Width, base.Height));
            Random random = new Random();
            int num3 = random.Next(0, 100000);
            string text = "bstSnapshot_" + num3 + ".jpg";
            string text2 = "final_" + text;
            string name = "Software\\BlueStacks\\Guests\\Android\\Config";
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
            if ((int)registryKey.GetValue("FileSystem", 0) == 0)
            {
                Logger.Info("Shared folders disabled");
            }
            else
            {
                string sharedFolderDir = BlueStacks.hyperDroid.Common.Strings.SharedFolderDir;
                string sharedFolderName = BlueStacks.hyperDroid.Common.Strings.SharedFolderName;
                string text3 = Path.Combine(sharedFolderDir, text);
                string outputImage = Path.Combine(sharedFolderDir, text2);
                bitmap.Save(text3, ImageFormat.Jpeg);
                try
                {
                    BlueStacks.hyperDroid.Common.Utils.AddUploadTextToImage(text3, outputImage);
                    File.Delete(text3);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to add upload text to snapshot. err: " + ex.ToString());
                    text2 = text;
                    outputImage = text3;
                }
                string text4 = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + BlueStacks.hyperDroid.Common.Strings.SharePicUrl;
                string text5 = "/mnt/sdcard/windows/" + sharedFolderName + "/" + Path.GetFileName(text2);
                Logger.Info("androidPath: " + text5);
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("data", text5);
                Logger.Info("Sending snapshot upload request.");
                string text6 = "";
                try
                {
                    text6 = Client.Post(text4, dictionary, null, false);
                }
                catch (Exception ex2)
                {
                    Logger.Error(ex2.ToString());
                    Logger.Error("Post failed. url = {0}, data = {1}", text4, dictionary);
                }
                if (text6.Contains("error") && !this.snapshotErrorShown)
                {
                    this.snapshotErrorShown = true;
                    this.snapshotErrorToast = new Toast(this, BlueStacks.hyperDroid.Locale.Strings.SnapshotErrorToastText);
                    Animate.AnimateWindow(this.snapshotErrorToast.Handle, 500, 262148);
                    this.snapshotErrorToast.Show();
                    Thread thread = new Thread((ThreadStart)delegate
                    {
                        Thread.Sleep(3000);
                        Animate.AnimateWindow(this.snapshotErrorToast.Handle, 500, 327688);
                        this.snapshotErrorShown = false;
                    });
                    thread.IsBackground = true;
                    thread.Start();
                }
            }
        }

        private int GetGuestX(int x, int y)
        {
            int num = 0;
            int num2 = x - this.mScaledDisplayArea.X;
            if (num2 < 0)
            {
                num2 = 0;
            }
            else if (num2 >= this.mScaledDisplayArea.Width)
            {
                num2 = this.mScaledDisplayArea.Width;
            }
            if (this.mScaledDisplayArea.Width == 0)
            {
                return 0;
            }
            num2 = (int)((float)num2 * 32768f / (float)this.mScaledDisplayArea.Width);
            int num3 = y - this.mScaledDisplayArea.Y;
            if (num3 < 0)
            {
                num3 = 0;
            }
            else if (num3 >= this.mScaledDisplayArea.Height)
            {
                num3 = this.mScaledDisplayArea.Height;
            }
            if (this.mScaledDisplayArea.Height == 0)
            {
                return 0;
            }
            num3 = (int)((float)num3 * 32768f / (float)this.mScaledDisplayArea.Height);
            if (!this.IsPortrait() && !this.mEmulatedPortraitMode)
            {
                if (!this.mRotateGuest180)
                {
                    return num2;
                }
                return 32768 - num2;
            }
            if (!this.mRotateGuest180)
            {
                return 32768 - num3;
            }
            return num3;
        }

        private int GetGuestY(int x, int y)
        {
            int num = 0;
            int num2 = y - this.mScaledDisplayArea.Y;
            if (num2 < 0)
            {
                num2 = 0;
            }
            else if (num2 >= this.mScaledDisplayArea.Height)
            {
                num2 = this.mScaledDisplayArea.Height;
            }
            if (this.mScaledDisplayArea.Height == 0)
            {
                return 0;
            }
            num2 = (int)((float)num2 * 32768f / (float)this.mScaledDisplayArea.Height);
            int num3 = x - this.mScaledDisplayArea.X;
            if (num3 < 0)
            {
                num3 = 0;
            }
            else if (num3 >= this.mScaledDisplayArea.Width)
            {
                num3 = this.mScaledDisplayArea.Width;
            }
            if (this.mScaledDisplayArea.Width == 0)
            {
                return 0;
            }
            num3 = (int)((float)num3 * 32768f / (float)this.mScaledDisplayArea.Width);
            if (!this.IsPortrait() && !this.mEmulatedPortraitMode)
            {
                if (!this.mRotateGuest180)
                {
                    return num2;
                }
                return 32768 - num2;
            }
            if (!this.mRotateGuest180)
            {
                return num3;
            }
            return 32768 - num3;
        }
    }
}
