using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using BlueStacks.hyperDroid.Common.Interop;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
    public class HTTPHandler
    {
        [CompilerGenerated]
        private static UIHelper.Action _003C_003E9__CachedAnonymousMethodDelegate1;

        [CompilerGenerated]
        private static UIHelper.Action _003C_003E9__CachedAnonymousMethodDelegate3;

        [CompilerGenerated]
        private static UIHelper.Action _003C_003E9__CachedAnonymousMethodDelegate5;

        [CompilerGenerated]
        private static UIHelper.Action _003C_003E9__CachedAnonymousMethodDelegate7;

        [CompilerGenerated]
        private static UIHelper.Action _003C_003E9__CachedAnonymousMethodDelegate9;

        public static void QuitFrontend(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got QuitFrontend {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                HTTPHandler.WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in QuitFrontend: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void ShowTileInterface(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got ShowTileInterface {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                HTTPHandler.WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in ShowTileInterface: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void UpdateWindowTitle(HttpListenerRequest req, HttpListenerResponse res)
        {
        }

        public static void SwitchToLauncher(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got SwitchToLauncher {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                IntPtr intPtr = Window.FindWindow(null, Strings.AppTitle);
                Logger.Info("Sending WM_USER_SWITCH_TO_LAUNCHER to Frontend handle {0}", intPtr);
                Window.SendMessage(intPtr, 1026u, IntPtr.Zero, IntPtr.Zero);
                HTTPHandler.WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server SwitchToLauncher: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void SwitchToWindows(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got SwitchToWindows {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                IntPtr intPtr = Window.FindWindow(null, Strings.AppTitle);
                Logger.Info("Sending WM_CLOSE to Frontend handle {0}", intPtr);
                Window.SendMessage(intPtr, 16u, IntPtr.Zero, IntPtr.Zero);
                HTTPHandler.WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server SwitchToWindows: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void SwitchOrientation(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got SwitchOrientation {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                string s = req.QueryString["orientation"];
                try
                {
                    int num = int.Parse(s);
                    Logger.Info("Orientation change to {0}", num);
                    Console.s_Console.OrientationHandler(num);
                }
                catch (Exception ex)
                {
                    Logger.Info("Got exec in orientation change");
                    Logger.Info(ex.ToString());
                }
            }
            catch (Exception ex2)
            {
                Logger.Error("Exception in SwitchOrientation: " + ex2.ToString());
            }
        }

        public static void ShowWindow(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got ShowWindow {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                UIHelper.RunOnUIThread(Console.s_Console, delegate
                {
                    Console.s_Console.HandleWMUserShowWindow();
                });
                HTTPHandler.WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server ShowWindow: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void ShareScreenshot(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got ShareSnapshot {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                Console.s_Console.HandleShareButtonClicked();
                HTTPHandler.WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server ShareSnapshot: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void ToggleScreen(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got ToggleScreen {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                UIHelper.RunOnUIThread(Console.s_Console, delegate
                {
                    Console.s_Console.ToggleFullScreen();
                });
                HTTPHandler.WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server ToggleScreen: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void GoBack(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got BackPress {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                UIHelper.RunOnUIThread(Console.s_Console, delegate
                {
                    if (Console.s_Console.mFrontendState == Console.State.Connected)
                    {
                        Logger.Info("Back Button Clicked");
                        Console.s_Console.HandleKeyEvent(Keys.Escape, true);
                        Thread.Sleep(100);
                        Console.s_Console.HandleKeyEvent(Keys.Escape, false);
                    }
                });
                HTTPHandler.WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server BackPress: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void CloseScreen(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got CloseScreen {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                UIHelper.RunOnUIThread(Console.s_Console, delegate
                {
                    Console.s_Console.Close();
                });
                HTTPHandler.WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server CloseScreen: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void HandleSoftControlBarEvent(HttpListenerRequest req, HttpListenerResponse res)
        {
            try
            {
                string text = req.QueryString["visible"];
                if (text != null)
                {
                    Console.s_Console.SoftControlBarVisible(text != "0");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in HandleSoftControlBarEvent: " + ex.ToString());
            }
        }

        public static void PingHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got Ping {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                HTTPHandler.WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server Ping");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void UpdateGraphicsDrivers(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got UpdateGraphicsDrivers {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                HTTPHandler.WriteSuccessJson(res);
                UIHelper.RunOnUIThread(Console.s_Console, delegate
                {
                    Console.s_Console.UpdateGraphicsDrivers();
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server UpdateGraphicsDrivers: " + ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        public static void CommandLineArgs(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("HTTPHandler: Got CommandLineArgs {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                List<string> list = new List<string>();
                Logger.Info("Data:");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                    list.Add(requestData.data[text]);
                }
                string[] args = list.ToArray();
                Console.s_Console.ActionOnRelaunch(args);
                HTTPHandler.WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server CommandLineArgs");
                Logger.Error(ex.ToString());
                HTTPHandler.WriteErrorJson(ex.Message, res);
            }
        }

        private static void WriteSuccessJson(HttpListenerResponse res)
        {
            JSonWriter jSonWriter = new JSonWriter();
            jSonWriter.WriteArrayBegin();
            jSonWriter.WriteObjectBegin();
            jSonWriter.WriteMember("success", true);
            jSonWriter.WriteObjectEnd();
            jSonWriter.WriteArrayEnd();
            BlueStacks.hyperDroid.Common.HTTP.Utils.Write(jSonWriter.ToString(), res);
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

        public static void SendSysTrayNotification(string title, string status, string message)
        {
            Logger.Info("Sending Notifications for files sent to windows");
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.HKLMConfigRegKeyPath);
            int num = (int)registryKey.GetValue("AgentServerPort", 2861);
            string url = string.Format("http://127.0.0.1:{0}/{1}", num, "showtraynotification");
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("message", message);
            data.Add("title", title);
            data.Add("status", status);
            Thread thread = new Thread((ThreadStart)delegate
            {
                try
                {
                    Client.Post(url, data, null, false);
                }
                catch (Exception ex)
                {
                    Logger.Error("Cannot send orientation to guest: " + ex.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private static bool IsWindows7AndBelow()
        {
            System.Version v = new System.Version(6, 2, 9200, 0);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= v)
            {
                return false;
            }
            return true;
        }

        public static void UpdateGpsCoordinates(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Inside UpdateGpsCoordinates\nHTTPHandler:  {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.HKLMConfigRegKeyPath);
                int num = (int)registryKey.GetValue("GpsMode", 0);
                int num2 = (int)registryKey.GetValue("GpsSource", 0);
                string str = (string)registryKey.GetValue("GpsLatitude", null);
                string str2 = (string)registryKey.GetValue("GpsLongitude", null);
                if (num != 0)
                {
                    switch (num2)
                    {
                        default:
                            if (!HTTPHandler.IsWindows7AndBelow())
                            {
                                goto case 8;
                            }
                            break;
                        case 0:
                            break;
                        case 8:
                            BlueStacks.hyperDroid.Common.HTTP.Utils.Write(str + "," + str2, res);
                            return;
                    }
                }
                Logger.Info("Stopping Gps Service, gpsMode = " + num + ", gpsSource = " + num2 + ", IsWindows7AndBelow() = " + HTTPHandler.IsWindows7AndBelow());
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write("No Coordinates Available so far", res);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error Occured, Err : ", ex.ToString()));
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write("exception", res);
            }
        }

        public static void PickFilesFromWindows(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Inside PickFilesFromWindows\nHTTPHandler: Got Pick Files To From Windows {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            RequestData requestData = HTTPUtils.ParseRequest(req);
            try
            {
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text in allKeys)
                {
                    Logger.Info("Key = " + text + ", Value = " + requestData.data[text]);
                }
                string sharedFolderDir = Strings.SharedFolderDir;
                string text2 = "";
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (string.Compare(requestData.data["filesNo"].ToUpper(), "MULTIPLE") == 0)
                {
                    openFileDialog.Multiselect = true;
                }
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                if (requestData.data["mimeType"].ToUpper().Contains("VIDEO") || requestData.data["mimeType"].ToUpper().Contains("AUDIO"))
                {
                    text2 = "Video & Audio Files | *.dat; *.wmv; *.3g2; *.3gp; *.3gp2; *.3gpp; *.amv; *.asf;*.avi; *.bin; *.cue; *.divx; *.dv; *.flv; *.gxf; *.iso; *.m1v;*.m2v; *.m2t; *.m2ts; *.m4v; *.mkv; *.mov; *.mp2; *.mp2v; *.mp4;*.mp4v; *.mpa; *.mpe; *.mpeg; *.mpeg1; *.mpeg2; *.mpeg4; *.mpg;*.mpv2; *.mts; *.nsv; *.nuv; *.ogg; *.ogm; *.ogv; *.ogx; *.ps; *.rec;*.rm; *.rmvb; *.tod; *.ts; *.tts; *.vob; *.vro; *.webm; *.mp3";
                }
                else if (requestData.data["mimeType"].ToUpper().Contains("IMAGE"))
                {
                    text2 = "Image Files|*.jpg;*.jpeg;*.png;*.gif";
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                }
                else if (text2.Length < 1)
                {
                    text2 = "All Files|*.*";
                }
                openFileDialog.Filter = text2;
                if (openFileDialog.ShowDialog(Console.s_Console) == DialogResult.OK)
                {
                    string[] array = new string[openFileDialog.SafeFileNames.Length];
                    for (int j = 0; j < openFileDialog.SafeFileNames.Length; j++)
                    {
                        array[j] = openFileDialog.SafeFileNames[j];
                        if (File.Exists(Path.Combine(sharedFolderDir, openFileDialog.SafeFileNames[j])))
                        {
                            array[j] = HTTPHandler.NextAvailableFileName(sharedFolderDir, openFileDialog.SafeFileNames[j]);
                        }
                        Logger.Info("Will Copy : " + openFileDialog.FileNames[j] + "  to " + Path.Combine(sharedFolderDir, array[j]));
                    }
                    for (int k = 0; k < array.Length; k++)
                    {
                        Logger.Info("Copying : " + openFileDialog.FileNames[k] + "  to " + Path.Combine(sharedFolderDir, array[k]));
                        File.Copy(openFileDialog.FileNames[k], Path.Combine(sharedFolderDir, array[k]));
                    }
                    BlueStacks.hyperDroid.Common.HTTP.Utils.Write(string.Join("\t", array), res);
                    Logger.Info("Response Sent: Copied");
                }
                else
                {
                    Logger.Info("Response Sent: false");
                    BlueStacks.hyperDroid.Common.HTTP.Utils.Write("false", res);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error Occured, Err : " + ex.ToString());
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write("false", res);
            }
        }

        private static string NextAvailableFileName(string sharedFolder, string fileName)
        {
            Logger.Info("Inside NextAvailableFileName");
            int num = 1;
            string text = fileName;
            string[] array = new string[2]
			{
				Path.GetFileNameWithoutExtension(fileName),
				Path.GetExtension(fileName)
			};
            Logger.Info("fileName = " + array[0] + ", fileExtension = " + array[1]);
            while (File.Exists(Path.Combine(sharedFolder, text)))
            {
                text = array[0] + num.ToString() + array[1];
                num++;
            }
            return text;
        }

        public static void SendMultipleFilesToWindows(RequestData requestData, HttpListenerResponse res)
        {
            Logger.Info("Multiple Files");
            string text = null;
            string text2 = null;
            string sharedFolderDir = Strings.SharedFolderDir;
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.Description = "Choose folder to copy files";
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            DialogResult dialogResult = folderBrowserDialog.ShowDialog(Console.s_Console);
            if (dialogResult.Equals(DialogResult.OK))
            {
                string selectedPath = folderBrowserDialog.SelectedPath;
                Logger.Debug("User Select " + selectedPath + " Directory");
                string[] allKeys = requestData.data.AllKeys;
                foreach (string text3 in allKeys)
                {
                    try
                    {
                        Logger.Info("Key: {0}, Value: {1}", text3, requestData.data[text3]);
                        if (File.Exists(Path.Combine(sharedFolderDir, requestData.data[text3].Trim())))
                        {
                            if (string.Compare(selectedPath, sharedFolderDir, false) != 0)
                            {
                                if (File.Exists(Path.Combine(selectedPath, requestData.data[text3].Trim())))
                                {
                                    dialogResult = MessageBox.Show(Console.s_Console, "Overwrite " + requestData.data[text3].Trim() + "?", "File already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                                    if (dialogResult == DialogResult.No)
                                    {
                                        File.Delete(Path.Combine(sharedFolderDir, requestData.data[text3].Trim()));
                                        goto end_IL_0106;
                                    }
                                    if (File.Exists(Path.Combine(selectedPath, requestData.data[text3].Trim())))
                                    {
                                        File.Delete(Path.Combine(selectedPath, requestData.data[text3].Trim()));
                                    }
                                }
                                File.Move(Path.Combine(sharedFolderDir, requestData.data[text3].Trim()), Path.Combine(selectedPath, requestData.data[text3].Trim()));
                            }
                            text = ((text != null) ? (text + "\n" + Path.Combine(selectedPath, requestData.data[text3].Trim())) : Path.Combine(selectedPath, requestData.data[text3].Trim()));
                        }
                        else
                        {
                            text2 = ((text2 != null) ? (text2 + "\n" + Path.Combine(selectedPath, requestData.data[text3].Trim())) : Path.Combine(selectedPath, requestData.data[text3].Trim()));
                            Logger.Error(requestData.data[text3] + " does not exist in sharedfolder");
                        }
                    end_IL_0106: ;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error Occured, Err: " + ex.ToString());
                        text2 = ((text2 != null) ? (text2 + "\n" + Path.Combine(selectedPath, requestData.data[text3].Trim())) : Path.Combine(selectedPath, requestData.data[text3].Trim()));
                    }
                }
                if (text != null)
                {
                    HTTPHandler.SendSysTrayNotification("Successfully copied files:", "success", text);
                }
                if (text2 != null)
                {
                    HTTPHandler.SendSysTrayNotification("Cannot copy files:", "error", text2);
                }
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write("true", res);
            }
            else
            {
                Logger.Info("User cancelled browser dialog");
                string[] allKeys2 = requestData.data.AllKeys;
                foreach (string name in allKeys2)
                {
                    try
                    {
                        File.Delete(Path.Combine(sharedFolderDir, requestData.data[name].Trim()));
                    }
                    catch (Exception ex2)
                    {
                        Logger.Error("Error Occured, Err : " + ex2.ToString());
                    }
                }
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write("false", res);
            }
        }

        public static void SendSingleFileToWindows(RequestData requestData, HttpListenerResponse res)
        {
            Logger.Info("Single File");
            string text = null;
            string[] allKeys = requestData.data.AllKeys;
            int num = 0;
            if (num < allKeys.Length)
            {
                string text2 = allKeys[num];
                text = text2;
            }
            string text3 = null;
            string text4 = null;
            string sharedFolderDir = Strings.SharedFolderDir;
            if (text == null)
            {
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write("false", res);
            }
            string text5 = Path.GetExtension(requestData.data[text]).Replace(".", "");
            Logger.Debug("File Extension = " + text5);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = text5 + " files (*." + text5 + ")| *." + text5;
            saveFileDialog.AddExtension = true;
            saveFileDialog.Title = "Save File";
            saveFileDialog.AutoUpgradeEnabled = true;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.DefaultExt = text5;
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.FileName = requestData.data[text];
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.ValidateNames = true;
            DialogResult dialogResult = saveFileDialog.ShowDialog(Console.s_Console);
            if (dialogResult.Equals(DialogResult.OK))
            {
                string fileName = saveFileDialog.FileName;
                Logger.Info("User Selected " + fileName + " Path");
                try
                {
                    Logger.Info("Key: {0}, Value: {1}", text, requestData.data[text]);
                    if (File.Exists(Path.Combine(sharedFolderDir, requestData.data[text].Trim())))
                    {
                        if (string.Compare(fileName, Path.Combine(sharedFolderDir, requestData.data[text]), false) != 0)
                        {
                            if (File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                            File.Move(Path.Combine(sharedFolderDir, requestData.data[text].Trim()), fileName);
                        }
                        text3 = fileName;
                    }
                    else
                    {
                        text4 = fileName;
                        Logger.Error(requestData.data[text] + " does not exist in sharedfolder");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error Occured, Err: " + ex.ToString());
                    text4 = fileName;
                }
                if (text3 != null)
                {
                    HTTPHandler.SendSysTrayNotification("Successfully copied files:", "success", text3);
                }
                else if (text4 != null)
                {
                    HTTPHandler.SendSysTrayNotification("Cannot copy files:", "error", text4);
                }
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write("true", res);
            }
            else
            {
                Logger.Info("User cancelled save Prebundled dialog");
                try
                {
                    File.Delete(Path.Combine(sharedFolderDir, requestData.data[text].Trim()));
                }
                catch (Exception ex2)
                {
                    Logger.Error("Error Occured, Err : " + ex2.ToString());
                }
                BlueStacks.hyperDroid.Common.HTTP.Utils.Write("false", res);
            }
        }

        public static void SendFilesToWindows(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Inside SendFilesToWindows\nHTTPHandler: Got Send Files To Windows {0} request from {1}", req.HttpMethod, req.RemoteEndPoint.ToString());
            RequestData requestData = HTTPUtils.ParseRequest(req);
            if (requestData.data.Count > 1)
            {
                HTTPHandler.SendMultipleFilesToWindows(requestData, res);
            }
            else
            {
                HTTPHandler.SendSingleFileToWindows(requestData, res);
            }
        }
    }
}
