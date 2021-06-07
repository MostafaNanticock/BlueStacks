using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Agent
{
    public class ClipboardMgr : Form
    {
        private const int WM_DRAWCLIPBOARD = 776;

        private const int WM_CHANGECBCHAIN = 781;

        private IntPtr m_NextClipboardViewer;

        private bool guestFinishedBooting;

        private string CachedText = "";

        [DllImport("User32.dll")]
        private static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        public ClipboardMgr()
        {
            base.WindowState = FormWindowState.Minimized;
            base.Load += this.OnLoad;
            this.RegisterForClipBoardNotifications();
        }

        protected override void OnActivated(EventArgs args)
        {
            base.Hide();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            base.Hide();
        }

        protected override void Dispose(bool disposing)
        {
            ClipboardMgr.ChangeClipboardChain(base.Handle, this.m_NextClipboardViewer);
        }

        private void RegisterForClipBoardNotifications()
        {
            this.m_NextClipboardViewer = (IntPtr)ClipboardMgr.SetClipboardViewer((int)base.Handle);
        }

        public bool CheckIfGuestFinishedBooting()
        {
            try
            {
                Logger.Info("Check if android is booted ");
                string url = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + VmCmdHandler.s_PingPath;
                Client.Get(url, null, false, 1000);
                Logger.Info("Guest finished booting");
                this.guestFinishedBooting = true;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Guest not booted yet");
                Logger.Error(ex.Message);
                return false;
            }
        }

        private void ProcessClipboardData()
        {
            if (Clipboard.ContainsText())
            {
                if (!this.guestFinishedBooting && !this.CheckIfGuestFinishedBooting())
                {
                    return;
                }
                string text = Clipboard.GetText();
                Logger.Info("ClipboardMgr: Got clipboardText");
                if (string.Compare(this.CachedText, text) != 0)
                {
                    try
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                        dictionary.Add("text", text);
                        string text2 = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + HDAgent.s_ClipboardDataPath;
                        Logger.Info("ClipboardMgr: Sending post request to {0}", text2);
                        string text3 = Client.Post(text2, dictionary, null, false);
                        Logger.Info("ClipboardMgr: Got response: {0}", text3);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Exception in Sending ClipboardCommand {0}", ex.ToString());
                    }
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 776:
                    this.ProcessClipboardData();
                    ClipboardMgr.SendMessage(this.m_NextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;
                case 781:
                    if (m.WParam == this.m_NextClipboardViewer)
                    {
                        this.m_NextClipboardViewer = m.LParam;
                    }
                    else
                    {
                        ClipboardMgr.SendMessage(this.m_NextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        public void SetCachedText(string text)
        {
            this.CachedText = text;
        }

        public string GetCachedText()
        {
            return this.CachedText;
        }
    }
}
