using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using BlueStacks.hyperDroid.Core.VMCommand;
using BlueStacks.hyperDroid.Device;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BlueStacks.hyperDroid.Frontend.Interop
{
    public class Opengl
    {
        public delegate void GlReadyHandler();

        public delegate void GlInitFailedHandler();

        private const int GL_MODE_SOFT = 0;

        private const int GL_MODE_SYS = 1;

        private const int GL_MODE_SYS_OLD = 2;

        private static string adbPath;

        private static int glMode;

        private static bool initialized;

        private static EventWaitHandle glReadyEvent;

        [CompilerGenerated]
        private static Command.LineHandler _003C_003E9__CachedAnonymousMethodDelegate7;

        [CompilerGenerated]
        private static Command.LineHandler _003C_003E9__CachedAnonymousMethodDelegate8;

        [CompilerGenerated]
        private static Command.LineHandler _003C_003E9__CachedAnonymousMethodDelegateb;

        [CompilerGenerated]
        private static Command.LineHandler _003C_003E9__CachedAnonymousMethodDelegatec;

        [DllImport("HD-OpenGl-Native.dll")]
        private static extern void HdLoggerInit(Logger.HdLoggerCallback cb);

        [DllImport("sys_renderer.dll")]
        private static extern void sys_renderer_logger_thread(Logger.HdLoggerCallback cb, SafeWaitHandle evt);

        [DllImport("sys_renderer.dll")]
        private static extern int sys_renderer_init(IntPtr h, int x, int y, int width, int height, SafeWaitHandle evt);

        [DllImport("sys_renderer.dll")]
        private static extern IntPtr sys_renderer_get_subwindow();

        [DllImport("HD-OpenGl-Native.dll")]
        private static extern int PgaUtilsIsHotAttach();

        [DllImport("HD-OpenGl-Native.dll")]
        private static extern int PgaServerInit(IntPtr h, int x, int y, int width, int height, SafeWaitHandle evt);

        [DllImport("HD-OpenGl-Native.dll")]
        private static extern void PgaServerHandleCommand(int scancode);

        [DllImport("HD-OpenGl-Native.dll")]
        private static extern IntPtr PgaServerGetSubwindow();

        [DllImport("HD-OpenGl-Native.dll")]
        private static extern IntPtr PgaServerHandleOrientation(float hscale, float vscale, int orientation);

        [DllImport("HD-OpenGl-Native.dll")]
        private static extern IntPtr PgaServerHandleAppActivity(string package, string activity);

        [DllImport("HD-OpenGl-Native.dll")]
        private static extern int GetPgaServerInitStatus(StringBuilder glVendor, StringBuilder glRenderer, StringBuilder glVersion);

        public static bool Init(string vmName, IntPtr h, int x, int y, int width, int height, GlReadyHandler glReadyHandler, GlInitFailedHandler glInitFailedHandler)
        {
            string text = "Software\\BlueStacks";
            string name = text + "\\Guests\\" + vmName + "\\Config";
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(text);
            RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(name);
            Opengl.adbPath = (string)registryKey.GetValue("InstallDir") + "HD-Adb.exe";
            Opengl.glMode = (int)registryKey2.GetValue("GlMode");
            registryKey.Close();
            registryKey2.Close();
            Logger.Info("glMode: " + Opengl.glMode);
            if (Opengl.glMode == 0)
            {
                Console.sPgaInitDone = true;
                glReadyHandler();
                Opengl.SignalGlReady(vmName);
                return true;
            }
            if (Opengl.glMode != 1 && Opengl.glMode != 2)
            {
                throw new SystemException("Unsupported GlMode " + Opengl.glMode);
            }
            Opengl.HdLoggerInit(Logger.GetHdLoggerCallback());
            EventWaitHandle evt = new EventWaitHandle(false, EventResetMode.ManualReset);
            Thread thread = new Thread((ThreadStart)delegate
            {
                if (Opengl.PgaUtilsIsHotAttach() == 0)
                {
                    Logger.Info("Stopping Zygote");
                    Opengl.StopZygote(vmName);
                }
                Logger.Info("Initializing System Renderer");
                if (Opengl.glMode == 2)
                {
                    Opengl.sys_renderer_init(h, x, y, width, height, evt.SafeWaitHandle);
                }
                if (Opengl.glMode == 1)
                {
                    Opengl.PgaServerInit(h, 0, 0, width, height, evt.SafeWaitHandle);
                }
                evt.WaitOne();
                Opengl.initialized = true;
                if (!Opengl.GetPgaServerInitStatus())
                {
                    glInitFailedHandler();
                }
                else
                {
                    if (Opengl.PgaUtilsIsHotAttach() == 0)
                    {
                        Logger.Info("Starting Zygote");
                        Opengl.StartZygote(vmName);
                    }
                    glReadyHandler();
                    Opengl.SignalGlReady(vmName);
                }
            });
            thread.IsBackground = true;
            thread.Start();
            return true;
        }

        private static bool GetPgaServerInitStatus()
        {
            StringBuilder stringBuilder = new StringBuilder(512);
            StringBuilder stringBuilder2 = new StringBuilder(512);
            StringBuilder stringBuilder3 = new StringBuilder(512);
            Logger.Info("Calling GetPgaServerInitStatus");
            int pgaServerInitStatus = Opengl.GetPgaServerInitStatus(stringBuilder, stringBuilder2, stringBuilder3);
            Console.sPgaInitDone = true;
            if (pgaServerInitStatus != 0)
            {
                Logger.Info("PgaServerInit failed");
                return false;
            }
            Profile.GlVendor = stringBuilder.ToString();
            Profile.GlRenderer = stringBuilder2.ToString();
            Profile.GlVersion = stringBuilder3.ToString();
            Logger.Info("GlVendor: " + Profile.GlVendor);
            Logger.Info("GlRenderer: " + Profile.GlRenderer);
            Logger.Info("GlVersion: " + Profile.GlVersion);
            return true;
        }

        private static IntPtr GetSubWindow()
        {
            if (!Opengl.initialized)
            {
                return IntPtr.Zero;
            }
            if (Opengl.glMode == 2)
            {
                return Opengl.sys_renderer_get_subwindow();
            }
            if (Opengl.glMode == 1)
            {
                return Opengl.PgaServerGetSubwindow();
            }
            return IntPtr.Zero;
        }

        public static bool ShowSubWindow()
        {
            IntPtr subWindow = Opengl.GetSubWindow();
            if (subWindow == IntPtr.Zero)
            {
                return false;
            }
            Window.ShowWindow(subWindow, 8);
            return true;
        }

        public static bool HideSubWindow()
        {
            IntPtr subWindow = Opengl.GetSubWindow();
            if (subWindow == IntPtr.Zero)
            {
                return false;
            }
            Window.ShowWindow(subWindow, 0);
            return true;
        }

        public static bool IsSubWindowVisible()
        {
            IntPtr subWindow = Opengl.GetSubWindow();
            if (subWindow == IntPtr.Zero)
            {
                return false;
            }
            return Window.IsWindowVisible(subWindow);
        }

        public static bool ResizeSubWindow(int x, int y, int cx, int cy)
        {
            Window.SetWindowPos(Opengl.GetSubWindow(), IntPtr.Zero, x, y, cx, cy, 4u);
            return true;
        }

        public static bool DrawFB(int cx, int cy, IntPtr buffer, bool ConsoleAccess)
        {
            if (Opengl.glMode == 0)
            {
                return false;
            }
            if (Opengl.glMode != 1 && Opengl.glMode != 2)
            {
                return true;
            }
            if (ConsoleAccess && !Opengl.IsSubWindowVisible())
            {
                return false;
            }
            return true;
        }

        public static void HandleOrientation(float hscale, float vscale, int orientation)
        {
            if (Opengl.glMode == 1)
            {
                Opengl.PgaServerHandleOrientation(hscale, vscale, orientation);
            }
        }

        public static void HandleCommand(int scancode)
        {
            if (Opengl.glMode == 1)
            {
                Opengl.PgaServerHandleCommand(scancode);
            }
        }

        public static void HandleAppActivity(string package, string activity)
        {
            Opengl.PgaServerHandleAppActivity(package, activity);
        }

        public static void StopZygote(string vmName)
        {
            while (true)
            {
                try
                {
                    Command command = new Command();
                    command.Attach(vmName);
                    command.SetOutputHandler(delegate(string line)
                    {
                        Logger.Info("OUT: " + line);
                    });
                    command.SetErrorHandler(delegate(string line)
                    {
                        Logger.Info("ERR: " + line);
                    });
                    int num = command.Run(new string[1]
					{
						"/system/bin/stop"
					});
                    if (num != 0)
                    {
                        throw new ApplicationException("VM command failed: " + num);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug("Cannot stop Zygote: " + ex.ToString());
                    Thread.Sleep(500);
                    Logger.Debug("Retrying...");
                    continue;
                }
                break;
            }
            Thread.Sleep(100);
        }

        private static void StartZygote(string vmName)
        {
            while (true)
            {
                try
                {
                    Command command = new Command();
                    command.Attach(vmName);
                    command.SetOutputHandler(delegate(string line)
                    {
                        Logger.Info("OUT: " + line);
                    });
                    command.SetErrorHandler(delegate(string line)
                    {
                        Logger.Info("ERR: " + line);
                    });
                    int num = command.Run(new string[1]
					{
						"/system/bin/start"
					});
                    if (num == 0)
                    {
                        return;
                    }
                    throw new ApplicationException("Cannot start Zygote: " + num);
                }
                catch (Exception ex)
                {
                    Logger.Debug("Cannot start Zygote: " + ex.ToString());
                    Thread.Sleep(500);
                    Logger.Debug("Retrying...");
                }
            }
        }

        private static void SignalGlReady(string vmName)
        {
            string name = "Global\\BlueStacks_Frontend_Gl_Ready_" + vmName;
            Opengl.glReadyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, name);
            Opengl.glReadyEvent.Set();
        }
    }
}
