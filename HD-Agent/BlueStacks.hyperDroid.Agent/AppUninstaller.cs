using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;
using CodeTitans.JSon;
using System;
using System.Collections.Generic;
using System.IO;

namespace BlueStacks.hyperDroid.Agent
{
    public class AppUninstaller
    {
        private static string s_appsDotJsonFile = Path.Combine(BlueStacks.hyperDroid.Common.Strings.GadgetDir, "apps.json");

        public static int s_systemApps = 0;

        public static AppInfo[] s_originalJson = null;

        public static void AppUninstalled(string packageName)
        {
            string text = AppUninstaller.RemoveFromJson(packageName);
            GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("UnInstalled", text, packageName, 1));
            Logger.Info("Sending App Install stats");
            string versionFromPackage = HDAgent.GetVersionFromPackage(packageName);
            Stats.SendAppInstallStats(text, packageName, versionFromPackage, "false");
            if (text == "")
            {
                text = packageName;
            }
            string message = text + " " + BlueStacks.hyperDroid.Locale.Strings.UninstallSuccess;
            if (Features.IsFeatureEnabled(4u))
            {
                SysTray.ShowInfoShort(BlueStacks.hyperDroid.Locale.Strings.BalloonTitle, message);
            }
        }

        public static int SilentUninstallApp(string appName, string package, bool nolookup)
        {
            AppUninstaller.s_originalJson = JsonParser.GetAppList();
            Logger.Info("nolookup: " + nolookup);
            if (!nolookup)
            {
                string text = default(string);
                string text2 = default(string);
                string text3 = default(string);
                JsonParser.GetAppInfoFromAppName(appName, out text, out text2, out text3);
                Logger.Info("AppUninstaller: Got image name: " + text2);
                if (text2 == null)
                {
                    Logger.Info("AppUninstaller: App not found");
                    return -1;
                }
            }
            int num = AppUninstaller.UninstallApp(package);
            if (num == 0)
            {
                Logger.Info("AppUninstaller: Uninstallation successful");
            }
            else
            {
                Logger.Info("AppUninstaller: Uninstallation failed");
            }
            return num;
        }

        public static int UninstallApp(string packageName)
        {
            try
            {
                Logger.Info("AppUninstaller: In uninstall app");
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("pkg", packageName);
                string input;
                try
                {
                    input = HTTPHandler.Post(VmCmdHandler.s_ServerPort, HDAgent.s_UninstallPath, dictionary);
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception when sending uninstall post request");
                    Logger.Error(ex.ToString());
                    return 1;
                }
                IJSonReader iJSonReader = new JSonReader();
                IJSonObject iJSonObject = iJSonReader.ReadAsJSonObject(input);
                if (iJSonObject["result"].StringValue == "ok")
                {
                    return 0;
                }
                return 1;
            }
            catch (Exception ex2)
            {
                Logger.Error(ex2.ToString());
                return 1;
            }
        }

        public static string RemoveFromJson(string packageName)
        {
            Logger.Info("AppUninstaller: Removing app from json: " + packageName);
            AppUninstaller.s_originalJson = JsonParser.GetAppList();
            int num = 0;
            string result = "";
            for (int i = 0; i < AppUninstaller.s_originalJson.Length; i++)
            {
                if (AppUninstaller.s_originalJson[i].package == packageName)
                {
                    result = AppUninstaller.s_originalJson[i].name;
                    num++;
                }
            }
            AppInfo[] array = new AppInfo[AppUninstaller.s_originalJson.Length - num];
            int j = 0;
            int num2 = 0;
            for (; j < AppUninstaller.s_originalJson.Length; j++)
            {
                if (AppUninstaller.s_originalJson[j].package == packageName)
                {
                    AppUninstaller.RemoveIcon(AppUninstaller.s_originalJson[j].img);
                    AppUninstaller.RemoveFromLibrary(AppUninstaller.s_originalJson[j].name, AppUninstaller.s_originalJson[j].package, AppUninstaller.s_originalJson[j].img);
                    AppUninstaller.RemoveAppTile(AppUninstaller.s_originalJson[j].package);
                }
                else
                {
                    array[num2] = AppUninstaller.s_originalJson[j];
                    num2++;
                }
            }
            JsonParser.WriteJson(array);
            return result;
        }

        private static void RemoveIcon(string imageFile)
        {
            Logger.Info("AppUninstaller: Removing icon " + imageFile);
            string path = Path.Combine(BlueStacks.hyperDroid.Common.Strings.GadgetDir, imageFile);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static void RemoveFromLibrary(string appName, string packageName, string img)
        {
            Logger.Info("Removing {0} from library", appName);
            string path = Path.Combine(BlueStacks.hyperDroid.Common.Strings.LibraryDir, BlueStacks.hyperDroid.Common.Strings.MyAppsDir);
            string path2 = Path.Combine(BlueStacks.hyperDroid.Common.Strings.LibraryDir, BlueStacks.hyperDroid.Common.Strings.IconsDir);
            string text = Path.Combine(BlueStacks.hyperDroid.Common.Strings.LibraryDir, BlueStacks.hyperDroid.Common.Strings.StoreAppsDir);
            string b = appName + ".lnk";
            string text2 = img.Substring(img.LastIndexOf("."));
            string b2 = img.Substring(0, img.Length - text2.Length) + ".ico";
            string[] files = Directory.GetFiles(path);
            foreach (string text3 in files)
            {
                if (Path.GetFileName(text3) == b)
                {
                    Logger.Info("Deleting {0}", text3);
                    File.Delete(text3);
                    return;
                }
            }
            try
            {
                string[] files2 = Directory.GetFiles(text);
                foreach (string text4 in files2)
                {
                    if (Path.GetFileName(text4) == b)
                    {
                        Logger.Info("Deleting {0}", text4);
                        File.Delete(text4);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when deleting from {0}", text);
                Logger.Error(ex.Message);
            }
            string[] files3 = Directory.GetFiles(path2);
            int num = 0;
            string text5;
            while (true)
            {
                if (num < files3.Length)
                {
                    text5 = files3[num];
                    if (!(Path.GetFileName(text5) == b2))
                    {
                        num++;
                        continue;
                    }
                    break;
                }
                return;
            }
            Logger.Info("Deleting {0}", text5);
            File.Delete(text5);
        }

        private static void RemoveAppTile(string packageName)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string path = Path.Combine(folderPath, "Microsoft\\Windows\\Application Shortcuts\\BlueStacks\\");
            string path2 = packageName + ".lnk";
            string text = Path.Combine(path, path2);
            if (File.Exists(text))
            {
                Logger.Info("AppUninstaller: Removing app tile " + text);
                File.Delete(text);
            }
        }
    }
}
