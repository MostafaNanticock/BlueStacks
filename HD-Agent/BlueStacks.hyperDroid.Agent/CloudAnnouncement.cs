using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BlueStacks.hyperDroid.Agent
{
    internal class CloudAnnouncement
    {
        private static string s_announcementDir = Path.Combine(Strings.BstUserDataDir, "Announcements");

        private static string s_appsDir = Path.Combine(Strings.LibraryDir, Strings.MyAppsDir);

        private static string s_productLogo = Path.Combine(HDAgent.s_InstallDir, "ProductLogo.png");

        private static int s_msgId = -1;

        private static bool s_uploadStats = true;

        private static string s_configPath = "Software\\BlueStacks\\Guests\\Android\\Config";

        private static string s_hostKeyPath = "Software\\BlueStacks\\Agent\\Cloud";

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate13;

        public static string Dir
        {
            get
            {
                return CloudAnnouncement.s_announcementDir;
            }
        }

        public static Image ProductLogo
        {
            get
            {
                return Image.FromFile(CloudAnnouncement.s_productLogo);
            }
        }

        public static bool ShowAnnouncement()
        {
            if (!Features.IsFeatureEnabled(1u))
            {
                Logger.Debug("Broadcast message feature disabled. Ignoring...");
                return false;
            }
            int num = -1;
            string text = null;
            string text2 = "";
            string text3 = "false";
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(CloudAnnouncement.s_configPath);
            RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(CloudAnnouncement.s_hostKeyPath);
            Logger.Debug("Showing announcement");
            try
            {
                if (Directory.Exists(CloudAnnouncement.s_announcementDir))
                {
                    string[] files = Directory.GetFiles(CloudAnnouncement.s_announcementDir);
                    for (int i = 0; i < files.Length; i++)
                    {
                        try
                        {
                            if (File.Exists(files[i]))
                            {
                                File.Delete(files[i]);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Failed to delete file. err: " + ex.Message);
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(CloudAnnouncement.s_announcementDir);
                }
            }
            catch (Exception ex2)
            {
                Logger.Error("Failed to delete/create announcement dir. err: " + ex2.Message);
                if (!Directory.Exists(CloudAnnouncement.s_announcementDir))
                {
                    Directory.CreateDirectory(CloudAnnouncement.s_announcementDir);
                }
            }
            try
            {
                num = (int)registryKey.GetValue("LastAnnouncementId");
            }
            catch (Exception ex3)
            {
                Logger.Error("Failed to get AnnouncementId. Error: " + ex3.ToString() + " Showing welcome message.");
                num = -1;
            }
            string arg = (string)registryKey2.GetValue("Host");
            string url = arg + "/getAnnouncement";
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("x_last_msg_id", Convert.ToString(num));
            dictionary.Add("x_locale", CultureInfo.CurrentCulture.Name.ToLower());
            RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
            string value = (string)registryKey3.GetValue("InstallType", "");
            dictionary.Add("x_install_type", value);
            text = Client.Get(url, dictionary, false);
            if (text == null)
            {
                Logger.Error("Failed to get announcement data.");
                return false;
            }
            Logger.Debug("Announcement resp: " + text);
            string input = text;
            JSonReader jSonReader = new JSonReader();
            IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
            text3 = iJSonObject["success"].StringValue.Trim();
            text2 = iJSonObject["reason"].StringValue.Trim();
            if (string.Compare(text3, "false", true) == 0)
            {
                Logger.Info("Could not get announcement msg: " + text2);
                return false;
            }
            num = Convert.ToInt32(iJSonObject["msgId"].StringValue.Trim());
            Logger.Debug("Last Announcement ID: " + num);
            CloudAnnouncement.s_msgId = num;
            string imageURL = iJSonObject["imageUrl"].StringValue.Trim();
            Image image = CloudAnnouncement.DownloadDisplayImage(imageURL);
            AnnouncementMessage announcementMessage = new AnnouncementMessage(image, iJSonObject);
            if (announcementMessage.FileName.Length < 3)
            {
                announcementMessage.FileName = "downloadedFile.exe";
            }
            try
            {
                CloudAnnouncement.s_uploadStats = true;
                CloudAnnouncement.ShowFetchedMsg(announcementMessage);
            }
            catch (Exception ex4)
            {
                Logger.Error("Failed to fetch announcement message. error: " + ex4.ToString());
                return false;
            }
            Logger.Debug("Updating announcement ID to: " + num);
            registryKey.SetValue("LastAnnouncementId", num, RegistryValueKind.DWord);
            return true;
        }

        private static void InstallApp(string appURL, string pkgName, string storeType)
        {
            string url = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + Strings.AppInstallUrl;
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("url", appURL);
            data.Add("package", pkgName);
            data.Add("storeType", storeType);
            Thread thread = new Thread((ThreadStart)delegate
            {
                try
                {
                    string fileName = Path.Combine(HDAgent.s_InstallDir, "HD-RunApp.exe");
                    Process.Start(fileName);
                    Logger.Info("sending app click resp for: {0} to url: {1}", pkgName, url);
                    Client.Post(url, data, null, false);
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception installing store app: " + ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void ShowNotification(string action, string title, string message, string actionURL, string fileName, string imageURL)
        {
            Image image = (imageURL == null) ? CloudAnnouncement.ProductLogo : CloudAnnouncement.DownloadDisplayImage(imageURL);
            AnnouncementMessage m = new AnnouncementMessage(image, title, message, action, "", actionURL, fileName);
            CloudAnnouncement.s_uploadStats = false;
            CloudAnnouncement.ShowFetchedMsg(m);
        }

        private static void ShowFetchedMsg(AnnouncementMessage m)
        {
            switch (m.Action)
            {
                case "None":
                    GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("Announcement", m.Action, m.Title + ": " + m.Msg, 1));
                    CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false, null);
                    break;
                case "Amazon App":
                    CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false, delegate
                    {
                        GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("Announcement", m.Action, m.ActionURL, 1));
                        CloudAnnouncement.InstallApp(m.ActionURL, m.PkgName, "amz");
                        CloudAnnouncement.UpdateClickStats();
                    });
                    break;
                case "Opera App":
                    CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false, delegate
                    {
                        GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("Announcement", m.Action, m.ActionURL, 1));
                        CloudAnnouncement.InstallApp(m.ActionURL, m.PkgName, "opera");
                        CloudAnnouncement.UpdateClickStats();
                    });
                    break;
                case "Web URL":
                    CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false, delegate
                    {
                        GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("Announcement", m.Action, m.ActionURL, 1));
                        Process.Start(m.ActionURL);
                        CloudAnnouncement.UpdateClickStats();
                    });
                    break;
                case "Download and Execute":
                    CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false, delegate
                    {
                        GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("Announcement", m.Action, m.ActionURL, 1));
                        CloudAnnouncement.UpdateClickStats();
                        Thread thread2 = new Thread((ThreadStart)delegate
                        {
                            Random random = new Random();
                            AnnouncementMessage announcementMessage = m;
                            announcementMessage.FileName += " ";
                            string arg = m.FileName.Substring(0, m.FileName.IndexOf(' '));
                            string text = m.FileName.Substring(m.FileName.IndexOf(' ') + 1);
                            arg = random.Next() + "_" + arg;
                            arg = Path.Combine(CloudAnnouncement.s_announcementDir, arg);
                            try
                            {
                                WebClient webClient = new WebClient();
                                webClient.DownloadFile(m.ActionURL, arg);
                                Thread.Sleep(2000);
                                Process process = new Process();
                                process.StartInfo.UseShellExecute = true;
                                process.StartInfo.CreateNoWindow = true;
                                if ((arg.ToLowerInvariant().EndsWith(".msi") || arg.ToLowerInvariant().EndsWith(".exe")) && !BlueStacks.hyperDroid.Common.Utils.IsSignedByBlueStacks(arg))
                                {
                                    Logger.Info("Not executing unsigned binary " + arg);
                                    goto end_IL_008d;
                                }
                                if (arg.ToLowerInvariant().EndsWith(".msi"))
                                {
                                    process.StartInfo.FileName = "msiexec";
                                    text = "/i " + arg + " " + text;
                                    process.StartInfo.Arguments = text;
                                }
                                else
                                {
                                    process.StartInfo.FileName = arg;
                                    process.StartInfo.Arguments = text;
                                }
                                Logger.Info("Starting process: {0} {1}", process.StartInfo.FileName, text);
                                process.Start();
                            end_IL_008d: ;
                            }
                            catch (Exception ex2)
                            {
                                Logger.Error("Failed to download and execute. err: " + ex2.ToString());
                            }
                        });
                        thread2.IsBackground = true;
                        thread2.Start();
                    });
                    break;
                case "Start Android App":
                    CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false, delegate
                    {
                        GoogleAnalytics.TrackEventAsync(new GoogleAnalytics.Event("Announcement", m.Action, m.ActionURL, 1));
                        CloudAnnouncement.UpdateClickStats();
                        try
                        {
                            string text2 = HDAgent.s_InstallDir + "\\HD-RunApp.exe";
                            string[] array = m.FileName.Split(' ');
                            Logger.Info("Broadcast: Starting RunApp: {0} with args: -p {1} -a {2} -nl", text2, array[0], array[1]);
                            Process.Start(text2, "-p " + array[0] + " -a " + array[1] + " -nl");
                        }
                        catch (Exception ex3)
                        {
                            Logger.Error("Failed to start android app: {0}. Error: {1}", m.FileName, ex3.ToString());
                        }
                    });
                    break;
                case "Silent Install":
                    {
                        Logger.Info("Got update request. Initializing silent install...");
                        Thread thread = new Thread((ThreadStart)delegate
                        {
                            Random random2 = new Random();
                            AnnouncementMessage announcementMessage2 = m;
                            announcementMessage2.FileName += " ";
                            string arg2 = m.FileName.Substring(0, m.FileName.IndexOf(' '));
                            string text3 = m.FileName.Substring(m.FileName.IndexOf(' ') + 1);
                            arg2 = random2.Next() + "_" + arg2;
                            arg2 = Path.Combine(CloudAnnouncement.s_announcementDir, arg2);
                            try
                            {
                                WebClient webClient2 = new WebClient();
                                webClient2.DownloadFile(m.ActionURL, arg2);
                                Thread.Sleep(2000);
                                Process process2 = new Process();
                                process2.StartInfo.UseShellExecute = true;
                                process2.StartInfo.CreateNoWindow = true;
                                if ((arg2.ToLowerInvariant().EndsWith(".msi") || arg2.ToLowerInvariant().EndsWith(".exe")) && !BlueStacks.hyperDroid.Common.Utils.IsSignedByBlueStacks(arg2))
                                {
                                    Logger.Info("Not executing unsigned binary " + arg2);
                                    goto end_IL_008d;
                                }
                                if (arg2.ToLowerInvariant().EndsWith(".msi"))
                                {
                                    process2.StartInfo.FileName = "msiexec";
                                    text3 = "/i " + arg2 + " " + text3;
                                    process2.StartInfo.Arguments = text3;
                                }
                                else
                                {
                                    process2.StartInfo.FileName = arg2;
                                    process2.StartInfo.Arguments = text3;
                                }
                                Logger.Info("Starting process: {0} {1}", process2.StartInfo.FileName, text3);
                                process2.Start();
                            end_IL_008d: ;
                            }
                            catch (Exception ex4)
                            {
                                Logger.Error("Silent install failed.");
                                Logger.Error("Failed to download and execute. err: " + ex4.ToString());
                            }
                        });
                        thread.IsBackground = true;
                        thread.Start();
                        break;
                    }
                case "Free App":
                    Logger.Info("Free App notification recvd. Starting tray animation.");
                    try
                    {
                        SysTray.StartTrayAnimation(m.Title, m.Msg);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Exception in 'Free App' notification: " + ex.ToString());
                    }
                    break;
                default:
                    Logger.Error("Announcement: Invalid msg type rcvd: " + m.Action);
                    break;
            }
        }

        private static Image DownloadDisplayImage(string imageURL)
        {
            WebClient webClient = new WebClient();
            Stream stream = webClient.OpenRead(imageURL);
            Bitmap result = new Bitmap(stream);
            stream.Flush();
            stream.Close();
            return result;
        }

        public static void UpdateClickStats()
        {
            if (CloudAnnouncement.s_uploadStats)
            {
                Thread thread = new Thread((ThreadStart)delegate
                {
                    try
                    {
                        string text = null;
                        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(CloudAnnouncement.s_hostKeyPath);
                        string arg = (string)registryKey.GetValue("Host");
                        string url = arg + "/updateAnnouncementStats";
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                        dictionary.Add("x_last_msg_id", Convert.ToString(CloudAnnouncement.s_msgId));
                        text = Client.Get(url, dictionary, false);
                        if (text == null)
                        {
                            Logger.Info("Could not send click stats.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Failed to send click stats: " + ex.ToString());
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }
    }
}
