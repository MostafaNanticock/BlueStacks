using BlueStacks.hyperDroid.Common.HTTP;
using BlueStacks.hyperDroid.Device;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common
{
    public class VmCmdHandler
    {
        private static string s_Received = null;

        public static string s_AgentServerPortPath = "setwindowsagentaddr";

        public static string s_FrontendServerPortPath = "setwindowsfrontendaddr";

        public static string s_PingPath = "ping";

        public static ushort s_ServerPort
        {
            get
            {
                return (ushort)GuestNetwork.GetHostPort(false, 9999);
            }
        }

        public static void SyncConfig()
        {
            long num = (DateTime.UtcNow.Ticks - 621355968000000000L) / 10000;
            string cmd = "settime " + num;
            VmCmdHandler.RunCommand(cmd);
            string standardName = TimeZone.CurrentTimeZone.StandardName;
            TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            string text = utcOffset.ToString();
            text = ((text[0] == '-') ? "GMT" + text : "GMT+" + text);
            string text2 = TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now).ToString();
            string sysInfo = Profile.GetSysInfo("Select DaylightBias from Win32_TimeZone");
            string text3;
            if (text2 == "True")
            {
                text3 = utcOffset.Add(new TimeSpan(0, Convert.ToInt32(sysInfo), 0)).ToString();
                text3 = ((text3[0] == '-') ? "GMT" + text3 : "GMT+" + text3);
            }
            else
            {
                text3 = text;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("baseUtcOffset", text3);
            dictionary.Add("isDaylightSavingTime", text2);
            dictionary.Add("daylightBias", sysInfo);
            dictionary.Add("utcOffset", text);
            dictionary.Add("standardName", standardName);
            string path = "settz";
            VmCmdHandler.SendRequest(path, dictionary);
            string arg = CultureInfo.CurrentCulture.Name.ToLower();
            cmd = "setlocale " + arg;
            if (VmCmdHandler.RunCommand(cmd) != null)
            {
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.HKLMConfigRegKeyPath);
                registryKey.SetValue("ConfigSynced", 1);
                registryKey.Flush();
                registryKey.Close();
            }
        }

        public static void SetKeyboard(bool isDesktop)
        {
            string cmd = (!isDesktop) ? string.Format("usesoftkeyboard") : string.Format("usehardkeyboard");
            VmCmdHandler.RunCommand(cmd);
        }

        public static string FqdnSend(int port, string serverIn)
        {
            try
            {
                string arg;
                if (string.Compare(serverIn, "agent", true) == 0)
                {
                    arg = VmCmdHandler.s_AgentServerPortPath;
                    goto IL_0044;
                }
                if (string.Compare(serverIn, "frontend", true) == 0)
                {
                    arg = VmCmdHandler.s_FrontendServerPortPath;
                    goto IL_0044;
                }
                Logger.Error("Unknown server: " + serverIn);
                return null;
            IL_0044:
                if (port == 0)
                {
                    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
                    if (string.Compare(serverIn, "agent", true) == 0)
                    {
                        port = (int)registryKey.GetValue("AgentServerPort", 2861);
                    }
                    else if (string.Compare(serverIn, "frontend", true) == 0)
                    {
                        port = (int)registryKey.GetValue("FrontendServerPort", 2861);
                    }
                }
                new Dictionary<string, string>();
                string arg2 = "10.0.2.2:" + port.ToString();
                string cmd = arg + " " + arg2;
                return VmCmdHandler.RunCommand(cmd);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when sending fqdn post request");
                Logger.Error(ex.Message);
                return null;
            }
        }

        public static string RunCommand(string cmd)
        {
            int num = cmd.IndexOf(' ');
            string text;
            string text2;
            if (num == -1)
            {
                text = cmd;
                text2 = "";
            }
            else
            {
                text = cmd.Substring(0, num);
                text2 = cmd.Substring(num + 1);
            }
            Logger.Info("Will send command: {0} to {1}", text2, text);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("arg", text2);
            return VmCmdHandler.SendRequest(text, dictionary);
        }

        public static string SendRequest(string path, Dictionary<string, string> data)
        {
            TimeSpan timeout = new TimeSpan(0, 0, 1);
            int num = 60;
            int num2 = 3;
            while (num > 0)
            {
                try
                {
                    string text = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + path;
                    if (num2 != 0)
                    {
                        num2--;
                        Logger.Info("Sending request to " + text);
                    }
                    string text2 = (path == "runex" || path == "run" || path == "powerrun") ? Client.Post(text, data, null, false, 3000) : ((!(path == VmCmdHandler.s_AgentServerPortPath)) ? Client.Post(text, data, null, false) : Client.Post(text, data, null, false, 1000));
                    Logger.Info("Got response for {0}: {1}", path, text2);
                    IJSonReader iJSonReader = new JSonReader();
                    IJSonObject iJSonObject = iJSonReader.ReadAsJSonObject(text2);
                    VmCmdHandler.s_Received = iJSonObject["result"].StringValue;
                    if (VmCmdHandler.s_Received != "ok" && VmCmdHandler.s_Received != "error")
                    {
                        VmCmdHandler.s_Received = null;
                    }
                }
                catch (Exception ex)
                {
                    if (num2 != 0)
                    {
                        Logger.Error("Exception in SendRequest for {0}: {1}", path, ex.Message);
                    }
                    VmCmdHandler.s_Received = null;
                }
                if (VmCmdHandler.s_Received != null)
                {
                    return VmCmdHandler.s_Received;
                }
                Thread.Sleep(timeout);
                num--;
            }
            return null;
        }

        public static void RunCommandAsync(string cmd, UIHelper.Action continuation, Control control)
        {
            TimerCallback callback = delegate
            {
                VmCmdHandler.RunCommand(cmd);
                if (continuation != null)
                {
                    UIHelper.RunOnUIThread(control, continuation);
                }
            };
            new System.Threading.Timer(callback, null, 0, -1);
        }
    }
}
