using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Agent
{
    public class HTTPHandler
    {
        private static System.Threading.Timer s_AppTimeoutVerifier = null;

        private static DateTime s_LastActivityClickedTime;

        private static int s_TimeoutMins = 10;

        private static DateTime s_LastActivityStartTime;

        private static string s_LastActivity = null;

        private static string s_LastActivityPackage = null;

        private static string s_FileSharerPackageName = "com.bluestacks.windowsfilemanager";

        private static string s_FrontendState = "Interacting";

        private static LinkedList<AndroidNotification> s_PendingNotifications = new LinkedList<AndroidNotification>();

        private static System.Threading.Timer s_NotificationTimer = null;

        private static int s_NotificationTimeout = 5000;

        private static bool[] s_NotificationLockHelper;

        private static int s_LockForTurn;

        private static object s_NotificationLock;

        private static string s_AppsDotJsonFile;

        private static string s_AppStoresDir;

        private static string s_MyAppsDir;

        private static string s_IconsDir;

        private static string s_IconFile;

        private static string s_Png2ico;

        public static object s_sync;

        private static string s_CloudRegKey;

        public static string Get(int port, string path)
        {
            Logger.Info("HTTPHandler: Sending get request to http://127.0.0.1:{0}/{1}", port, path);
            string url = "http://127.0.0.1:" + port + "/" + path;
            string text = null;
            if (port == VmCmdHandler.s_ServerPort)
            {
                if (!BlueStacks.hyperDroid.Common.Utils.StartServiceIfNeeded())
                {
                    BlueStacks.hyperDroid.Common.Utils.WaitForBootComplete();
                }
                for (int num = 30; num > 0; num--, Thread.Sleep(2000))
                {
                    try
                    {
                        text = Client.Get(url, null, false);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Exception in get request");
                        Logger.Error(ex.Message);
                        continue;
                    }
                    break;
                }
            }
            else
            {
                text = Client.Get(url, null, false);
            }
            Logger.Debug("HTTPHandler: Got response: " + text);
            return text;
        }

        public static string Post(int port, string path, Dictionary<string, string> data)
        {
            Logger.Info("HTTPHandler: Sending post request to http://127.0.0.1:{0}/{1}", port, path);
            string url = "http://127.0.0.1:" + port + "/" + path;
            string text = null;
            if (port == VmCmdHandler.s_ServerPort)
            {
                bool flag = BlueStacks.hyperDroid.Common.Utils.StartServiceIfNeeded();
                if (!flag)
                {
                    BlueStacks.hyperDroid.Common.Utils.WaitForBootComplete();
                }
                for (int num = 30; num > 0; num--, Thread.Sleep(2000))
                {
                    try
                    {
                        text = Client.Post(url, data, null, false);
                        if (text.Contains("INSTALL_FAILED_INSUFFICIENT_STORAGE") && !flag)
                        {
                            Logger.Info("Got response: {0}", text);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Exception in post request");
                        Logger.Error(ex.Message);
                        if (num <= 27 && !BlueStacks.hyperDroid.Common.Utils.IsProcessAlive(Strings.FrontendLockName) && !BlueStacks.hyperDroid.Common.Utils.IsGlHotAttach())
                        {
                            Logger.Info("Starting Frontend. BstCommandProcessor not running but service is running.");
                            BlueStacks.hyperDroid.Common.Utils.StartHiddenFrontend();
                        }
                        continue;
                    }
                    break;
                }
            }
            else
            {
                text = Client.Post(url, data, null, false);
            }
            Logger.Info("HTTPHandler: Got response for " + path + ": " + text);
            return text;
        }

        public static string PostFile(int port, string path, string file)
        {
            Logger.Info("HTTPHandler: Uploading {0} to http://127.0.0.1:{1}/{2}", file, port, path);
            WebClient webClient = new WebClient();
            string address = "http://127.0.0.1:" + port + "/" + path;
            string text = null;
            if (port == VmCmdHandler.s_ServerPort)
            {
                if (!BlueStacks.hyperDroid.Common.Utils.StartServiceIfNeeded())
                {
                    BlueStacks.hyperDroid.Common.Utils.WaitForBootComplete();
                }
                for (int num = 10; num > 0; num--, Thread.Sleep(2000))
                {
                    try
                    {
                        byte[] bytes = webClient.UploadFile(address, file);
                        text = Encoding.UTF8.GetString(bytes);
                    }
                    catch (WebException ex)
                    {
                        Logger.Error("Exception in post file");
                        HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                        Logger.Error(ex.Message);
                        if (httpWebResponse != null && httpWebResponse.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            if (num == 1)
                            {
                                throw ex;
                            }
                            num = 2;
                        }
                        if (num <= 7 && !BlueStacks.hyperDroid.Common.Utils.IsProcessAlive(Strings.FrontendLockName) && !BlueStacks.hyperDroid.Common.Utils.IsGlHotAttach())
                        {
                            Logger.Info("Starting Frontend. BstCommandProcessor not running but service is running.");
                            BlueStacks.hyperDroid.Common.Utils.StartHiddenFrontend();
                        }
                        continue;
                    }
                    break;
                }
            }
            else
            {
                byte[] bytes2 = webClient.UploadFile(address, file);
                text = Encoding.UTF8.GetString(bytes2);
            }
            Logger.Info("HTTPHandler: Got response for " + path + ": " + text);
            return text;
        }

        private static void WriteErrorJson(string reason, HttpListenerResponse res)
        {
            JSonWriter jSonWriter = new JSonWriter();
            jSonWriter.WriteArrayBegin();
            jSonWriter.WriteObjectBegin();
            jSonWriter.WriteMember("success", false);
            jSonWriter.WriteMember("reason", reason);
            jSonWriter.WriteObjectEnd();
            jSonWriter.WriteArrayEnd();
            BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
        }

        public static void ShowTileInterface(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got ShowTileInterface {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                KeyboardSend.KeyDown(Keys.LWin);
                KeyboardSend.KeyUp(Keys.LWin);
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteArrayBegin();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                jSonWriter.WriteArrayEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in ShowTileInterface: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void StopAppHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got StopApp {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                bool value = false;
                RequestData requestData = HTTPUtils.ParseRequest(req);
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                    if (string.Compare(text, "pkg", true) == 0)
                    {
                        value = true;
                        string cmd = "StopApp " + requestData.data[text];
                        VmCmdHandler.RunCommand(cmd);
                    }
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteArrayBegin();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", value);
                jSonWriter.WriteObjectEnd();
                jSonWriter.WriteArrayEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in StopAppInterface: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void ApkInstalled(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got ApkInstalled {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            bool flag = false;
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data:");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                    string input = requestData.data[text];
                    JSonReader jSonReader = new JSonReader();
                    IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
                    string text2 = iJSonObject["package"].StringValue.Trim();
                    Logger.Info("package: {0}", text2);
                    string version = iJSonObject["version"].StringValue.Trim();
                    if (text2 == "com.android.vending")
                    {
                        Logger.Info("HTTPHandler: Not creating shortcut for " + text2);
                        break;
                    }
                    Logger.Info("Removing package if present");
                    lock (HTTPHandler.s_sync)
                    {
                        AppUninstaller.RemoveFromJson(text2);
                    }
                    Logger.Info("Files:");
                    string[] allKeys2 = requestData.files.AllKeys;
                    foreach (string text3 in allKeys2)
                    {
                        Logger.Info("Key: {0}, Value: {1}", text3, requestData.files[text3]);
                        string text4 = requestData.files[text3];
                        string fileName = Path.GetFileName(text4);
                        string text5 = Path.Combine(Strings.GadgetDir, fileName);
                        try
                        {
                            if (File.Exists(text5))
                            {
                                File.Delete(text5);
                            }
                            File.Move(text4, text5);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Exception when handling app icons");
                            Logger.Error(ex.ToString());
                        }
                    }
                    string text6 = iJSonObject["activities"].StringValue.Trim();
                    Logger.Info(text6);
                    jSonReader = new JSonReader();
                    IJSonObject iJSonObject2 = jSonReader.ReadAsJSonObject(text6);
                    for (int k = 0; k < iJSonObject2.Length; k++)
                    {
                        string img = iJSonObject2[k]["img"].StringValue.Trim();
                        string activity = iJSonObject2[k]["activity"].StringValue.Trim();
                        string name = iJSonObject2[k]["name"].StringValue.Trim();
                        lock (HTTPHandler.s_sync)
                        {
                            BlueStacks.hyperDroid.Agent.ApkInstall.AppInstalled(name, text2, activity, img, version);
                        }
                    }
                    if (iJSonObject2.Length == 0 && !text2.Contains("android"))
                    {
                        if (!BlueStacks.hyperDroid.Common.Utils.IsOEM("wildtangent"))
                        {
                            flag = true;
                        }
                    }
                    else
                    {
                        flag = false;
                    }
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteArrayBegin();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                jSonWriter.WriteArrayEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
                if (flag)
                {
                    MessageBox.Show("App has been installed");
                }
            }
            catch (Exception ex2)
            {
                Logger.Error("Exception in Server ApkInstalled");
                Logger.Error(ex2.ToString());
                HTTPHandler.WriteErrorJson(ex2.Message, res);
            }
        }

        public static void AppUninstalled(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got AppUninstalled {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data:");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                    string input = requestData.data[text];
                    JSonReader jSonReader = new JSonReader();
                    IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
                    string text2 = iJSonObject["package"].StringValue.Trim();
                    Logger.Info("package: {0}", text2);
                    lock (HTTPHandler.s_sync)
                    {
                        AppUninstaller.AppUninstalled(text2);
                    }
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteArrayBegin();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                jSonWriter.WriteArrayEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server AppUninstalled");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void GetAppList(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got GetAppList {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            RequestData requestData = HTTPUtils.ParseRequest(req);
            Logger.Info("QueryString:");
            string[] allKeys = requestData.queryString.AllKeys;
            foreach (string text in allKeys)
            {
                Logger.Info("Key: {0}, Value: {1}", text, requestData.queryString[text]);
            }
            string path = HTTPHandler.s_AppsDotJsonFile;
            Logger.Info("HTTPHandler: Reading apps.json");
            StreamReader streamReader = new StreamReader(path);
            string input = streamReader.ReadToEnd();
            streamReader.Close();
            JSonReader jSonReader = new JSonReader();
            IJSonObject o = jSonReader.ReadAsJSonObject(input);
            JSonWriter jSonWriter = new JSonWriter();
            jSonWriter.WriteObjectBegin();
            jSonWriter.WriteMember("json");
            jSonWriter.Write(o);
            jSonWriter.WriteObjectEnd();
            Logger.Info(jSonWriter.ToString());
            BlueStacks.hyperDroid.Common.HTTP.Utils.Write(HTTPHandler.CheckForJsonp(jSonWriter.ToString(), req), res);
        }

        public static void GetAppImage(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got GetAppList {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            RequestData requestData = HTTPUtils.ParseRequest(req);
            Logger.Info("QueryString:");
            string[] allKeys = requestData.queryString.AllKeys;
            foreach (string text in allKeys)
            {
                Logger.Info("Key: {0}, Value: {1}", text, requestData.queryString[text]);
            }
            string path = requestData.queryString["image"];
            string path2 = Path.Combine(Strings.GadgetDir, path);
            if (File.Exists(path2))
            {
                byte[] array = File.ReadAllBytes(path2);
                res.Headers.Add("Cache-Control: max-age=2592000");
                res.OutputStream.Write(array, 0, array.Length);
            }
            else
            {
                res.StatusCode = 404;
                res.StatusDescription = "Not Found.";
            }
        }

        public static void ApkInstall(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got ApkInstall {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data:");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string value = BlueStacks.hyperDroid.Agent.ApkInstall.InstallApk(requestData.data["path"]);
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteMember("reason", value);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server AppInstall");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void AppUninstall(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got AppUninstall {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data:");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string text2 = requestData.data["package"];
                Logger.Info("package: {0}", text2);
                string text3 = requestData.data["name"];
                Logger.Info("name: {0}", text3);
                string text4 = requestData.data["nolookup"];
                Logger.Info("nolookup: {0}", text4);
                bool value = (byte)((AppUninstaller.SilentUninstallApp(text3, text2, text4 != null) == 0) ? 1 : 0) != 0;
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteArrayBegin();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", value);
                jSonWriter.WriteObjectEnd();
                jSonWriter.WriteArrayEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server AppUninstall");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void RunApp(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got RunApp {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                bool value = true;
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data:");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string arg = requestData.data["package"];
                string arg2 = requestData.data["activity"];
                string text2 = requestData.data["apkUrl"];
                if (req.UserAgent.Contains("BlueStacks"))
                {
                    string text3 = "runex " + arg + "/" + arg2;
                    if (text2 != null)
                    {
                        text3 = text3 + " " + text2;
                    }
                    value = HDAgent.DoRunCmd(text3);
                }
                else
                {
                    string fileName = HDAgent.s_InstallDir + "\\HD-RunApp.exe";
                    string text4 = "-p " + arg + " -a " + arg2;
                    if (text2 != null)
                    {
                        text4 = text4 + " -url " + text2;
                    }
                    Logger.Info("Starting RunApp");
                    Process.Start(fileName, text4);
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", value);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server RunApp");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void InstallAppByURL(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got InstallAppByURL {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                string text = requestData.data["appURL"];
                string text2 = requestData.data["storeType"];
                string fileName = Path.Combine(HDAgent.s_InstallDir, "HD-RunApp.exe");
                Process.Start(fileName);
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("url", text);
                string path;
                switch (text2)
                {
                    case "amz":
                        Logger.Info("Trying to install an Amazon app: {0}", text);
                        path = Strings.AppInstallUrl;
                        break;
                    case "opera":
                        Logger.Info("Trying to install an Opera app: {0}", text);
                        path = HDAgent.s_BrowserInstallPath;
                        break;
                    default:
                        Logger.Error("Invalid storeType: " + text2);
                        return;
                }
                try
                {
                    HTTPHandler.Post(VmCmdHandler.s_ServerPort, path, dictionary);
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception when sending post request");
                    Logger.Error(ex.ToString());
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteArrayBegin();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                jSonWriter.WriteArrayEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex2)
            {
                Logger.Error("Exception in InstallAppByURL");
                Logger.Error(ex2.ToString());
                HTTPHandler.WriteErrorJson(ex2.Message, res);
            }
        }

        public static void SaveCCPin(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got SaveCCPin {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req, false);
                Logger.Info("Data");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value hidden", text);
                }
                Logger.Info("QueryString");
                string[] allKeys2 = requestData.queryString.AllKeys;
                foreach (string text2 in allKeys2)
                {
                    Logger.Info("Key: {0}, Value: {1}", text2, requestData.queryString[text2]);
                }
                string text3 = requestData.queryString["callback"];
                if (requestData.data["email"] != null)
                {
                    HDAgent.Email = requestData.data["email"];
                }
                if (requestData.data["key"] != null)
                {
                    Auth.Token.Key = requestData.data["key"];
                }
                if (requestData.data["secret"] != null)
                {
                    Auth.Token.Secret = requestData.data["secret"];
                }
                if (requestData.data["pin"] != null)
                {
                    HDAgent.CCPin = requestData.data["pin"];
                    Logger.Info("Connected to Cloud");
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                string text4 = "";
                text4 = ((text3 != null) ? text3 + "(" + jSonWriter.ToString() + ");" : jSonWriter.ToString());
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(text4, res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server SaveCCPin");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void CCPin(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got CCPin {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("QueryString");
                string[] allKeys = requestData.queryString.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.queryString[text]);
                }
                string text2 = requestData.queryString["callback"];
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(HTTPHandler.s_CloudRegKey);
                string value = (string)registryKey.GetValue("CCPin", "null");
                jSonWriter.WriteMember("pin", value);
                jSonWriter.WriteObjectEnd();
                string text3 = "";
                text3 = ((text2 != null) ? text2 + "(" + jSonWriter.ToString() + ");" : jSonWriter.ToString());
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(text3, res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server CCPin");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void CCUrl(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got CCUrl {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("QueryString");
                string[] allKeys = requestData.queryString.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.queryString[text]);
                }
                string text2 = requestData.queryString["callback"];
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(HTTPHandler.s_CloudRegKey);
                string value = (string)registryKey.GetValue("Host", Strings.ChannelsUrl);
                jSonWriter.WriteMember("url", value);
                jSonWriter.WriteObjectEnd();
                string text3 = "";
                text3 = ((text2 != null) ? text2 + "(" + jSonWriter.ToString() + ");" : jSonWriter.ToString());
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(text3, res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server CCUrl");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void Restart(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got Restart {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            JSonWriter jSonWriter = new JSonWriter();
            jSonWriter.WriteArrayBegin();
            jSonWriter.WriteObjectBegin();
            jSonWriter.WriteMember("success", true);
            jSonWriter.WriteObjectEnd();
            jSonWriter.WriteArrayEnd();
            BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            Process.Start(Path.Combine(HDAgent.s_InstallDir, "HD-Restart.exe"), "Android");
        }

        public static void Ping(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got Ping {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteArrayBegin();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                jSonWriter.WriteArrayEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(HTTPHandler.CheckForJsonp(jSonWriter.ToString(), req), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server Ping");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void AppCrashedInfo(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got AppCrashedInfo {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            string text = "";
            string text2 = "";
            string text3 = "";
            string text4 = "";
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data:");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text5 in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text5, requestData.data[text5]);
                    string input = requestData.data[text5];
                    JSonReader jSonReader = new JSonReader();
                    IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
                    text = iJSonObject["shortPackageName"].StringValue.Trim();
                    Logger.Info("shortPackageName: {0}", text);
                    try
                    {
                        text2 = iJSonObject["packageName"].StringValue.Trim();
                        Logger.Info("packageName: {0}", text2);
                        text3 = iJSonObject["versionCode"].StringValue.Trim();
                        Logger.Info("versionCode: {0}", text3);
                        text4 = iJSonObject["versionName"].StringValue.Trim();
                        Logger.Info("versionName: {0}", text4);
                    }
                    catch
                    {
                        Logger.Error("Only shortPackageName received");
                    }
                }
                Logger.Info("Files:");
                string[] allKeys2 = requestData.files.AllKeys;
                foreach (string text6 in allKeys2)
                {
                    Logger.Info("Key: {0}, Value: {1}", text6, requestData.files[text6]);
                    string path = requestData.files[text6];
                    Path.GetFileName(path);
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteArrayBegin();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                jSonWriter.WriteArrayEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server AppCrashedInfo");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void DoAction(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got DoAction {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.queryString[text]);
                }
                if (requestData.data["action"] == "openforgotpassword")
                {
                    Registry.LocalMachine.OpenSubKey(HTTPHandler.s_CloudRegKey);
                    string fileName = Service.Host + "/?forgotpassword=1";
                    Process.Start(fileName);
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server DoAction");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void GetUserData(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got GetUserData {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Agent\\Cloud");
                string value = (string)registryKey.GetValue("Email", "");
                registryKey.Close();
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("guid", User.GUID);
                jSonWriter.WriteMember("email", value);
                jSonWriter.WriteMember("version", "0.9.4.4078");
                jSonWriter.WriteMember("culture", CultureInfo.CurrentCulture.Name.ToLower());
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(HTTPHandler.CheckForJsonp(jSonWriter.ToString(), req), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server GetUserData");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void ShowNotification(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got ShowNotification {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string action = requestData.data["msgAction"];
                string title = requestData.data["displayTitle"];
                string message = requestData.data["displayMsg"];
                string actionURL = requestData.data["actionURL"];
                string fileName = requestData.data["fileName"];
                string imageURL = requestData.data["imageURL"];
                CloudAnnouncement.ShowNotification(action, title, message, actionURL, fileName, imageURL);
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server ShowNotification");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void ShowFeNotification(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got ShowFeNotification {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
                int num = (int)registryKey.GetValue("FrontendServerPort");
                string text = "http://127.0.0.1:" + num + "/" + Strings.ShowFeNotificationUrl;
                RequestData requestData = HTTPUtils.ParseRequest(req);
                JSonReader jSonReader = new JSonReader();
                IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(requestData.data["data"]);
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("data", iJSonObject.ToString());
                Logger.Info("Sending Fe-notification request to: " + text);
                Client.Post(text, dictionary, null, false);
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in ShowFeNotification: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void SendAppDataToFE(string package, string activity)
        {
            Logger.Info("HTTPHandler:SendAppDataToFE(\"{0}\",\"{1}\")", package, activity);
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
                int num = (int)registryKey.GetValue("FrontendServerPort");
                string text = "http://127.0.0.1:" + num + "/" + Strings.AppDataFEUrl;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("package", package);
                dictionary.Add("activity", activity);
                Logger.Info("Sending SendAppDataToFE request to: " + text);
                Client.Post(text, dictionary, null, false);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in SendAppDataToFE: " + ex.ToString());
            }
        }

        public static void SwitchToLauncher(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got SwitchToLauncher {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
                int num = (int)registryKey.GetValue("FrontendServerPort");
                string text = "http://127.0.0.1:" + num + "/" + Strings.SwitchToLauncherUrl;
                Logger.Info("Sending SwitchToLauncher request to: " + text);
                Client.Get(text, null, false);
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in SwitchToLauncher: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void SwitchToWindows(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got SwitchToWindows {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
                int num = (int)registryKey.GetValue("FrontendServerPort");
                string text = "http://127.0.0.1:" + num + "/" + Strings.SwitchToWindowsUrl;
                Logger.Info("Sending SwitchToWindows request to: " + text);
                Client.Get(text, null, false);
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in SwitchToWindows: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void ShowSysTrayNotification(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got ShowSysTrayNotification {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Debug("Tray notification Data:");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Debug("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string message = requestData.data["message"];
                string title = requestData.data["title"];
                int timeout = Convert.ToInt32(requestData.data["timeout"]);
                if (requestData.data["status"] != null && requestData.data["status"].Equals("error"))
                {
                    SysTray.ShowTrayStatus(ToolTipIcon.Error, title, message, timeout);
                }
                else
                {
                    SysTray.ShowTrayStatus(ToolTipIcon.Info, title, message, timeout);
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in ShowSysTrayNotification: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void ExitAgent(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got ExitAgent {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
                SysTray.DisposeIcon();
                Application.Exit();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in ExitAgent: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
                Environment.Exit(-1);
            }
        }

        public static void QuitFrontend(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got QuitFrontend {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
                int num = (int)registryKey.GetValue("FrontendServerPort");
                string text = "http://127.0.0.1:" + num + "/" + Strings.QuitFrontend;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("reason", "app_exiting");
                Logger.Info("Sending Quit request to: " + text);
                Client.Post(text, dictionary, null, false);
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in QuitFrontend: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void AddApp(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got AddApp {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string text2 = "";
                string text3 = "";
                Logger.Info("Files:");
                string[] allKeys2 = requestData.files.AllKeys;
                foreach (string text4 in allKeys2)
                {
                    Logger.Info("Key: {0}, Value: {1}", text4, requestData.files[text4]);
                    text2 = requestData.files[text4];
                    text3 = Path.GetFileName(text2);
                }
                string text5 = requestData.data["app_type"];
                switch (text5)
                {
                    case "app":
                        {
                            string input = requestData.data["name"];
                            string inImage = text3;
                            string text6 = requestData.data["package"];
                            string text7 = requestData.data["activity"];
                            string inVersion;
                            try
                            {
                                inVersion = requestData.data["version"];
                            }
                            catch
                            {
                                inVersion = "Unknown";
                            }
                            string text8 = Path.Combine(Strings.GadgetDir, text3);
                            if (File.Exists(text8))
                            {
                                File.Delete(text8);
                            }
                            File.Move(text2, text8);
                            input = Regex.Replace(input, "[\\x22\\\\\\/:*?|<>]", " ");
                            lock (HTTPHandler.s_sync)
                            {
                                JsonParser.AddToJson(new AppInfo(input, inImage, text6, text7, "0", "no", inVersion));
                            }
                            BlueStacks.hyperDroid.Agent.ApkInstall.ResizeImage(text8);
                            string text9 = BlueStacks.hyperDroid.Agent.ApkInstall.ConvertToIco(HTTPHandler.s_Png2ico, text8, HTTPHandler.s_IconsDir);
                            if (!File.Exists(text9))
                            {
                                text9 = HTTPHandler.s_IconFile;
                            }
                            string target = Path.Combine(HDAgent.s_InstallDir, "HD-RunApp.exe");
                            string text10 = Path.Combine(HTTPHandler.s_MyAppsDir, input) + ".lnk";
                            string targetArgs = "-p " + text6 + " -a " + text7;
                            if (HDAgent.CreateShortcut(target, text10, "", text9, targetArgs, 0) != 0)
                            {
                                Logger.Error("Couldn't create shorcut {0}", text10);
                            }
                            else
                            {
                                Logger.Info("Created shorcut {0}", text10);
                            }
                            break;
                        }
                    case "store":
                        {
                            string input = requestData.data["name"];
                            string inImage = text3;
                            string text6 = requestData.data["package"];
                            string text7 = requestData.data["activity"];
                            string inVersion;
                            try
                            {
                                inVersion = requestData.data["version"];
                            }
                            catch
                            {
                                inVersion = "Unknown";
                            }
                            string text8 = Path.Combine(Strings.GadgetDir, text3);
                            if (File.Exists(text8))
                            {
                                File.Delete(text8);
                            }
                            File.Move(text2, text8);
                            input = Regex.Replace(input, "[\\x22\\\\\\/:*?|<>]", " ");
                            lock (HTTPHandler.s_sync)
                            {
                                JsonParser.AddToJson(new AppInfo(input, inImage, text6, text7, "0", "yes", inVersion));
                            }
                            BlueStacks.hyperDroid.Agent.ApkInstall.ResizeImage(text8);
                            string text9 = BlueStacks.hyperDroid.Agent.ApkInstall.ConvertToIco(HTTPHandler.s_Png2ico, text8, HTTPHandler.s_IconsDir);
                            if (!File.Exists(text9))
                            {
                                text9 = HTTPHandler.s_IconFile;
                            }
                            string target = Path.Combine(HDAgent.s_InstallDir, "HD-RunApp.exe");
                            string text10 = Path.Combine(HTTPHandler.s_AppStoresDir, input) + ".lnk";
                            string targetArgs = "-p " + text6 + " -a " + text7;
                            if (HDAgent.CreateShortcut(target, text10, "", text9, targetArgs, 0) != 0)
                            {
                                Logger.Error("Couldn't create shorcut {0}", text10);
                            }
                            else
                            {
                                Logger.Info("Created shorcut {0}", text10);
                            }
                            break;
                        }
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server AddApp");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void LogAndroidClickEvent(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got LogAndroidClickEvent {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string text2 = requestData.data["package"];
                string homeVersion = requestData.data["clickloc"];
                string appName = requestData.data["appname"];
                string appVersion = requestData.data["appver"];
                if (string.Compare(text2, "com.bluestacks.home", true) != 0 && string.Compare(text2, "com.bluestacks.setup", true) != 0 && string.Compare(text2, "mpi.v23", true) != 0 && string.Compare(text2, "com.android.systemui", true) != 0 && string.Compare(text2, "com.bluestacks.s2p", true) != 0 && string.Compare(text2, "com.bluestacks.gamepophome", true) != 0)
                {
                    Stats.SendAppStats(appName, text2, appVersion, homeVersion, Stats.AppType.app);
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server LogAndroidClickEvent");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void CheckUserInactive(object obj)
        {
            Logger.Info("In CheckUserInactive CallBack, Last activity at : " + HTTPHandler.s_LastActivityClickedTime);
            Mutex mutex = default(Mutex);
            bool flag = BlueStacks.hyperDroid.Common.Utils.IsAlreadyRunning(Strings.FrontendLockName, out mutex);
            if (!flag)
            {
                mutex.Close();
            }
            if (flag && TimeSpan.Compare(DateTime.Now.Subtract(HTTPHandler.s_LastActivityClickedTime), new TimeSpan(0, HTTPHandler.s_TimeoutMins, 0)) <= -1)
            {
                return;
            }
            Logger.Info("User is Idle");
            HTTPHandler.s_FrontendState = "Idle";
            HTTPHandler.SendActivityEndedStats();
            if (HTTPHandler.s_AppTimeoutVerifier != null)
            {
                HTTPHandler.s_AppTimeoutVerifier.Dispose();
                HTTPHandler.s_AppTimeoutVerifier = null;
            }
        }

        public static void SetLastActivityClicked(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got SetLastActivityClicked {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string strA = requestData.data["status"];
                if (string.Compare(strA, "true") == 0)
                {
                    HTTPHandler.s_LastActivityClickedTime = DateTime.Now;
                    if (string.Compare(HTTPHandler.s_FrontendState, "Interacting", true) != 0)
                    {
                        HTTPHandler.s_FrontendState = "Interacting";
                        if (HTTPHandler.s_AppTimeoutVerifier == null)
                        {
                            HTTPHandler.s_AppTimeoutVerifier = new System.Threading.Timer(HTTPHandler.CheckUserInactive, null, HTTPHandler.s_TimeoutMins * 60 * 1000, HTTPHandler.s_TimeoutMins * 60 * 1000);
                        }
                        else
                        {
                            HTTPHandler.s_AppTimeoutVerifier.Change(HTTPHandler.s_TimeoutMins * 60 * 1000, HTTPHandler.s_TimeoutMins * 60 * 1000);
                        }
                        HTTPHandler.s_LastActivityStartTime = HTTPHandler.s_LastActivityClickedTime;
                    }
                }
                else if (string.Compare(HTTPHandler.s_FrontendState, "Interacting", true) == 0)
                {
                    HTTPHandler.s_FrontendState = "Inactive";
                    HTTPHandler.s_LastActivityClickedTime = DateTime.Now;
                    if (HTTPHandler.s_AppTimeoutVerifier != null)
                    {
                        HTTPHandler.s_AppTimeoutVerifier.Dispose();
                        HTTPHandler.s_AppTimeoutVerifier = null;
                    }
                    HTTPHandler.SendActivityEndedStats();
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in SetLastActivityClicked");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void LogWebAppChannelClickEvent(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got LogWebAppChannelClickEvent {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string packageName = requestData.data["package"];
                string homeVersion = requestData.data["clickloc"];
                string appName = requestData.data["appname"];
                Stats.SendWebAppChannelStats(appName, packageName, homeVersion);
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server LogWebAppChannelClickEvent");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void LogAndroidSearchEvent(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got LogAndroidSearchEvent {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                Logger.Info("Data");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string keyword = requestData.data["keyword"];
                Stats.SendSearchAppStats(keyword);
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server LogAndroidSearchEvent");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void SetClipboardData(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got SetClipboardData {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Debug("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string text2 = requestData.data["text"];
                Logger.Debug("ClipboradText {0}", text2);
                Clipboard.SetText(text2);
                HDAgent.clipboardClient.SetCachedText(text2);
                Logger.Debug("CachedText {0}", HDAgent.clipboardClient.GetCachedText());
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server SetClipboardData");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void SendPendingNotifications(object obj)
        {
            Logger.Info("Inside SendPendingNotifications");
            HTTPHandler.s_NotificationLockHelper[1] = true;
            HTTPHandler.s_LockForTurn = 0;
            if (HTTPHandler.s_NotificationLockHelper[0] && HTTPHandler.s_LockForTurn == 0)
            {
                Logger.Info(string.Format("Critical resources are being used, returning"));
                HTTPHandler.s_NotificationLockHelper[1] = false;
            }
            else
            {
                try
                {
                    while (HTTPHandler.s_PendingNotifications.Count > 0)
                    {
                        if (HTTPHandler.s_PendingNotifications.First.Value.NotificationSent)
                        {
                            HTTPHandler.s_PendingNotifications.RemoveFirst();
                            continue;
                        }
                        string package = HTTPHandler.s_PendingNotifications.First.Value.Package;
                        string message = HTTPHandler.s_PendingNotifications.First.Value.Message;
                        SysTray.ShowInfoShort(package, message);
                        HTTPHandler.s_PendingNotifications.RemoveFirst();
                        if (HTTPHandler.s_PendingNotifications.Count != 0)
                        {
                            break;
                        }
                        HTTPHandler.s_PendingNotifications.AddFirst(new AndroidNotification(package, message));
                        HTTPHandler.s_PendingNotifications.First.Value.NotificationSent = true;
                        break;
                    }
                    if (HTTPHandler.s_PendingNotifications.Count == 0)
                    {
                        HTTPHandler.s_NotificationTimer.Dispose();
                        HTTPHandler.s_NotificationTimer = null;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error Occured, Err : " + ex.ToString());
                }
                HTTPHandler.s_NotificationLockHelper[1] = false;
            }
        }

        public static void NotificationHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got NotificationHandler {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            if (!Features.IsFeatureEnabled(1024u))
            {
                Logger.Info("Android notifications disabled. Not showing.");
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            }
            else
            {
                HTTPHandler.s_NotificationLockHelper[0] = true;
                HTTPHandler.s_LockForTurn = 1;
                while (HTTPHandler.s_NotificationLockHelper[1] && HTTPHandler.s_LockForTurn == 1)
                {
                }
                try
                {
                    RequestData requestData = HTTPUtils.ParseRequest(req, false);
                    Logger.Info("Data");
                    string[] allKeys = requestData.data.AllKeys;
                    foreach (string text in allKeys)
                    {
                        Logger.Debug("Key: {0}, Value: {1}", text, requestData.data[text]);
                        string input = requestData.data[text];
                        JSonReader jSonReader = new JSonReader();
                        IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
                        string text2 = iJSonObject["pkg"].StringValue.Trim();
                        iJSonObject["id"].StringValue.Trim();
                        string text3 = iJSonObject["content"].StringValue.Trim();
                        string text4 = iJSonObject["tickerText"].StringValue.Trim();
                        text3 = text3.Replace("*bst*", "\n");
                        string text5 = (text3 == "") ? text4 : text3;
                        string text6 = default(string);
                        string path = default(string);
                        string text7 = default(string);
                        string text8 = default(string);
                        if (!JsonParser.GetAppInfoFromPackageName(text2, out text6, out path, out text7, out text8))
                        {
                            Logger.Error("Systray: Notifying app {0} not found!", text2);
                        }
                        else if (text2 == "bn.ereader" || text2 == "com.amazon.venezia" || text2 == "getjar.android.client" || text2 == "me.onemobile.android" || text2 == "com.movend.gamebox" || text2 == "com.android.vending")
                        {
                            Logger.Info("HTTPHandler: Not showing notification for " + text2);
                        }
                        else if (text3.Contains("%"))
                        {
                            Logger.Info("HTTPHandler: Not showing notification for {0} because the content seems to show download info", text2);
                        }
                        else
                        {
                            Logger.Info("HTTPHandler: Showing notification for " + text2);
                            Path.Combine(Strings.GadgetDir, path);
                            lock (HTTPHandler.s_NotificationLock)
                            {
                                bool flag = false;
                                while (HTTPHandler.s_PendingNotifications.Count > 0 && string.Compare(HTTPHandler.s_PendingNotifications.Last.Value.Package, text6, true) == 0 && !HTTPHandler.s_PendingNotifications.Last.Value.NotificationSent)
                                {
                                    if (!HTTPHandler.s_PendingNotifications.First.Value.OldNotificationFlag)
                                    {
                                        flag = true;
                                    }
                                    HTTPHandler.s_PendingNotifications.RemoveLast();
                                }
                                if (flag)
                                {
                                    HTTPHandler.s_PendingNotifications.AddLast(new AndroidNotification(text6, text5));
                                    HTTPHandler.s_PendingNotifications.Last.Value.NotificationSent = true;
                                }
                                HTTPHandler.s_PendingNotifications.AddLast(new AndroidNotification(text6, text5));
                                if (HTTPHandler.s_PendingNotifications.Count == 1)
                                {
                                    SysTray.ShowInfoShort(text6, text5);
                                    HTTPHandler.s_PendingNotifications.Last.Value.NotificationSent = true;
                                    if (HTTPHandler.s_NotificationTimer == null)
                                    {
                                        HTTPHandler.s_NotificationTimer = new System.Threading.Timer(HTTPHandler.SendPendingNotifications, null, HTTPHandler.s_NotificationTimeout, HTTPHandler.s_NotificationTimeout);
                                    }
                                    else
                                    {
                                        HTTPHandler.s_NotificationTimer.Change(HTTPHandler.s_NotificationTimeout, HTTPHandler.s_NotificationTimeout);
                                    }
                                }
                                else
                                {
                                    if (HTTPHandler.s_PendingNotifications.Count > 0 && HTTPHandler.s_PendingNotifications.First.Value.NotificationSent)
                                    {
                                        HTTPHandler.s_PendingNotifications.RemoveFirst();
                                    }
                                    while (HTTPHandler.s_PendingNotifications.Count > 0 && string.Compare(HTTPHandler.s_PendingNotifications.First.Value.Package, text6, true) != 0)
                                    {
                                        SysTray.ShowInfoShort(HTTPHandler.s_PendingNotifications.First.Value.Package, HTTPHandler.s_PendingNotifications.First.Value.Message);
                                        HTTPHandler.s_PendingNotifications.RemoveFirst();
                                    }
                                }
                            }
                        }
                    }
                    JSonWriter jSonWriter2 = new JSonWriter();
                    jSonWriter2.WriteObjectBegin();
                    jSonWriter2.WriteMember("success", true);
                    jSonWriter2.WriteObjectEnd();
                    BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter2.ToString(), res);
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception in Server NotificationHandler");
                    Logger.Error(ex.ToString());
                    HTTPHandler.WriteErrorJson(ex.Message, res);
                }
                HTTPHandler.s_NotificationLockHelper[0] = false;
            }
        }

        public static void IsAppInstalled(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got IsAppInstalled {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                }
                string text2 = requestData.data["package"];
                string value = "Unknown";
                bool flag;
                try
                {
                    string url = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + VmCmdHandler.s_PingPath;
                    Client.Get(url, null, false, 3000);
                    Logger.Info("Guest booted. Will send request.");
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    dictionary.Add("package", text2);
                    string input = HTTPHandler.Post(VmCmdHandler.s_ServerPort, Strings.IsPackageInstalledUrl, dictionary);
                    JSonReader jSonReader = new JSonReader();
                    IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
                    string a = iJSonObject["result"].StringValue.Trim();
                    if (a == "ok")
                    {
                        Logger.Info("App installed");
                        value = iJSonObject["version"].StringValue.Trim();
                        flag = true;
                    }
                    else
                    {
                        Logger.Info("App not installed");
                        flag = false;
                    }
                }
                catch (Exception)
                {
                    Logger.Info("Guest not booted. Will read from apps.json");
                    if (JsonParser.IsAppInstalled(text2, out value))
                    {
                        Logger.Info("Found in apps.json");
                        flag = true;
                    }
                    else
                    {
                        Logger.Info("Not found in apps.json");
                        flag = false;
                    }
                }
                JSonWriter jSonWriter = new JSonWriter();
                jSonWriter.WriteObjectBegin();
                jSonWriter.WriteMember("installed", flag);
                if (flag)
                {
                    jSonWriter.WriteMember("version", value);
                }
                jSonWriter.WriteMember("success", true);
                jSonWriter.WriteObjectEnd();
                Logger.Info("Sending response: " + jSonWriter.ToString());
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
                Logger.Info("Sent response");
            }
            catch (Exception ex2)
            {
                Logger.Error("Exception in Server IsAppInstalled");
                Logger.Error(ex2.ToString());
                HTTPHandler.WriteErrorJson(ex2.Message, res);
            }
        }

        public static void RestartAgent(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got RestartAgent {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            string path = (string)Registry.LocalMachine.OpenSubKey("SOFTWARE\\BlueStacks").GetValue("InstallDir");
            string text = Path.Combine(path, "HD-Agent.exe");
            Logger.Info("Agent Path " + text);
            string text2 = Path.Combine(Path.GetTempPath(), "BstBatFile.bat");
            Logger.Info("Temp File Path " + text2);
            if (!File.Exists(text2))
            {
                using (FileStream fileStream = File.Create(text2))
                {
                    fileStream.Close();
                }
            }
            Logger.Info("Temp File " + text2 + " Created");
            using (StreamWriter streamWriter = new StreamWriter(text2))
            {
                streamWriter.WriteLine("ping 192.0.2.2 -n 1 -w 100000 > nul");
                streamWriter.WriteLine("call \"" + text + "\"");
            }
            Logger.Info("Temp File " + text2 + " Data Written");
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c \"" + text2 + "\"";
                Logger.Info("Calling " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
            }
            Logger.Info("Bat File Called");
            JSonWriter jSonWriter = new JSonWriter();
            jSonWriter.WriteObjectBegin();
            jSonWriter.WriteMember("success", true);
            jSonWriter.WriteObjectEnd();
            BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
            Environment.Exit(0);
        }

        public static void SystrayVisibility(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got SystrayVisibility {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                string strA = requestData.data["visible"];
                if (string.Compare(strA, "true") == 0)
                {
                    SysTray.SetTrayIconVisibility(true);
                }
                else
                {
                    SysTray.SetTrayIconVisibility(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server SystrayVisibility");
                Logger.Error(ex.ToString());
            }
        }

        public static void TopActivityInfo(HttpListenerRequest req, HttpListenerResponse res)
        {
            Mutex mutex = default(Mutex);
            if (!BlueStacks.hyperDroid.Common.Utils.IsAlreadyRunning(Strings.FrontendLockName, out mutex))
            {
                mutex.Close();
                if (HTTPHandler.s_AppTimeoutVerifier != null)
                {
                    HTTPHandler.s_AppTimeoutVerifier.Dispose();
                    HTTPHandler.s_AppTimeoutVerifier = null;
                }
            }
            else
            {
                Logger.Info("HTTPHandler: Got TopActivityInfo {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
                try
                {
                    RequestData requestData = HTTPUtils.ParseRequest(req);
                    Logger.Info("Data");
                    string[] allKeys = requestData.data.AllKeys;
                    foreach (string text in allKeys)
                    {
                        Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                    }
                    string text2 = requestData.data["packageName"];
                    string text3 = requestData.data["activityName"];
                    Logger.Info("packageName = {0}, activityName = {1}", text2, text3);
                    HTTPHandler.SendActivityChangedStats(text2, text3);
                    HTTPHandler.SendAppDataToFE(text2, text3);
                    JSonWriter jSonWriter = new JSonWriter();
                    jSonWriter.WriteObjectBegin();
                    jSonWriter.WriteMember("success", true);
                    jSonWriter.WriteObjectEnd();
                    BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception in Server TopActivityInfo");
                    Logger.Error(ex.ToString());
                    HTTPHandler.WriteErrorJson(ex.Message, res);
                }
            }
        }

        private static void SendActivityEndedStats()
        {
            Logger.Info("In SendActivityEndedStats");
            if (HTTPHandler.s_LastActivity != null && DateTime.Compare(HTTPHandler.s_LastActivityStartTime, HTTPHandler.s_LastActivityClickedTime) <= 0)
            {
                ulong num = (ulong)HTTPHandler.s_LastActivityClickedTime.Subtract(HTTPHandler.s_LastActivityStartTime).TotalSeconds;
                if (string.Compare(HTTPHandler.s_FrontendState, "idle", true) != 0 && num < 5 && string.Compare(HTTPHandler.s_LastActivityPackage, HTTPHandler.s_FileSharerPackageName, true) != 0)
                {
                    Logger.Info("Activity was active only for " + num + " seconds. So, not reporting stats");
                }
                else
                {
                    Logger.Info("Last Activity = " + HTTPHandler.s_LastActivity + ", Package = " + HTTPHandler.s_LastActivityPackage + ", StartTime = " + HTTPHandler.s_LastActivityStartTime + ", EndTime = " + HTTPHandler.s_LastActivityClickedTime);
                    Stats.SendActivityEndedStats(HTTPHandler.s_LastActivity, HTTPHandler.s_LastActivityPackage, num);
                }
            }
        }

        private static void SendActivityChangedStats(string package, string activity)
        {
            Logger.Info("In SendActivityChangedStats");
            try
            {
                if (string.Compare(HTTPHandler.s_FrontendState, "Interacting", true) != 0)
                {
                    HTTPHandler.s_LastActivity = activity;
                    HTTPHandler.s_LastActivityPackage = package;
                }
                else
                {
                    if (HTTPHandler.s_AppTimeoutVerifier == null)
                    {
                        HTTPHandler.s_AppTimeoutVerifier = new System.Threading.Timer(HTTPHandler.CheckUserInactive, null, HTTPHandler.s_TimeoutMins * 60 * 1000, HTTPHandler.s_TimeoutMins * 60 * 1000);
                    }
                    else
                    {
                        HTTPHandler.s_AppTimeoutVerifier.Change(HTTPHandler.s_TimeoutMins * 60 * 1000, HTTPHandler.s_TimeoutMins * 60 * 1000);
                    }
                    HTTPHandler.s_LastActivityClickedTime = DateTime.Now;
                    HTTPHandler.SendActivityEndedStats();
                    HTTPHandler.s_LastActivityStartTime = HTTPHandler.s_LastActivityClickedTime;
                    HTTPHandler.s_LastActivity = activity;
                    HTTPHandler.s_LastActivityPackage = package;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error Occured, Err: " + ex.ToString());
            }
        }

        private static string CheckForJsonp(string json, HttpListenerRequest req)
        {
            RequestData requestData = HTTPUtils.ParseRequest(req, false);
            string text = requestData.queryString["callback"];
            if (text == null)
            {
                return json;
            }
            return text + "(" + json + ");";
        }

        static HTTPHandler()
        {
            bool[] array = HTTPHandler.s_NotificationLockHelper = new bool[2];
            HTTPHandler.s_LockForTurn = 0;
            HTTPHandler.s_NotificationLock = new object();
            HTTPHandler.s_AppsDotJsonFile = Path.Combine(Strings.GadgetDir, "apps.json");
            HTTPHandler.s_AppStoresDir = Path.Combine(Strings.LibraryDir, Strings.StoreAppsDir);
            HTTPHandler.s_MyAppsDir = Path.Combine(Strings.LibraryDir, Strings.MyAppsDir);
            HTTPHandler.s_IconsDir = Path.Combine(Strings.LibraryDir, Strings.IconsDir) + "\\";
            HTTPHandler.s_IconFile = Path.Combine(HDAgent.s_InstallDir, "BlueStacks.ico");
            HTTPHandler.s_Png2ico = Path.Combine(HDAgent.s_InstallDir, "HD-png2ico.exe");
            HTTPHandler.s_sync = new object();
            HTTPHandler.s_CloudRegKey = "Software\\BlueStacks\\Agent\\Cloud";
        }
    }
}
