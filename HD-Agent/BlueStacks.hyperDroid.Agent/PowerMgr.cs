using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Agent
{
    internal class PowerMgr : Form
    {
        private enum POWER_WPARAM
        {
            PBT_APMQUERYSUSPEND,
            PBT_APMQUERYSTANDBY,
            PBT_APMQUERYSUSPENDFAILED,
            PBT_APMQUERYSTANDBYFAILED,
            PBT_APMSUSPEND,
            PBT_APMSTANDBY,
            PBT_APMRESUMECRITICAL,
            PBT_APMRESUMESUSPEND,
            PBT_APMRESUMESTANDBY,
            PBTF_APMRESUMEFROMFAILURE = 1,
            PBT_APMBATTERYLOW = 9,
            PBT_APMPOWERSTATUSCHANGE,
            PBT_APMOEMEVENT,
            PBT_APMRESUMEAUTOMATIC = 18,
            PBT_POWERSETTINGCHANGE = 32787
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;

            public int DataLength;
        }

        private enum POWER_LPARAM
        {

        }

        private const int WM_POWERBROADCAST = 536;

        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;

        private static bool s_FrontendStopped;

        private IntPtr m_HPowerNotify;

        private Guid GUID_POWERSCHEME_PERSONALITY = new Guid(610108737, 14659, 17442, 176, 37, 19, 167, 132, 246, 121, 183);

        [DllImport("user32", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

        [DllImport("user32", CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnregisterPowerSettingNotification(IntPtr handle);

        public PowerMgr()
        {
            base.WindowState = FormWindowState.Minimized;
            base.Load += this.OnLoad;
            this.RegisterForPowerNotifications();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            base.Hide();
        }

        public void RegisterForPowerNotifications()
        {
            this.m_HPowerNotify = PowerMgr.RegisterPowerSettingNotification(base.Handle, ref this.GUID_POWERSCHEME_PERSONALITY, 0);
        }

        public void UnregisterForPowerNotifications()
        {
            PowerMgr.UnregisterPowerSettingNotification(this.m_HPowerNotify);
        }

        protected override void OnActivated(EventArgs args)
        {
            base.Hide();
        }

        protected override void WndProc(ref Message m)
        {
            int msg = m.Msg;
            if (msg == 536)
            {
                POWER_WPARAM pOWER_WPARAM = (POWER_WPARAM)m.WParam.ToInt32();
                Logger.Info("PowerMgr: WM_POWERBROADCAST wParam = " + pOWER_WPARAM);
                switch (pOWER_WPARAM)
                {
                    case POWER_WPARAM.PBT_POWERSETTINGCHANGE:
                        {
                            POWERBROADCAST_SETTING pOWERBROADCAST_SETTING = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(m.LParam, typeof(POWERBROADCAST_SETTING));
                            IntPtr intPtr = (IntPtr)((int)m.LParam + Marshal.SizeOf(pOWERBROADCAST_SETTING));
                            if (pOWERBROADCAST_SETTING.PowerSetting == this.GUID_POWERSCHEME_PERSONALITY)
                            {
                                Logger.Info("PowerMgr: power plan changed");
                                int num = PowerMgr.SetMaxCPUFreqPowerPlan();
                                Logger.Info("PowerMgr: SetMaxCPUFreqPowerPlan = " + num);
                                num = PowerMgr.ActivateMaxCPUFreqPowerPlan();
                                Logger.Info("PowerMgr: ActivateMaxCPUFreqPowerPlan = " + num);
                            }
                            break;
                        }
                    case POWER_WPARAM.PBT_APMPOWERSTATUSCHANGE:
                        {
                            PowerState powerState = PowerState.GetPowerState();
                            Logger.Info("PowerMgr: AC Line: {0}", powerState.ACLineStatus);
                            Logger.Info("PowerMgr: Battery: {0}", powerState.BatteryFlag);
                            Logger.Info("PowerMgr: Battery life %: {0}", powerState.BatteryLifePercent);
                            break;
                        }
                    case POWER_WPARAM.PBT_APMSUSPEND:
                        {
                            Mutex mutex = default(Mutex);
                            if (Utils.IsAlreadyRunning(Strings.FrontendLockName, out mutex))
                            {
                                Logger.Info("PowerMgr: Killing HD-Frontend");
                                Utils.KillProcessByName("HD-Frontend");
                                if (!Utils.IsGlHotAttach())
                                {
                                    Utils.StopServiceNoWait("bsthdandroidsvc");
                                }
                                PowerMgr.s_FrontendStopped = true;
                            }
                            else
                            {
                                mutex.Close();
                                Logger.Info("PowerMgr: HD-Frontend not running");
                            }
                            break;
                        }
                    case POWER_WPARAM.PBT_APMRESUMECRITICAL:
                    case POWER_WPARAM.PBT_APMRESUMESUSPEND:
                    case POWER_WPARAM.PBT_APMRESUMESTANDBY:
                    case POWER_WPARAM.PBT_APMRESUMEAUTOMATIC:
                        if (PowerMgr.s_FrontendStopped && !Utils.IsGlHotAttach())
                        {
                            PowerMgr.s_FrontendStopped = false;
                            Logger.Info("PowerMgr: Starting HD-Frontend");
                            this.StartFrontend();
                        }
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private static int RunCmd(string cmd)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", "/c " + cmd);
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            Process process = Process.Start(processStartInfo);
            process.WaitForExit();
            int exitCode = process.ExitCode;
            process.Close();
            return exitCode;
        }

        public static int SetMaxCPUFreqPowerPlan()
        {
            string text = "54533251-82be-4824-96c1-47b60b740d00";
            string text2 = "bc5038f7-23e0-4960-96da-33abaf5935ec";
            Guid powerActiveScheme = PowerState.GetPowerActiveScheme();
            string text3 = "powercfg -setdcvalueindex " + powerActiveScheme + " " + text + " " + text2 + " 100";
            Logger.Info("PowerMgr: Setting MaxCPUFreqPowerPlan <cmd.exe /c " + text3 + ">");
            return PowerMgr.RunCmd(text3);
        }

        public static int ActivateMaxCPUFreqPowerPlan()
        {
            Guid powerActiveScheme = PowerState.GetPowerActiveScheme();
            string text = "powercfg -setactive " + powerActiveScheme;
            Logger.Info("PowerMgr: ActivateMaxCPUFreqPowerPlan <cmd.exe /c " + text + ">");
            return PowerMgr.RunCmd(text);
        }

        private void StartFrontend()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string path = (string)registryKey.GetValue("InstallDir");
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = Path.Combine(path, "HD-Frontend.exe");
            processStartInfo.Arguments = string.Format("\"{0}\" \"{1}\"", "Android", "hidemode");
            Logger.Debug("PowerMgr: Frontend path {0} {1}", processStartInfo.FileName, processStartInfo.Arguments);
            Process.Start(processStartInfo);
        }
    }
}
