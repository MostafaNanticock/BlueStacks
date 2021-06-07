using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Launcher
{
    public class StartLauncher
    {
        [CompilerGenerated]
        private static ThreadExceptionEventHandler _003C_003E9__CachedAnonymousMethodDelegate2;

        [CompilerGenerated]
        private static UnhandledExceptionEventHandler _003C_003E9__CachedAnonymousMethodDelegate3;

        public static void Main(string[] args)
        {
            StartLauncher.InitExceptionHandlers();
            Logger.InitUserLog();
            //Logger.Info("Starting HD-StartLauncher PID {0}", Process.GetCurrentProcess().Id);
            //Logger.Info("IsAdministrator: {0}", User.IsAdministrator());
            //Logger.Info("no. of arguments = " + args.Length);

            for (int i = 0; i < args.Length; i++)
            {
                //Logger.Debug("arg{0}: {1}", i, args[i]);
            }

            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string path = (string)registryKey.GetValue("InstallDir");
            //Logger.Info("Starting HD-Agent");
            string fileName = Path.Combine(path, "HD-Agent.exe");

            try
            {
                Process.Start(fileName);
            }
            catch (Exception) { }

            string fileName2 = Path.Combine(path, "HD-RunApp.exe");
            ////Logger.Info("Starting HD-RunApp");

            if (Utils.IsOEM("Acer"))
            {
                string arg = "com.bluestacks.appmart";
                string arg2 = "com.bluestacks.appmart.MainActivity";
                Process.Start(fileName2, "-p " + arg + " -a " + arg2);
            }

            else if (args.Length >= 2)
            {
                string arg3 = args[0];
                string arg4 = args[1];
                string text = "";

                if (args.Length == 3)
                {
                    text = args[2];
                }

                if (text != "")
                {
                    Process.Start(fileName2, "-p " + arg3 + " -a " + arg4 + " -url " + text + " -nl");
                }
                else
                {
                    Process.Start(fileName2, "-p " + arg3 + " -a " + arg4 + " -nl");
                }
            }
            else
            {
                Process.Start(fileName2, "Android");
                //Process.Start(fileName2, "");
            }
            Logger.Info("Exiting HD-StartLauncher PID {0}", Process.GetCurrentProcess().Id);
        }

        private static void InitExceptionHandlers()
        {
            Application.ThreadException += delegate(object obj, ThreadExceptionEventArgs evt)
            {
                //Logger.Error("StartLauncher: Unhandled Exception:");
                //Logger.Error(evt.Exception.ToString());
                Environment.Exit(1);
            };

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += delegate(object obj, UnhandledExceptionEventArgs evt)
            {
                //Logger.Error("StartLauncher: Unhandled Exception:");
                //Logger.Error(evt.ExceptionObject.ToString());
                Environment.Exit(1);
            };
        }
    }
}
