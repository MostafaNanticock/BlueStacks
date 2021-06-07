using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Threading;

namespace BlueStacks.hyperDroid.Agent
{
    public class AppSyncer
    {
        private static string s_appSyncDir = Path.Combine(BlueStacks.hyperDroid.Common.Strings.BstUserDataDir, "AppSync");

        private static string s_syncDoneDir = Path.Combine(AppSyncer.s_appSyncDir, "done");

        private static string s_syncRetryDir = Path.Combine(AppSyncer.s_appSyncDir, "retry");

        private static string s_syncNewDir = Path.Combine(AppSyncer.s_appSyncDir, "new");

        private static string s_syncInvalidDir = Path.Combine(AppSyncer.s_appSyncDir, "invalid");

        private static int SyncIntervalSecs
        {
            get
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Agent\\Cloud");
                return (int)registryKey.GetValue("SyncIntervalSecs", 60);
            }
        }

        public static void Sync()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
            string strA = (string)registryKey.GetValue("InstallType", "complete");
            bool flag = string.Compare(strA, "nconly", true) == 0;
            AppSyncer.SetupEnv();
            TimeSpan timeout = new TimeSpan(0, 0, 60);
            if (Utils.IsOEM("AMD"))
            {
                timeout = new TimeSpan(0, 0, 15);
            }
            while (true)
            {
                try
                {
                    RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Agent\\AppSync");
                    if ((int)registryKey2.GetValue("Enabled", 1) != 1)
                    {
                        Thread.Sleep(timeout);
                    }
                    else if (flag)
                    {
                        string a = (string)registryKey2.GetValue("NCPaused", "no");
                        if (a == "no")
                        {
                            AppSyncer.ReadSMSJob();
                        }
                    }
                    else
                    {
                        ServiceController serviceController = new ServiceController(BlueStacks.hyperDroid.Common.Strings.AndroidServiceName);
                        if (serviceController.Status != ServiceControllerStatus.Stopped && serviceController.Status != ServiceControllerStatus.StopPending)
                        {
                            AppSyncer.RetryFailedApps();
                            AppSyncer.AppSyncJob();
                        }
                        AppSyncer.ReadSMSJob();
                    }
                }
                catch (WebException ex)
                {
                    throw ex;
                }
                catch (Auth.Token.EMalformed eMalformed)
                {
                    throw eMalformed;
                }
                catch (Exception ex2)
                {
                    Logger.Error("AppSyncer: {0}", ex2.ToString());
                }
                finally
                {
                    Thread.Sleep(new TimeSpan(0, 0, AppSyncer.SyncIntervalSecs));
                }
            }
        }

        private static void ReadSMSJob()
        {
            try
            {
                Logger.Info("ReadSMSJob: Starting ReadSMSJob(SyncIntervalSecs {0})", AppSyncer.SyncIntervalSecs);
                IJSonObject iJSonObject = SMS.ReadSMS(Auth.Token.Key, Auth.Token.Secret);
                if (Service.Success(iJSonObject))
                {
                    IJSonObject iJSonObject2 = iJSonObject["sms"];
                    for (int i = 0; i < iJSonObject2.Length; i++)
                    {
                        string stringValue = iJSonObject2[i]["msg"].StringValue;
                        string stringValue2 = iJSonObject2[i]["sender"].StringValue;
                        SysTray.ShowSMSMessage("Message from " + stringValue2, stringValue);
                    }
                }
                Logger.Info("ReadSMSJob: Exiting ReadSMSJob");
            }
            catch (Exception ex)
            {
                Logger.Error("ReadSMSJob exception");
                Logger.Error(ex.ToString());
                throw ex;
            }
        }

        private static void AppSyncJob()
        {
            try
            {
                Logger.Info("AppSyncer: Starting AppSyncJob (SyncIntervalSecs {0})", AppSyncer.SyncIntervalSecs);
                IJSonObject iJSonObject = BlueStacks.hyperDroid.Cloud.Services.Sync.AppList2(Auth.Token.Key, Auth.Token.Secret);
                string text = HTTPHandler.Get(VmCmdHandler.s_ServerPort, HDAgent.s_InstalledPacakgesPath);
                JSonReader jSonReader = new JSonReader();
                IJSonObject installedApps = jSonReader.ReadAsJSonObject(text);
                if (text == null)
                {
                    Logger.Info("AppSyncer: Exiting AppSyncJob");
                }
                else
                {
                    if (Service.Success(iJSonObject))
                    {
                        AppSyncer.ProcessBluestacksApps(iJSonObject["apps"]["bluestacks"], installedApps);
                    }
                    Logger.Info("AppSyncer: Exiting AppSyncJob");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AppSyncJob exception");
                Logger.Error(ex.ToString());
                throw ex;
            }
        }

        private static void ProcessBluestacksApps(IJSonObject bluestacksApps, IJSonObject installedApps)
        {
            Logger.Info("In ProcessBluestacksApps");
            Logger.Debug(bluestacksApps.ToString());
            for (int i = 0; i < bluestacksApps.Length; i++)
            {
                string text = bluestacksApps[i]["apk_url"].StringValue.Trim();
                if (text != null && !(text == string.Empty))
                {
                    string text2 = text.Substring(text.LastIndexOf('/') + 1);
                    string stringValue = bluestacksApps[i]["app_name"].StringValue;
                    string stringValue2 = bluestacksApps[i]["app_pkg_name"].StringValue;
                    int int32Value = bluestacksApps[i]["version_code"].Int32Value;
                    int num = AppSyncer.CheckIfAppInstalled(stringValue2, int32Value, installedApps);
                    Logger.Info("installed: " + num);
                    if (bluestacksApps[i]["install_status"].IsFalse)
                    {
                        if (num == 1)
                        {
                            Logger.Info("AppSyncer: Uninstalling <{0}> <{1}>", stringValue, stringValue2);
                            AppSyncer.UninstallApk(text2, stringValue, stringValue2);
                        }
                    }
                    else
                    {
                        string text3 = Path.Combine(AppSyncer.s_syncNewDir, text2);
                        switch (num)
                        {
                            case -1:
                                return;
                            case 0:
                                {
                                    string path = Path.Combine(AppSyncer.s_syncRetryDir, text2);
                                    string path2 = Path.Combine(AppSyncer.s_syncInvalidDir, text2);
                                    if (!File.Exists(path) && !File.Exists(path2))
                                    {
                                        Logger.Info("AppSyncer: Downloading {0}", text);
                                        BlueStacks.hyperDroid.Cloud.Services.Sync.DownloadApp(text, text3, Auth.Token.Key, Auth.Token.Secret);
                                        Logger.Info("AppSyncer: Installing <{0}> <{1}>", text2, text3);
                                        AppSyncer.InstallApk(text2, text3);
                                        Logger.Info("******************** Installed {0} with version {1}", stringValue, int32Value);
                                    }
                                    else
                                    {
                                        Logger.Info("AppSyncer: Din't download {0}. Exists in retry or invalid.", text2);
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
        }

        private static void ProcessAmazonApps(IJSonObject amazonApps, IJSonObject installedApps)
        {
            Logger.Info("In ProcessAmazonApps");
            Logger.Debug(amazonApps.ToString());
            for (int i = 0; i < amazonApps.Length; i++)
            {
                string text = amazonApps[i]["amz_url"].StringValue.Trim();
                if (text != null && !(text == string.Empty))
                {
                    string stringValue = amazonApps[i]["amz_name"].StringValue;
                    string stringValue2 = amazonApps[i]["app_pkg_name"].StringValue;
                    int num = AppSyncer.CheckIfAppInstalled(stringValue2, -1, installedApps);
                    if (amazonApps[i]["install_status"].IsFalse)
                    {
                        if (num == 1)
                        {
                            Logger.Info("AppSyncer: Uninstalling <{0}> <{1}>", stringValue, stringValue2);
                            AppSyncer.UninstallApk(null, stringValue, stringValue2);
                        }
                    }
                    else if (num == -1)
                    {
                        break;
                    }
                }
            }
        }

        private static void RetryFailedApps()
        {
            Logger.Info("AppSyncer: Retrying failed installs");
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(AppSyncer.s_syncRetryDir);
                FileInfo[] files = directoryInfo.GetFiles("*.apk");
                FileInfo[] array = files;
                foreach (FileInfo fileInfo in array)
                {
                    AppSyncer.InstallApk(fileInfo.Name, fileInfo.FullName);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AppSyncer: {0}", ex.ToString());
            }
        }

        private static void UninstallApk(string fileName, string name, string pkgName)
        {
            if (fileName != null)
            {
                string file = Path.Combine(AppSyncer.s_syncNewDir, fileName);
                string file2 = Path.Combine(AppSyncer.s_syncDoneDir, fileName);
                string file3 = Path.Combine(AppSyncer.s_syncRetryDir, fileName);
                string file4 = Path.Combine(AppSyncer.s_syncInvalidDir, fileName);
                AppSyncer.DeleteFile(file);
                AppSyncer.DeleteFile(file2);
                AppSyncer.DeleteFile(file3);
                AppSyncer.DeleteFile(file4);
            }
            try
            {
                AppUninstaller.SilentUninstallApp(name, pkgName, false);
            }
            catch (Exception ex)
            {
                Logger.Error("AppSyncer: {0}", ex.ToString());
            }
        }

        private static void DeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    Logger.Info("AppSyncer: Deleting " + file);
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AppSyncer: Failed to delete " + file);
                Logger.Error(ex.ToString());
            }
        }

        public static int CheckIfAppInstalled(string appPackage, int appVersion, IJSonObject installedApps)
        {
            Logger.Info("Checking if {0}:{1} installed", appPackage, appVersion);
            try
            {
                string text = installedApps["result"].StringValue.Trim();
                if (text != "ok")
                {
                    Logger.Error("result: {0}", text);
                    return -1;
                }
                string text2 = installedApps["installed_packages"].ToString();
                Logger.Debug(text2);
                JSonReader jSonReader = new JSonReader();
                IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(text2);
                for (int i = 0; i < iJSonObject.Length; i++)
                {
                    string strB = iJSonObject[i]["package"].StringValue.Trim();
                    int int32Value = iJSonObject[i]["version"].Int32Value;
                    if (string.Compare(appPackage, strB, true) == 0)
                    {
                        if (appVersion == -1)
                        {
                            return 1;
                        }
                        if (appVersion == int32Value)
                        {
                            return 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in CheckIfAppInstalled");
                Logger.Error(ex.ToString());
                return -1;
            }
            return 0;
        }

        private static void InstallApk(string fileName, string filePath)
        {
            string text = Path.Combine(AppSyncer.s_syncDoneDir, fileName);
            string text2 = Path.Combine(AppSyncer.s_syncRetryDir, fileName);
            string text3 = Path.Combine(AppSyncer.s_syncInvalidDir, fileName);
            InstallerCodes installerCodes = AppSyncer.CallApkInstaller(filePath);
            switch (installerCodes)
            {
                case InstallerCodes.SUCCESS_CODE:
                    Logger.Info("AppSyncer: {0} installed sucessfully", fileName);
                    if (File.Exists(text))
                    {
                        File.Delete(text);
                    }
                    File.Move(filePath, text);
                    return;
                case InstallerCodes.INSTALL_FAILED_CONTAINER_ERROR:
                case InstallerCodes.INSTALL_FAILED_CPU_ABI_INCOMPATIBLE:
                case InstallerCodes.INSTALL_FAILED_INVALID_APK:
                case InstallerCodes.INSTALL_FAILED_OLDER_SDK:
                    Logger.Info("AppSyncer: {0} installation failed !! {1}", fileName, installerCodes);
                    if (File.Exists(text3))
                    {
                        File.Delete(text3);
                    }
                    File.Move(filePath, text3);
                    return;
                case InstallerCodes.INSTALL_FAILED_INSUFFICIENT_STORAGE:
                    SysTray.ShowErrorLong(BlueStacks.hyperDroid.Locale.Strings.BalloonTitle, BlueStacks.hyperDroid.Locale.Strings.InsufficientStorageMessage);
                    break;
            }
            Logger.Info("AppSyncer: {0} installation failed !! {1}", fileName, installerCodes);
            if (File.Exists(text2))
            {
                File.Delete(text2);
            }
            File.Move(filePath, text2);
        }

        private static InstallerCodes CallApkInstaller(string apkPath)
        {
            Logger.Info("AppSyncer: Installing apk: {0}", apkPath);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string path = (string)registryKey.GetValue("InstallDir");
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = Path.Combine(path, "HD-ApkHandler.exe");
            processStartInfo.Arguments = "\"" + apkPath + "\" silent";
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            Logger.Info("AppSyncer: installer path {0}", processStartInfo.FileName);
            Process process = Process.Start(processStartInfo);
            process.WaitForExit();
            Logger.Info("AppSyncer: Exit code " + process.ExitCode);
            return (InstallerCodes)process.ExitCode;
        }

        private static void SetupEnv()
        {
            try
            {
                AppSyncer.CreateDir(AppSyncer.s_appSyncDir);
                AppSyncer.CreateDir(AppSyncer.s_syncNewDir);
                AppSyncer.CreateDir(AppSyncer.s_syncRetryDir);
                AppSyncer.CreateDir(AppSyncer.s_syncInvalidDir);
                AppSyncer.CreateDir(AppSyncer.s_syncDoneDir);
            }
            catch (Exception ex)
            {
                Logger.Error("AppSyncer: {0}", ex.ToString());
            }
        }

        private static bool CreateDir(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    return true;
                }
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Logger.Error("AppSyncer: Error occured while creating directory: {0}. Exception: {1}", path, ex);
                return false;
            }
            return true;
        }
    }
}
