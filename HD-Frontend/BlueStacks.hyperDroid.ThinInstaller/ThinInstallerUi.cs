using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.ThinInstaller
{
    public class ThinInstallerUi : Form
    {
        public delegate void MessageAction();

        public enum InstallType
        {
            FULL,
            SPLIT,
            COMPLETE,
            DOWNLOAD_ONLY,
            APK_TO_EXE
        }

        public const int WM_USER = 1024;

        public const int WM_USER_START_AGENT = 1025;

        public const int WM_USER_INSTALL_AMIDEBUG = 1026;

        public const int WM_USER_INSTALL_AVG = 1027;

        public const int WM_USER_INSTALL_APP = 1028;

        public const int WM_USER_LAUNCH_APP = 1029;

        public const int WM_USER_LAUNCH_FRONTEND = 1030;

        private const int AW_HOR_POSITIVE = 1;

        private const int AW_HOR_NEGATIVE = 2;

        private const int AW_VER_POSITIVE = 4;

        private const int AW_CENTER = 16;

        private const int AW_HIDE = 65536;

        private const int AW_ACTIVATE = 131072;

        private const int AW_SLIDE = 262144;

        private const int AW_BLEND = 524288;

        public static Mutex s_ThinInstallerLock;

        public static object s_singleAction = new object();

        private static IntPtr s_ParentHandle = IntPtr.Zero;

        private static string s_CommonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        private static string s_Locale = CultureInfo.CurrentCulture.Name;

        private static Dictionary<string, string> s_LocalizedString = new Dictionary<string, string>();

        private SynchronizationContext m_FormContext;

        private PictureBox m_HeaderPBox;

        private Panel m_InstallDirPanel;

        private Panel m_CenterPanel1;

        private Panel m_CenterPanel2;

        private Label m_MarketDescLbl;

        private CheckBox m_MarketNotificationsChk;

        private CheckBox m_MarketMailChk;

        private Panel m_MarketPanel;

        private Label m_AVGHeadingLbl;

        private Label m_BrowsePathLbl;

        private LinkLabel m_AVGDescLbl;

        private CheckBox m_AVGToolbarChk;

        private TextBox m_BrowsePathtxt;

        private LinkLabel m_AVGDescLbl2;

        private Panel m_AVGPanel;

        private bool m_SetP2DM;

        private bool m_ShowP2DM = true;

        private bool m_InstallAVG;

        private bool m_P2DMOptionsShown;

        private bool m_ShowAVG;

        private bool m_ShowAVGToolbarScreen;

        private bool m_AVGOptionShown;

        private bool m_AVGToolbarInstallDone;

        private bool m_SilentInstall;

        private bool m_isUpgrade;

        private bool m_upgradeP2DM;

        private bool m_MsiDownloaded;

        private bool m_UseCustomImages;

        private string m_CustomImagesUrl;

        private Panel m_EULAPanel;

        private LinkLabel m_LinkLbl;

        private Button m_ContinueBtn;

        private Button m_NextBtn;

        private Button m_BackBtn;

        private Button m_InstallBtn;

        private Button m_BrowseBtn;

        private Button m_ExitBtn;

        private Label m_ProgressLbl;

        private ProgressBar m_ProgressBar;

        private System.Windows.Forms.Timer m_AnimationTimer;

        private int m_AnimationImgIndex;

        private bool m_ResetTimer = true;

        private bool m_FirstPanel;

        public static string s_SetupFilePath;

        private static Thread s_FakeProgressThread = null;

        private bool m_ExitAllThreads;

        private string m_HeaderText = "Installing " + ApkStrings.s_AppName + "...";

        private static string s_FontName = BlueStacks.hyperDroid.Common.Utils.GetSystemFontName();

        public static string s_ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        public static string s_SetupDir = Path.Combine(ThinInstallerUi.s_ProgramData, "BlueStacksSetup");

        public static string s_ImageDir = Path.Combine(ThinInstallerUi.s_SetupDir, "Images");

        public static bool s_GlCheck = true;

        public static bool s_SoftGl = false;

        public static bool s_WebPlugin = false;

        public static uint s_InstalledFeatures = 268435455u;

        public static bool s_ApkToExeBuild = false;

        public static bool s_RelaunchRequired = false;

        public static string s_ShortcutPath = null;

        public static string s_ApkFilePath;

        public static string s_CurrentOEM = "BlueStacks";

        public static bool s_LicenseAccepted = false;

        public static ThinInstallerUi s_ThisForm = null;

        public static string s_ProgramName = string.Format("ThinInstaller_{0}", "0.9.4.4078");

        public static string s_AmiDebugUrl = Strings.ChannelsUrl + "/jbamiapkurl";

        public static string s_AmiDebugFilePath = Path.Combine(ThinInstallerUi.s_SetupDir, "AmiDebug.apk");

        public static string s_AmiPackageName = "mpi.v23";

        public static string s_AmiActivityName = "mpi.v23.AMI";

        public static string s_MsiUrl = string.Format("http://cdn.bluestacks.com/downloads/{0}/split_msi/BlueStacks_HD_AppPlayerKK_setup_{0}_REL.msi", "0.9.4.4078");

        public static string s_AVGApkUrl = "http://cdn.bluestacks.com/public/appsettings/app-packages/3rdparty/antivirus-avgbuild.store.apk";

        private bool m_IsSpawnApps;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate25;

        [CompilerGenerated]
        private static MessageAction _003C_003E9__CachedAnonymousMethodDelegate32;

        [CompilerGenerated]
        private static MessageAction _003C_003E9__CachedAnonymousMethodDelegate33;

        [CompilerGenerated]
        private static MessageAction _003C_003E9__CachedAnonymousMethodDelegate36;

        [CompilerGenerated]
        private static MessageAction _003C_003E9__CachedAnonymousMethodDelegate37;

        [DllImport("user32.dll")]
        public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public ThinInstallerUi(InstallType installType, bool isSilent, string oem)
        {
            this.m_SilentInstall = isSilent;
            ThinInstallerUi.s_CurrentOEM = oem;
            Logger.Info("Populating Default Engilsh Strings");
            if (ThinInstallerUi.PopulateLocaleStrings("en-US"))
            {
                Logger.Info("Successfully Populated English Strings");
            }
            if (string.Compare(ThinInstallerUi.s_Locale, "en-US") != 0 && ThinInstallerUi.PopulateLocaleStrings(ThinInstallerUi.s_Locale))
            {
                Logger.Info("Successfully Populated Localized Strings");
            }
            this.m_isUpgrade = ThinInstallerUi.IsUpgrade();
            if (this.m_isUpgrade)
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.AndroidKeyBasePath);
                string input = (string)registryKey.GetValue("BootParameters");
                Regex regex = new Regex("P2DM=(\\w)");
                Match match = regex.Match(input);
                string value = match.Groups[1].Value;
                this.m_upgradeP2DM = (value == "1");
                registryKey.Close();
            }
            if (ThinInstallerUi.s_CurrentOEM == "360")
            {
                ThinInstallerUi.s_MsiUrl = string.Format("http://cdn.bluestacks.com/downloads/{0}/split_msi_360/BlueStacks_HD_AppPlayerPro_360_setup_{0}_REL.msi", "0.9.4.4078");
            }
            if (BlueStacks.hyperDroid.Common.Utils.IsRunningInSpawner())
            {
                ThinInstallerUi.s_MsiUrl = string.Format("http://cdn.bluestacks.com/downloads/SpawnAppsInstaller/{0}/split_msi/BlueStacks_HD_AppPlayerKK_setup_{0}_REL.msi", "0.9.4.4078");
                this.m_IsSpawnApps = true;
            }
            if (!Directory.Exists(ThinInstallerUi.s_SetupDir))
            {
                Directory.CreateDirectory(ThinInstallerUi.s_SetupDir);
            }
            try
            {
                string identity = new SecurityIdentifier("S-1-1-0").Translate(typeof(NTAccount)).ToString();
                Logger.Info("Setting directory permissions");
                DirectoryInfo directoryInfo = new DirectoryInfo(ThinInstallerUi.s_SetupDir);
                DirectorySecurity accessControl = directoryInfo.GetAccessControl();
                accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                directoryInfo.SetAccessControl(accessControl);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to set permissions. err: " + ex.ToString());
            }
            ThinInstallerUi.s_ThisForm = this;
            if (installType == InstallType.APK_TO_EXE)
            {
                ThinInstallerUi.s_ApkToExeBuild = true;
            }
            if (installType == InstallType.SPLIT || ThinInstallerUi.s_ApkToExeBuild)
            {
                try
                {
                    if (this.m_UseCustomImages)
                    {
                        BlueStacks.hyperDroid.Common.Utils.ExtractImages(ThinInstallerUi.s_ImageDir, "genericImages");
                    }
                    else
                    {
                        BlueStacks.hyperDroid.Common.Utils.ExtractImages(ThinInstallerUi.s_ImageDir, "installer");
                    }
                    if (this.m_UseCustomImages)
                    {
                        Thread thread = new Thread((ThreadStart)delegate
                        {
                            try
                            {
                                this.DownloadInstallerImages(this.m_CustomImagesUrl);
                            }
                            catch (Exception ex6)
                            {
                                Logger.Error("Exception in DownloadInstallerImages");
                                Logger.Error(ex6.ToString());
                            }
                        });
                        thread.IsBackground = true;
                        thread.Start();
                    }
                }
                catch (Exception ex2)
                {
                    Logger.Error("Failed to get installerImages. error: " + ex2.ToString());
                }
                if (!BlueStacks.hyperDroid.Common.Utils.IsBlueStacksInstalled())
                {
                    RegistryKey registryKey2 = Registry.LocalMachine.CreateSubKey(Strings.RegBasePath);
                    registryKey2.SetValue("InstallType", "split");
                }
            }
            this.m_FormContext = SynchronizationContext.Current;
            base.Load += this.OnLoad;
            base.Shown += this.OnShown;
            base.FormClosing += this.ClosingForm;
            Thread thread2 = new Thread((ThreadStart)delegate
            {
                this.CheckIfAgentDone();
                try
                {
                    this.UpdateProgressLabel("VerifyDependence");
                    this.StartFakeProgress(15, false);
                    if (ThinInstallerUi.s_CurrentOEM != "360" && !ThinInstallerUi.s_ApkToExeBuild && BlueStacks.hyperDroid.Common.Utils.IsP2DMEnabled())
                    {
                        this.InstallAndLaunchAmiDebug();
                    }
                }
                catch (Exception ex5)
                {
                    Logger.Error("Failed to install AmiDebug. Error: " + ex5.ToString());
                }
                base.FormClosing -= this.ClosingForm;
                base.Close();
            });
            thread2.IsBackground = true;
            thread2.Start();
            Size size = new Size(800, 600);
            int num = 12;
            int x = 50;
            int width = 500;
            int width2 = 700;
            int num2 = 130;
            int x2 = 250;
            int num3 = 86;
            int height = 30;
            if (Screen.PrimaryScreen.Bounds.Height <= 600)
            {
                size = new Size(480, 500);
                num = 9;
                x = 20;
                width = 370;
                width2 = 380;
                num2 = 100;
                x2 = 75;
                num3 = 76;
                height = 25;
            }
            else
            {
                size = new Size(800, 600);
            }
            try
            {
                base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch (Exception)
            {
            }
            string text = "";
            if (ThinInstallerUi.s_ApkToExeBuild)
            {
                text = ((!ThinInstallerUi.s_LocalizedString.ContainsKey("InstallingProduct")) ? "Installing Product " + ApkStrings.s_AppName : string.Format(ThinInstallerUi.s_LocalizedString["InstallingProduct"], ApkStrings.s_AppName));
            }
            else
            {
                try
                {
                    RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
                    text = ((!ThinInstallerUi.s_LocalizedString.ContainsKey("InstallingProduct")) ? string.Format("Installing Product {0}", (string)registryKey3.GetValue("ApkToExeName", ApkStrings.s_AppName)) : string.Format(ThinInstallerUi.s_LocalizedString["InstallingProduct"], (string)registryKey3.GetValue("ApkToExeName", ApkStrings.s_AppName)));
                }
                catch (Exception)
                {
                    text = ((!ThinInstallerUi.s_LocalizedString.ContainsKey("InstallingProduct")) ? "Installing Product " + ApkStrings.s_AppName : string.Format(ThinInstallerUi.s_LocalizedString["InstallingProduct"], ApkStrings.s_AppName));
                }
            }
            this.m_HeaderText = text;
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("Progress"))
            {
                base.Name = text.Replace(ThinInstallerUi.s_LocalizedString["Progress"], "");
                this.Text = text.Replace(ThinInstallerUi.s_LocalizedString["Progress"], "");
            }
            else
            {
                base.Name = text;
                this.Text = text;
            }
            base.MaximizeBox = false;
            base.MinimizeBox = true;
            base.ClientSize = size;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.AliceBlue;
            this.m_HeaderPBox = new PictureBox();
            this.m_HeaderPBox.BackgroundImage = this.GetImageFromFile("bg");
            this.m_HeaderPBox.Location = new Point(0, 0);
            this.m_HeaderPBox.Size = new Size(base.ClientSize.Width, 70);
            this.m_HeaderPBox.Paint += this.OnHeaderPBoxPaint;
            this.m_AVGPanel = new Panel();
            this.m_AVGPanel.Location = new Point(0, this.m_HeaderPBox.Size.Height);
            this.m_AVGPanel.Size = new Size(base.ClientSize.Width, base.ClientSize.Height - 2 * this.m_HeaderPBox.Size.Height);
            this.m_AVGPanel.BackColor = Color.AliceBlue;
            this.m_AVGPanel.Visible = false;
            this.m_AVGPanel.Enabled = false;
            this.m_AVGPanel.BackgroundImageLayout = ImageLayout.Stretch;
            this.m_MarketPanel = new Panel();
            this.m_MarketPanel.Location = new Point(0, this.m_HeaderPBox.Size.Height);
            this.m_MarketPanel.Size = new Size(base.ClientSize.Width, base.ClientSize.Height - 2 * this.m_HeaderPBox.Size.Height);
            this.m_MarketPanel.BackColor = Color.AliceBlue;
            this.m_MarketPanel.Visible = false;
            this.m_MarketPanel.Enabled = false;
            this.m_MarketPanel.BackgroundImageLayout = ImageLayout.Stretch;
            this.m_InstallDirPanel = new Panel();
            this.m_InstallDirPanel.Location = new Point(0, this.m_HeaderPBox.Size.Height);
            this.m_InstallDirPanel.Size = new Size(base.ClientSize.Width, base.ClientSize.Height - 2 * this.m_HeaderPBox.Size.Height);
            this.m_InstallDirPanel.BackColor = Color.AliceBlue;
            this.m_InstallDirPanel.Visible = false;
            this.m_InstallDirPanel.Enabled = false;
            this.m_InstallDirPanel.BackgroundImageLayout = ImageLayout.Stretch;
            this.m_CenterPanel1 = new Panel();
            this.m_CenterPanel1.Location = new Point(0, this.m_HeaderPBox.Size.Height);
            this.m_CenterPanel1.Size = new Size(base.ClientSize.Width, base.ClientSize.Height - 2 * this.m_HeaderPBox.Size.Height);
            this.m_CenterPanel1.BackColor = Color.AliceBlue;
            this.m_CenterPanel1.BackgroundImageLayout = ImageLayout.Stretch;
            if (ThinInstallerUi.s_ApkToExeBuild && !this.m_ShowAVG)
            {
                this.m_CenterPanel1.BackgroundImage = this.GetImageFromFile("SetupImage1");
            }
            else
            {
                this.m_CenterPanel1.BackgroundImage = this.GetImageFromFile("HomeScreen");
            }
            this.m_CenterPanel2 = new Panel();
            this.m_CenterPanel2.Location = new Point(0, this.m_HeaderPBox.Size.Height);
            this.m_CenterPanel2.Size = new Size(base.ClientSize.Width, base.ClientSize.Height - 2 * this.m_HeaderPBox.Size.Height);
            this.m_CenterPanel2.BackColor = Color.AliceBlue;
            if (ThinInstallerUi.s_ApkToExeBuild && !this.m_ShowAVG)
            {
                this.m_CenterPanel2.BackgroundImage = this.GetImageFromFile("SetupImage1");
            }
            else
            {
                this.m_CenterPanel2.BackgroundImage = this.GetImageFromFile("HomeScreen");
            }
            this.m_CenterPanel2.BackgroundImageLayout = ImageLayout.Stretch;
            this.m_CenterPanel2.Visible = false;
            this.m_CenterPanel2.Enabled = false;
            this.m_ProgressLbl = new Label();
            this.m_ProgressLbl.BackColor = Color.AliceBlue;
            this.m_ProgressLbl.Location = new Point(20, this.m_HeaderPBox.Size.Height + this.m_CenterPanel1.Size.Height + 10);
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("BlueStacksDownMan"))
            {
                this.m_ProgressLbl.Text = ThinInstallerUi.s_LocalizedString["BlueStacksDownMan"];
            }
            else
            {
                this.m_ProgressLbl.Text = "BlueStacksDownMan";
            }
            this.m_ProgressLbl.AutoSize = true;
            this.m_ProgressLbl.Visible = false;
            this.m_ProgressLbl.Enabled = false;
            this.m_ProgressBar = new ProgressBar();
            this.m_ProgressBar.Value = 0;
            this.m_ProgressBar.Minimum = 0;
            this.m_ProgressBar.Maximum = 100;
            this.m_ProgressBar.Width = base.Size.Width - 60;
            this.m_ProgressBar.Visible = false;
            this.m_ProgressBar.Enabled = false;
            this.m_ProgressBar.Location = new Point(20, this.m_CenterPanel1.Size.Height + this.m_HeaderPBox.Size.Height + this.m_ProgressLbl.Size.Height + 10);
            this.m_EULAPanel = new Panel();
            this.m_EULAPanel.Location = new Point(0, this.m_CenterPanel1.Bottom);
            this.m_EULAPanel.Size = new Size(base.ClientSize.Width, base.ClientSize.Height - this.m_CenterPanel1.Bottom);
            this.m_EULAPanel.BackColor = Color.White;
            this.m_LinkLbl = new LinkLabel();
            this.m_LinkLbl.Location = new Point(x, 25);
            this.m_LinkLbl.Size = new Size(width, 25);
            this.m_LinkLbl.TabIndex = 0;
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("ContinueStatement"))
            {
                this.m_LinkLbl.Text = ThinInstallerUi.s_LocalizedString["ContinueStatement"];
            }
            else
            {
                this.m_LinkLbl.Text = "ContinueStatement";
            }
            this.m_LinkLbl.DisabledLinkColor = Color.Red;
            this.m_LinkLbl.VisitedLinkColor = Color.Blue;
            this.m_LinkLbl.LinkBehavior = LinkBehavior.HoverUnderline;
            this.m_LinkLbl.LinkColor = Color.Blue;
            this.m_LinkLbl.LinkClicked += this.HandleLinkClick;
            this.m_LinkLbl.Links.Add(35, 5, "http://terms.legal.bluestacks.com");
            this.m_LinkLbl.Font = new Font(ThinInstallerUi.s_FontName, 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_LinkLbl.AutoSize = true;
            this.m_ContinueBtn = new Button();
            this.m_ContinueBtn.Location = new Point(base.Right - num2, this.m_LinkLbl.Top - 5);
            this.m_ContinueBtn.Size = new Size(num3, height);
            this.m_ContinueBtn.Cursor = Cursors.Hand;
            this.m_ContinueBtn.TabIndex = 2;
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("Continue"))
            {
                this.m_ContinueBtn.Text = "&" + ThinInstallerUi.s_LocalizedString["Continue"];
            }
            else
            {
                this.m_ContinueBtn.Text = "&Continue";
            }
            this.m_ContinueBtn.UseVisualStyleBackColor = true;
            this.m_ContinueBtn.BackColor = Color.SteelBlue;
            this.m_ContinueBtn.ForeColor = Color.White;
            this.m_ContinueBtn.Font = new Font(ThinInstallerUi.s_FontName, (float)num, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_ContinueBtn.Click += this.ShowNextScreen;
            this.m_ContinueBtn.AutoSize = true;
            this.m_ContinueBtn.FlatStyle = FlatStyle.Flat;
            this.m_ContinueBtn.FlatAppearance.BorderSize = 0;
            this.m_NextBtn = new Button();
            this.m_NextBtn.Location = new Point(base.Right - num2, this.m_LinkLbl.Top - 5);
            this.m_NextBtn.Size = new Size(num3, height);
            this.m_NextBtn.TabIndex = 2;
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("Next"))
            {
                this.m_NextBtn.Text = "&" + ThinInstallerUi.s_LocalizedString["Next"];
            }
            else
            {
                this.m_NextBtn.Text = "&Next";
            }
            this.m_NextBtn.UseVisualStyleBackColor = true;
            this.m_NextBtn.Font = new Font(ThinInstallerUi.s_FontName, (float)num, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_NextBtn.Click += this.ShowInstallScreen;
            this.m_NextBtn.AutoSize = true;
            this.m_BrowseBtn = new Button();
            this.m_BrowseBtn.Location = new Point(base.Right - num2, base.Size.Height / 3 + 50);
            this.m_BrowseBtn.Size = new Size(num3, height);
            this.m_BrowseBtn.TabIndex = 2;
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("Browse"))
            {
                this.m_BrowseBtn.Text = "&" + ThinInstallerUi.s_LocalizedString["Browse"];
            }
            else
            {
                this.m_BrowseBtn.Text = "&Browse";
            }
            this.m_BrowseBtn.UseVisualStyleBackColor = true;
            this.m_BrowseBtn.Font = new Font(ThinInstallerUi.s_FontName, (float)num, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_BrowseBtn.Click += this.SetDataDirectory;
            this.m_BrowseBtn.AutoSize = true;
            this.m_InstallBtn = new Button();
            this.m_InstallBtn.Location = new Point(base.Right - num2, this.m_LinkLbl.Top - 5);
            this.m_InstallBtn.Size = new Size(num3, height);
            this.m_InstallBtn.TabIndex = 2;
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("Install"))
            {
                this.m_InstallBtn.Text = "&" + ThinInstallerUi.s_LocalizedString["Install"];
            }
            else
            {
                this.m_InstallBtn.Text = "&Install";
            }
            this.m_InstallBtn.UseVisualStyleBackColor = true;
            this.m_InstallBtn.Font = new Font(ThinInstallerUi.s_FontName, (float)num, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_InstallBtn.Click += this.StartInstallation;
            this.m_InstallBtn.AutoSize = true;
            this.m_InstallBtn.Visible = false;
            this.m_InstallBtn.Enabled = false;
            this.m_BackBtn = new Button();
            this.m_BackBtn.Location = new Point(num2 - num3, this.m_LinkLbl.Top - 5);
            this.m_BackBtn.Size = new Size(num3, height);
            this.m_BackBtn.TabIndex = 2;
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("Back"))
            {
                this.m_BackBtn.Text = "&" + ThinInstallerUi.s_LocalizedString["Back"];
            }
            else
            {
                this.m_BackBtn.Text = "&Back";
            }
            this.m_BackBtn.UseVisualStyleBackColor = true;
            this.m_BackBtn.Font = new Font(ThinInstallerUi.s_FontName, (float)num, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_BackBtn.Click += this.ShowNextScreen;
            this.m_BackBtn.AutoSize = true;
            this.m_BackBtn.Visible = false;
            this.m_BackBtn.Enabled = false;
            this.m_ExitBtn = new Button();
            this.m_ExitBtn.Location = new Point(this.m_ContinueBtn.Right + 20, this.m_LinkLbl.Top - 2);
            this.m_ExitBtn.Name = "button2";
            this.m_ExitBtn.Size = new Size(75, 23);
            this.m_ExitBtn.TabIndex = 3;
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("Exit"))
            {
                this.m_ExitBtn.Text = "&" + ThinInstallerUi.s_LocalizedString["Exit"];
            }
            else
            {
                this.m_ExitBtn.Text = "&Exit";
            }
            this.m_ExitBtn.UseVisualStyleBackColor = true;
            this.m_ExitBtn.Click += this.QuitInstaller;
            base.Controls.Add(this.m_AVGPanel);
            base.Controls.Add(this.m_MarketPanel);
            base.Controls.Add(this.m_InstallDirPanel);
            base.Controls.Add(this.m_CenterPanel1);
            base.Controls.Add(this.m_CenterPanel2);
            base.Controls.Add(this.m_ProgressLbl);
            base.Controls.Add(this.m_ProgressBar);
            base.Controls.Add(this.m_HeaderPBox);
            base.Controls.Add(this.m_EULAPanel);
            this.m_EULAPanel.Controls.Add(this.m_LinkLbl);
            this.m_EULAPanel.Controls.Add(this.m_ContinueBtn);
            this.m_EULAPanel.Controls.Add(this.m_InstallBtn);
            this.m_EULAPanel.Controls.Add(this.m_NextBtn);
            this.m_EULAPanel.Controls.Add(this.m_BackBtn);
            this.m_MarketDescLbl = new Label();
            this.m_MarketDescLbl.BackColor = Color.AliceBlue;
            this.m_MarketDescLbl.Location = new Point(x2, 150);
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("RunsBest"))
            {
                this.m_MarketDescLbl.Text = ThinInstallerUi.s_LocalizedString["RunsBest"];
            }
            else
            {
                this.m_MarketDescLbl.Text = "RunsBest";
            }
            this.m_MarketDescLbl.AutoSize = true;
            this.m_MarketDescLbl.Font = new Font(ThinInstallerUi.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_MarketMailChk = new CheckBox();
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("AppStore"))
            {
                this.m_MarketMailChk.Text = ThinInstallerUi.s_LocalizedString["AppStore"];
            }
            else
            {
                this.m_MarketMailChk.Text = "AppStore";
            }
            this.m_MarketMailChk.Location = new Point(x2, 200);
            this.m_MarketMailChk.Size = new Size(200, 25);
            this.m_MarketMailChk.Checked = true;
            this.m_MarketMailChk.Font = new Font(ThinInstallerUi.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_MarketNotificationsChk = new CheckBox();
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("AppNotify"))
            {
                this.m_MarketNotificationsChk.Text = ThinInstallerUi.s_LocalizedString["AppNotify"];
            }
            else
            {
                this.m_MarketNotificationsChk.Text = "AppNotify";
            }
            this.m_MarketNotificationsChk.Location = new Point(x2, 225);
            this.m_MarketNotificationsChk.Size = new Size(200, 25);
            this.m_MarketNotificationsChk.Checked = true;
            this.m_MarketNotificationsChk.Font = new Font(ThinInstallerUi.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_MarketPanel.Controls.Add(this.m_MarketDescLbl);
            this.m_MarketPanel.Controls.Add(this.m_MarketMailChk);
            this.m_MarketPanel.Controls.Add(this.m_MarketNotificationsChk);
            this.m_AVGHeadingLbl = new Label();
            this.m_AVGHeadingLbl.BackColor = Color.AliceBlue;
            this.m_AVGHeadingLbl.Location = new Point(50, 10);
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("BlueStacksAVG"))
            {
                this.m_AVGHeadingLbl.Text = ThinInstallerUi.s_LocalizedString["BlueStacksAVG"];
            }
            else
            {
                this.m_AVGHeadingLbl.Text = "BlueStacksAVG";
            }
            this.m_AVGHeadingLbl.AutoSize = true;
            this.m_AVGHeadingLbl.Font = new Font(ThinInstallerUi.s_FontName, 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.m_AVGDescLbl = new LinkLabel();
            this.m_AVGDescLbl.BackColor = Color.AliceBlue;
            this.m_AVGDescLbl.Location = new Point(50, this.m_AVGHeadingLbl.Bottom);
            this.m_AVGDescLbl.MaximumSize = new Size(width2, 0);
            this.m_AVGDescLbl.TabIndex = 0;
            this.m_AVGDescLbl.DisabledLinkColor = Color.Red;
            this.m_AVGDescLbl.VisitedLinkColor = Color.Blue;
            this.m_AVGDescLbl.LinkBehavior = LinkBehavior.HoverUnderline;
            this.m_AVGDescLbl.LinkColor = Color.Blue;
            this.m_AVGDescLbl.LinkClicked += this.HandleLinkClick;
            this.m_AVGDescLbl.Links.Add(48, 10, "http://www.avg.com/ww-en/antivirus-for-android-pro");
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("LaunchAVG"))
            {
                this.m_AVGDescLbl.Text = ThinInstallerUi.s_LocalizedString["LaunchAVG"] + "\n\n\n";
            }
            else
            {
                this.m_AVGDescLbl.Text = "LaunchAVG\n\n\n";
            }
            this.m_AVGDescLbl.Font = new Font(ThinInstallerUi.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_AVGDescLbl.AutoSize = true;
            this.m_BrowsePathLbl = new Label();
            this.m_BrowsePathLbl.BackColor = Color.AliceBlue;
            this.m_BrowsePathLbl.Location = new Point(30, base.Size.Height / 3 - 40);
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("DataFolderLocation"))
            {
                this.m_BrowsePathLbl.Text = ThinInstallerUi.s_LocalizedString["DataFolderLocation"];
            }
            else
            {
                this.m_BrowsePathLbl.Text = "DataFolderLocation";
            }
            this.m_BrowsePathLbl.AutoSize = true;
            this.m_BrowsePathLbl.Font = new Font(ThinInstallerUi.s_FontName, 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.m_BrowsePathtxt = new TextBox();
            this.m_BrowsePathtxt.Location = new Point(30, base.Size.Height / 3);
            this.m_BrowsePathtxt.Text = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            this.m_BrowsePathtxt.Font = new Font(ThinInstallerUi.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_BrowsePathtxt.Size = new Size(base.Width - 70, 50);
            this.m_BrowsePathtxt.AutoSize = true;
            this.m_InstallDirPanel.Controls.Add(this.m_BrowsePathLbl);
            this.m_InstallDirPanel.Controls.Add(this.m_BrowsePathtxt);
            this.m_InstallDirPanel.Controls.Add(this.m_BrowseBtn);
            this.m_AVGToolbarChk = new CheckBox();
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("AVGDescription"))
            {
                this.m_AVGToolbarChk.Text = ThinInstallerUi.s_LocalizedString["AVGDescription"];
            }
            else
            {
                this.m_AVGToolbarChk.Text = "AVGDescription";
            }
            this.m_AVGToolbarChk.Location = new Point(50, this.m_AVGHeadingLbl.Bottom + 50);
            this.m_AVGToolbarChk.Size = new Size(width2, 150);
            this.m_AVGToolbarChk.Checked = true;
            this.m_AVGToolbarChk.Font = new Font(ThinInstallerUi.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_AVGToolbarChk.Visible = false;
            this.m_AVGToolbarChk.Enabled = false;
            this.m_AVGDescLbl2 = new LinkLabel();
            this.m_AVGDescLbl2.BackColor = Color.AliceBlue;
            this.m_AVGDescLbl2.Location = new Point(50, this.m_AVGPanel.Height - 60);
            this.m_AVGDescLbl2.MaximumSize = new Size(width2, 0);
            this.m_AVGDescLbl2.TabIndex = 0;
            this.m_AVGDescLbl2.DisabledLinkColor = Color.Red;
            this.m_AVGDescLbl2.VisitedLinkColor = Color.Blue;
            this.m_AVGDescLbl2.LinkBehavior = LinkBehavior.HoverUnderline;
            this.m_AVGDescLbl2.LinkColor = Color.Blue;
            this.m_AVGDescLbl2.LinkClicked += this.HandleLinkClick;
            this.m_AVGDescLbl2.Links.Add(93, 26, "http://www.avg.com/eu-en/eula-avg-2013-all-1-0");
            this.m_AVGDescLbl2.Links.Add(124, 14, "http://www.avg.com/eu-en/privacy");
            if (ThinInstallerUi.s_LocalizedString.ContainsKey("AVGLicenseStatement"))
            {
                this.m_AVGDescLbl2.Text = ThinInstallerUi.s_LocalizedString["AVGLicenseStatement"];
            }
            else
            {
                this.m_AVGDescLbl2.Text = "AVGLicenseStatement";
            }
            this.m_AVGDescLbl2.Font = new Font(ThinInstallerUi.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.m_AVGDescLbl2.AutoSize = true;
            this.m_AVGDescLbl2.Visible = false;
            this.m_AVGDescLbl2.Enabled = false;
            if (this.m_ShowAVGToolbarScreen)
            {
                if (ThinInstallerUi.s_LocalizedString.ContainsKey("RecommendAVG"))
                {
                    LinkLabel aVGDescLbl = this.m_AVGDescLbl;
                    aVGDescLbl.Text = aVGDescLbl.Text + ThinInstallerUi.s_LocalizedString["RecommendAVG"] + "\n";
                }
                else
                {
                    LinkLabel aVGDescLbl2 = this.m_AVGDescLbl;
                    aVGDescLbl2.Text += "RecommendAVG\n";
                }
                this.m_AVGToolbarChk.Visible = true;
                this.m_AVGToolbarChk.Enabled = true;
                this.m_AVGDescLbl2.Visible = true;
                this.m_AVGDescLbl2.Enabled = true;
            }
            this.m_AVGPanel.Controls.Add(this.m_AVGHeadingLbl);
            this.m_AVGPanel.Controls.Add(this.m_AVGDescLbl);
            this.m_AVGPanel.Controls.Add(this.m_AVGToolbarChk);
            this.m_AVGPanel.Controls.Add(this.m_AVGDescLbl2);
            if (installType == InstallType.DOWNLOAD_ONLY)
            {
                if (ThinInstallerUi.s_LocalizedString.ContainsKey("DownloadRuntimeData"))
                {
                    this.m_ProgressLbl.Text = ThinInstallerUi.s_LocalizedString["DownloadRuntimeData"];
                }
                else
                {
                    this.m_ProgressLbl.Text = "DownloadRuntimeData";
                }
                this.ShowRuntimeDownloadProgressScreen();
            }
            if (this.m_SilentInstall)
            {
                base.WindowState = FormWindowState.Minimized;
                if (this.m_ShowAVG)
                {
                    this.m_InstallAVG = true;
                }
                this.m_SetP2DM = true;
                this.StartInstallation();
            }
            if (this.m_IsSpawnApps)
            {
                this.m_SetP2DM = true;
                this.StartInstallation();
            }
        }

        private static bool PopulateLocaleStrings(string locale)
        {
            try
            {
                string text = "ThininstallerStrings_" + locale + ".txt";
                if (!File.Exists(text))
                {
                    Logger.Info("File does not Exist: " + text);
                    return false;
                }
                string[] array = File.ReadAllLines(text);
                string[] array2 = array;
                foreach (string text2 in array2)
                {
                    if (text2.IndexOf("=") != -1)
                    {
                        string[] array3 = text2.Split('=');
                        ThinInstallerUi.s_LocalizedString[array3[0].Trim()] = array3[1].Trim();
                    }
                }
                Logger.Info("Successfully Stored Localized Strings");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error Occured, Err" + ex.ToString());
                return false;
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (this.m_SilentInstall)
            {
                base.Visible = false;
                base.Enabled = false;
            }
        }

        private void OnShown(object sender, EventArgs e)
        {
        }

        protected override void OnActivated(EventArgs args)
        {
            if (this.m_SilentInstall)
            {
                base.Enabled = false;
                base.Visible = false;
            }
        }

        private void ClosingForm(object o, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void DownloadInstallerImages(string baseUrl)
        {
            if (!Directory.Exists(ThinInstallerUi.s_ImageDir))
            {
                Directory.CreateDirectory(ThinInstallerUi.s_ImageDir);
            }
            Uri baseUri = new Uri(baseUrl);
            int num = 1;
            try
            {
                Uri uri;
                string text2;
                do
                {
                    num++;
                    string text = "SetupImage" + num + ".jpg";
                    uri = new Uri(baseUri, text);
                    text2 = Path.Combine(ThinInstallerUi.s_ImageDir, text);
                    Logger.Info("Trying to download {0} to {1}", uri.ToString(), text2);
                }
                while (ThinInstallerUi.DownloadRemoteImageFile(uri, text2));
                Logger.Info("Could not download {0}. Aborting.", uri.ToString());
            }
            catch (Exception)
            {
            }
        }

        private static bool DownloadRemoteImageFile(Uri uri, string fileName)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if ((httpWebResponse.StatusCode == HttpStatusCode.OK || httpWebResponse.StatusCode == HttpStatusCode.MovedPermanently || httpWebResponse.StatusCode == HttpStatusCode.Found) && httpWebResponse.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {
                using (Stream stream = httpWebResponse.GetResponseStream())
                {
                    using (Stream stream2 = File.OpenWrite(fileName))
                    {
                        byte[] array = new byte[4096];
                        int num;
                        do
                        {
                            num = stream.Read(array, 0, array.Length);
                            stream2.Write(array, 0, num);
                        }
                        while (num != 0);
                    }
                }
                return true;
            }
            return false;
        }

        private Image GetImageFromFile(string resourceName)
        {
            string filename = Path.Combine(ThinInstallerUi.s_ImageDir, resourceName + ".jpg");
            return Image.FromFile(filename, true);
        }

        private void HandleLinkClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string fileName = e.Link.LinkData as string;
            Process.Start(fileName);
        }

        private void QuitInstaller(object sender, EventArgs e)
        {
            this.SleepAndExit(-1);
        }

        public static string ExtractMsi()
        {
            string path = "BlueStacks_" + Path.GetRandomFileName();
            string text = Path.Combine(Path.GetTempPath(), path);
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            try
            {
                ResourceManager resourceManager = new ResourceManager("installerMsi", Assembly.GetExecutingAssembly());
                byte[] buffer = (byte[])resourceManager.GetObject("InstallerBinary");
                string path2 = string.Format("BlueStacks_HD_AppPlayerSplit_setup_{0}_REL.msi", "0.9.4.4078");
                string text2 = Path.Combine(text, path2);
                BinaryWriter binaryWriter = new BinaryWriter(File.Open(text2, FileMode.Create));
                binaryWriter.Write(buffer);
                binaryWriter.Close();
                return text2;
            }
            catch (Exception ex)
            {
                Logger.Error("Error when trying to extract msi. Will download the full msi instead.");
                Logger.Error(ex.ToString());
                return null;
            }
        }

        public void DownloadAndInstallMsi()
        {
            string msiManifestUrl = ThinInstallerUi.s_MsiUrl + ".manifest";
            string path = ThinInstallerUi.s_MsiUrl.Substring(ThinInstallerUi.s_MsiUrl.LastIndexOf("/") + 1);
            string text = Path.Combine(ThinInstallerUi.s_SetupDir, path);
            Logger.Info("Downloading Msi to: " + text);
            this.UpdateProgressLabel("DownloadingBlueStacks");
            if (File.Exists(text))
            {
                ThinInstallerUi.s_SetupFilePath = text;
                this.m_MsiDownloaded = true;
                ThinInstallerUi.s_RelaunchRequired = false;
                this.InstallMsiNonBlocking();
            }
            else
            {
                Thread thread = new Thread((ThreadStart)delegate
                {
                    while (!this.m_MsiDownloaded)
                    {
                        int nrWorkers = 3;
                        SplitDownloader splitDownloader = new SplitDownloader(msiManifestUrl, ThinInstallerUi.s_SetupDir, BlueStacks.hyperDroid.Common.Utils.UserAgent(User.GUID), nrWorkers);
                        splitDownloader.Download(delegate(int percent)
                        {
                            this.UpdateProgressBar(percent);
                        }, delegate(string filePath)
                        {
                            ThinInstallerUi.s_SetupFilePath = filePath;
                            this.m_MsiDownloaded = true;
                            ThinInstallerUi.s_RelaunchRequired = false;
                            this.InstallMsiNonBlocking();
                        }, delegate(Exception ex)
                        {
                            Logger.Error("MSI download failed");
                            Logger.Error(ex.ToString());
                            Thread.Sleep(2000);
                        });
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private string GetAVGLocale()
        {
            string text = CultureInfo.CurrentCulture.Name.ToLower();
            string text2 = text.Substring(text.IndexOf("-") + 1);
            switch (text2)
            {
                case "us":
                case "es":
                case "pt":
                case "ru":
                case "fr":
                case "cz":
                case "sk":
                case "ge":
                case "sp":
                case "pb":
                case "pl":
                case "sc":
                case "nl":
                case "da":
                case "it":
                case "jp":
                case "hu":
                case "ms":
                case "zt":
                case "zh":
                case "tr":
                case "ko":
                case "id":
                case "in":
                    return text2;
                default:
                    return "us";
            }
        }

        private void SetDataDirectory(object sender, EventArgs e)
        {
            Logger.Info("Inside SetDataDirectory..");
            Logger.Info("Showing Browse Folder Dialog");
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.Description = "Select the Directory where you want to store data. (Minimum 1 GB)";
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            DialogResult dialogResult = folderBrowserDialog.ShowDialog();
            if (dialogResult.Equals(DialogResult.OK))
            {
                ThinInstallerUi.s_CommonAppData = folderBrowserDialog.SelectedPath;
            }
            else
            {
                ThinInstallerUi.s_CommonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            }
            this.m_BrowsePathtxt.Text = ThinInstallerUi.s_CommonAppData;
        }

        private void ShowNextScreen(object sender, EventArgs e)
        {
            Logger.Info("Showing next screen");
            this.m_CenterPanel2.Visible = false;
            this.m_CenterPanel2.Enabled = false;
            this.m_CenterPanel1.Visible = false;
            this.m_CenterPanel1.Enabled = false;
            this.m_MarketPanel.Visible = false;
            this.m_MarketPanel.Enabled = false;
            this.m_ContinueBtn.Visible = false;
            this.m_ContinueBtn.Enabled = false;
            this.m_InstallBtn.Visible = false;
            this.m_InstallBtn.Enabled = false;
            this.m_BackBtn.Visible = false;
            this.m_BackBtn.Enabled = false;
            this.m_LinkLbl.Visible = false;
            this.m_LinkLbl.Enabled = false;
            this.m_P2DMOptionsShown = false;
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Bluestacks");
            if (registryKey != null && registryKey.GetValue("UserDataDir") != null)
            {
                ThinInstallerUi.s_CommonAppData = Directory.GetParent(Directory.GetParent((string)registryKey.GetValue("UserDataDir")).FullName).FullName;
                Logger.Info("It is an Upgrade. Skipping Next dialog");
                this.NextClicked();
            }
            else if (registryKey != null && registryKey.GetValue("UserDefinedDir") != null)
            {
                ThinInstallerUi.s_CommonAppData = (string)registryKey.GetValue("UserDefinedDir");
                Logger.Info("It is an Upgrade. Skipping Next dialog");
                this.NextClicked();
            }
            else
            {
                this.m_NextBtn.Visible = true;
                this.m_NextBtn.Enabled = true;
                this.m_InstallDirPanel.Visible = true;
                this.m_InstallDirPanel.Enabled = true;
            }
        }

        private void ShowInstallScreen(object sender, EventArgs e)
        {
            Logger.Info("Inside ShowInstallScreen");
            ThinInstallerUi.s_CommonAppData = this.m_BrowsePathtxt.Text;
            if (!Directory.Exists(ThinInstallerUi.s_CommonAppData))
            {
                MessageBox.Show(string.Format(ThinInstallerUi.s_LocalizedString["DirectoryNotFound"], ThinInstallerUi.s_CommonAppData), ThinInstallerUi.s_LocalizedString["BlueStacksInstaller"], MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                DriveInfo driveInfo = new DriveInfo(ThinInstallerUi.s_CommonAppData);
                if (driveInfo.TotalFreeSpace < 1073741824)
                {
                    MessageBox.Show(ThinInstallerUi.s_LocalizedString["NotEnoughSpace"], ThinInstallerUi.s_LocalizedString["BlueStacksInstaller"], MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    string path = Path.Combine(ThinInstallerUi.s_CommonAppData, "Check_BlueStacks.txt");
                    if (!File.Exists(path))
                    {
                        try
                        {
                            using (FileStream fileStream = File.Create(path))
                            {
                                fileStream.Close();
                            }
                            File.Delete(path);
                        }
                        catch (Exception ex)
                        {
                            Logger.Info("Exception while creating Check Prebundled, Err: " + ex.ToString());
                            MessageBox.Show(ThinInstallerUi.s_LocalizedString["NotEnoughPermissions"], ThinInstallerUi.s_LocalizedString["BlueStacksInstaller"], MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            return;
                        }
                    }
                    Logger.Info("Showing Install screen");
                    this.NextClicked();
                }
            }
        }

        private void NextClicked()
        {
            Logger.Info("Inside Next Clicked");
            if (!this.m_P2DMOptionsShown && this.m_ShowP2DM)
            {
                Logger.Info("Showing P2DM options screen");
                this.m_P2DMOptionsShown = true;
                if (BlueStacks.hyperDroid.Common.Utils.IsP2DMEnabled())
                {
                    Logger.Info("P2DM is currently enabled");
                    if (!this.m_ShowAVG)
                    {
                        this.StartInstallation();
                    }
                    else
                    {
                        this.NextClicked();
                    }
                }
                else
                {
                    Logger.Info("P2DM is currently not enabled");
                    if (!this.m_ShowAVG)
                    {
                        this.m_InstallBtn.Visible = true;
                        this.m_InstallBtn.Enabled = true;
                        this.m_BackBtn.Visible = true;
                        this.m_BackBtn.Enabled = true;
                    }
                    this.m_InstallDirPanel.Visible = false;
                    this.m_InstallDirPanel.Enabled = false;
                    this.m_NextBtn.Visible = false;
                    this.m_NextBtn.Enabled = false;
                    ThinInstallerUi.AnimateWindow(this.m_MarketPanel.Handle, 200, 262145);
                    this.m_MarketPanel.Visible = true;
                    this.m_MarketPanel.Enabled = true;
                }
            }
            else if (!this.m_AVGOptionShown && this.m_ShowAVG)
            {
                Logger.Info("Showing AVG toolbar screen");
                this.m_AVGOptionShown = true;
                this.m_NextBtn.Visible = false;
                this.m_NextBtn.Enabled = false;
                this.m_BackBtn.Visible = true;
                this.m_BackBtn.Enabled = true;
                this.m_InstallBtn.Visible = true;
                this.m_InstallBtn.Enabled = true;
                ThinInstallerUi.AnimateWindow(this.m_AVGPanel.Handle, 200, 262145);
                this.m_AVGPanel.Visible = true;
                this.m_AVGPanel.Enabled = true;
            }
            else
            {
                this.StartInstallation();
            }
        }

        private void EvaluateP2DM()
        {
            if (this.m_ShowP2DM && this.m_P2DMOptionsShown)
            {
                if (!this.m_MarketMailChk.Checked && !this.m_MarketNotificationsChk.Checked)
                {
                    return;
                }
                this.m_SetP2DM = true;
            }
        }

        private void StartInstallation(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (this.m_ShowAVGToolbarScreen && this.m_AVGToolbarChk.Checked)
            {
                Logger.Info("Will install AVG toolbar");
                this.m_InstallAVG = true;
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.RegBasePath);
                registryKey.SetValue("InstallAVG", "true");
            }
            this.StartInstallation();
        }

        private void StartInstallation()
        {
            Logger.Info("Starting installation...");
            this.EvaluateP2DM();
            this.m_InstallDirPanel.Visible = false;
            this.m_InstallDirPanel.Enabled = false;
            this.m_MarketPanel.Visible = false;
            this.m_MarketPanel.Enabled = false;
            this.m_AVGPanel.Visible = false;
            this.m_AVGPanel.Enabled = false;
            this.m_ProgressBar.Visible = true;
            this.m_ProgressBar.Enabled = true;
            this.m_ProgressLbl.Visible = true;
            this.m_ProgressLbl.Enabled = true;
            this.m_EULAPanel.Visible = false;
            this.m_EULAPanel.Enabled = false;
            this.m_AnimationImgIndex = 1;
            this.m_AnimationTimer = new System.Windows.Forms.Timer();
            this.m_AnimationTimer.Tick += this.OnAnimationTimerTick;
            this.m_AnimationTimer.Interval = 100;
            this.m_AnimationTimer.Start();
            if (BlueStacks.hyperDroid.Common.Utils.IsBlueStacksInstalled() && ThinInstallerUi.s_ApkToExeBuild)
            {
                this.CheckInstallState();
            }
            else
            {
                if (ThinInstallerUi.s_ApkToExeBuild || this.m_ShowAVG)
                {
                    ThinInstallerUi.s_RelaunchRequired = true;
                    this.SetRelaunchConfig();
                }
                try
                {
                    ThinInstallerUi.s_SetupFilePath = ThinInstallerUi.ExtractMsi();
                    if (ThinInstallerUi.s_SetupFilePath == null)
                    {
                        this.DownloadAndInstallMsi();
                    }
                    else
                    {
                        this.InstallMsiNonBlocking();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                    this.SleepAndExit(-1);
                }
            }
        }

        private bool IsAVGInstalled()
        {
            string name = "SOFTWARE\\Microsoft\\Internet Explorer\\Toolbar";
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
            if (registryKey != null)
            {
                return registryKey.GetValue("{95B7759C-8C7F-4BF1-B163-73684A933233}") != null;
            }
            return false;
        }

        private bool IsAVGEnabled()
        {
            try
            {
                string name = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Ext\\Settings\\{95B7759C-8C7F-4BF1-B163-73684A933233}";
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(name);
                if (registryKey != null)
                {
                    int num = (int)registryKey.GetValue("Flags");
                    if (num == 1)
                    {
                        Logger.Info("AVG disabled");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when trying to check if AVG enabled");
                Logger.Error(ex.Message);
                Logger.Info("Treating AVG as disabled");
                return false;
            }
            Logger.Info("AVG enabled");
            return true;
        }

        private void SleepAndExit(int errorCode)
        {
            Logger.Info("In SleepAndExit with " + errorCode);
            base.Visible = false;
            base.Enabled = false;
            Thread.Sleep(5000);
            Environment.Exit(errorCode);
        }

        private void CheckIfAgentDone()
        {
            Logger.Info("Checking if agent done");
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
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

        private void ShowRuntimeDownloadProgressScreen()
        {
            this.m_MarketPanel.Visible = false;
            this.m_MarketPanel.Enabled = false;
            this.m_ProgressBar.Visible = true;
            this.m_ProgressBar.Enabled = true;
            this.m_ProgressLbl.Visible = true;
            this.m_ProgressLbl.Enabled = true;
            this.m_EULAPanel.Visible = false;
            this.m_EULAPanel.Enabled = false;
            this.m_AnimationImgIndex = 1;
            this.m_AnimationTimer = new System.Windows.Forms.Timer();
            this.m_AnimationTimer.Tick += this.OnAnimationTimerTick;
            this.m_AnimationTimer.Interval = 100;
            this.m_AnimationTimer.Start();
        }

        private void OnHeaderPBoxPaint(object sender, PaintEventArgs e)
        {
            SolidBrush brush = new SolidBrush(Color.Black);
            float x = 0f;
            float y = 0f;
            float width = (float)this.m_HeaderPBox.Size.Width;
            float height = (float)this.m_HeaderPBox.Size.Height;
            RectangleF layoutRectangle = new RectangleF(50f, 20f, width, height);
            Pen pen = new Pen(Color.White);
            e.Graphics.DrawRectangle(pen, x, y, width, height);
            e.Graphics.DrawString(this.m_HeaderText, new Font(ThinInstallerUi.s_FontName, 18f), brush, layoutRectangle);
        }

        private void ShowThankYouScreen()
        {
            this.m_AnimationTimer.Stop();
            Image imageFromFile = this.GetImageFromFile("ThankYouImage");
            this.m_CenterPanel1.BackgroundImage = imageFromFile;
            this.m_CenterPanel2.Visible = false;
            this.m_CenterPanel2.Enabled = false;
            this.m_CenterPanel1.Visible = true;
            this.m_CenterPanel1.Enabled = true;
            ThinInstallerUi.AnimateWindow(this.m_CenterPanel1.Handle, 200, 262146);
            this.m_CenterPanel1.Visible = true;
            this.m_CenterPanel1.Enabled = true;
        }

        private void OnAnimationTimerTick(object o, EventArgs evt)
        {
            if (this.m_ResetTimer)
            {
                this.m_AnimationTimer.Interval = 5000;
                this.m_ResetTimer = false;
            }
            string resourceName = "SetupImage" + Convert.ToString(this.m_AnimationImgIndex);
            this.m_AnimationImgIndex++;
            try
            {
                Image imageFromFile = this.GetImageFromFile(resourceName);
                if (imageFromFile == null)
                {
                    this.m_AnimationImgIndex = 1;
                    resourceName = "SetupImage1";
                    imageFromFile = this.GetImageFromFile(resourceName);
                }
                if (this.m_FirstPanel)
                {
                    this.m_CenterPanel2.Visible = false;
                    this.m_CenterPanel2.Enabled = false;
                    this.m_CenterPanel1.BackgroundImage = imageFromFile;
                    ThinInstallerUi.AnimateWindow(this.m_CenterPanel1.Handle, 200, 262145);
                    this.m_CenterPanel1.Visible = true;
                    this.m_CenterPanel1.Enabled = true;
                    this.m_FirstPanel = false;
                }
                else
                {
                    this.m_CenterPanel1.Visible = false;
                    this.m_CenterPanel1.Enabled = false;
                    this.m_CenterPanel2.BackgroundImage = imageFromFile;
                    ThinInstallerUi.AnimateWindow(this.m_CenterPanel2.Handle, 200, 262145);
                    this.m_CenterPanel2.Visible = true;
                    this.m_CenterPanel2.Enabled = true;
                    this.m_FirstPanel = true;
                }
            }
            catch (Exception)
            {
                this.m_AnimationImgIndex = 1;
            }
        }

        private void InstallMsiNonBlocking()
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                this.InstallMsi();
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void StartFakeProgress(int timeToComplete, bool updateProgressLabel)
        {
            if (ThinInstallerUi.s_FakeProgressThread != null)
            {
                ThinInstallerUi.s_FakeProgressThread.Abort();
                Thread.Sleep(1500);
            }
            ThinInstallerUi.s_FakeProgressThread = new Thread((ThreadStart)delegate
            {
                int num = 0;
                int num2 = (int)Math.Ceiling(100.0 / (double)timeToComplete);
                int num3 = 0;
                while (num < timeToComplete && !this.m_ExitAllThreads)
                {
                    if (updateProgressLabel)
                    {
                        this.UpdateInstallerProgressLabel();
                    }
                    num3 = num * num2;
                    if (num3 >= 100 - num2)
                    {
                        break;
                    }
                    this.UpdateProgressBar(num3);
                    num++;
                    Thread.Sleep(1000);
                }
            });
            ThinInstallerUi.s_FakeProgressThread.IsBackground = true;
            ThinInstallerUi.s_FakeProgressThread.Start();
        }

        private void InstallMsi()
        {
            this.m_HeaderPBox.Refresh();
            this.UpdateProgressLabel("StartInstall");
            string str = "";
            if (!ThinInstallerUi.s_GlCheck)
            {
                str += " NOGLCHECK=1";
            }
            if (ThinInstallerUi.s_SoftGl)
            {
                str += " SOFTGL=1";
            }
            if (ThinInstallerUi.s_WebPlugin)
            {
                str += " WEBPLUGIN=1";
            }
            if (ThinInstallerUi.s_CommonAppData.IndexOf(' ') != -1)
            {
                ThinInstallerUi.s_CommonAppData = "\"" + ThinInstallerUi.s_CommonAppData + "\"";
            }
            str += string.Format(" P2DM={0}", this.m_SetP2DM ? "1" : "0");
            str += " FEATURES=" + ThinInstallerUi.s_InstalledFeatures;
            str += " OEM=" + ApkStrings.s_OEM;
            str += string.Format(" APPPLAYER=YES");
            str += " COMMONDATAFOLDER=" + ThinInstallerUi.s_CommonAppData;
            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "msiexec.exe";
            process.StartInfo.Arguments = "/i \"" + ThinInstallerUi.s_SetupFilePath + "\" /qn {str}";
            if (!BlueStacks.hyperDroid.Common.Utils.IsOSWinXP())
            {
                process.StartInfo.Verb = "runas";
            }
            Logger.Info("Starting installation with msiexecArgs: " + process.StartInfo.Arguments);
            this.StartFakeProgress(35, true);
            try
            {
                process.Start();
            }
            catch (Win32Exception ex)
            {
                int nativeErrorCode = ex.NativeErrorCode;
                Logger.Error("Failed to start installer error code: {0}. exception: {1}", nativeErrorCode, ex.ToString());
                GoogleAnalytics.TrackEvent(ThinInstallerUi.s_ProgramName, new GoogleAnalytics.Event("Failed", "MSIExecFailed", nativeErrorCode.ToString(), 1), Strings.GAUserAccountInstaller);
                if (nativeErrorCode == 1223)
                {
                    MessageBox.Show(ThinInstallerUi.s_LocalizedString["InstallationInterrupted"], ThinInstallerUi.s_LocalizedString["InstallationFailed"], MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    MessageBox.Show(ThinInstallerUi.s_LocalizedString["ContactInstallationFailed"], ThinInstallerUi.s_LocalizedString["InstallationFailed"], MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                this.SleepAndExit(-1);
            }
            Logger.Info("Waiting for installer to complete...");
            process.WaitForExit();
            int exitCode = process.ExitCode;
            Logger.Info("Installer exit code: " + exitCode);
            if (exitCode == 0)
            {
                ThinInstallerUi.SendMessageToParent(1025);
                try
                {
                    if (!this.m_MsiDownloaded)
                    {
                        this.RebrandToNotificationCenter();
                    }
                }
                catch (Exception ex2)
                {
                    Logger.Error("Failed to change name. err: " + ex2.ToString());
                }
                if (!this.m_MsiDownloaded)
                {
                    ThinInstallerUi.CreateRuntimeUninstallEntry();
                }
                this.DoApkToExeStuff();
            }
            else
            {
                GoogleAnalytics.TrackEventAsync(ThinInstallerUi.s_ProgramName, new GoogleAnalytics.Event("Failed", "MSIExecFailed", exitCode.ToString(), 1), Strings.GAUserAccountInstaller);
                string text = "";
                try
                {
                    EventLog eventLog = new EventLog("Application", Environment.MachineName);
                    for (int num = eventLog.Entries.Count - 1; num >= 0; num--)
                    {
                        EventLogEntry eventLogEntry = eventLog.Entries[num];
                        if (eventLogEntry.EntryType.ToString().Equals("Error") && eventLogEntry.Source.Equals("MsiInstaller"))
                        {
                            if (!(DateTime.Now.Subtract(eventLogEntry.TimeGenerated) > new TimeSpan(1, 0, 0)))
                            {
                                Logger.Info("MSI error Message :  " + eventLogEntry.Message + "\n");
                                text = eventLogEntry.Message.Substring(eventLogEntry.Message.IndexOf("--") + 3);
                            }
                            break;
                        }
                    }
                    eventLog.Close();
                }
                catch (Exception ex3)
                {
                    Logger.Error("Failed to get event logs. err: " + ex3.ToString());
                }
                if (string.IsNullOrEmpty(text))
                {
                    text = ThinInstallerUi.s_LocalizedString["FailedToInstall"];
                }
                bool flag = true;
                string fileName = default(string);
                string text2 = default(string);
                if ((text.Contains(Strings.GLUnsupportedError) || text.Contains(Strings.GLUnsupportedErrorForApkToExe)) && !BlueStacks.hyperDroid.Common.Utils.IsGraphicsDriverUptodate(out fileName, out text2, (string)null))
                {
                    flag = false;
                    DialogResult dialogResult = MessageBox.Show(text, ThinInstallerUi.s_LocalizedString["InstallationFailed"], MessageBoxButtons.YesNo, MessageBoxIcon.Hand);
                    if (dialogResult == DialogResult.Yes)
                    {
                        Logger.Info("User clicked 'Yes'");
                        Process.Start(fileName);
                    }
                }
                if (flag)
                {
                    MessageBox.Show(text, ThinInstallerUi.s_LocalizedString["InstallationFailed"], MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                this.m_ExitAllThreads = true;
                this.SleepAndExit(-1);
            }
            this.UpdateProgressBar(100);
            try
            {
                if (!this.m_MsiDownloaded)
                {
                    File.Delete(ThinInstallerUi.s_SetupFilePath);
                }
            }
            catch (Exception)
            {
            }
            if (this.m_InstallAVG)
            {
                try
                {
                    this.InstallAVGToolbar();
                }
                catch (Exception ex5)
                {
                    Logger.Error("Error when installing AVG toolbar");
                    Logger.Error(ex5.ToString());
                }
                this.m_AVGToolbarInstallDone = true;
            }
            else if (this.m_ShowAVGToolbarScreen)
            {
                ThinInstallerUi.SendAVGStats("unchecked", -1);
            }
            if (!ThinInstallerUi.s_ApkToExeBuild && !this.m_ShowAVG)
            {
                this.m_ExitAllThreads = true;
                this.SleepAndExit(0);
            }
        }

        public static void LaunchFrontend()
        {
            Logger.Info("Launching frontend");
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
            string path = (string)registryKey.GetValue("InstallDir");
            string fileName = Path.Combine(path, "HD-RunApp.exe");
            string arguments = string.Format("");
            Process.Start(fileName, arguments);
        }

        private void RebrandToNotificationCenter()
        {
            Logger.Info("product name");
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
            string[] subKeyNames = registryKey.GetSubKeyNames();
            int num = 0;
            RegistryKey registryKey2;
            while (true)
            {
                if (num < subKeyNames.Length)
                {
                    string name = subKeyNames[num];
                    registryKey2 = registryKey.OpenSubKey(name);
                    if (registryKey2 != null)
                    {
                        string strA = Convert.ToString(registryKey2.GetValue("DisplayName"));
                        if (string.Compare(strA, "BlueStacks", true) == 0)
                        {
                            break;
                        }
                    }
                    num++;
                    continue;
                }
                return;
            }
            string text = registryKey2.ToString();
            string text2 = text.Substring(text.IndexOf('\\') + 1);
            Logger.Info("Bst uninstall key: " + text2);
            RegistryKey registryKey3 = Registry.LocalMachine.CreateSubKey(text2);
            registryKey3.SetValue("DisplayName", "Notification Center");
            registryKey3.Close();
        }

        private void InstallAVGToolbar()
        {
            Logger.Info("Installing AVG toolbar");
            ResourceManager resourceManager = new ResourceManager("avg", Assembly.GetExecutingAssembly());
            byte[] buffer = (byte[])resourceManager.GetObject("InstallerBinary");
            string path = "AVGToolbarInstaller.exe";
            string text = Path.Combine(Path.GetTempPath(), path);
            BinaryWriter binaryWriter = new BinaryWriter(File.Open(text, FileMode.Create));
            binaryWriter.Write(buffer);
            binaryWriter.Close();
            base.Visible = false;
            base.Enabled = false;
            string str = "";
            str += "/SILENT ";
            str += "/PASSWORD=TB38GF9P66 ";
            str += "/INSTALL ";
            str += "/ENABLEDSP ";
            str += "/ENABLEHOMEPAGE ";
            str += "/LOCAL=" + this.GetAVGLocale() + " ";
            str += "/PROFILE=SATB ";
            str += "/BROWSER=ALL ";
            str += "/DISTRIBUTIONSOURCE=bl011 ";
            Logger.Info("Arguments for avg: " + str);
            Process process = new Process();
            process.StartInfo.FileName = text;
            process.StartInfo.Arguments = str;
            process.Start();
            process.WaitForExit();
            int exitCode = process.ExitCode;
            Logger.Info("avg exitcode = " + exitCode);
            if (exitCode == 0)
            {
                ThinInstallerUi.SendAVGStats("success", exitCode);
            }
            else
            {
                ThinInstallerUi.SendAVGStats("failure", exitCode);
            }
        }

        public static void SendAVGStats(string avgInstallStatus, int avgExitCode)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                string url = Service.Host + "/" + Strings.AVGInstallStatsUrl;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("avg_install_status", avgInstallStatus);
                dictionary.Add("avg_exit_code", Convert.ToString(avgExitCode));
                try
                {
                    Logger.Info("Sending AVG Install Stats for: {0}", avgInstallStatus);
                    string text = Client.Post(url, dictionary, null, false);
                    Logger.Info("Got AVG Install Stat response: {0}", text);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void StartAgent()
        {
            Logger.Info("Starting Agent");
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
                string path = (string)registryKey.GetValue("InstallDir");
                string fileName = Path.Combine(path, "HD-Agent.exe");
                Process.Start(fileName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        public void CheckInstallState()
        {
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.RegBasePath);
                string text = (string)registryKey.GetValue("InstallType");
                if (string.Compare(text, "uninstalled", true) == 0)
                {
                    Logger.Info("Runtime was uninstalled and is being installed again");
                    registryKey.SetValue("InstallType", "split");
                    ThinInstallerUi.s_RelaunchRequired = true;
                    this.SetRelaunchConfig();
                    ThinInstallerUi.CreateRuntimeUninstallEntry();
                    string path = (string)registryKey.GetValue("InstallDir");
                    string fileName = Path.Combine(path, "HD-Quit.exe");
                    Process process = new Process();
                    process.StartInfo.FileName = fileName;
                    process.Start();
                    process.WaitForExit();
                    fileName = Path.Combine(path, "HD-Agent.exe");
                    process = new Process();
                    process.StartInfo.FileName = fileName;
                    process.Start();
                    fileName = Path.Combine(path, "HD-Frontend.exe");
                    process = new Process();
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Arguments = "Android";
                    process.Start();
                    this.DoApkToExeStuff();
                }
                else if (string.Compare(text, "complete", true) == 0 || string.IsNullOrEmpty(text) || string.Compare(text, "full", true) == 0)
                {
                    this.DoApkToExeStuff();
                }
                else if (string.Compare(text, "split", true) == 0)
                {
                    if (ThinInstallerUi.s_ApkToExeBuild || this.m_ShowAVG)
                    {
                        ThinInstallerUi.SendMessageToParent(1025);
                        ThinInstallerUi.s_RelaunchRequired = true;
                        this.DoApkToExeStuff();
                    }
                    else
                    {
                        MessageBox.Show(ThinInstallerUi.s_LocalizedString["DownloadInProgress"], ThinInstallerUi.s_LocalizedString["InstallInProgress"], MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to check for installation status. err: " + ex.ToString());
            }
        }

        public static void CreateRuntimeUninstallEntry()
        {
            Logger.Info("Creating runtime uninstall entry");
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.RegBasePath);
                string path = (string)registryKey.GetValue("InstallDir");
                string value = Path.Combine(path, "HD-RuntimeUninstaller.exe");
                string value2 = Path.Combine(path, "BlueStacks.ico");
                string subkey = Strings.UninstallKey + "\\" + Strings.RuntimeDisplayName;
                RegistryKey registryKey2 = Registry.LocalMachine.CreateSubKey(subkey);
                registryKey2.SetValue("DisplayName", Strings.RuntimeDisplayName);
                registryKey2.SetValue("DisplayIcon", value2);
                registryKey2.SetValue("Publisher", Strings.CompanyName);
                registryKey2.SetValue("DisplayVersion", "0.9.4.4078");
                registryKey2.SetValue("UninstallString", value);
                registryKey2.Flush();
                registryKey2.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to create uninstall entry. err: " + ex.ToString());
            }
        }

        public void SetRelaunchConfig()
        {
            Logger.Info("Copying self to Setup dir");
            string executablePath = Application.ExecutablePath;
            string fileName = Path.GetFileName(executablePath);
            string text = Path.Combine(ThinInstallerUi.s_SetupDir, fileName);
            File.Copy(executablePath, text, true);
            string sourceFileName = executablePath + ".config";
            string destFileName = text + ".config";
            File.Copy(sourceFileName, destFileName, true);
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.RegBasePath);
            Logger.Info("Setting relaunch Data: " + text);
            registryKey.SetValue("ApkToExeFile", text);
            Logger.Info("Setting ApkToExe app name: " + ApkStrings.s_AppName);
            registryKey.SetValue("ApkToExeName", ApkStrings.s_AppName);
        }

        private void UpdateDownloadProgressFromReg()
        {
            int num = 0;
            this.UpdateProgressLabel("DownloadAppRuntime");
            while (true)
            {
                try
                {
                    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
                    num = (int)registryKey.GetValue("DownloadProgress", 0);
                    this.UpdateProgressBar(num);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to update download progress. Err: " + ex.ToString());
                }
                if (num != 100)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                break;
            }
            Logger.Info("Download completed 100%");
        }

        public void DoApkToExeStuff()
        {
            if (!ThinInstallerUi.s_ApkToExeBuild && !this.m_ShowAVG)
            {
                Logger.Info("Returning from DoApkToExeStuff");
            }
            else
            {
                if (this.m_AnimationTimer == null)
                {
                    Logger.Info("Starting animation");
                    this.m_AnimationTimer = new System.Windows.Forms.Timer();
                    this.m_AnimationTimer.Tick += this.OnAnimationTimerTick;
                    this.m_AnimationTimer.Interval = 6000;
                    this.m_AnimationTimer.Start();
                    Logger.Info("Animation started");
                }
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.RegBasePath);
                string text = "";
                if (ThinInstallerUi.s_RelaunchRequired)
                {
                    Thread thread = new Thread((ThreadStart)delegate
                    {
                        this.UpdateDownloadProgressFromReg();
                    });
                    thread.IsBackground = true;
                    thread.Start();
                    while (true)
                    {
                        text = (string)registryKey.GetValue("ContinueApkToExe");
                        if (string.Compare(text, "yes", true) == 0)
                        {
                            break;
                        }
                        Thread.Sleep(2000);
                    }
                    Logger.Info("Agent completed BlueStacks installation. Continuing ApkToExe now.");
                }
                try
                {
                    this.UpdateProgressLabel("VerifyDependence");
                    this.StartFakeProgress(15, false);
                    if (ThinInstallerUi.s_CurrentOEM != "360" && (!this.m_isUpgrade || !this.m_upgradeP2DM) && this.m_SetP2DM)
                    {
                        this.InstallAndLaunchAmiDebug();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to install AmiDebug. Error: " + ex.ToString());
                }
                registryKey.DeleteValue("ApkToExeFile", false);
                registryKey.DeleteValue("ContinueApkToExe", false);
                registryKey.DeleteValue("ApkToExeName", false);
                if (ApkStrings.s_ApkURL != "")
                {
                    this.DownloadApk();
                }
                if (this.m_ShowAVG)
                {
                    this.DownloadAVGApk();
                }
            }
        }

        public void DownloadAVGApk()
        {
            string path = "AVG.apk";
            ThinInstallerUi.s_ApkFilePath = Path.Combine(ThinInstallerUi.s_SetupDir, path);
            Logger.Info("Downloading Apk Prebundled to: " + ThinInstallerUi.s_ApkFilePath);
            this.UpdateProgressLabel("DownloadAVG");
            Thread thread = new Thread((ThreadStart)delegate
            {
                ThinInstallerUi thinInstallerUi = this;
                bool downloaded = false;
                while (!downloaded)
                {
                    Downloader.Download(3, ThinInstallerUi.s_AVGApkUrl, ThinInstallerUi.s_ApkFilePath, delegate(int percent)
                    {
                        this.UpdateProgressBar(percent);
                    }, delegate
                    {
                        try
                        {
                            ThinInstallerUi.SendMessageToParent(1027);
                            int num = 60;
                            while (num > 0)
                            {
                                num--;
                                if (!this.m_InstallAVG)
                                {
                                    break;
                                }
                                if (this.m_AVGToolbarInstallDone)
                                {
                                    break;
                                }
                                Thread.Sleep(2000);
                            }
                            this.m_ExitAllThreads = true;
                            this.SleepAndExit(0);
                        }
                        catch (Exception ex2)
                        {
                            Logger.Error("Exception while installing apk");
                            Logger.Error(ex2.ToString());
                        }
                    }, delegate(Exception ex)
                    {
                        Logger.Error("Failed to download Prebundled: {0}. err: {1}", ThinInstallerUi.s_ApkFilePath, ex.Message);
                        Logger.Info("Retrying failed download in 2 seonds...");
                        Thread.Sleep(2000);
                        downloaded = false;
                    });
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void InstallAVGApk(string apkFile)
        {
            string path = default(string);
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks"))
            {
                path = (string)registryKey.GetValue("InstallDir");
            }
            string arguments = "-apk \"" + apkFile + "\" -s";
            string fileName = Path.Combine(path, "HD-ApkHandler.exe");
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            int num = 60;
            int num2 = 0;
            while (num > 0)
            {
                num--;
                Logger.Info("Starting {0} with args: {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
                process.Start();
                process.WaitForExit();
                num2 = process.ExitCode;
                Logger.Info("{0} exitcode {1}", process.StartInfo.FileName, num2);
                if (num2 == 0)
                {
                    break;
                }
                Logger.Error("Apk installation failed. err: " + num2);
                Thread.Sleep(5000);
            }
        }

        public void DownloadApk()
        {
            string path = ApkStrings.s_AppName + ".apk";
            ThinInstallerUi.s_ApkFilePath = Path.Combine(ThinInstallerUi.s_SetupDir, path);
            Logger.Info("Downloading Apk Prebundled to: " + ThinInstallerUi.s_ApkFilePath);
            this.UpdateProgressLabelFormatted("DownloadingProduct", ApkStrings.s_AppName);
            Thread thread = new Thread((ThreadStart)delegate
            {
                Downloader.Download(3, ApkStrings.s_ApkURL, ThinInstallerUi.s_ApkFilePath, delegate(int percent)
                {
                    this.UpdateProgressBar(percent);
                }, delegate(string filePath)
                {
                    try
                    {
                        this.InstallAndLaunchApk(filePath);
                    }
                    catch (Exception ex2)
                    {
                        Logger.Error("Exception while installing apk: " + ex2.ToString());
                        MessageBox.Show(string.Format(ThinInstallerUi.s_LocalizedString["ErrorWhileInstallingFile"], ApkStrings.s_AppName), string.Format(ThinInstallerUi.s_LocalizedString["ProductInstallationFailed"], ApkStrings.s_AppName), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        Environment.Exit(0);
                    }
                }, delegate(Exception ex)
                {
                    Logger.Error("Failed to download Prebundled: {0}. err: {1}", ThinInstallerUi.s_ApkFilePath, ex.Message);
                    Logger.Info("Retrying failed download in 2 seonds...");
                    Thread.Sleep(2000);
                    this.DownloadApk();
                });
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void InstallAndLaunchApk(string filePath)
        {
            Logger.Info("Installing Apk...");
            this.UpdateProgressLabelFormatted("InstallingProduct", ApkStrings.s_AppName);
            this.StartFakeProgress(10, false);
            ThinInstallerUi.SendMessageToParent(1028);
            try
            {
                this.AddUninstallEntry();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to create uninstall entry. err:" + ex.ToString());
                Application.Exit();
            }
            if (string.Compare(ApkStrings.s_OEM, "bstkm", true) == 0 || string.Compare(ApkStrings.s_OEM, "Acer", true) == 0)
            {
                Logger.Info("Copying library shortcut to desktop for: " + ApkStrings.s_PackageName);
                this.CopyLibraryShortcut();
            }
            ThinInstallerUi.SendMessageToParent(1029);
            if (!this.m_ShowAVG)
            {
                this.m_ExitAllThreads = true;
                this.SleepAndExit(0);
            }
        }

        public static void LaunchApp()
        {
            if (ThinInstallerUi.s_CurrentOEM != "360")
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
                string path = (string)registryKey.GetValue("InstallDir");
                string fileName = Path.Combine(path, "HD-RunApp.exe");
                string text = "-p " + ApkStrings.s_PackageName + " -a " + ApkStrings.s_ActivityName;
                Logger.Info("Starting app with args: " + text);
                Process.Start(fileName, text);
            }
        }

        private void CopyLibraryShortcut()
        {
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
                string path = (string)registryKey.GetValue("InstallDir");
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string path2 = Strings.LibraryName + ".lnk";
                string destFileName = Path.Combine(folderPath, path2);
                File.Copy(Path.Combine(path, path2), destFileName, true);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to copy library shortcut. err: " + ex.ToString());
            }
        }

        public static void InstallApk(string apkFile, bool copyShortcut)
		{
			string path = default(string);
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks"))
			{
				path = (string)registryKey.GetValue("InstallDir");
			}
			string arguments = "-apk \""+apkFile+"\" -s";
			string fileName = Path.Combine(path, "HD-ApkHandler.exe");
			Process process = new Process();
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.FileName = fileName;
			process.StartInfo.Arguments = arguments;
			int num = 60;
			int num2 = 0;
			while (num > 0)
			{
				num--;
				Logger.Info("Starting {0} with args: {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
				process.Start();
				process.WaitForExit();
				num2 = process.ExitCode;
				Logger.Info("{0} exitcode {1}", process.StartInfo.FileName, num2);
				if (num2 == 0)
				{
					break;
				}
				Logger.Error("Apk installation failed. err: " + num2);
				if (num2 == 5)
				{
					MessageBox.Show(string.Format(ThinInstallerUi.s_LocalizedString["ProductInstallationFailed"], Strings.GLUnsupportedErrorForApkToExe), string.Format(ThinInstallerUi.s_LocalizedString["ProductInstallationFailed"], ApkStrings.s_AppName), MessageBoxButtons.OK, MessageBoxIcon.Hand);
					Application.Exit();
				}
				Thread.Sleep(5000);
			}
			if (num2 != 0)
			{
				Logger.Error("failed to install apk. err: " + num2);
				MessageBox.Show(string.Format(ThinInstallerUi.s_LocalizedString["ApkInstallFailed"], num2));
				Application.Exit();
			}
			if (copyShortcut)
			{
				try
				{
					Thread.Sleep(5000);
					string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
					string text = ApkStrings.s_AppName+".lnk";
					int num3 = 1;
					Logger.Info("Checking for most recent shortcut Prebundled...");
					while (true)
					{
						string text2 = ApkStrings.s_AppName+"-"+num3+".lnk";
						string path2 = Strings.MyAppsDir+"\\"+text2;
						path2 = Path.Combine(Strings.LibraryDir, path2);
						if (!File.Exists(path2))
						{
							break;
						}
						Logger.Info("Shortcut Prebundled: {0} exists. Checking for next possible name...", text2);
						text = text2;
						num3++;
					}
					string path3 = Strings.MyAppsDir+"\\"+text;
					path3 = Path.Combine(Strings.LibraryDir, path3);
					string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
					string text3 = Path.Combine(folderPath2, "Apps");
					string text4 = Path.Combine(folderPath, text);
					Logger.Info("Copying shortcut: {0} to {1}", path3, text4);
					int num4 = 0;
					while (num4 < 30)
					{
						try
						{
							num4++;
							Logger.Info("Copying shortcut, attempt: {0}", num4);
							File.Copy(path3, text4, true);
							ThinInstallerUi.s_ShortcutPath = path3;
							try
							{
								Logger.Info("Creating start menu shortcut...");
								if (!Directory.Exists(text3))
								{
									Directory.CreateDirectory(text3);
								}
								string text5 = Path.Combine(text3, text);
								Logger.Info("copying start menu shortcut to: " + text5);
								File.Copy(path3, text5, true);
							}
							catch (Exception ex)
							{
								Logger.Error("Failed to create start menu shortcut. err: " + ex.ToString());
							}
							return;
						}
						catch (Exception)
						{
							Thread.Sleep(1000);
						}
					}
				}
				catch (Exception ex3)
				{
					Logger.Error("Failed to create desktop shortcut. err: " + ex3.ToString());
				}
			}
		}

        private void AddUninstallEntry()
        {
            Logger.Info("Adding uninstall entry...");
            if (ThinInstallerUi.s_ShortcutPath == null)
            {
                Logger.Error("Could not add unisntall entry. App shortcut was not created. Aborting.");
            }
            else
            {
                string subkey = Strings.UninstallKeyPrefix + ApkStrings.s_AppName;
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(subkey);
                RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
                string path = (string)registryKey2.GetValue("InstallDir");
                string arg = Path.Combine(path, "HD-ApkHandler.exe");
                string s_AppName = ApkStrings.s_AppName;
                string value = "\"" + arg + "\" -u -p " + ApkStrings.s_PackageName;
                Logger.Info("icon Data: {0}\\Icons\\{1}.{2}.ico", Strings.LibraryDir, ApkStrings.s_PackageName, ApkStrings.s_ActivityName);
                string value2 = "\"" + Strings.LibraryDir + "\\Icons\\" + ApkStrings.s_PackageName + "." + ApkStrings.s_ActivityName + ".ico\"";
                string value3 = "\"" + arg + "\" Android \"" + ApkStrings.s_PackageName + "\" \"" + ApkStrings.s_ActivityName + "\"";
                registryKey.SetValue("DisplayName", ApkStrings.s_AppName);
                registryKey.SetValue("DisplayIcon", value2);
                registryKey.SetValue("Publisher", Strings.CompanyName);
                registryKey.SetValue("DisplayVersion", "0.9.4.4078");
                registryKey.SetValue("UninstallString", value);
                registryKey.SetValue("RunCommand", value3);
                registryKey.Flush();
                registryKey.Close();
            }
        }

        private void InstallAndLaunchAmiDebug()
        {
            Logger.Info("Downloading AmiDebug...");
            WebClient webClient = new WebClient();
            webClient.DownloadFile(ThinInstallerUi.s_AmiDebugUrl, ThinInstallerUi.s_AmiDebugFilePath);
            Thread thread = new Thread((ThreadStart)delegate
            {
                Logger.Info("Installing AmiDebug...");
                ThinInstallerUi.SendMessageToParent(1026);
                Logger.Info("AmiDebug Installation completed.");
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void LaunchAmiDebug()
        {
            Logger.Info("Starting AmiDebug...");
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("package", ThinInstallerUi.s_AmiPackageName);
            dictionary.Add("activity", ThinInstallerUi.s_AmiActivityName);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.HKLMConfigRegKeyPath);
            int num = (int)registryKey.GetValue("AgentServerPort", 2861);
            string arg = "runapp";
            string text = "http://127.0.0.1:" + num + "/" + arg;
            Logger.Info("ThinInstallerUi: Sending post request to {0}", text);
            Client.PostWithRetries(text, dictionary, null, false, 10, 500);
        }

        public static bool IsUpgrade()
        {
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
                string text = (string)registryKey.GetValue("Version", "");
                if (!string.IsNullOrEmpty(text))
                {
                    string version = text.Substring(0, text.LastIndexOf('.')) + ".0";
                    string version2 = "0.9.4.4078".Substring(0, "0.9.4.4078".LastIndexOf('.')) + ".0";
                    System.Version v = new System.Version(version);
                    System.Version v2 = new System.Version(version2);
                    Logger.Info("Installed Version: {0}, new version: {1}", text, "0.9.4.4078");
                    if (v2 <= v)
                    {
                        return false;
                    }
                    Logger.Info("Upgrading from version: {0} to {1}", text, "0.9.4.4078");
                    RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(Strings.HKLMConfigRegKeyPath);
                    ThinInstallerUi.s_CurrentOEM = (string)registryKey2.GetValue("OEM", "BlueStacks");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            return false;
        }

        private void UpdateInstallerProgressLabel()
        {
            if (ThinInstallerUi.s_ApkToExeBuild)
            {
                this.UpdateProgressLabel("InstallingProgress");
            }
            else
            {
                try
                {
                    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\BlueStacks", true);
                    string text = (string)registryKey.GetValue("InstallProgress", "");
                    if (string.IsNullOrEmpty(text))
                    {
                        text = "CopyFiles";
                    }
                    if (string.Compare(text, "Installation completed", true) == 0)
                    {
                        this.UpdateProgressLabel("InstallCompleted");
                        if (!ThinInstallerUi.s_ApkToExeBuild && !this.m_ShowAVG)
                        {
                            this.ShowThankYouScreen();
                        }
                        registryKey.DeleteValue("InstallProgress");
                    }
                    else if (ThinInstallerUi.s_LocalizedString.ContainsKey(text))
                    {
                        this.UpdateProgressLabelFormatted("InstallProgressBar", ThinInstallerUi.s_LocalizedString[text]);
                    }
                    else
                    {
                        this.UpdateProgressLabelFormatted("InstallProgressBar", text);
                    }
                }
                catch (Exception)
                {
                    string text = "Installing";
                }
            }
        }

        public void UpdateProgressLabelFormatted(string msg, string format)
        {
            if (ThinInstallerUi.s_LocalizedString.ContainsKey(msg) && ThinInstallerUi.s_LocalizedString[msg].IndexOf("{0}") != -1)
            {
                this.UpdateProgressLabel(string.Format(ThinInstallerUi.s_LocalizedString[msg], format));
            }
            else
            {
                this.UpdateProgressLabel(msg + ": " + format);
            }
        }

        public void UpdateProgressLabel(string msg)
        {
            SendOrPostCallback d = delegate
            {
                if (ThinInstallerUi.s_LocalizedString.ContainsKey(msg))
                {
                    this.m_ProgressLbl.Text = ThinInstallerUi.s_LocalizedString[msg];
                }
                else
                {
                    this.m_ProgressLbl.Text = msg;
                }
            };
            try
            {
                this.m_FormContext.Send(d, null);
            }
            catch (Exception)
            {
            }
        }

        public void UpdateProgressBar(int amount)
        {
            SendOrPostCallback d = delegate
            {
                this.m_ProgressBar.Value = amount;
            };
            try
            {
                this.m_FormContext.Send(d, null);
            }
            catch (Exception)
            {
            }
        }

        public static void SendMessageToParent(int message)
        {
            Logger.Info("Will try to send message {0} to {1} ", message, ThinInstallerUi.s_ParentHandle);
            if (ThinInstallerUi.s_ParentHandle == IntPtr.Zero)
            {
                ThinInstallerUi.HandleMessages(message);
            }
            else
            {
                ThinInstallerUi.SendMessage(ThinInstallerUi.s_ParentHandle, message, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public static void HandleMessages(int message)
        {
            string apkName;
            string filePath;
            switch (message)
            {
                case 1025:
                    Logger.Info("Received message WM_USER_START_AGENT");
                    ThinInstallerUi.PerformAction(delegate
                    {
                        ThinInstallerUi.StartAgent();
                    });
                    Logger.Info("Processed message WM_USER_START_AGENT");
                    break;
                case 1026:
                    Logger.Info("Received message WM_USER_INSTALL_AMIDEBUG");
                    ThinInstallerUi.PerformAction(delegate
                    {
                        ThinInstallerUi.InstallApk(ThinInstallerUi.s_AmiDebugFilePath, false);
                        ThinInstallerUi.CheckIfVendingInstalled();
                    });
                    Logger.Info("Processed message WM_USER_INSTALL_AMIDEBUG");
                    break;
                case 1027:
                    Logger.Info("Received message WM_USER_INSTALL_AVG");
                    ThinInstallerUi.PerformAction(delegate
                    {
                        apkName = "AVG.apk";
                        filePath = Path.Combine(ThinInstallerUi.s_SetupDir, apkName);
                        ThinInstallerUi.InstallAVGApk(filePath);
                    });
                    Logger.Info("Processed message WM_USER_INSTALL_AVG");
                    break;
                case 1028:
                    Logger.Info("Received message WM_USER_INSTALL_APP");
                    ThinInstallerUi.PerformAction(delegate
                    {
                        apkName = ApkStrings.s_AppName + ".apk";
                        filePath = Path.Combine(ThinInstallerUi.s_SetupDir, apkName);
                        ThinInstallerUi.InstallApk(filePath, true);
                    });
                    Logger.Info("Processed message WM_USER_INSTALL_APP");
                    break;
                case 1029:
                    Logger.Info("Received message WM_USER_LAUNCH_APP");
                    ThinInstallerUi.PerformAction(delegate
                    {
                        ThinInstallerUi.LaunchApp();
                    });
                    Logger.Info("Processed message WM_USER_LAUNCH_APP");
                    break;
                case 1030:
                    Logger.Info("Received message WM_USER_LAUNCH_FRONTEND");
                    ThinInstallerUi.PerformAction(delegate
                    {
                        ThinInstallerUi.LaunchFrontend();
                    });
                    Logger.Info("Processed message WM_USER_LAUNCH_FRONTEND");
                    break;
            }
        }

        public static void PerformAction(MessageAction messageAction)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                lock (ThinInstallerUi.s_singleAction)
                {
                    messageAction();
                }
            });
            thread.IsBackground = true;
            thread.Start();
            thread.Join();
        }

        private static void CheckIfVendingInstalled()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("package", "com.android.vending");
            string url = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + Strings.IsPackageInstalledUrl;
            int num = 180;
            while (num > 0)
            {
                num--;
                try
                {
                    string text = Client.Post(url, dictionary, null, false);
                    if (text.Contains("ok"))
                    {
                        Logger.Info("Ami completely installed");
                        return;
                    }
                }
                catch (Exception)
                {
                }
                Thread.Sleep(1000);
            }
        }
    }
}
