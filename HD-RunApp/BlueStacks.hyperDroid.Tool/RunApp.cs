using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Tool
{
    public class RunApp
    {
        private static string s_AppName = "";

        private static string s_AppIcon = "";

        private static string s_AppPackage;

        private static string s_appsDotJsonFile = Path.Combine(Strings.GadgetDir, "apps.json");

        private static int s_AgentPort;

        private static string s_RunAppPath = "runapp";

        private static string s_UninstallPath = "uninstall";

        private static Dictionary<string, string> data = new Dictionary<string, string>();

        [CompilerGenerated]
        private static ThreadExceptionEventHandler _003C_003E9__CachedAnonymousMethodDelegate2;

        [CompilerGenerated]
        private static UnhandledExceptionEventHandler _003C_003E9__CachedAnonymousMethodDelegate3;

        public static int Main(string[] args)
        {
            Logger.InitUserLog();
            //Logger.Info("RunApp: Starting RunApp PID {0}", Process.GetCurrentProcess().Id);
            RunApp.InitExceptionHandlers();

            bool flag = false;
            if (args.Length == 1 && args[0].Trim().EndsWith(".bluestacks"))
            {
                RunApp.Win8LaunchApp(args[0]);
                return 0;
            }

            if (args.Length == 3 && args[2].Trim().Contains("apktoexe"))
            {
                flag = true;
            }

            if (!BlueStacks.hyperDroid.Common.Utils.IsBlueStacksInstalled())
            {
                if (flag)
                {
                    RunApp.CleanUpUninstallEntry(args);
                    Environment.Exit(0);
                }

                MessageBox.Show(Strings.BlueStacksNotFound, "BlueStacks runtime could not be detected.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Environment.Exit(0);
            }

            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.HKLMConfigRegKeyPath);
                RunApp.s_AgentPort = (int)registryKey.GetValue("AgentServerPort", 2861);
                int num = args.Length;
                //Renamed From 'flag1'
                bool NoErrors = true;
                bool hidemode = false;

                for (int i = 0; i < num; i++)
                {
                    //Logger.Debug("CMD: arg{0}: {1}", i, args[i]);
                }

                if (num > 0 && args[0].StartsWith("bluestacks:"))
                {
                    args[0] = args[0].Replace("bluestacks:", "");
                }
                string text4;

                if (num == 0)
                {
                    NoErrors = false;
                }
                else if (args[0] == "uninstall")
                {
                    if (num != 3)
                    {
                        //Logger.Error("uninstall requires 3 arguments, {0} given", num);
                        NoErrors = false;
                    }
                    else
                    {
                        string text2 = default(string);
                        string text3 = default(string);

                        if (args[1].Trim().EndsWith(".exe"))
                        {
                            string text = args[2].Trim();
                            int length = text.LastIndexOf(' ') - text.IndexOf(' ') - 1;
                            text2 = text.Substring(text.IndexOf(' ') + 1, length);
                            JsonParser.GetAppInfoFromPackageName(text2, out RunApp.s_AppName, out RunApp.s_AppIcon, out text3, out text3);
                        }
                        else
                        {
                            RunApp.s_AppName = Path.GetFileNameWithoutExtension(args[1]);
                            JsonParser.GetAppInfoFromAppName(RunApp.s_AppName, out text2, out RunApp.s_AppIcon, out text3);
                            if (args[2].Trim().Contains("apktoexe"))
                            {
                                RunApp.CleanUpUninstallEntry(args);
                            }
                        }
                        if (string.IsNullOrEmpty(text2))
                        {
                            Logger.Error("PackageName can not be null for uninstalling an app");
                            NoErrors = false;
                        }
                        else
                        {
                            if (JsonParser.IsPackageNameSystemApp(text2))
                            {
                                MessageBox.Show("Uninstalling a pre-bundled app is not supported.", "BlueStacks Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            }
                            else
                            {
                                RunApp.data.Clear();
                                RunApp.data.Add("package", text2);
                                RunApp.data.Add("KeyName", RunApp.s_AppName);
                                text4 = "http://127.0.0.1:{RunApp.s_AgentPort}/{RunApp.s_UninstallPath}";
                                Logger.Info("HDApkInstaller: Sending post request to {0}", text4);
                                GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event(RunApp.s_UninstallPath, text2, RunApp.s_AppName, 1));
                                Client.PostWithRetries(text4, RunApp.data, null, false, 10, 500);
                            }
                            Environment.Exit(0);
                        }
                    }
                }

                if (num == 1)
                {
                    RunApp.LaunchFrontend(args[0], false);
                    Environment.Exit(0);
                }

                if (num == 2 && args[1] == "hidemode")
                {
                    RunApp.LaunchFrontend(args[0], true);
                    Environment.Exit(0);
                }

                if (NoErrors && num >= 3)
                {
                    NoErrors = ((num == 4 && args[3] == "nolookup") || (num == 5 && args[4] == "nolookup") || JsonParser.GetAppData(args[1], args[2], out RunApp.s_AppName, out RunApp.s_AppIcon));
                }

                if (!NoErrors || num < 3)
                {
                    MessageBox.Show("This app is not installed. Please install the app and try again.", "BlueStacks Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Logger.Info("RunApp arguments: ");
                    foreach (string str in args)
                    {
                        Logger.Info("arg: " + str);
                    }
                    Logger.Info("App not found. Exiting RunApp");
                    Environment.Exit(0);
                }

                RunApp.s_AppPackage = args[1];
                RunApp.LaunchFrontend(args[0], hidemode);
                RunApp.data.Clear();
                RunApp.data.Add("package", args[1]);
                RunApp.data.Add("activity", args[2]);

                if (num >= 4 && args[3] != "nolookup")
                {
                    RunApp.data.Add("apkUrl", args[3]);
                }

                text4 = "http://127.0.0.1:{RunApp.s_AgentPort}/{RunApp.s_RunAppPath}";
                //Logger.Info("HDApkInstaller: Sending post request to {0}", text4);
                GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event(RunApp.s_RunAppPath, args[1], args[2], 1), Strings.GAUserAccountAppClicks);
                Client.PostWithRetries(text4, RunApp.data, null, false, 10, 500);
            }
            catch (Exception ex)
            {
                Logger.Error("Got Exception");
                Logger.Error(ex.ToString());
                if (flag)
                {
                    RunApp.CleanUpUninstallEntry(args);
                }
            }

            Environment.Exit(0);
            return 0;
        }

        public static void Win8LaunchApp(string fullFileName)
        {
            string text = fullFileName.Substring(fullFileName.LastIndexOf('\\') + 1);
            string text2 = text.Substring(0, text.IndexOf(' '));
            string text3 = text.Substring(text.IndexOf(' ') + 1, text.LastIndexOf(' ') - text.IndexOf(' '));
            Logger.Info("Launching win8 app with PackageName: {0}, activityName: {1}", text2, text3);
            RunApp.LaunchFrontend("Android", false);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("package", text2);
            dictionary.Add("activity", text3);
            string text4 = "http://127.0.0.1:2861/{RunApp.s_RunAppPath}";
            Logger.Info("Sending post request to {0}", text4);
            Client.PostWithRetries(text4, dictionary, null, false, 10, 500);
        }

        public static void CleanUpUninstallEntry(string[] args)
        {
            bool flag = false;
            string text = args[2].Substring(args[2].Trim().IndexOf('_') + 1);
            string text2 = Strings.BstPrefix + text;
            Logger.Info("Cleaning up uninstall entry for {0}", text);
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.UninstallKey);
            try
            {
                string name = Strings.UninstallKey + "\\" + text2;
                RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(name);
                string a = (string)registryKey2.GetValue("Silent");
                if (a == "yes")
                {
                    flag = true;
                }
            }
            catch (Exception)
            {
            }
            Logger.Info("Key: " + registryKey.ToString());
            registryKey.DeleteSubKeyTree(text2);
            registryKey.Close();
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string path = text + ".lnk";
            string text3 = Path.Combine(folderPath, path);
            string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            folderPath2 = Path.Combine(folderPath2, Strings.LibraryName);
            string text4 = Path.Combine(folderPath2, path);
            try
            {
                Logger.Info("Deleting shortcut file: " + text3);
                File.Delete(text3);
                Logger.Info("Deleting shortcut file: " + text4);
                File.Delete(text4);
            }
            catch (Exception ex2)
            {
                Logger.Error("Failed to remove shortcut entry. err: " + ex2.ToString());
            }
            if (!flag)
            {
                MessageBox.Show(text + " has been uninstalled.", "App Player", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private static void LaunchFrontend(string vmName, bool hidemode)
        {
            //Logger.Info("In LaunchFrontend");
            string KeyName = "Software\\BlueStacks";
            string str = default(string);
            RegistryKey registryKey;
            using (registryKey = Registry.LocalMachine.OpenSubKey(KeyName))
            {
                str = (string)registryKey.GetValue("InstallDir");
            }

            string name2 = "Software\\BlueStacks\\Guests\\Android\\Config";
            using (registryKey = Registry.LocalMachine.OpenSubKey(name2, true))
            {
                registryKey.SetValue("ServiceStoppedGracefully", 1, RegistryValueKind.DWord);
                registryKey.Flush();
            }

            string fileName = str + "\\HD-Frontend.exe";
            if (hidemode)
            {
                //Logger.Info("Starting hidden frontend");
                Process.Start(fileName, vmName + " hidemode");
            }
            else
            {
                //Logger.Info("Starting visible frontend");
                Process.Start(fileName, vmName + " \"" + RunApp.s_AppName + "\" \"" + RunApp.s_AppIcon + "\"");
            }

            string text = "BlueStacks_Frontend_Gl_Ready_" + vmName;
            //Logger.Info("Trying to open event {0}", text);
            EventWaitHandle eventWaitHandle;

            while (true)
            {
                try
                {
                    eventWaitHandle = EventWaitHandle.OpenExisting(text);
                }
                catch (WaitHandleCannotBeOpenedException ex)
                {
                    string message = ex.Message;
                    Thread.Sleep(100);
                    continue;
                }
                break;
            }

            Logger.Info("Waiting on event {0}", text);
            eventWaitHandle.WaitOne();
        }

        private static void InitExceptionHandlers()
        {
            Application.ThreadException += delegate(object obj, ThreadExceptionEventArgs evt)
            {
                Logger.Error("RunApp: Unhandled Exception:");
                Logger.Error(evt.Exception.ToString());
                Environment.Exit(1);
            };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += delegate(object obj, UnhandledExceptionEventArgs evt)
            {
                Logger.Error("RunApp: Unhandled Exception:");
                Logger.Error(evt.ExceptionObject.ToString());
                Environment.Exit(1);
            };
        }
    }
}
