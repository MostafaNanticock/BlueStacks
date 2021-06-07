using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using BlueStacks.hyperDroid.Common.Interop;
using BlueStacks.hyperDroid.Locale;
using BlueStacks.hyperDroid.Updater;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Agent
{
    public class HDAgent : ApplicationContext
    {
        private const string BluestacksExe = "HD-Frontend.exe";

        private static Dictionary<string, int> s_InstalledPackages = null;

        private static int s_CCPinCheckSecs;

        private static Mutex s_HDAgentLock;

        public static string s_InstallDir;

        private static string s_GUID;

        public static int s_AgentPort = 2861;

        public static string s_InstallPath = "install";

        public static string s_BrowserInstallPath = "browserinstall";

        public static string s_UninstallPath = "uninstall";

        public static string s_InstalledPacakgesPath = "installedpackages";

        public static string s_ClipboardDataPath = "clipboard";

        public static string s_GetDiskUsage = BlueStacks.hyperDroid.Common.Strings.GetDiskUsage;

        public static ClipboardMgr clipboardClient;

        public static string s_RootDir = Path.Combine(BlueStacks.hyperDroid.Common.Strings.BstUserDataDir, "www");

        private static string s_CloudRegKey = "Software\\BlueStacks\\Agent\\Cloud";

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate3;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate4;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate5;

        [CompilerGenerated]
        private static DoWorkEventHandler _003C_003E9__CachedAnonymousMethodDelegate13;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate1b;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate1d;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate1f;

        [CompilerGenerated]
        private static ThreadExceptionEventHandler _003C_003E9__CachedAnonymousMethodDelegate22;

        [CompilerGenerated]
        private static UnhandledExceptionEventHandler _003C_003E9__CachedAnonymousMethodDelegate23;

        public static string CCPin
        {
            get
            {
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(HDAgent.s_CloudRegKey);
                string result = (string)registryKey.GetValue("CCPin", "null");
                registryKey.Close();
                return result;
            }
            set
            {
                using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(HDAgent.s_CloudRegKey))
                {
                    registryKey.SetValue("CCPin", value);
                    registryKey.Flush();
                }
                if (value != "null")
                {
                    SysTray.ShowCloudConnectedAlert(BlueStacks.hyperDroid.Locale.Strings.CloudConnectTitle, BlueStacks.hyperDroid.Locale.Strings.CloudConnectedMsg);
                }
                else
                {
                    SysTray.ShowCloudDisconnectedAlert(BlueStacks.hyperDroid.Locale.Strings.CloudConnectTitle, BlueStacks.hyperDroid.Locale.Strings.CloudDisconnectedMsg);
                }
            }
        }

        public static string Email
        {
            get
            {
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(HDAgent.s_CloudRegKey);
                string result = (string)registryKey.GetValue("Email", "null");
                registryKey.Close();
                return result;
            }
            set
            {
                using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(HDAgent.s_CloudRegKey))
                {
                    registryKey.SetValue("Email", value);
                    registryKey.Flush();
                }
            }
        }

        [DllImport("HD-ShortcutHandler.dll", CharSet = CharSet.Auto)]
        public static extern int CreateShortcut(string target, string shortcutName, string desc, string iconPath, string targetArgs, int initializeCom);

        [DllImport("HD-GpsLocator-Native.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void HdLoggerInit(Logger.HdLoggerCallback cb);

        [DllImport("HD-GpsLocator-Native.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int LaunchGpsLocator();

        [STAThread]
        private static void Main(string[] args)
        {
            Logger.InitLog(null, "Agent");
            HDAgent.InitExceptionHandlers();
            Logger.Info("HDAgent: Starting agent PID {0}", Process.GetCurrentProcess().Id);
            Logger.Info("HDAgent: CLR version {0}", Environment.Version);
            Logger.Info("HDAgent: IsAdministrator: {0}", User.IsAdministrator());
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks"))
            {
                HDAgent.s_InstallDir = (string)registryKey.GetValue("InstallDir");
            }
            Directory.SetCurrentDirectory(HDAgent.s_InstallDir);
            Logger.Info("HDAgent: CurrentDirectory: {0}", Directory.GetCurrentDirectory());
            try
            {
                Logger.Info("Checking if Silent Updater running(Installing)");
                RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\BlueStacks\\Updater\\Manifest");
                if (registryKey2 != null && registryKey2.GetValue("Status") != null)
                {
                    if (string.Compare((string)registryKey2.GetValue("Status"), "Installing") == 0)
                    {
                        return;
                    }
                    if (string.Compare((string)registryKey2.GetValue("Status"), "RollBack") == 0)
                    {
                        return;
                    }
                }
                Logger.Info("Silent Updater not installing");
            }
            catch (Exception ex)
            {
                Logger.Error("Error Occured, Err: " + ex.ToString());
                return;
            }
            BlueStacks.hyperDroid.Locale.Strings.InitLocalization();
            bool flag = false;
            if (args.Length == 1 && string.Compare(args[0].Trim(), "wait") == 0)
            {
                flag = true;
            }
            if (flag)
            {
                Logger.Info("Was asked to wait. Sleeping for 60 sec");
                Thread.Sleep(60000);
            }
            if (BlueStacks.hyperDroid.Common.Utils.IsAlreadyRunning(BlueStacks.hyperDroid.Common.Strings.HDAgentLockName, out HDAgent.s_HDAgentLock))
            {
                HDAgent.HandleAlreadyRunning();
            }
            Environment.SetEnvironmentVariable("SPAWNAPPS_APP_NAME", "");
            ServicePointManager.DefaultConnectionLimit = 10;
            ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback(HDAgent.ValidateRemoteCertificate));
            RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
            string strA = (string)registryKey3.GetValue("InstallType");
            if (string.Compare(strA, "split", true) == 0)
            {
                HDAgent.SendInstallEvent("started");
                Logger.Info("Installation type is split. Starting downlaod of remaining components");
                Thread thread = new Thread((ThreadStart)delegate
                {
                    HDAgent.DownloadRemainingComponents();
                });
                thread.IsBackground = true;
                thread.Start();
            }
            else if (string.Compare(strA, "nconly", true) == 0)
            {
                Logger.Info("InstallType is nconly.");
                Thread thread2 = new Thread((ThreadStart)delegate
                {
                    HDAgent.CreateWin8TileWall();
                });
                thread2.IsBackground = true;
                thread2.Start();
            }
            if (User.IsFirstTimeLaunch() && User.GUID == "")
            {
                Guid guid;
                try
                {
                    guid = UUID.GenerateUUID(UUID.UUIDTYPE.GLOBAL);
                }
                catch (UUID.EUUIDLocalOnly)
                {
                    guid = Guid.NewGuid();
                }
                catch (UUID.EUUID eUUID)
                {
                    Logger.Error(eUUID.ToString());
                    throw eUUID;
                }
                User.GUID = guid.ToString();
            }
            Application.EnableVisualStyles();
            HDAgent.s_GUID = User.GUID;
            ApkInstall.InitApkInstall();
            BackgroundWorker backgroundWorker = HDAgent.CreateCCWorker();
            Thread thread3 = new Thread(HDAgent.SetupHTTPServer);
            thread3.IsBackground = true;
            thread3.Start();
            RegistryKey registryKey4 = Registry.LocalMachine.OpenSubKey(HDAgent.s_CloudRegKey);
            HDAgent.s_CCPinCheckSecs = (int)registryKey4.GetValue("CCPinCheckSecs", 5);
            registryKey4.Close();
            registryKey4 = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMConfigRegKeyPath);
            if ((int)registryKey4.GetValue("SystemStats", 0) == 0)
            {
                Stats.SendSystemInfoStats();
            }
            backgroundWorker.RunWorkerAsync();
            EventLog eventLog = new EventLog("Application");
            eventLog.EntryWritten += HDAgent.EventLogWritten;
            eventLog.EnableRaisingEvents = true;
            EventLog eventLog2 = new EventLog("System");
            eventLog2.EntryWritten += HDAgent.EventLogWritten;
            eventLog2.EnableRaisingEvents = true;
            if (Features.IsFeatureEnabled(256u))
            {
                try
                {
                    registryKey4 = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Updater");
                    registryKey4.DeleteValue("Status", false);
                    Thread thread4 = new Thread((ThreadStart)delegate
                    {
                        Manager.DoWorkflow();
                    });
                    thread4.IsBackground = true;
                    thread4.Start();
                }
                catch (Exception ex2)
                {
                    Logger.Error("Exception when trying to create updater thread");
                    Logger.Error(ex2.ToString());
                }
            }
            int major = Environment.OSVersion.Version.Major;
            int minor = Environment.OSVersion.Version.Minor;
            if (major == 6 && minor >= 1)
            {
                try
                {
                    HDAgent.CopyLibraryIfNeeded();
                }
                catch (Exception ex3)
                {
                    Logger.Error(ex3.ToString());
                }
            }
            Logger.Info("Starting Gps Locator");
            try
            {
                Thread thread5 = new Thread(HDAgent.StartGpsLocator);
                thread5.IsBackground = true;
                thread5.Start();
            }
            catch (Exception ex4)
            {
                Logger.Error("Error Occured, Err: {0}", ex4.ToString());
            }
            HDAgent.GetDefaultBrowserProgId();
            HDAgent context = new HDAgent();
            Application.Run(context);
            Logger.Info("Exiting HDAgent PID {0}", Process.GetCurrentProcess().Id);
        }

        private static void GetDefaultBrowserProgId()
        {
            Logger.Info("Reading default browser information");
            string text = "Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\http\\UserChoice";
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(text);
                if (registryKey == null)
                {
                    Logger.Info(text + " not found");
                }
                else
                {
                    string text2 = (string)registryKey.GetValue("ProgId", "");
                    if (text2 != "")
                    {
                        Logger.Info("ProgId: " + text2);
                        RegistryKey registryKey2 = Registry.LocalMachine.CreateSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMConfigRegKeyPath);
                        registryKey2.SetValue("ProgId", text2);
                        registryKey2.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        private static void StartGpsLocator()
        {
            Logger.Info("Inside Start GpsLocator");
            try
            {
                try
                {
                    Logger.Info("Checking if Gps Enabled");
                    if ((int)Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMConfigRegKeyPath).GetValue("GpsMode") == 0)
                    {
                        Logger.Info("GpsMode is Disabled.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error Occured, Err: " + ex.ToString());
                }
                System.Version v = new System.Version(6, 2, 9200, 0);
                if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= v)
                {
                    try
                    {
                        HDAgent.HdLoggerInit(Logger.GetHdLoggerCallback());
                        HDAgent.LaunchGpsLocator();
                        Logger.Info("Back from Native Call");
                    }
                    catch (Exception ex2)
                    {
                        Logger.Error("Error Occured, Err: " + ex2.ToString());
                    }
                }
                else
                {
                    Logger.Info("Need Windows 8 or Higher for GpsLocator to work.");
                }
            }
            catch (Exception ex3)
            {
                Logger.Error("Error Occured, Err: " + ex3.ToString());
            }
        }

        private static void CreateWin8TileWall()
        {
            string str = "null";
            bool flag = false;
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
                string value = (string)registryKey.GetValue("CurrentBuildNumber", "9300");
                int num = Convert.ToInt32(value);
                if (num >= 9200 & num < 9600)
                {
                    str = "Windows 8";
                    flag = true;
                }
                if (num >= 9600)
                {
                    str = "Windows 8.1";
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to find os version. Ignoring tile creation: " + ex.ToString());
                flag = false;
            }
            Logger.Info("os version = " + str);
            if (flag)
            {
                Logger.Info("Os is windows 8. Creating tiles");
                RegistryKey registryKey2 = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks");
                string strA = (string)registryKey2.GetValue("TilesCreated");
                if (string.Compare(strA, "true", true) == 0)
                {
                    Logger.Info("Tiles already created. Ignoring.");
                }
                else
                {
                    string path = (string)registryKey2.GetValue("InstallDir");
                    string fileName = Path.Combine(path, "HD-TileCreator.exe");
                    Process process = new Process();
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Arguments = "wall";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        registryKey2.SetValue("TilesCreated", "true");
                    }
                }
            }
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }

        private static void HandleAlreadyRunning()
        {
            Logger.Info("Agent already running");
            string url = "http://127.0.0.1:2861/" + BlueStacks.hyperDroid.Common.Strings.SystrayVisibilityUrl;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("visible", "true");
            try
            {
                Client.Post(url, dictionary, null, false);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when sending HTTP message to SystrayVisibilityUrl");
                Logger.Error(ex.ToString());
            }
            Environment.Exit(1);
        }

        private static void SendInstallEvent(string status)
        {
            Dictionary<string, string> userData = BlueStacks.hyperDroid.Common.Utils.GetUserData();
            userData.Add("install_status", status);
            userData.Add("install_type", "android-install");
            Logger.Info("Sending android-install {0} stats", status);
            try
            {
                string url = BlueStacks.hyperDroid.Common.Strings.ChannelsUrl + "/" + BlueStacks.hyperDroid.Common.Strings.BsInstallStatsUrl;
                Client.Post(url, userData, null, false, 10000);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send install event: " + ex.Message);
            }
            Logger.Info("Sent android-install {0} stats", status);
        }

        private static void DownloadRemainingComponents()
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string setupDir = Path.Combine(folderPath, "BlueStacksSetup");
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software");
            string arg = (string)registryKey.GetValue("BstRuntimeUrl", BlueStacks.hyperDroid.Common.Strings.CDNDownloadUrl);
            registryKey.Close();
            string fileName;
            string manifestURL;
            if (BlueStacks.hyperDroid.Common.Utils.IsOEM("AMD"))
            {
                fileName = Path.Combine(setupDir, string.Format("runtimedata_amd_{0}.zip", "0.9.4.4078"));
                manifestURL = string.Format("{0}/{1}/split_runtime/runtimedata_amd_{1}.zip.manifest", arg, "0.9.4.4078");
            }
            else
            {
                fileName = Path.Combine(setupDir, string.Format("runtimedata_{0}.zip", "0.9.4.4078"));
                manifestURL = string.Format("{0}/{1}/split_runtime/runtimedata_{1}.zip.manifest", arg, "0.9.4.4078");
            }
            if (File.Exists(fileName))
            {
                HDAgent.CompleteBluestacksSetup(fileName);
            }
            else
            {
                Thread thread = new Thread((ThreadStart)delegate
                {
                    int nrWorkers = 3;
                    bool downloaded = false;
                    while (!downloaded)
                    {
                        SplitDownloader splitDownloader = new SplitDownloader(manifestURL, setupDir, BlueStacks.hyperDroid.Common.Utils.UserAgent(User.GUID), nrWorkers);
                        splitDownloader.Download(delegate(int percent)
                        {
                            RegistryKey registryKey2 = Registry.LocalMachine.CreateSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
                            registryKey2.SetValue("DownloadProgress", percent);
                        }, delegate(string filePath)
                        {
                            downloaded = true;
                            try
                            {
                                HDAgent.CompleteBluestacksSetup(filePath);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("Exception in CompleteBlueStacksSetup: " + ex.ToString());
                            }
                        }, delegate(Exception e)
                        {
                            downloaded = false;
                            GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("InvalidDownload", manifestURL, fileName, 1), BlueStacks.hyperDroid.Common.Strings.GAUserAccountInstaller);
                            Logger.Error("Download runtime error: " + e.ToString());
                            Thread.Sleep(10000);
                        });
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private static void CompleteBluestacksSetup(string fileName)
        {
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
            string path = (string)registryKey.GetValue("DataDir");
            string text = Path.Combine(path, "Android");
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            try
            {
                string mD5HashFromFile = BlueStacks.hyperDroid.Common.Utils.GetMD5HashFromFile(fileName);
                Logger.Info("runtime md5sum: " + mD5HashFromFile);
            }
            catch (Exception)
            {
                Logger.Error("Failed to compute md5sum.");
            }
            int num = BlueStacks.hyperDroid.Common.Utils.Unzip(fileName, text);
            Logger.Info("Unzip runtime exited with error code: " + num);
            if (num != 0)
            {
                HDAgent.SendInstallEvent("failed");
                Logger.Error("Failed to unzip runtime data. Aborting");
                MessageBox.Show("Failed to extract runtime data. Error: " + num);
                try
                {
                    Logger.Info("Deleting corrupted downloaded file...");
                    File.Delete(fileName);
                }
                catch
                {
                }
            }
            else
            {
                registryKey.SetValue("InstallType", "complete");
                Logger.Info("Full setup completed.");
                HDAgent.SendInstallEvent("completed");
                Mutex mutex = default(Mutex);
                bool flag = BlueStacks.hyperDroid.Common.Utils.IsAlreadyRunning(BlueStacks.hyperDroid.Common.Strings.FrontendLockName, out mutex);
                if (flag)
                {
                    Logger.Info("Frontend already running.");
                }
                else
                {
                    mutex.Close();
                }
                bool flag2 = BlueStacks.hyperDroid.Common.Utils.IsAlreadyRunning(BlueStacks.hyperDroid.Common.Strings.ThinInstallerLockName, out mutex);
                if (flag2)
                {
                    Logger.Info("Thin Installer already running.");
                }
                else
                {
                    mutex.Close();
                }
                if (!flag && !flag2)
                {
                    mutex.Close();
                    Logger.Info("Both Frontend and Thin Installer not running. Starting frontend.");
                    string path2 = (string)registryKey.GetValue("InstallDir");
                    string fileName2 = Path.Combine(path2, "HD-RunApp.exe");
                    Process process = new Process();
                    process.StartInfo.FileName = fileName2;
                    process.StartInfo.Arguments = "";
                    if (!BlueStacks.hyperDroid.Common.Utils.IsGlHotAttach())
                    {
                        process.StartInfo.Arguments = "-h";
                    }
                    process.Start();
                }
                try
                {
                    registryKey.DeleteValue("DownloadProgress");
                }
                catch (Exception)
                {
                }
                int num2 = 5;
                while (num2-- > 0)
                {
                    try
                    {
                        string text2 = (string)registryKey.GetValue("ApkToExeFile", "none");
                        Logger.Info("ApkToExe installation pending for: " + text2);
                        if (string.Compare(text2, "none") != 0)
                        {
                            registryKey.SetValue("ContinueApkToExe", "yes");
                            Mutex mutex2 = default(Mutex);
                            if (BlueStacks.hyperDroid.Common.Utils.IsAlreadyRunning(BlueStacks.hyperDroid.Common.Strings.ApkThinInstallerLockName, out mutex2))
                            {
                                Logger.Info("ApkToExe is already running. No need to relaunch.");
                            }
                            else
                            {
                                mutex2.Close();
                                Process process2 = new Process();
                                process2.StartInfo.FileName = text2;
                                process2.Start();
                            }
                            break;
                        }
                    }
                    catch (Exception ex3)
                    {
                        Logger.Error("Failed to complete pending ApkToExe installation. error: " + ex3.ToString());
                    }
                    Thread.Sleep(1000);
                }
                registryKey.Close();
            }
        }

        private static void CopyLibraryIfNeeded()
        {
            string libraryName = BlueStacks.hyperDroid.Common.Strings.LibraryName;
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(folderPath, "Microsoft\\Windows\\Libraries");
            string path2 = Environment.ExpandEnvironmentVariables("%Public%");
            string path3 = Path.Combine(path2, "Libraries");
            string path4 = libraryName + ".library-ms";
            string text = libraryName + ".lnk";
            path = Path.Combine(path, path4);
            path3 = Path.Combine(path3, path4);
            if (!File.Exists(path))
            {
                Logger.Info("Copying library from {0} to {1}", path3, path);
                File.Copy(path3, path, true);
            }
        }

        private static void EventLogWritten(object source, EntryWrittenEventArgs e)
        {
            Logger.Debug("EventLog written");
            string message = e.Entry.Message;
            Regex regex = new Regex("(HD-.+).exe");
            Match match = regex.Match(message);
            if (match.Success)
            {
                string value = match.Groups[1].Value;
                Logger.Info("Event log for {0} written", value);
                Logger.Info("Message:\n{0}", message);
                string url = Service.Host + "/" + BlueStacks.hyperDroid.Common.Strings.BinaryCrashStatsUrl;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("binary", HDAgent.GetURLSafeBase64String(value));
                dictionary.Add("message", HDAgent.GetURLSafeBase64String(message));
                Client.Post(url, dictionary, null, false);
            }
        }

        private static void SetupHTTPServer()
        {
            Dictionary<string, Server.RequestHandler> dictionary = new Dictionary<string, Server.RequestHandler>();
            dictionary.Add("/installed", HTTPHandler.ApkInstalled);
            dictionary.Add("/uninstalled", HTTPHandler.AppUninstalled);
            dictionary.Add("/getapplist", HTTPHandler.GetAppList);
            dictionary.Add("/install", HTTPHandler.ApkInstall);
            dictionary.Add("/uninstall", HTTPHandler.AppUninstall);
            dictionary.Add("/runapp", HTTPHandler.RunApp);
            dictionary.Add("/InstallAppByURL", HTTPHandler.InstallAppByURL);
            dictionary.Add("/saveccpin", HTTPHandler.SaveCCPin);
            dictionary.Add("/ccpin", HTTPHandler.CCPin);
            dictionary.Add("/ccurl", HTTPHandler.CCUrl);
            dictionary.Add("/ping", HTTPHandler.Ping);
            dictionary.Add("/AppCrashedInfo", HTTPHandler.AppCrashedInfo);
            dictionary.Add("/doaction", HTTPHandler.DoAction);
            dictionary.Add("/getuserdata", HTTPHandler.GetUserData);
            dictionary.Add("/shownotification", HTTPHandler.ShowNotification);
            dictionary.Add("/showfenotification", HTTPHandler.ShowFeNotification);
            dictionary.Add("/quitfrontend", HTTPHandler.QuitFrontend);
            dictionary.Add("/addapp", HTTPHandler.AddApp);
            dictionary.Add("/getappimage", HTTPHandler.GetAppImage);
            dictionary.Add("/showtraynotification", HTTPHandler.ShowSysTrayNotification);
            dictionary.Add("/switchtolauncher", HTTPHandler.SwitchToLauncher);
            dictionary.Add("/switchtowindows", HTTPHandler.SwitchToWindows);
            dictionary.Add("/restart", HTTPHandler.Restart);
            dictionary.Add("/logappclick", HTTPHandler.LogAndroidClickEvent);
            dictionary.Add("/logwebappchannelclick", HTTPHandler.LogWebAppChannelClickEvent);
            dictionary.Add("/logappsearch", HTTPHandler.LogAndroidSearchEvent);
            dictionary.Add("/notification", HTTPHandler.NotificationHandler);
            dictionary.Add("/clipboard", HTTPHandler.SetClipboardData);
            dictionary.Add("/isappinstalled", HTTPHandler.IsAppInstalled);
            dictionary.Add("/topActivityInfo", HTTPHandler.TopActivityInfo);
            dictionary.Add("/systrayvisibility", HTTPHandler.SystrayVisibility);
            dictionary.Add("/restartagent", HTTPHandler.RestartAgent);
            dictionary.Add("/showtileinterface", HTTPHandler.ShowTileInterface);
            dictionary.Add("/useractive", HTTPHandler.SetLastActivityClicked);
            dictionary.Add("/exitagent", HTTPHandler.ExitAgent);
            dictionary.Add("/StopApp", HTTPHandler.StopAppHandler);
            Server server = new Server(HDAgent.s_AgentPort, dictionary, HDAgent.s_RootDir);
            server.Start();
            Logger.Info("Server listening on port " + server.Port);
            Logger.Info("Serving static content from " + server.RootDir);
            HDAgent.s_AgentPort = server.Port;
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
                registryKey.SetValue("AgentServerPort", server.Port, RegistryValueKind.DWord);
                registryKey.Flush();
                registryKey.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when trying to write AgentServerPort to the registry");
                Logger.Error(ex.ToString());
            }
            Thread thread = new Thread(HDAgent.SendFqdn);
            thread.IsBackground = true;
            thread.Start();
            server.Run();
        }

        private static void SendFqdn()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
            while (true)
            {
                string strA = (string)registryKey.GetValue("InstallType");
                if ((string.Compare(strA, "complete", true) == 0 || string.Compare(strA, "full", true) == 0) && VmCmdHandler.FqdnSend(HDAgent.s_AgentPort, "Agent") != null)
                {
                    break;
                }
                Thread.Sleep(2000);
            }
        }

        private static BackgroundWorker CreateCCWorker()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += delegate
            {
                while (true)
                {
                    if (HDAgent.CCPin == "null")
                    {
                        Logger.Debug("HDAgent: Retrying Checking of CCPin after CCPinCheckSecs = {0} intervals", HDAgent.s_CCPinCheckSecs);
                        Thread.Sleep(new TimeSpan(0, 0, HDAgent.s_CCPinCheckSecs));
                    }
                    else
                    {
                        Logger.Info("HDAgent: Got CCPin (not null)");
                        Logger.Info("HDAgent: Doing AppSyncer work");
                        try
                        {
                            AppSyncer.Sync();
                        }
                        catch (WebException ex)
                        {
                            Logger.Error("HDAgent: {0}", ex.Message);
                            if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
                            {
                                HDAgent.CCPin = "null";
                            }
                        }
                        catch (Auth.Token.EMalformed eMalformed)
                        {
                            Logger.Error("HDAgent: {0}", eMalformed.ToString());
                            HDAgent.CCPin = "null";
                        }
                    }
                }
            };
            return backgroundWorker;
        }

        public static void UpdateFrontendTitle(string title)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                int num = 5;
                while (num > 0)
                {
                    try
                    {
                        num--;
                        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
                        int num2 = (int)registryKey.GetValue("FrontendServerPort", 0);
                        if (num2 != 0)
                        {
                            string text = string.Format("http://127.0.0.1:{0}/{1}", num2, "updatetitle");
                            Dictionary<string, string> dictionary = new Dictionary<string, string>();
                            dictionary.Add("title", title);
                            Logger.Info("Sending updateTitle request to: {0} for new title: {1}.", text, title);
                            Client.Post(text, dictionary, null, false);
                            goto end_IL_0007;
                        }
                        return;
                    end_IL_0007: ;
                    }
                    catch (Exception ex)
                    {
                        if (num == 0)
                        {
                            Logger.Error("Failed to send update title request. err: " + ex.ToString());
                        }
                    }
                    Thread.Sleep(1000);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public HDAgent()
        {
            SysTray.Init();
            if (!BlueStacks.hyperDroid.Common.Utils.IsOSWinXP())
            {
                int num = PowerMgr.SetMaxCPUFreqPowerPlan();
                Logger.Warning("PowerMgr: SetMaxCPUFreqPowerPlan = " + num);
                num = PowerMgr.ActivateMaxCPUFreqPowerPlan();
                Logger.Warning("PowerMgr: ActivateMaxCPUFreqPowerPlan = " + num);
                PowerMgr powerMgr = new PowerMgr();
                powerMgr.Show();
            }
            string arg = "";
            bool flag = BlueStacks.hyperDroid.Common.Utils.IsProxyEnabled(out arg);
            Logger.Info("Proxy server enabled = " + flag + " " + arg);
            HDAgent.clipboardClient = new ClipboardMgr();
            HDAgent.clipboardClient.Show();
            HDAgent.CheckAnnouncement();
            HDAgent.UploadUsageStats();
            HDAgent.CheckTwitterFeeds();
        }

        private static void CheckTwitterFeeds()
        {
            int sleepTime = 5;
            Thread thread = new Thread((ThreadStart)delegate
            {
                while (true)
                {
                    try
                    {
                        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
                        string value = (string)registryKey.GetValue("TwitterFetchInterval", "5");
                        if (!string.IsNullOrEmpty(value))
                        {
                            sleepTime = Convert.ToInt32(value);
                            if (sleepTime <= 0 || sleepTime > 1500)
                            {
                                sleepTime = 5;
                            }
                        }
                        string text = (string)registryKey.GetValue("TwitterName");
                        if (!string.IsNullOrEmpty(text))
                        {
                            HDAgent.ShowTwitterFeeds(text);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Failed to show twitter feeds. err: " + ex.ToString());
                    }
                    Logger.Debug("Twitter thread: Sleeping for " + sleepTime + " min");
                    Thread.Sleep(sleepTime * 60 * 1000);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private static void ShowTwitterFeeds(string twitterName)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Agent\\AppSync");
            string a = (string)registryKey.GetValue("NCPaused", "no");
            if (!(a == "yes"))
            {
                RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
                string path = (string)registryKey2.GetValue("DataDir");
                string text = Path.Combine(path, "UserData\\TwitterData");
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                }
                string url = "http://search.twitter.com/search.json?q=" + twitterName;
                string input = Client.Get(url, null, false, 5000);
                JSonReader jSonReader = new JSonReader();
                IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
                int length = iJSonObject["results"].Length;
                for (int i = 0; i < 3; i++)
                {
                    string stringValue = iJSonObject["results"][i]["from_user_name"].StringValue;
                    string stringValue2 = iJSonObject["results"][i]["text"].StringValue;
                    string stringValue3 = iJSonObject["results"][i]["profile_image_url_https"].StringValue;
                    string text2 = text + "\\twitterImage" + i + ".png";
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(stringValue3, text2);
                    CustomAlert.ShowCloudAnnouncement(text2, "Tweet from: " + stringValue, stringValue2, true, delegate
                    {
                        Process.Start("http://www.twitter.com");
                    });
                }
            }
        }

        private static void CheckAnnouncement()
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                while (true)
                {
                    try
                    {
                        while (true)
                        {
                            if (CloudAnnouncement.ShowAnnouncement())
                            {
                                break;
                            }
                            Thread.Sleep(3600000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Failed to show announcement. err: " + ex.ToString());
                        Thread.Sleep(3600000);
                        continue;
                    }
                    Thread.Sleep(86400000);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private static void UploadUsageStats()
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                while (true)
                {
                    try
                    {
                        while (true)
                        {
                            Thread.Sleep(60000);
                            if (Stats.UploadUsageStats())
                            {
                                break;
                            }
                            Thread.Sleep(3600000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Failed to upload stats. err: " + ex.ToString());
                        Thread.Sleep(3600000);
                        continue;
                    }
                    Thread.Sleep(86400000);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static bool DoRunCmd(string request)
        {
            bool result = false;
            if (VmCmdHandler.RunCommand(request) == "ok")
            {
                result = true;
                if (request.Contains("mpi.v23"))
                {
                    Logger.Info("starting amidebug. not sending message to frontend.");
                    return result;
                }
                string appTitle = BlueStacks.hyperDroid.Common.Strings.AppTitle;
                IntPtr intPtr = Window.FindWindow(null, appTitle);
                if (intPtr != IntPtr.Zero)
                {
                    Logger.Info("Sending WM_USER_SHOW_WINDOW to Frontend Handle {0}", intPtr);
                    Window.SendMessage(intPtr, 1025u, IntPtr.Zero, IntPtr.Zero);
                }
            }
            string str = "";
            string text = "";
            string text2 = "";
            string str2 = "";
            string text3 = "";
            if (request.StartsWith("runex"))
            {
                Regex regex = new Regex("^runex\\s+");
                text = regex.Replace(request, "");
                text = text.Substring(0, text.IndexOf('/'));
                if (!JsonParser.GetAppInfoFromPackageName(text, out str, out str2, out text2, out text3))
                {
                    Logger.Error("Failed to get App info for: {0}. Not adding in launcher dock.", text);
                    return result;
                }
            }
            HDAgent.GetVersionFromPackage(text);
            string str3 = Path.Combine(BlueStacks.hyperDroid.Common.Strings.LibraryDir, BlueStacks.hyperDroid.Common.Strings.MyAppsDir);
            string text4 = str3 + str + ".lnk";
            str2 = BlueStacks.hyperDroid.Common.Strings.GadgetDir + str2;
            return result;
        }

        public static string GetVersionFromPackage(string packageName)
        {
            string result = "";
            if (HDAgent.s_InstalledPackages == null)
            {
                HDAgent.s_InstalledPackages = new Dictionary<string, int>();
            }
            if (!HDAgent.s_InstalledPackages.ContainsKey(packageName))
            {
                HDAgent.GetInstalledPackages();
            }
            int value = default(int);
            if (HDAgent.s_InstalledPackages.TryGetValue(packageName, out value))
            {
                result = Convert.ToString(value);
            }
            return result;
        }

        private static void GetInstalledPackages()
        {
            string input = HTTPHandler.Get(VmCmdHandler.s_ServerPort, HDAgent.s_InstalledPacakgesPath);
            JSonReader jSonReader = new JSonReader();
            IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
            string text = iJSonObject["result"].StringValue.Trim();
            if (text != "ok")
            {
                Logger.Error("result: {0}", text);
            }
            else
            {
                string text2 = iJSonObject["installed_packages"].ToString();
                Logger.Debug(text2);
                jSonReader = new JSonReader();
                IJSonObject iJSonObject2 = jSonReader.ReadAsJSonObject(text2);
                for (int i = 0; i < iJSonObject2.Length; i++)
                {
                    string key = iJSonObject2[i]["package"].StringValue.Trim();
                    int int32Value = iJSonObject2[i]["version"].Int32Value;
                    try
                    {
                        HDAgent.s_InstalledPackages.Add(key, int32Value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static string GetURLSafeBase64String(string originalString)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(originalString));
        }

        private static void InitExceptionHandlers()
        {
            Application.ThreadException += delegate(object obj, ThreadExceptionEventArgs evt)
            {
                Logger.Error("HDAgent: Unhandled Exception:");
                Logger.Error(evt.Exception.ToString());
                try
                {
                    HDAgent.UploadCrashLogs(evt.Exception.ToString());
                }
                catch (Exception ex2)
                {
                    Logger.Error(ex2.ToString());
                }
                Environment.Exit(1);
            };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += delegate(object obj, UnhandledExceptionEventArgs evt)
            {
                Logger.Error("HDAgent: Unhandled Exception:");
                Logger.Error(evt.ExceptionObject.ToString());
                try
                {
                    HDAgent.UploadCrashLogs(evt.ExceptionObject.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
                Environment.Exit(1);
            };
        }

        private static void UploadCrashLogs(string errorMsg)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.HKCURegKeyPath);
            string arg = (string)registryKey.GetValue("Host");
            string url = arg + "/" + BlueStacks.hyperDroid.Common.Strings.AgentCrashReportUrl;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("error", errorMsg);
            Client.Post(url, dictionary, null, true);
        }

        public static bool IsWindowOpen(string title)
        {
            IntPtr value = Window.FindWindow(null, title);
            if (value == IntPtr.Zero)
            {
                Logger.Info("{0} not open", title);
                return false;
            }
            Logger.Info("{0} already open", title);
            return true;
        }
    }
}
