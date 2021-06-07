using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using BlueStacks.hyperDroid.Common.Interop;
using BlueStacks.hyperDroid.Locale;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.ApkInstaller
{
    public class HDApkInstaller : Form
    {
        private const string logName = "HD-ApkHandler";

        private const int PROC_KILL_TIMEOUT = 10000;

        private static string s_InstallPath = "install";

        private static string s_InstallDir = null;

        private ProgressBar m_ProgressBar;

        private Label m_Label;

        private static Mutex s_HDApkInstallerLock;

        private static bool s_IsSilent;

        //[CompilerGenerated]
        //private static ThreadExceptionEventHandler _003C_003E9__CachedAnonymousMethodDelegate2;

        //[CompilerGenerated]
        //private static UnhandledExceptionEventHandler _003C_003E9__CachedAnonymousMethodDelegate3;

        public static void Main(string[] args)
        {
            if (BlueStacks.hyperDroid.Common.Utils.IsAlreadyRunning(BlueStacks.hyperDroid.Common.Strings.HDApkInstallerLockName, out HDApkInstaller.s_HDApkInstallerLock))
            {
                MessageBox.Show(BlueStacks.hyperDroid.Common.Strings.ApkHandlerAlreadyRunning, "BlueStacks Apk Handler", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Environment.Exit(-2);
            }

            Logger.InitUserLog();
            BlueStacks.hyperDroid.Locale.Strings.InitLocalization();
            Logger.Info("IsAdministrator: {0}", User.IsAdministrator());
            HDApkInstaller.InitExceptionHandlers();
            Application.EnableVisualStyles();

            if (args.Length != 1 && args.Length != 2)
                return;

            if (args.Length == 2 && args[1] == "silent")
            {
                HDApkInstaller.s_IsSilent = true;
            }
            else
            {
                HDApkInstaller.s_IsSilent = false;
            }
            HDApkInstaller mainForm = new HDApkInstaller(args[0]);

            if (!HDApkInstaller.s_IsSilent)
                Application.Run(mainForm);
        }

        private void InstallApk(object apk)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            HDApkInstaller.s_InstallDir = (string)registryKey.GetValue("InstallDir");
            Logger.Info("HDApkInstaller: Installing {0}", (string)apk);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("path", (string)apk);
            RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
            int num = (int)registryKey2.GetValue("AgentServerPort", 2861);
            string text = "http://127.0.0.1:" + num + "/" + HDApkInstaller.s_InstallPath;
            Logger.Info("HDApkInstaller: Sending post request to {0}", text);
            GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("install", (string)apk, "", 1));
            string input = Client.PostWithRetries(text, dictionary, null, false, 10, 500);
            JSonReader jSonReader = new JSonReader();
            IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
            string text2 = "";
            text2 = iJSonObject["reason"].StringValue.Trim();
            InstallerCodes installerCodes = InstallerCodes.SUCCESS_CODE;
            try
            {
                installerCodes = (InstallerCodes)Enum.Parse(typeof(InstallerCodes), text2);
            }
            catch
            {
                Logger.Error("HDApkInstaller: Failed to recognize Installer Codes : " + text2);
                Environment.Exit(-1);
            }
            base.Visible = false;
            if (installerCodes == InstallerCodes.SUCCESS_CODE)
            {
                Logger.Info("HDApkInstaller: Installation Successful");
                string text4 = "Apk " + BlueStacks.hyperDroid.Locale.Strings.InstallSuccess;
                Logger.Info("HDApkInstaller: Exit with code 0");
                Environment.Exit(0);
            }
            else
            {
                Logger.Info("HDApkInstaller: Installation Failed");
                Logger.Info("HDApkInstaller: Got Error: {0}", text2);
                if (!HDApkInstaller.s_IsSilent)
                {
                    string text3 = "Apk " + BlueStacks.hyperDroid.Locale.Strings.InstallFail + ": " + installerCodes;
                    MessageBox.Show(text3, this.Text, MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                Environment.Exit((int)installerCodes);
            }
        }

        private static void InitExceptionHandlers()
        {
            Application.ThreadException += delegate(object obj, ThreadExceptionEventArgs evt)
            {
                Logger.Error("HDApkInstaller: Unhandled Exception:");
                Logger.Error(evt.Exception.ToString());
                Environment.Exit(-1);
            };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += delegate(object obj, UnhandledExceptionEventArgs evt)
            {
                Logger.Error("HDApkInstaller: Unhandled Exception:");
                Logger.Error(evt.ExceptionObject.ToString());
                Environment.Exit(-1);
            };
        }

        private HDApkInstaller(string apk)
        {
            Window.FreeConsole();
            //if (!HDApkInstaller.s_IsSilent)
            //{
            //    this.InitializeComponents();
            //}

            this.InitializeComponent();

            if (!HDApkInstaller.s_IsSilent)
                this.Hide();

            this.Install(apk);
        }

        private void Install(string apk)
        {
            Thread thread = new Thread(this.InstallApk);
            thread.Start(apk);
        }

        private void InitializeComponent()
        {
            int height = 70;
            int num = 220;
            base.SuspendLayout();
            base.StartPosition = FormStartPosition.CenterScreen;
            base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            base.SizeGripStyle = SizeGripStyle.Hide;
            base.ShowIcon = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.ShowInTaskbar = true;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.ClientSize = new Size(num, height);
            this.Text = "BlueStacks Apk Handler";
            this.m_Label = new Label();
            this.m_Label.Location = new Point(num / 4, 5);
            this.m_Label.Size = new Size(num, 35);
            this.m_Label.Text = BlueStacks.hyperDroid.Locale.Strings.UserWaitText;
            this.m_ProgressBar = new ProgressBar();
            this.m_ProgressBar.Location = new Point(num / 4, 40);
            this.m_ProgressBar.Size = new Size(num / 2, 20);
            this.m_ProgressBar.Style = ProgressBarStyle.Marquee;
            this.m_ProgressBar.MarqueeAnimationSpeed = 25;
            base.Controls.Add(this.m_Label);
            base.Controls.Add(this.m_ProgressBar);
            base.ResumeLayout(false);
            base.PerformLayout();
            Logger.Info("HDApkInstaller: Components Initialized");
        }

        protected override void Dispose(bool disposing)
        {
            if (!HDApkInstaller.s_IsSilent)
            {
                this.m_ProgressBar.Dispose();
                base.Dispose(disposing);
                Environment.Exit(0);
            }
        }
    }
}
