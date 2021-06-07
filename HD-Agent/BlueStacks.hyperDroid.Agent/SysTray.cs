using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using BlueStacks.hyperDroid.Common.UI;
using BlueStacks.hyperDroid.Locale;
using BlueStacks.hyperDroid.Updater;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Agent
{
    internal class SysTray
    {
        private enum Resolutions
        {
            R640x480,
            R800x480,
            R1024x600,
            FullScreen
        }

        private static string s_AgentOnlineText = "App Player online";

        private static string s_AgentOfflineText = "App Player";

        private static NotifyIcon s_SysTrayIcon;

        private static ContextMenuStrip s_ContextMenuStrip = null;

        private static UninstallerForm s_UninstallerForm;

        private static bool s_TrayAnimationStarted = false;

        private static string s_NotificationTitle = "";

        private static string s_NotificationMsg = "";

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate1;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate6;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate11;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate12;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate13;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate14;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate16;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate18;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate1e;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate1f;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate20;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate24;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate26;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate2d;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate32;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate34;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate39;

        [CompilerGenerated]
        private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate3b;

        public static void Init()
        {
            if (!Features.IsFeatureEnabled(64u))
            {
                Logger.Info("Disabling systray support because feature is disabled.");
            }
            else
            {
                if (BlueStacks.hyperDroid.Common.Utils.IsInstallTypeNCOnly())
                {
                    SysTray.s_AgentOnlineText = "Notification Center online";
                    SysTray.s_AgentOfflineText = "Notification Center";
                }
                else if (Features.UseDefaultNetworkText())
                {
                    SysTray.s_AgentOnlineText = BlueStacks.hyperDroid.Locale.Strings.NetworkAvailableIconText;
                    SysTray.s_AgentOfflineText = BlueStacks.hyperDroid.Locale.Strings.NetworkUnavailableIconText;
                }
                else
                {
                    SysTray.s_AgentOnlineText = "App Player online";
                    SysTray.s_AgentOfflineText = "App Player";
                }
                SysTray.s_SysTrayIcon = new NotifyIcon();
                SysTray.s_SysTrayIcon.BalloonTipClicked += SysTray.AppAndExplorerLauncher;
                SysTray.s_SysTrayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    SysTray.s_SysTrayIcon.Text = SysTray.s_AgentOnlineText;
                }
                else
                {
                    SysTray.s_SysTrayIcon.Text = SysTray.s_AgentOfflineText;
                }
                NotifyIcon notifyIcon = SysTray.s_SysTrayIcon;
                notifyIcon.Text += " (0.9.4.4078)";
                SysTray.s_SysTrayIcon.MouseDown += SysTray.OnSysTrayMouseDown;
                SysTray.s_SysTrayIcon.MouseUp += SysTray.OnSysTrayMouseUp;
                NetworkChange.NetworkAvailabilityChanged += SysTray.OnNetworkAvailabilityChanged;
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
                string text = (string)registryKey.GetValue("InstallType");
                if (string.Compare(text, "uninstalled", true) != 0)
                {
                    SysTray.s_SysTrayIcon.Visible = true;
                }
                else
                {
                    Logger.Info("Not showing tray icon for intallType: " + text);
                }
            }
        }

        public static void StartTrayAnimation(string title, string msg)
        {
            SysTray.s_NotificationTitle = title;
            SysTray.s_NotificationMsg = msg;
            if (!SysTray.s_TrayAnimationStarted)
            {
                SysTray.s_TrayAnimationStarted = true;
                Thread thread = new Thread((ThreadStart)delegate
                {
                    SysTray.StartAnimation();
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }

        public static void StopTrayAnimation()
        {
            SysTray.s_TrayAnimationStarted = false;
            SysTray.s_SysTrayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private static void StartAnimation()
        {
            SysTray.ShowInfoLong(SysTray.s_NotificationTitle, SysTray.s_NotificationMsg);
            string s_InstallDir = HDAgent.s_InstallDir;
            while (true)
            {
                if (!SysTray.s_TrayAnimationStarted)
                {
                    break;
                }
                for (int i = 1; i <= 6; i++)
                {
                    string fileName = Path.Combine(s_InstallDir, "trayIcon" + i + ".ico");
                    Icon icon = new Icon(fileName);
                    SysTray.s_SysTrayIcon.Icon = icon;
                    Thread.Sleep(300);
                }
                Thread.Sleep(2000);
            }
            SysTray.s_SysTrayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private static void HandleTrayAnimationClick()
        {
            SysTray.ShowInfoLong(SysTray.s_NotificationTitle, SysTray.s_NotificationMsg);
        }

        public static void SetTrayIconVisibility(bool visible)
        {
            SysTray.s_SysTrayIcon.Visible = visible;
        }

        private static void AddContextMenus()
        {
            if (SysTray.s_ContextMenuStrip != null)
            {
                SysTray.s_ContextMenuStrip.Dispose();
            }
            SysTray.s_ContextMenuStrip = new ContextMenuStrip();
            SysTray.s_SysTrayIcon.ContextMenuStrip = SysTray.s_ContextMenuStrip;
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
            string strA = (string)registryKey.GetValue("InstallType");
            if (string.Compare(strA, "nconly", true) == 0 || string.Compare(strA, "uninstalled", true) == 0)
            {
                SysTray.AddNotificationCenterMenuItems();
            }
            else
            {
                SysTray.AddAppPlayerMenuItems();
            }
        }

        private static void AddAppPlayerMenuItems()
        {
            ToolStripSeparator value = new ToolStripSeparator();
            ToolStripSeparator value2 = new ToolStripSeparator();
            SysTray.AddZipLogsContextMenu();
            if (Features.IsFeatureEnabled(512u))
            {
                SysTray.AddRestartBlueStacksContextMenu();
            }
            SysTray.AddPortraitModeContextMenu();
            if (Features.IsFeatureEnabled(32u))
            {
                SysTray.AddBstUsageContextMenu();
            }
            if (Features.IsFeatureEnabled(256u))
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
                string strA = (string)registryKey.GetValue("InstallType");
                if (string.Compare(strA, "complete", true) == 0 || string.Compare(strA, "full", true) == 0)
                {
                    SysTray.AddUpdatesContextMenu();
                }
            }
            SysTray.s_ContextMenuStrip.Items.Add(value);
            if (SysTray.AddOptionalContextMenu())
            {
                SysTray.s_ContextMenuStrip.Items.Add(value2);
            }
            SysTray.AddStopContextMenu();
            SysTray.s_ContextMenuStrip.ShowCheckMargin = false;
            SysTray.s_ContextMenuStrip.ShowImageMargin = false;
        }

        private static void AddNotificationCenterMenuItems()
        {
            ToolStripSeparator value = new ToolStripSeparator();
            ToolStripSeparator value2 = new ToolStripSeparator();
            SysTray.AddZipLogsContextMenu();
            SysTray.s_ContextMenuStrip.Items.Add(value);
            SysTray.AddSMSSetupMenu();
            SysTray.AddNotificationsMenu();
            SysTray.AddTwitterFeedsMenu();
            SysTray.s_ContextMenuStrip.Items.Add(value2);
            SysTray.AddPauseSyncingMenu();
            SysTray.s_ContextMenuStrip.ShowCheckMargin = false;
            SysTray.s_ContextMenuStrip.ShowImageMargin = false;
        }

        private static void OnSysTrayMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                SysTray.s_SysTrayIcon.ContextMenuStrip = null;
            }
        }

        private static void OnSysTrayMouseUp(object sender, MouseEventArgs e)
        {
            if (SysTray.s_TrayAnimationStarted)
            {
                SysTray.HandleTrayAnimationClick();
            }
            else
            {
                SysTray.AddContextMenus();
                MethodInfo method = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(SysTray.s_SysTrayIcon, null);
            }
        }

        private static void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                SysTray.s_SysTrayIcon.Text = SysTray.s_AgentOnlineText;
            }
            else
            {
                SysTray.s_SysTrayIcon.Text = SysTray.s_AgentOfflineText;
            }
            NotifyIcon notifyIcon = SysTray.s_SysTrayIcon;
            notifyIcon.Text += " (0.9.4.4078)";
        }

        public static void ShowInfoShort(string title, string message)
        {
            SysTray.ShowTrayStatus(ToolTipIcon.Info, title, message, 1000);
        }

        public static void ShowInstallAlert(string appName, string imagePath, string title, string installMsg)
        {
            CustomAlert.ShowInstallAlert(imagePath, title, installMsg, delegate
            {
                Logger.Info("Clicked on InstallAlert");
                string arg = default(string);
                string text = default(string);
                string arg2 = default(string);
                if (!JsonParser.GetAppInfoFromAppName(appName, out arg, out text, out arg2))
                {
                    Logger.Error("Failed to launch app: {0}. No info found in json", appName);
                }
                else
                {
                    string fileName = HDAgent.s_InstallDir + "\\HD-RunApp.exe";
                    Process.Start(fileName, "-p " + arg + " -a " + arg2);
                }
            });
        }

        public static void ShowUninstallAlert(string title, string uninstallMsg)
        {
            string imagePath = Path.Combine(HDAgent.s_InstallDir, "ProductLogo.png");
            CustomAlert.ShowUninstallAlert(imagePath, title, uninstallMsg);
        }

        public static void ShowCloudConnectedAlert(string title, string cloudConnectedMsg)
        {
            string imagePath = Path.Combine(HDAgent.s_InstallDir, "cloudIcon.png");
            CustomAlert.ShowCloudConnectedAlert(imagePath, title, cloudConnectedMsg);
        }

        public static void ShowCloudDisconnectedAlert(string title, string cloudDisconnectedMsg)
        {
            string imagePath = Path.Combine(HDAgent.s_InstallDir, "cloudIcon.png");
            CustomAlert.ShowCloudDisconnectedAlert(imagePath, title, cloudDisconnectedMsg, delegate
            {
                string fileName = HDAgent.s_InstallDir + "\\HD-SignUp.exe";
                Process.Start(fileName);
            });
        }

        public static void ShowSMSMessage(string title, string msg)
        {
            string imagePath = Path.Combine(HDAgent.s_InstallDir, "cloudIcon.png");
            CustomAlert.ShowSMSMessage(imagePath, title, msg);
        }

        public static void ShowAndroidNotification(string msg, string name, string package, string activity, string imagePath)
        {
            CustomAlert.ShowAndroidNotification(imagePath, name, msg, delegate
            {
                string fileName = HDAgent.s_InstallDir + "\\HD-RunApp.exe";
                Logger.Info("Starting RunApp");
                Process.Start(fileName, "-p " + package + " -a " + activity);
            });
        }

        public static void ShowInfoLong(string title, string message)
        {
            SysTray.ShowTrayStatus(ToolTipIcon.Info, title, message, 2000);
        }

        private static void ShowWarningShort(string title, string message)
        {
            SysTray.ShowTrayStatus(ToolTipIcon.Warning, title, message, 1000);
        }

        private static void ShowWarningLong(string title, string message)
        {
            SysTray.ShowTrayStatus(ToolTipIcon.Warning, title, message, 2000);
        }

        private static void ShowErrorShort(string title, string message)
        {
            SysTray.ShowTrayStatus(ToolTipIcon.Error, title, message, 1000);
        }

        public static void ShowErrorLong(string title, string message)
        {
            SysTray.ShowTrayStatus(ToolTipIcon.Error, title, message, 2000);
        }

        public static void ShowTrayStatus(ToolTipIcon icon, string title, string message, int timeout)
        {
            Logger.Info("icon type = " + icon);
            int num = 30;
            while (num > 0 && SysTray.s_SysTrayIcon == null)
            {
                Thread.Sleep(1000);
                num--;
            }
            lock (SysTray.s_SysTrayIcon)
            {
                SysTray.s_SysTrayIcon.BalloonTipTitle = title;
                SysTray.s_SysTrayIcon.BalloonTipIcon = icon;
                SysTray.s_SysTrayIcon.BalloonTipText = message;
                SysTray.s_SysTrayIcon.ShowBalloonTip(timeout);
            }
        }

        public static void LaunchExplorer(string message)
        {
            try
            {
                string[] array = message.Split('\n');
                string fullName = Directory.GetParent(array[0]).FullName;
                string fileName = "explorer.exe";
                string arguments = (array.Length != 1) ? fullName : "/Select, " + array[0];
                Process.Start(fileName, arguments);
            }
            catch (Exception ex)
            {
                Logger.Error("Error Occured, Err : " + ex.ToString());
            }
        }

        public static void AppAndExplorerLauncher(object sender, EventArgs e)
        {
            Logger.Info("Clicked on BalloonTip");
            if (SysTray.s_TrayAnimationStarted)
            {
                CloudAnnouncement.UpdateClickStats();
            }
            SysTray.StopTrayAnimation();
            try
            {
                string installSuccess = BlueStacks.hyperDroid.Locale.Strings.InstallSuccess;
                string balloonTipTitle = ((NotifyIcon)sender).BalloonTipTitle;
                string balloonTipText = ((NotifyIcon)sender).BalloonTipText;
                string text = "";
                text = ((!balloonTipText.Contains(installSuccess)) ? balloonTipTitle : balloonTipText.Substring(0, balloonTipText.LastIndexOf(installSuccess) - 1));
                if (string.Compare(text, "Successfully copied files:", true) == 0 || string.Compare(text, "Cannot copy files:", true) == 0)
                {
                    SysTray.LaunchExplorer(balloonTipText);
                }
                else if (text.Contains("Graphics Driver Checker"))
                {
                    SysTray.UpdateGraphicsDrivers();
                }
                else
                {
                    Logger.Info("Launching " + text);
                    string arg = "com.bluestacks.appmart";
                    string arg2 = "com.bluestacks.appmart.StartTopAppsActivity";
                    string fileName = HDAgent.s_InstallDir + "\\HD-RunApp.exe";
                    string text2 = default(string);
                    if (!JsonParser.GetAppInfoFromAppName(text, out arg, out text2, out arg2))
                    {
                        Logger.Error("Failed to launch app: {0}. No info found in json. Starting home app", text);
                        SysTray.StopTrayAnimation();
                        Process.Start(fileName, "-p " + arg + " -a " + arg2);
                    }
                    else
                    {
                        Process.Start(fileName, "-p " + arg + " -a " + arg2);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        private static void UpdateGraphicsDrivers()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMConfigRegKeyPath);
            int num = (int)registryKey.GetValue("FrontendServerPort", 2862);
            string url = string.Format("http://127.0.0.1:{0}/{1}", num, "updategraphicsdrivers");
            Thread thread = new Thread((ThreadStart)delegate
            {
                try
                {
                    Client.Get(url, null, false);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private static void AddResolutionContextMenu()
        {
            ToolStripMenuItem[] array = new ToolStripMenuItem[4]
			{
				new ToolStripMenuItem("640x480"),
				new ToolStripMenuItem("800x480"),
				new ToolStripMenuItem("1024x600"),
				new ToolStripMenuItem("FullScreen (Default)")
			};
            int num = SysTray.FindCurrentResolution();
            if (num != -1)
            {
                array[num].Checked = true;
            }
            array[0].Click += delegate(object o, EventArgs a)
            {
                SysTray.ChangeResolution(Resolutions.R640x480, o);
            };
            array[1].Click += delegate(object o, EventArgs a)
            {
                SysTray.ChangeResolution(Resolutions.R800x480, o);
            };
            array[2].Click += delegate(object o, EventArgs a)
            {
                SysTray.ChangeResolution(Resolutions.R1024x600, o);
            };
            array[3].Click += delegate(object o, EventArgs a)
            {
                SysTray.ChangeResolution(Resolutions.FullScreen, o);
            };
            ToolStripMenuItem value = new ToolStripMenuItem("Resolution", null, array);
            SysTray.s_ContextMenuStrip.Items.Add(value);
        }

        private static void AddAppUninstallContextMenu()
        {
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("Uninstall an App");
            toolStripMenuItem.Click += delegate
            {
                SysTray.UninstallApp();
            };
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void AddGpsContextMenu()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
            int num = (int)registryKey.GetValue("GpsMode");
            if (num == 1)
            {
                ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("GPS Settings");
                toolStripMenuItem.Click += delegate
                {
                    GpsSettings gpsSettings = new GpsSettings();
                    gpsSettings.ShowDialog();
                };
                SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
            }
        }

        private static void AddFreeForBetaContextMenu()
        {
            string freeDuringBeta = BlueStacks.hyperDroid.Locale.Strings.FreeDuringBeta;
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(freeDuringBeta);
            toolStripMenuItem.Enabled = false;
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void AddUpdatesContextMenu()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Updater");
            string text = (string)registryKey.GetValue("Status", BlueStacks.hyperDroid.Locale.Strings.CheckForUpdates);
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(text);
            if (text == BlueStacks.hyperDroid.Locale.Strings.CheckForUpdates)
            {
                toolStripMenuItem.Click += delegate
                {
                    Thread thread2 = new Thread((ThreadStart)delegate
                    {
                        Manager.DoWorkflow(true);
                    });
                    thread2.IsBackground = true;
                    thread2.Start();
                };
            }
            else if (text == BlueStacks.hyperDroid.Locale.Strings.InstallUpdates)
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string path = folderPath + "\\BlueStacksSetup";
                string fileName = Path.GetFileName(new Uri(Manifest.URL).LocalPath);
                string setupPath = Path.Combine(path, fileName);
                if (File.Exists(setupPath))
                {
                    toolStripMenuItem.Click += delegate
                    {
                        Manager.UpdateBlueStacks(setupPath);
                    };
                }
                else
                {
                    Logger.Info("{0} not found. Adding default context menu", setupPath);
                    toolStripMenuItem.Text = BlueStacks.hyperDroid.Locale.Strings.CheckForUpdates;
                    toolStripMenuItem.Click += delegate
                    {
                        Thread thread = new Thread((ThreadStart)delegate
                        {
                            Manager.DoWorkflow(true);
                        });
                        thread.IsBackground = true;
                        thread.Start();
                    };
                }
            }
            else
            {
                toolStripMenuItem.Enabled = false;
            }
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void AddBstUsageContextMenu()
        {
            ToolStripSeparator value = new ToolStripSeparator();
            SysTray.s_ContextMenuStrip.Items.Add(value);
            int installedAppCount = JsonParser.GetInstalledAppCount();
            string text = installedAppCount + " " + ((installedAppCount > 1) ? BlueStacks.hyperDroid.Locale.Strings.Apps : BlueStacks.hyperDroid.Locale.Strings.App) + " " + BlueStacks.hyperDroid.Locale.Strings.Installed;
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(text);
            toolStripMenuItem.Enabled = false;
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void AddRestartBlueStacksContextMenu()
        {
            string restartBlueStacks = BlueStacks.hyperDroid.Locale.Strings.RestartBlueStacks;
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(restartBlueStacks);
            toolStripMenuItem.Click += delegate
            {
                SysTray.RestartBlueStacks();
            };
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void RestartBlueStacks()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string str = (string)registryKey.GetValue("InstallDir");
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = str + "HD-Restart.exe";
            processStartInfo.Arguments = "Android";
            Logger.Info("SysTray: Starting " + processStartInfo.FileName);
            Process.Start(processStartInfo);
        }

        private static void AddPortraitModeContextMenu()
        {
            string rotatePortraitApps = BlueStacks.hyperDroid.Locale.Strings.RotatePortraitApps;
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(rotatePortraitApps);
            string name2 = "Software\\BlueStacks\\Guests\\Android\\FrameBuffer\\0";
            string name = "EmulatePortraitMode";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(name2, true);
            ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem(BlueStacks.hyperDroid.Locale.Strings.GetLocalizedString("RotatePortraitAppsAutomatic"));
            toolStripMenuItem2.Checked = (key.GetValue(name) == null);
            toolStripMenuItem2.Click += delegate
            {
                key.DeleteValue(name, false);
            };
            ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem(BlueStacks.hyperDroid.Locale.Strings.GetLocalizedString("RotatePortraitAppsEnabled"));
            toolStripMenuItem3.Checked = ((int)key.GetValue(name, 0) != 0);
            toolStripMenuItem3.Click += delegate
            {
                key.SetValue(name, 1, RegistryValueKind.DWord);
            };
            ToolStripMenuItem toolStripMenuItem4 = new ToolStripMenuItem(BlueStacks.hyperDroid.Locale.Strings.GetLocalizedString("RotatePortraitAppsDisabled"));
            toolStripMenuItem4.Checked = ((int)key.GetValue(name, 1) == 0);
            toolStripMenuItem4.Click += delegate
            {
                key.SetValue(name, 0, RegistryValueKind.DWord);
            };
            toolStripMenuItem.DropDown.Items.Add(toolStripMenuItem2);
            toolStripMenuItem.DropDown.Items.Add(toolStripMenuItem3);
            toolStripMenuItem.DropDown.Items.Add(toolStripMenuItem4);
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void AddZipLogsContextMenu()
        {
            string uploadDebugLogs = BlueStacks.hyperDroid.Locale.Strings.UploadDebugLogs;
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(uploadDebugLogs);
            toolStripMenuItem.Click += delegate
            {
                SysTray.ZipLogsToEmail();
            };
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void ZipLogsToEmail()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string str = (string)registryKey.GetValue("InstallDir");
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = str + "HD-LogCollector.exe";
            Logger.Info("SysTray: Starting " + processStartInfo.FileName);
            Process.Start(processStartInfo);
        }

        private static bool AddOptionalContextMenu()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
            bool result = false;
            object value = registryKey.GetValue("User1");
            if (value != null)
            {
                string val = (string)value;
                SysTray.AddOption("User1", val);
                result = true;
            }
            value = registryKey.GetValue("User2");
            if (value != null)
            {
                string val2 = (string)value;
                SysTray.AddOption("User2", val2);
                result = true;
            }
            value = registryKey.GetValue("User3");
            if (value != null)
            {
                string val3 = (string)value;
                SysTray.AddOption("User3", val3);
                result = true;
            }
            value = registryKey.GetValue("User4");
            if (value != null)
            {
                string val4 = (string)value;
                SysTray.AddOption("User4", val4);
                result = true;
            }
            value = registryKey.GetValue("User5");
            if (value != null)
            {
                string val5 = (string)value;
                SysTray.AddOption("User5", val5);
                result = true;
            }
            return result;
        }

        private static void AddOption(string file, string val)
        {
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(val);
            toolStripMenuItem.Click += delegate
            {
                SysTray.RunBatchFile(file);
            };
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void AddStopContextMenu()
        {
            string quitBlueStacks = BlueStacks.hyperDroid.Locale.Strings.QuitBlueStacks;
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(quitBlueStacks);
            toolStripMenuItem.Click += delegate
            {
                SysTray.Quit(false);
            };
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void AddSMSSetupMenu()
        {
            string sMSSetupMenu = BlueStacks.hyperDroid.Locale.Strings.SMSSetupMenu;
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(sMSSetupMenu);
            toolStripMenuItem.Click += delegate
            {
                SMSSettings sMSSettings = new SMSSettings();
                sMSSettings.ShowDialog();
            };
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void AddPauseSyncingMenu()
        {
            RegistryKey key = Registry.LocalMachine.CreateSubKey(BlueStacks.hyperDroid.Common.Strings.AppSyncRegKeyPath);
            string isPaused = (string)key.GetValue("NCPaused", "no");
            string text = (string.Compare(isPaused, "yes", true) != 0) ? BlueStacks.hyperDroid.Locale.Strings.PauseSync : BlueStacks.hyperDroid.Locale.Strings.ResumeSync;
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(text);
            toolStripMenuItem.Click += delegate
            {
                if (string.Compare(isPaused, "yes", true) == 0)
                {
                    key.SetValue("NCPaused", "no");
                }
                else
                {
                    key.SetValue("NCPaused", "yes");
                }
            };
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void AddNotificationsMenu()
        {
            string showNotificationsMenu = BlueStacks.hyperDroid.Locale.Strings.ShowNotificationsMenu;
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(showNotificationsMenu);
            toolStripMenuItem.Click += delegate
            {
                SysTray.ShowNotifications();
            };
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void AddTwitterFeedsMenu()
        {
            string text = "Twitter Settings";
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(text);
            toolStripMenuItem.Click += delegate
            {
                TwitterSettings twitterSettings = new TwitterSettings();
                twitterSettings.ShowDialog();
            };
            SysTray.s_ContextMenuStrip.Items.Add(toolStripMenuItem);
        }

        private static void ShowNotifications()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.CloudRegKeyPath);
            string arg = (string)registryKey.GetValue("Host");
            string fileName = arg + "/" + BlueStacks.hyperDroid.Common.Strings.FoneLinkUrl + "?guid=" + User.GUID;
            Process.Start(fileName);
        }

        private static void RunBatchFile(string file)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string arg = (string)registryKey.GetValue("InstallDir");
            Logger.Info("Trying to launch: \"" + arg + file + ".bat\"");
            Process process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "\"" + arg + file + ".bat\"";
            process.Start();
            process.WaitForExit();
        }

        public static void DisposeIcon()
        {
            SysTray.s_SysTrayIcon.Dispose();
        }

        private static void Quit(bool exitSelfProcess)
        {
            Logger.Info("SysTray: Exiting BlueStacks");
            BlueStacks.hyperDroid.Common.Utils.StopService("bsthdandroidsvc");
            BlueStacks.hyperDroid.Common.Utils.KillProcessesByName(new string[5]
			{
				"HD-ApkHandler",
				"HD-Adb",
				"HD-Frontend",
				"HD-Restart",
				"HD-RunApp"
			});
            if (exitSelfProcess)
            {
                SysTray.s_SysTrayIcon.Dispose();
                Application.Exit();
            }
            else
            {
                SysTray.s_SysTrayIcon.Visible = false;
            }
        }

        private static void UninstallApp()
        {
            if (!HDAgent.IsWindowOpen(BlueStacks.hyperDroid.Locale.Strings.UninstallWindowTitle))
            {
                SysTray.s_UninstallerForm = new UninstallerForm();
                SysTray.s_UninstallerForm.ShowDialog();
            }
        }

        private static void ResolutionCheckedChanged(ToolStripMenuItem item)
        {
            if (item != null && item.Checked)
            {
                foreach (ToolStripItem item2 in item.Owner.Items)
                {
                    if (!item.Equals(item2))
                    {
                        ToolStripMenuItem toolStripMenuItem = item2 as ToolStripMenuItem;
                        if (toolStripMenuItem != null)
                        {
                            toolStripMenuItem.Checked = false;
                        }
                    }
                }
            }
        }

        private static int FindCurrentResolution()
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int num = Screen.PrimaryScreen.Bounds.Height - 48;
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\FrameBuffer\\0");
            if ((int)registryKey.GetValue("Width") == 640 && (int)registryKey.GetValue("Height") == 480 && ((int)registryKey.GetValue("FullScreen") == 0 || (int)registryKey.GetValue("FullScreen") == 1))
            {
                return 0;
            }
            if ((int)registryKey.GetValue("Width") == 800 && (int)registryKey.GetValue("Height") == 480 && ((int)registryKey.GetValue("FullScreen") == 0 || (int)registryKey.GetValue("FullScreen") == 1))
            {
                return 1;
            }
            if ((int)registryKey.GetValue("Width") == 1024 && (int)registryKey.GetValue("Height") == 600 && ((int)registryKey.GetValue("FullScreen") == 0 || (int)registryKey.GetValue("FullScreen") == 1))
            {
                return 2;
            }
            if ((int)registryKey.GetValue("Width") == width && (int)registryKey.GetValue("Height") == num && (int)registryKey.GetValue("FullScreen") == 2)
            {
                return 3;
            }
            return -1;
        }

        private static void ChangeResolution(Resolutions resolution, object sender)
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int num = Screen.PrimaryScreen.Bounds.Height - 48;
            ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
            if (!toolStripMenuItem.Checked)
            {
                DialogResult dialogResult = BlueStacks.hyperDroid.Common.UI.MessageBox.ShowMessageBox(BlueStacks.hyperDroid.Locale.Strings.MessageBoxTitle, BlueStacks.hyperDroid.Locale.Strings.MessageBoxText, BlueStacks.hyperDroid.Locale.Strings.OKButtonText, BlueStacks.hyperDroid.Locale.Strings.CancelButtonText, null);
                if (dialogResult != DialogResult.Cancel)
                {
                    RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Guests\\Android\\FrameBuffer\\0");
                    switch (resolution)
                    {
                        case Resolutions.R640x480:
                            if ((int)registryKey.GetValue("Width") == 640 && (int)registryKey.GetValue("Height") == 480)
                            {
                                if ((int)registryKey.GetValue("FullScreen") == 0)
                                {
                                    return;
                                }
                                if ((int)registryKey.GetValue("FullScreen") == 1)
                                {
                                    return;
                                }
                            }
                            registryKey.SetValue("Width", 640);
                            registryKey.SetValue("Height", 480);
                            registryKey.SetValue("FullScreen", 1);
                            break;
                        case Resolutions.R800x480:
                            if ((int)registryKey.GetValue("Width") == 800 && (int)registryKey.GetValue("Height") == 480)
                            {
                                if ((int)registryKey.GetValue("FullScreen") == 0)
                                {
                                    return;
                                }
                                if ((int)registryKey.GetValue("FullScreen") == 1)
                                {
                                    return;
                                }
                            }
                            registryKey.SetValue("Width", 800);
                            registryKey.SetValue("Height", 480);
                            registryKey.SetValue("FullScreen", 1);
                            break;
                        case Resolutions.R1024x600:
                            if ((int)registryKey.GetValue("Width") == 1024 && (int)registryKey.GetValue("Height") == 600)
                            {
                                if ((int)registryKey.GetValue("FullScreen") == 0)
                                {
                                    return;
                                }
                                if ((int)registryKey.GetValue("FullScreen") == 1)
                                {
                                    return;
                                }
                            }
                            registryKey.SetValue("Width", 1024);
                            registryKey.SetValue("Height", 600);
                            registryKey.SetValue("FullScreen", 1);
                            break;
                        case Resolutions.FullScreen:
                            if ((int)registryKey.GetValue("Width") == width && (int)registryKey.GetValue("Height") == num && (int)registryKey.GetValue("FullScreen") == 2)
                            {
                                return;
                            }
                            registryKey.SetValue("Width", width);
                            registryKey.SetValue("Height", num);
                            registryKey.SetValue("FullScreen", 2);
                            break;
                    }
                    toolStripMenuItem.Checked = true;
                    SysTray.ResolutionCheckedChanged(toolStripMenuItem);
                    registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
                    string str = (string)registryKey.GetValue("InstallDir");
                    ProcessStartInfo processStartInfo = new ProcessStartInfo();
                    processStartInfo.FileName = str + "HD-Restart.exe";
                    processStartInfo.Arguments = "Android";
                    Logger.Info("SysTray: Starting " + processStartInfo.FileName);
                    Process.Start(processStartInfo);
                }
            }
        }
    }
}
