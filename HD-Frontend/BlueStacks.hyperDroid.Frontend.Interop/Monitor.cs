using BlueStacks.hyperDroid.Common;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlueStacks.hyperDroid.Frontend.Interop
{
    public class Monitor
    {
        public delegate void ExitHandler();

        private delegate void ReadCallback();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TouchPoint
        {
            public ushort PosX;

            public ushort PosY;

            public TouchPoint(ushort x, ushort y)
            {
                this.PosX = x;
                this.PosY = y;
            }
        }

        private IntPtr handle;

        private uint id;

        [DllImport("HD-Frontend-Native.dll", SetLastError = true)]
        private static extern IntPtr MonitorVideoAttach(IntPtr handle);

        [DllImport("HD-Frontend-Native.dll", SetLastError = true)]
        private static extern bool MonitorSendScanCode(IntPtr handle, byte code);

        [DllImport("HD-Frontend-Native.dll", SetLastError = true)]
        private static extern bool MonitorSendMouseState(IntPtr handle, uint x, uint y, uint mask);

        [DllImport("HD-Frontend-Native.dll", SetLastError = true)]
        private static extern bool MonitorSendTouchState(IntPtr handle, TouchPoint[] points, int count);

        [DllImport("HD-Frontend-Native.dll", SetLastError = true)]
        private static extern bool MonitorSendCaptureStream(IntPtr handle, IntPtr streamBuf, int size);

        public Monitor(IntPtr handle, uint id, ExitHandler exitHandler)
        {
            this.handle = handle;
            this.id = id;
            ThreadStart start = delegate
            {
                while (Utils.IsProcessAlive(Convert.ToInt32(id)))
                {
                    Thread.Sleep(1000);
                }
                exitHandler();
            };
            Thread thread = new Thread(start);
            thread.IsBackground = true;
            thread.Start();
        }

        public void Close()
        {
        }

        public Video VideoAttach()
        {
            int num = 5;
            IntPtr intPtr = IntPtr.Zero;
            while (num > 0)
            {
                intPtr = Monitor.MonitorVideoAttach(this.handle);
                if (!(intPtr == IntPtr.Zero))
                {
                    break;
                }
                Logger.Error("FATAL ERROR: Cannot attach to monitor video. err: " + Marshal.GetLastWin32Error());
                Utils.KillAnotherFrontendInstance();
                Thread.Sleep(1000);
                num--;
            }
            if (num == 0)
            {
                Common.ThrowLastWin32Error("FATAL ERROR: Cannot attach to monitor video. err: " + Marshal.GetLastWin32Error());
            }
            Logger.Info("Video memory at 0x{0:X8}", intPtr.ToString("x"));
            Video video = new Video(intPtr);
            video.CheckMagic();
            return video;
        }

        public void SendScanCode(byte code)
        {
            if (!Monitor.MonitorSendScanCode(this.handle, code))
            {
                Common.ThrowLastWin32Error("Cannot send keyboard scan code");
            }
        }

        public void SendMouseState(uint x, uint y, uint mask)
        {
            if (!Monitor.MonitorSendMouseState(this.handle, x, y, mask))
            {
                Common.ThrowLastWin32Error("Cannot send mouse state");
            }
        }

        public void SendTouchState(TouchPoint[] points)
        {
            if (points == null)
            {
                points = new TouchPoint[0];
            }
            if (!Monitor.MonitorSendTouchState(this.handle, points, points.Length))
            {
                Common.ThrowLastWin32Error("Cannot send touch state");
            }
        }

        public void SendAudioCaptureStream(byte[] streamBuf, int size)
        {
            IntPtr intPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(streamBuf, 0, intPtr, size);
                Monitor.MonitorSendCaptureStream(this.handle, intPtr, size);
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
        }
    }
}
