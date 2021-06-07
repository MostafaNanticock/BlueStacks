using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Common.HTTP;
using BlueStacks.hyperDroid.Device;
using BlueStacks.hyperDroid.Locale;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace BlueStacks.hyperDroid.Common
{
    internal class Stats
    {
        public enum AppType
        {
            app,
            market,
            suggestedapps
        }

        public const string AppInstall = "true";

        public const string AppUninstall = "false";

        private static int s_numDaysToKeepStats = 15;

        private static int s_numAppsClicked = 19;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate1c;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate1e;

        private static string Timestamp
        {
            get
            {
                long num = DateTime.Now.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;
                num /= 10000000;
                return num.ToString();
            }
        }

        private static string Email
        {
            get
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Agent\\Cloud");
                string result = (string)registryKey.GetValue("Email", "");
                registryKey.Close();
                return result;
            }
        }

        public static void GetUsageStats(out string diskUsage, out string appUsage)
        {
            int installedAppCount = JsonParser.GetInstalledAppCount();
            appUsage = installedAppCount + " " + ((installedAppCount > 1) ? BlueStacks.hyperDroid.Locale.Strings.Apps : BlueStacks.hyperDroid.Locale.Strings.App) + " " + BlueStacks.hyperDroid.Locale.Strings.Installed;
            diskUsage = BlueStacks.hyperDroid.Locale.Strings.DiskUsageUnavailable;
            ServiceController serviceController = new ServiceController("bsthdandroidsvc");
            if (serviceController.Status == ServiceControllerStatus.Running)
            {
                string url = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + Strings.GetDiskUsage;
                string text = null;
                try
                {
                    text = Client.Get(url, null, false, 500);
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception in GetUsageStats: {0}", ex.Message);
                }
                if (text == null)
                {
                    Logger.Error("Failed to getUsageStats.");
                }
                else
                {
                    try
                    {
                        JSonReader jSonReader = new JSonReader();
                        IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(text);
                        string stringValue = iJSonObject["result"].StringValue;
                        if (string.Compare(stringValue, "ok", true) == 0)
                        {
                            double num = Convert.ToDouble(iJSonObject["diskUsage"][0]["sdCardAvailSize"].StringValue) / 1048576.0;
                            double num2 = Convert.ToDouble(iJSonObject["diskUsage"][0]["sdCardTotalSize"].StringValue) / 1048576.0;
                            double num3 = Convert.ToDouble(iJSonObject["diskUsage"][1]["dataFSAvailSize"].StringValue) / 1048576.0;
                            double num4 = Convert.ToDouble(iJSonObject["diskUsage"][1]["dataFSTotalSize"].StringValue) / 1048576.0;
                            double num5 = num2 + num4;
                            double num6 = num + num3;
                            double value = (num5 - num6) / num5 * 100.0;
                            diskUsage = Convert.ToInt32(value) + "% " + BlueStacks.hyperDroid.Locale.Strings.Of + " " + Convert.ToInt32(num5) + " MB " + BlueStacks.hyperDroid.Locale.Strings.DiskUsed;
                        }
                    }
                    catch (Exception ex2)
                    {
                        Logger.Error("Exception in GetUsageStats");
                        Logger.Error(ex2.ToString());
                    }
                }
            }
        }

        public static bool UploadUsageStats()
        {
            Logger.Debug("Uploading stats to cloud...");
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.HKLMConfigRegKeyPath);
            RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(Strings.HKCURegKeyPath);
            int num = 0;
            string text = "[";
            string text2 = (string)registryKey.GetValue("LastSent", "0;0;0");
            string[] array = text2.Split(';');
            int num2 = Convert.ToInt32(array[0]);
            int num3 = Convert.ToInt32(array[1]);
            int num4 = Convert.ToInt32(array[2]);
            JSonWriter jSonWriter = new JSonWriter();
            jSonWriter.WriteArrayBegin();
            try
            {
                string[] array2 = (string[])registryKey.GetValue("BstUsageStats");
                num = array2.Length;
                for (int i = 0; i < num; i++)
                {
                    string[] array3 = array2[i].Split('#');
                    text += "{";
                    text += "\"date\":\"" + array3[0] + "\",";
                    text += "\"time\":\"" + array3[1] + "\",";
                    text += "\"apps\":\"" + array3[2] + "\",";
                    text += "\"disk\":\"" + array3[3] + "\"";
                    text += "}";
                    jSonWriter.WriteObjectBegin();
                    DateTime time;
                    try
                    {
                        time = Convert.ToDateTime(array3[0]);
                    }
                    catch (Exception)
                    {
                        time = DateTime.ParseExact(array3[0], "dd-MM-yyyy", null);
                    }
                    Calendar calendar = CultureInfo.InvariantCulture.Calendar;
                    jSonWriter.WriteMember("date", array3[0]);
                    int weekOfYear = calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    if (num2 != weekOfYear)
                    {
                        jSonWriter.WriteMember("week", weekOfYear + "-" + time.Year);
                        num2 = weekOfYear;
                    }
                    if (num3 != time.Month)
                    {
                        jSonWriter.WriteMember("month", time.Month + "-" + time.Year);
                        num3 = time.Month;
                    }
                    if (num4 != time.Year)
                    {
                        jSonWriter.WriteMember("year", time.Year);
                        num4 = time.Year;
                    }
                    jSonWriter.WriteObjectEnd();
                    if (i != num - 1)
                    {
                        text += ",";
                    }
                }
                text += "]";
            }
            catch (Exception ex2)
            {
                Logger.Error("Failed to upload usage stats. Err: " + ex2.ToString());
                num = 0;
            }
            jSonWriter.WriteArrayEnd();
            if (num == 0)
            {
                Logger.Info("No usage stats available. Will check again later.");
                return true;
            }
            Logger.Info("sending usage stats: " + text);
            string arg = (string)registryKey2.GetValue("Host");
            string url = arg + "/" + Strings.UploadUsageUrl;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("usageData", text);
            dictionary.Add("numItems", Convert.ToString(num));
            dictionary.Add("email", Stats.Email);
            string text3 = Client.Post(url, dictionary, null, true);
            if (string.Compare(text3, "success", true) != 0)
            {
                Logger.Error("Could not send usage stats. Error: " + text3);
                return false;
            }
            Logger.Info("Sending usage count data: " + jSonWriter.ToString());
            url = arg + "/" + Strings.UploadUsageCountUrl;
            dictionary.Clear();
            dictionary.Add("usageCount", jSonWriter.ToString());
            dictionary.Add("numItems", Convert.ToString(num));
            text3 = Client.Post(url, dictionary, null, true);
            if (string.Compare(text3, "success", true) != 0)
            {
                Logger.Error("Could not send usage count stats. Error: " + text3);
                return false;
            }
            registryKey.SetValue("LastSent", num2 + ";" + num3 + ";" + num4);
            registryKey.DeleteValue("BstUsageStats");
            return true;
        }

        public static void UpdatePendingStatsQueue(string todaysUsage, string currentDate)
        {
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.HKLMConfigRegKeyPath);
            string arg = default(string);
            string text = default(string);
            Stats.GetUsageStats(out arg, out text);
            int num = Convert.ToInt32(text.Substring(0, text.IndexOf(' ')));
            todaysUsage = todaysUsage + "#" + num + "#" + arg;
            string[] array2;
            try
            {
                string[] array = (string[])registryKey.GetValue("BstUsageStats");
                int num2 = array.Length;
                if (num2 != 0)
                {
                    int i;
                    if (num2 >= Stats.s_numDaysToKeepStats)
                    {
                        num2 = Stats.s_numDaysToKeepStats - 1;
                        for (i = 0; i < num2; i++)
                        {
                            array[i] = array[i + 1];
                        }
                    }
                    array2 = new string[num2 + 1];
                    for (i = 0; i < num2; i++)
                    {
                        array2[i] = array[i];
                    }
                    array2[i] = todaysUsage;
                }
                else
                {
                    array2 = new string[1]
					{
						todaysUsage
					};
                }
            }
            catch (Exception)
            {
                int num2 = 0;
                array2 = new string[1]
				{
					todaysUsage
				};
            }
            registryKey.SetValue("BstUsageStats", array2);
        }

        public static void UploadCrashReport(string packageName, string versionCode, string versionName)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                try
                {
                    Stats.UploadCrashReportHelper(packageName, versionCode, versionName);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to upload crash report. err: " + ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void UploadCrashReportHelper(string packageName, string versionCode, string versionName)
        {
            Registry.LocalMachine.OpenSubKey(Strings.HKLMConfigRegKeyPath);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.HKCURegKeyPath);
            string value = (string)registryKey.GetValue("Email", "null");
            string arg = (string)registryKey.GetValue("Host");
            string url = arg + "/" + Strings.UploadCrashUrl;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("email", value);
            dictionary.Add("package_name", packageName);
            dictionary.Add("version_code", versionCode);
            dictionary.Add("version_name", versionName);
            Client.Post(url, dictionary, null, true);
        }

        public static void SendAppStats(string appName, string packageName, string appVersion, string homeVersion, AppType appType)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                try
                {
                    string url = Service.Host + "/" + Strings.AppClickStatsUrl;
                    bool flag = false;
                    string filepath = "";
                    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.HKLMConfigRegKeyPath);
                    string value = (string)registryKey.GetValue("FacebookUserId", "0");
                    Stats.s_numAppsClicked++;
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    dictionary.Add("email", Stats.GetURLSafeBase64String(Stats.Email));
                    dictionary.Add("app_name", Stats.GetURLSafeBase64String(appName));
                    dictionary.Add("app_pkg", Stats.GetURLSafeBase64String(packageName));
                    dictionary.Add("app_ver", Stats.GetURLSafeBase64String(appVersion));
                    dictionary.Add("home_app_ver", Stats.GetURLSafeBase64String(homeVersion));
                    dictionary.Add("user_time", Stats.GetURLSafeBase64String(Stats.Timestamp));
                    dictionary.Add("app_type", Stats.GetURLSafeBase64String(appType.ToString()));
                    if (!string.IsNullOrEmpty(value) && Stats.s_numAppsClicked == 20)
                    {
                        Stats.s_numAppsClicked = 0;
                        dictionary.Add("fb_user_id", value);
                        string activityNameFromPackageName = JsonParser.GetActivityNameFromPackageName(packageName);
                        string path = packageName + "." + activityNameFromPackageName + ".png";
                        flag = true;
                        filepath = Path.Combine(Strings.GadgetDir, path);
                    }
                    Logger.Info("Sending App Stats for: {0}", appName);
                    string text = (!flag) ? Client.Post(url, dictionary, null, false) : Client.HTTPGaeFileUploader(url, dictionary, null, filepath, "text/plain", false);
                    Logger.Info("Got App Stat response: {0}", text);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void SendWebAppChannelStats(string appName, string packageName, string homeVersion)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                string url = Service.Host + "/" + Strings.WebAppChannelClickStatsUrl;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("app_name", Stats.GetURLSafeBase64String(appName));
                dictionary.Add("app_pkg", Stats.GetURLSafeBase64String(packageName));
                dictionary.Add("home_app_ver", Stats.GetURLSafeBase64String(homeVersion));
                dictionary.Add("user_time", Stats.GetURLSafeBase64String(Stats.Timestamp));
                dictionary.Add("email", Stats.GetURLSafeBase64String(Stats.Email));
                try
                {
                    Logger.Info("Sending Channel App Stats for: {0}", appName);
                    string text = Client.Post(url, dictionary, null, false);
                    Logger.Info("Got Channel App Stat response: {0}", text);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void SendSearchAppStats(string keyword)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                string url = Service.Host + "/" + Strings.SearchAppStatsUrl;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("keyword", keyword);
                try
                {
                    Logger.Info("Sending Search App Stats for: {0}", keyword);
                    string text = Client.Post(url, dictionary, null, false);
                    Logger.Info("Got Search App Stat response: {0}", text);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void SendAppInstallStats(string appName, string packageName, string appVersion, string appInstall)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                string url = Service.Host + "/" + Strings.AppInstallStatsUrl;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("email", Stats.GetURLSafeBase64String(Stats.Email));
                dictionary.Add("app_name", Stats.GetURLSafeBase64String(appName));
                dictionary.Add("app_pkg", Stats.GetURLSafeBase64String(packageName));
                dictionary.Add("app_ver", Stats.GetURLSafeBase64String(appVersion));
                dictionary.Add("is_install", Stats.GetURLSafeBase64String(appInstall));
                dictionary.Add("user_time", Stats.GetURLSafeBase64String(Stats.Timestamp));
                try
                {
                    Logger.Info("Sending App Install Stats for: {0}", appName);
                    string text = Client.Post(url, dictionary, null, false);
                    Logger.Info("Got App Install Stat response: {0}", text);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void SendSystemInfoStats()
        {
            Stats.SendSystemInfoStatsAsync(null, true, null, null);
        }

        public static void SendSystemInfoStatsAsync(string host, bool createRegKey, Dictionary<string, string> glData, string guid)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                Stats.SendSystemInfoStatsSync(host, createRegKey, glData, guid);
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static string SendSystemInfoStatsSync(string host, bool createRegKey, Dictionary<string, string> glData, string guid)
        {
            Dictionary<string, string> dictionary = Profile.Info();
            Logger.Info("Got Device Profile Info:");
            foreach (KeyValuePair<string, string> item in dictionary)
            {
                Logger.Info(item.Key + " " + item.Value);
            }
            if (host == null)
            {
                host = Service.Host;
            }
            string url = host + "/" + Strings.SystemInfoStatsUrl;
            Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
            dictionary2.Add("p", Stats.GetURLSafeBase64String(dictionary["Processor"]));
            dictionary2.Add("nop", Stats.GetURLSafeBase64String(dictionary["NumberOfProcessors"]));
            dictionary2.Add("g", Stats.GetURLSafeBase64String(dictionary["GPU"]));
            dictionary2.Add("gd", Stats.GetURLSafeBase64String(dictionary["GPUDriver"]));
            dictionary2.Add("o", Stats.GetURLSafeBase64String(dictionary["OS"]));
            dictionary2.Add("osv", Stats.GetURLSafeBase64String(dictionary["OSVersion"]));
            dictionary2.Add("sr", Stats.GetURLSafeBase64String(dictionary["ScreenResolution"]));
            dictionary2.Add("bstr", Stats.GetURLSafeBase64String(dictionary["BlueStacksResolution"]));
            dictionary2.Add("dnv", Stats.GetURLSafeBase64String(dictionary["DotNetVersion"]));
            dictionary2.Add("osl", Stats.GetURLSafeBase64String(CultureInfo.CurrentCulture.Name.ToLower()));
            dictionary2.Add("oem_info", Stats.GetURLSafeBase64String(dictionary["OEMInfo"]));
            dictionary2.Add("ram", Stats.GetURLSafeBase64String(dictionary["RAM"]));
            if (glData != null)
            {
                dictionary2.Add("glmode", Stats.GetURLSafeBase64String(glData["GlMode"]));
                dictionary2.Add("glrendermode", Stats.GetURLSafeBase64String(glData["GlRenderMode"]));
                dictionary2.Add("gl_vendor", Stats.GetURLSafeBase64String(glData["GlVendor"]));
                dictionary2.Add("gl_renderer", Stats.GetURLSafeBase64String(glData["GlRenderer"]));
                dictionary2.Add("gl_version", Stats.GetURLSafeBase64String(glData["GlVersion"]));
            }
            else
            {
                dictionary2.Add("glmode", Stats.GetURLSafeBase64String(dictionary["GlMode"]));
                dictionary2.Add("glrendermode", Stats.GetURLSafeBase64String(dictionary["GlRenderMode"]));
            }
            if (guid != null)
            {
                dictionary2.Add("guid", Stats.GetURLSafeBase64String(guid));
            }
            string text = "not sent";
            try
            {
                Logger.Info("Sending System Info Stats");
                text = Client.Post(url, dictionary2, null, false, 10000);
                Logger.Info("Got System Info  response: {0}", text);
                if (createRegKey)
                {
                    RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.HKLMConfigRegKeyPath);
                    registryKey.SetValue("SystemStats", 1);
                    registryKey.Flush();
                    registryKey.Close();
                    return text;
                }
                return text;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return text;
            }
        }

        public static void SendUserActiveStats(string active)
        {
            Logger.Info("Inside SendUserActiveStats");
            Thread thread = new Thread((ThreadStart)delegate
            {
                try
                {
                    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\BlueStacks\\Guests\\Android\\Config");
                    string arg = string.Format("http://127.0.0.1:{0}", (int)registryKey.GetValue("AgentServerPort", 2861));
                    string text = arg + "/" + Strings.UserActiveUrl;
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    dictionary.Add("status", active);
                    Logger.Info("Sending User Active to {0}", text);
                    string text2 = Client.Post(text, dictionary, null, false);
                    Logger.Info("Got User Active response: {0}", text2);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error Occured, Err : " + ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void SendActivityEndedStats(string activity, string package, ulong seconds)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                string text = Service.Host + "/" + Strings.ActivityEndedStatsUrl;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("activity", Stats.GetURLSafeBase64String(activity));
                dictionary.Add("package", Stats.GetURLSafeBase64String(package));
                dictionary.Add("seconds", Stats.GetURLSafeBase64String(seconds.ToString()));
                try
                {
                    Logger.Info("Sending Activity Ended Stats to {0} for activity = {1} and package = {2}", text, activity, package);
                    string text2 = Client.Post(text, dictionary, null, false);
                    Logger.Info("Got Activity Ended Stats response: {0}", text2);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void SendBootStats(string type, bool booted, bool wait)
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                string text = Service.Host + "/" + Strings.BootStatsUrl;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("type", Stats.GetURLSafeBase64String(type));
                dictionary.Add("booted", Stats.GetURLSafeBase64String(booted.ToString()));
                try
                {
                    Logger.Info("Sending Boot Stats to {0}", text);
                    string text2 = Client.Post(text, dictionary, null, false);
                    Logger.Info("Got Boot Stats response: {0}", text2);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
            if (wait && !thread.Join(5000))
            {
                thread.Abort();
            }
        }

        public static void SendHomeScreenDisplayedStats()
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                string text = Service.Host + "/" + Strings.HomeScreenStatsUrl;
                try
                {
                    Logger.Info("Sending Home Screen Displayed Stats to {0}", text);
                    string text2 = Client.Get(text, null, false);
                    Logger.Info("Got Home Screen Displayed Stats response: {0}", text2);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void SendVirtStats()
        {
            Thread thread = new Thread((ThreadStart)delegate
            {
                string text = Service.Host + "/" + Strings.VirtStatsUrl;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("bst_virt_type", Profile.VirtType);
                try
                {
                    Logger.Info("Sending Virt Stats to {0}", text);
                    string text2 = Client.Post(text, dictionary, null, false);
                    Logger.Info("Got Virt Stats response: {0}", text2);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private static string GetURLSafeBase64String(string originalString)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(originalString));
        }
    }
}
