using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.UI;
using BlueStacks.hyperDroid.Locale;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Updater
{
    internal class Manager
    {
        private static string s_ManifestPath;

        private static WebClient s_Client;

        private static bool s_UserClicked;

        public static void DoWorkflow()
        {
            Manager.DoWorkflow(false);
        }

        public static void DoWorkflow(bool userClicked)
        {
            Manager.s_UserClicked = userClicked;
            if (Manager.UpdateNeeded())
            {
                Manager.DownloadAndInstall(Manifest.URL);
            }
            else
            {
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Updater");
                registryKey.DeleteValue("Status", false);
                string text = (string)registryKey.GetValue("ManifestURL");
                Logger.Info("manifestURL = {0}", text);
                Manager.Start(text);
            }
        }

        private static void Start(string url)
        {
            Manager.s_ManifestPath = string.Format("{0}\\{1}", BlueStacks.hyperDroid.Common.Strings.BstUserDataDir, "manifest.ini");
            Manager.s_Client = new WebClient();
            Manager.s_Client.Headers.Add("User-Agent", "BlueStacks");
            string[] array = Manager.PossibleManifestsHelper(url);
            string[] array2 = array;
            foreach (string arg in array2)
            {
                string text = arg + "?user_guid=" + User.GUID;
                Logger.Info("Start fetching manifest {0}", text);
                try
                {
                    Manager.s_Client.DownloadFile(new Uri(text), Manager.s_ManifestPath);
                    Manager.ManifestDownloadComplete();
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Error("DownloadFile failed: {0}", ex.ToString());
                }
            }
            Logger.Info("s_UserClicked = {0}", Manager.s_UserClicked.ToString());
            if (Manager.s_UserClicked)
            {
                Manager.NoUpdatesAvailable();
            }
        }

        private static string[] PossibleManifestsHelper(string url)
        {
            string oEM = Utils.OEM;
            string[] array = new string[2]
			{
				null,
				url + ".ini"
			};
            array[0] = url + "_" + Utils.OEM + ".ini";
            return array;
        }

        private static void ManifestDownloadComplete()
        {
            Logger.Info("Manifest downloaded successfully to {0}", Manager.s_ManifestPath);
            IniFile iniFile = new IniFile(Manager.s_ManifestPath);
            Manifest.Version = iniFile.GetValue("update", "version");
            Manifest.MD5 = iniFile.GetValue("update", "md5");
            Manifest.SHA1 = iniFile.GetValue("update", "sha1");
            Manifest.Size = iniFile.GetValue("update", "size");
            Manifest.URL = iniFile.GetValue("update", "url");
            Logger.Info("Manifest:\n\tversion = {0}\n\tmd5 = {1}\n\tsha1 = {2}\n\tsize = {3}\n\turl = {4}", Manifest.Version, Manifest.MD5, Manifest.SHA1, Manifest.Size, Manifest.URL);
            Manager.CheckAndInstallUpdate();
        }

        private static void CheckAndInstallUpdate()
        {
            if (Manager.UpdateNeeded())
            {
                Manager.DownloadAndInstall(Manifest.URL);
            }
            else if (Manager.s_UserClicked)
            {
                Manager.NoUpdatesAvailable();
            }
        }

        private static bool UpdateNeeded()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            System.Version v = new System.Version((string)registryKey.GetValue("Version"));
            if (Manifest.Version != null && Manifest.Version != "")
            {
                System.Version v2 = new System.Version(Manifest.Version);
                if (v2 > v)
                {
                    Logger.Info("Update needed");
                    return true;
                }
            }
            Logger.Info("Update not needed");
            return false;
        }

        private static void NoUpdatesAvailable()
        {
            Logger.Info("No updates available");
            string caption = "BlueStacks Updater";
            string text = "No new updates available";
            System.Windows.Forms.MessageBox.Show(text, caption, MessageBoxButtons.OK);
        }

        private static void DownloadAndInstall(string url)
        {
            Logger.Info("Downloading from {0}", url);
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string setupDir = folderPath + "\\BlueStacksSetup";
            string fileName = Path.GetFileName(new Uri(url).LocalPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string text = Path.Combine(setupDir, fileNameWithoutExtension);
            if (!Directory.Exists(setupDir))
            {
                Directory.CreateDirectory(setupDir);
            }
            RegistryKey key = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Updater");
            key.SetValue("Status", BlueStacks.hyperDroid.Locale.Strings.DownloadingUpdates);
            Logger.Info("Start downloading from {0}", url);
            if (File.Exists(text))
            {
                Manager.AskToInstall(text);
            }
            else
            {
                Thread thread = new Thread((ThreadStart)delegate
                {
                    int nrWorkers = 3;
                    bool downloaded = false;
                    while (!downloaded)
                    {
                        SplitDownloader splitDownloader = new SplitDownloader(url, setupDir, Utils.UserAgent(User.GUID), nrWorkers);
                        splitDownloader.Download(delegate(int percent)
                        {
                            key = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Updater");
                            key.SetValue("Status", BlueStacks.hyperDroid.Locale.Strings.DownloadingUpdates + " " + percent + "%");
                        }, delegate(string filePath)
                        {
                            try
                            {
                                downloaded = true;
                                Manager.AskToInstall(filePath);
                            }
                            catch (Exception ex2)
                            {
                                Logger.Error("Exception in AskToInstall. " + ex2.ToString());
                            }
                        }, delegate(Exception ex)
                        {
                            downloaded = false;
                            Logger.Error("Download Not Complete: " + ex.ToString());
                            Thread.Sleep(10000);
                        });
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private static void AskToInstall(string setupPath)
        {
            string mD5HashFromFile = Utils.GetMD5HashFromFile(setupPath);
            Logger.Info("md5 of downloaded file: " + mD5HashFromFile);
            Logger.Info("New version ({0}) of BlueStacks is available", Manifest.Version);
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Updater");
            registryKey.SetValue("Status", BlueStacks.hyperDroid.Locale.Strings.InstallUpdates);
            string title = "Update Available";
            string message = "Would you like to update to the latest version of BlueStacks?";
            DialogResult dialogResult = BlueStacks.hyperDroid.Common.UI.MessageBox.ShowMessageBox(title, message, "Install Now", "Remind Me Later", null);
            if (dialogResult == DialogResult.OK)
            {
                Manager.UpdateBlueStacks(setupPath);
            }
        }

        public static void UpdateBlueStacks(string setupPath)
        {
            Process.Start(setupPath);
        }
    }
}
