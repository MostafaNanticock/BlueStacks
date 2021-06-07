using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;

namespace BlueStacks.hyperDroid.Agent
{
    public class ApkInstall
    {
        private static string s_installDir = null;

        private static string s_appsDotJsonFile = Path.Combine(BlueStacks.hyperDroid.Common.Strings.GadgetDir, "apps.json");

        public static AppInfo[] s_originalJson = null;

        public static string s_packageName;

        public static string s_appName = null;

        public static string s_appIcon;

        public static string s_launchableActivityName;

        public static string s_version;

        private static string s_returnString = null;

        public static int InitApkInstall()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            ApkInstall.s_installDir = (string)registryKey.GetValue("InstallDir");
            return 0;
        }

        public static string InstallApk(string apk)
        {
            string text = "";
            try
            {
                Logger.Info("InstallApk: In InstallApk");
                string name = "Software\\BlueStacks\\Guests\\Android\\Config";
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
                int num = (int)registryKey.GetValue("FileSystem", 0);
                string text2 = null;
                if (num == 1)
                {
                    try
                    {
                        string name2 = "Software\\BlueStacks\\Guests\\Android\\SharedFolder\\0";
                        registryKey = Registry.LocalMachine.OpenSubKey(name2);
                        string text3 = (string)registryKey.GetValue("Name");
                        string text4 = (string)registryKey.GetValue("Path");
                        if (string.IsNullOrEmpty(text3) || string.IsNullOrEmpty(text4))
                        {
                            Logger.Error("Name or Path missing in sharedfolder regkey");
                            num = 0;
                        }
                        else
                        {
                            text = Path.Combine(text4, Path.GetFileNameWithoutExtension(apk) + "-" + DateTime.Now.Ticks.ToString() + Path.GetExtension(apk));
                            Logger.Info("newPath: " + text);
                            File.Copy(apk, text);
                            string text5 = "/mnt/sdcard/windows/" + text3 + "/" + Path.GetFileName(text);
                            Logger.Info("androidPath: " + text5);
                            Dictionary<string, string> dictionary = new Dictionary<string, string>();
                            dictionary.Add("path", text5);
                            text2 = HTTPHandler.Post(VmCmdHandler.s_ServerPort, HDAgent.s_InstallPath, dictionary);
                            File.Delete(text);
                            if (text2.Contains("INSTALL_FAILED_INSUFFICIENT_STORAGE") || text2.Contains("INSTALL_FAILED_INVALID_URI"))
                            {
                                num = 0;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.ToString());
                        num = 0;
                    }
                }
                try
                {
                    if (num == 0)
                    {
                        Logger.Info("Sending apk");
                        text2 = HTTPHandler.PostFile(VmCmdHandler.s_ServerPort, HDAgent.s_InstallPath, apk);
                    }
                }
                catch (Exception ex2)
                {
                    if (File.Exists(text))
                    {
                        File.Delete(text);
                    }
                    Logger.Error("Exception when sending install post request");
                    Logger.Error(ex2.ToString());
                    return "INSTALL_FAILED_SERVER_ERROR";
                }
                if (File.Exists(text))
                {
                    File.Delete(text);
                }
                IJSonReader iJSonReader = new JSonReader();
                IJSonObject iJSonObject = iJSonReader.ReadAsJSonObject(text2);
                if (iJSonObject["result"].StringValue == "ok")
                {
                    ApkInstall.s_returnString = "Success";
                }
                else
                {
                    ApkInstall.s_returnString = iJSonObject["reason"].StringValue;
                }
                return ApkInstall.s_returnString;
            }
            catch (Exception ex3)
            {
                if (File.Exists(text))
                {
                    File.Delete(text);
                }
                Logger.Error(ex3.ToString());
                return "Exception";
            }
        }

        public static void AppInstalled(string name, string package, string activity, string img, string version)
        {
            Logger.Info("Replacing invalid characters, if any, with whitespace");
            ApkInstall.s_appName = Regex.Replace(name, "[\\x22\\\\\\/:*?|<>]", " ");
            ApkInstall.s_packageName = package;
            ApkInstall.s_launchableActivityName = activity;
            ApkInstall.s_appIcon = img;
            ApkInstall.s_version = version;
            ApkInstall.s_originalJson = JsonParser.GetAppList();
            ApkInstall.AddToJson();
            ApkInstall.MakeLibraryChanges();
            string path = package + "." + activity + ".png";
            string text = Path.Combine(BlueStacks.hyperDroid.Common.Strings.GadgetDir, path);
            string path2 = string.Format("{0}.{1}.png", package, ".Main");
            string text2 = Path.Combine(BlueStacks.hyperDroid.Common.Strings.GadgetDir, path2);
            if (File.Exists(text2))
            {
                File.Copy(text2, text, true);
            }
            else
            {
                Utils.DownloadIcon(text, package);
            }
            ApkInstall.CreateWin8Tile();
            Logger.Info("InstallApk: Got AppName: {0}", ApkInstall.s_appName);
            Logger.Info("Sending App Install stats");
            GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("Installed", name, package, 1));
            Stats.SendAppInstallStats(name, package, version, "true");
            string str = ApkInstall.s_appName + " ";
            str += BlueStacks.hyperDroid.Locale.Strings.InstallSuccess;
            Path.Combine(BlueStacks.hyperDroid.Common.Strings.GadgetDir, ApkInstall.s_appIcon);
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMConfigRegKeyPath);
            try
            {
                if (Features.IsFeatureEnabled(2u))
                {
                    string[] array = new string[6]
					{
						"com.bluestacks.help",
						"com.facebook.katana",
						"com.twitter.android",
						"com.amazon.venezia",
						"getjar.android.client",
						"me.onemobile.android"
					};
                    int num = Array.IndexOf(array, package);
                    if (num == -1)
                    {
                        SysTray.ShowInfoShort(BlueStacks.hyperDroid.Locale.Strings.BalloonTitle, str);
                    }
                    else
                    {
                        Logger.Debug("Not showing notification for: " + package);
                    }
                }
                else
                {
                    Logger.Info("Not showing install notification...");
                }
            }
            catch (Exception ex)
            {
                registryKey.SetValue("InstallNotificationThreshold", 0);
                Logger.Error("Failed to get ShowInstallerNotification value. err: " + ex.Message);
            }
        }

        private static void AddToJson()
        {
            Logger.Info("InstallApk: Adding app to json: " + ApkInstall.s_appName);
            AppInfo[] array = new AppInfo[ApkInstall.s_originalJson.Length + 1];
            int num = 1;
            string arg = ApkInstall.s_appName;
            int i;
            for (i = 0; i < ApkInstall.s_originalJson.Length; i++)
            {
                if (ApkInstall.s_originalJson[i].name == ApkInstall.s_appName)
                {
                    if (ApkInstall.s_originalJson[i].package == ApkInstall.s_packageName && ApkInstall.s_originalJson[i].activity == ApkInstall.s_launchableActivityName)
                    {
                        ApkInstall.s_appName = ApkInstall.s_originalJson[i].name;
                        return;
                    }
                    ApkInstall.s_appName = arg + "-" + num;
                    num++;
                    i = 0;
                }
                array[i] = ApkInstall.s_originalJson[i];
            }
            array[i] = new AppInfo(ApkInstall.s_appName, ApkInstall.s_appIcon, ApkInstall.s_packageName, ApkInstall.s_launchableActivityName, "0", "no", ApkInstall.s_version);
            JsonParser.WriteJson(array);
        }

        private static void CreateWin8Tile()
        {
            bool flag = false;
            string str = null;
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
                RegistryKey registryKey2 = Registry.LocalMachine.CreateSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
                string path = (string)registryKey2.GetValue("InstallDir");
                string arg = Path.Combine(path, "HD-TileCreator.exe");
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = "\"" + arg + "\"";
                process.StartInfo.Arguments = "\"" + ApkInstall.s_packageName + "\" \"" + ApkInstall.s_launchableActivityName + "\" \"" + ApkInstall.s_appName + "\"";
                Logger.Info("Starting: " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
                process.Start();
            }
        }

        private static void MakeLibraryChanges()
        {
            Logger.Info("Making Library Changes");
            string text = Path.Combine(BlueStacks.hyperDroid.Common.Strings.LibraryDir, BlueStacks.hyperDroid.Common.Strings.MyAppsDir);
            string iconsDir = Path.Combine(BlueStacks.hyperDroid.Common.Strings.LibraryDir, BlueStacks.hyperDroid.Common.Strings.IconsDir);
            string png2ico = Path.Combine(ApkInstall.s_installDir, "HD-png2ico.exe");
            string text2 = Path.Combine(ApkInstall.s_installDir, "BlueStacks.ico");
            string imagePath = Path.Combine(BlueStacks.hyperDroid.Common.Strings.GadgetDir, ApkInstall.s_appIcon);
            ApkInstall.ResizeImage(imagePath);
            string text3 = ApkInstall.ConvertToIco(png2ico, imagePath, iconsDir);
            if (!File.Exists(text3))
            {
                text3 = text2;
            }
            string arguments = "-p " + ApkInstall.s_packageName + " -a " + ApkInstall.s_launchableActivityName;
            ApkInstall.CreateAppShortcut(Path.Combine(text, ApkInstall.s_appName + ".lnk"), arguments, text3, text, imagePath);
        }

        public static string ConvertToIco(string png2ico, string imagePath, string iconsDir)
        {
            Logger.Info("Converting {0}", imagePath);
            string fileName = Path.GetFileName(imagePath);
            int length = fileName.LastIndexOf(".");
            string path = fileName.Substring(0, length) + ".ico";
            string text = Path.Combine(iconsDir, path);
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "\"" + png2ico + "\"";
            process.StartInfo.Arguments = "\"" + text + "\" \"" + imagePath + "\"";
            Logger.Info(process.StartInfo.FileName + " " + process.StartInfo.Arguments);
            process.Start();
            process.WaitForExit();
            return text;
        }

        private static void CreateAppShortcut(string fileName, string arguments, string imglocation, string fileLocation, string imagePath)
        {
            string target = Path.Combine(ApkInstall.s_installDir, "HD-RunApp.exe");
            if (HDAgent.CreateShortcut(target, fileName, "", imglocation, arguments, 0) != 0)
            {
                Logger.Error("Couldn't create shorcut for " + arguments);
            }
            else
            {
                Logger.Info("Created shorcut {0} at {1}", fileName, fileLocation);
            }
        }

        public static void ResizeImage(string imagePath)
        {
            bool flag = false;
            Image image = Image.FromFile(imagePath);
            int num = image.Width;
            int num2 = image.Height;
            if (num >= 256)
            {
                int num3 = 248;
                num2 = (int)((float)num2 / ((float)num / (float)num3));
                num = num3;
                flag = true;
            }
            if (num2 >= 256)
            {
                int num4 = 248;
                num = (int)((float)num / ((float)num2 / (float)num4));
                num2 = num4;
                flag = true;
            }
            if (num % 8 != 0)
            {
                num -= num % 8;
                flag = true;
            }
            if (num2 % 8 != 0)
            {
                num2 -= num2 % 8;
                flag = true;
            }
            if (!flag)
            {
                image.Dispose();
            }
            else
            {
                Image image2 = new Bitmap(num, num2);
                Graphics graphics = Graphics.FromImage(image2);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, image2.Width, image2.Height);
                image.Dispose();
                File.Delete(imagePath);
                image2.Save(imagePath);
                image2.Dispose();
            }
        }
    }
}
