using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using BlueStacks.hyperDroid.Frontend.Interop;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
    public class Cursor
    {
        private class State
        {
            public int SlotId;

            public Bitmap PrimaryImage;

            public Bitmap SecondaryImage;

            public Pointer Pointer;

            public Point Position;

            public bool Clicked;

            public State(int slotId, Bitmap primaryImage, Bitmap secondaryImage)
            {
                this.SlotId = slotId;
                this.PrimaryImage = primaryImage;
                this.SecondaryImage = secondaryImage;
            }
        }

        private class Pointer : Form
        {
            private struct Win32Point
            {
                public int X;

                public int Y;

                public Win32Point(int x, int y)
                {
                    this.X = x;
                    this.Y = y;
                }
            }

            private struct Win32Size
            {
                public int Width;

                public int Height;

                public Win32Size(int width, int height)
                {
                    this.Width = width;
                    this.Height = height;
                }
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private struct BLENDFUNCTION
            {
                public byte BlendOp;

                public byte BlendFlags;

                public byte SourceConstantAlpha;

                public byte AlphaFormat;
            }

            private const int WS_EX_TRANSPARENT = 32;

            private const int WS_EX_TOOLWINDOW = 128;

            private const int WS_EX_LAYERED = 524288;

            private const byte AC_SRC_OVER = 0;

            private const byte AC_SRC_ALPHA = 1;

            private const int ULW_ALPHA = 2;

            private Bitmap mBitmap;

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams createParams = base.CreateParams;
                    createParams.ExStyle |= 32;
                    createParams.ExStyle |= 128;
                    createParams.ExStyle |= 524288;
                    return createParams;
                }
            }

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Win32Point pptDst, ref Win32Size psize, IntPtr hdcSrc, ref Win32Point pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr GetDC(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern bool DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern bool DeleteObject(IntPtr hObject);

            public Pointer()
            {
                base.SuspendLayout();
                base.ShowInTaskbar = false;
                base.FormBorderStyle = FormBorderStyle.None;
                base.TopMost = true;
                base.ResumeLayout();
            }

            public void SetBitmap(Bitmap bitmap)
            {
                if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    throw new ApplicationException("Bad bitmap");
                }
                this.mBitmap = bitmap;
            }

            public Bitmap GetBitmap()
            {
                return this.mBitmap;
            }

            public void Update(int x, int y)
            {
                IntPtr dC = Pointer.GetDC(IntPtr.Zero);
                IntPtr intPtr = Pointer.CreateCompatibleDC(dC);
                IntPtr intPtr2 = IntPtr.Zero;
                IntPtr hObject = IntPtr.Zero;
                try
                {
                    intPtr2 = this.mBitmap.GetHbitmap(Color.FromArgb(0));
                    hObject = Pointer.SelectObject(intPtr, intPtr2);
                    Win32Size win32Size = new Win32Size(this.mBitmap.Width, this.mBitmap.Height);
                    Win32Point win32Point = new Win32Point(0, 0);
                    Win32Point win32Point2 = new Win32Point(x, y);
                    BLENDFUNCTION bLENDFUNCTION = default(BLENDFUNCTION);
                    bLENDFUNCTION.BlendOp = 0;
                    bLENDFUNCTION.BlendFlags = 0;
                    bLENDFUNCTION.SourceConstantAlpha = 255;
                    bLENDFUNCTION.AlphaFormat = 1;
                    if (!Pointer.UpdateLayeredWindow(base.Handle, dC, ref win32Point2, ref win32Size, intPtr, ref win32Point, 0, ref bLENDFUNCTION, 2))
                    {
                        BlueStacks.hyperDroid.Frontend.Interop.Common.ThrowLastWin32Error("Cannot update layered window");
                    }
                }
                finally
                {
                    Pointer.ReleaseDC(IntPtr.Zero, dC);
                    if (intPtr2 != IntPtr.Zero)
                    {
                        Pointer.SelectObject(intPtr, hObject);
                        Pointer.DeleteObject(intPtr2);
                    }
                    Pointer.DeleteDC(intPtr);
                }
            }
        }

        private const int COUNT_MAX = 4;

        private const int INITIAL_X = 128;

        private const int INITIAL_Y = 128;

        private Console mConsole;

        private InputMapper mInputMapper;

        private State[] mCursors;

        public Cursor(Console console, string installDir)
        {
            this.mConsole = console;
            this.mCursors = new State[4];
            for (int i = 0; i < 4; i++)
            {
                string filename = installDir + "\\Cursor_" + i + "_Primary.png";
                Bitmap primaryImage = new Bitmap(filename);
                string filename2 = installDir + "\\Cursor_" + i + "_Secondary.png";
                Bitmap secondaryImage = new Bitmap(filename2);
                this.mCursors[i] = new State(i, primaryImage, secondaryImage);
            }
        }

        public void SetInputMapper(InputMapper inputMapper)
        {
            this.mInputMapper = inputMapper;
        }

        public void Attach(int identity)
        {
            UIHelper.RunOnUIThread(this.mConsole, delegate
            {
                try
                {
                    this.InternalAttach(identity);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
        }

        public void Detach(int identity)
        {
            UIHelper.RunOnUIThread(this.mConsole, delegate
            {
                try
                {
                    this.InternalDetach(identity);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
        }

        public void Move(int identity, float x, float y, bool absolute)
        {
            UIHelper.RunOnUIThread(this.mConsole, delegate
            {
                try
                {
                    this.InternalMove(identity, x, y, absolute);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
        }

        public void Click(int identity, bool down)
        {
            UIHelper.RunOnUIThread(this.mConsole, delegate
            {
                try
                {
                    this.InternalClick(identity, down);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
        }

        public void RaiseFocusChange()
        {
            bool flag = this.IsForegroundApplication();
            for (int i = 0; i < 4; i++)
            {
                State state = this.mCursors[i];
                if (state.Pointer != null)
                {
                    if (flag)
                    {
                        state.Pointer.Show();
                        this.mConsole.Activate();
                    }
                    else
                    {
                        state.Pointer.Hide();
                    }
                }
            }
        }

        public void GetNormalizedPosition(int identity, out float x, out float y)
        {
            State state = this.LookupCursor(identity);
            if (state == null)
            {
                x = 0f;
                y = 0f;
            }
            else
            {
                Rectangle scaledGuestDisplayArea = this.mConsole.GetScaledGuestDisplayArea();
                x = (float)state.Position.X / (float)scaledGuestDisplayArea.Width;
                y = (float)state.Position.Y / (float)scaledGuestDisplayArea.Height;
            }
        }

        private void InternalAttach(int identity)
        {
            Logger.Info("Cursor.Attach({0})", identity);
            State state = this.LookupCursor(identity);
            if (state == null)
            {
                Logger.Warning("Cannot find cursor slot for identity {0}", identity);
            }
            else if (state.Pointer != null)
            {
                Logger.Warning("Cursor slot ID %d already has a pointer", state.SlotId);
            }
            else
            {
                Logger.Info("Cursor using slot {0}", state.SlotId);
                state.Position.X = 128;
                state.Position.Y = 128;
                state.Clicked = false;
                state.Pointer = new Pointer();
                state.Pointer.SetBitmap(state.PrimaryImage);
                this.Move(identity, 0f, 0f, false);
                this.mConsole.Activate();
            }
        }

        private void InternalDetach(int identity)
        {
            Logger.Info("Cursor.Detach({0})", identity);
            State state = this.LookupCursor(identity);
            if (state == null)
            {
                Logger.Warning("Cannot find cursor slot for identity {0}", identity);
            }
            else
            {
                state.Pointer.Close();
                state.Pointer = null;
                state.Position.X = 0;
                state.Position.Y = 0;
                state.Clicked = false;
            }
        }

        private void InternalMove(int identity, float x, float y, bool absolute)
        {
            State state = this.LookupCursor(identity);
            if (state == null)
            {
                Logger.Warning("Cannot find cursor slot for identity {0}", identity);
            }
            else
            {
                Rectangle scaledGuestDisplayArea = this.mConsole.GetScaledGuestDisplayArea();
                state.Position.X += (int)x;
                if (state.Position.X < 0)
                {
                    state.Position.X = 0;
                }
                else if (state.Position.X > scaledGuestDisplayArea.Width)
                {
                    state.Position.X = scaledGuestDisplayArea.Width;
                }
                state.Position.Y += (int)y;
                if (state.Position.Y < 0)
                {
                    state.Position.Y = 0;
                }
                else if (state.Position.Y > scaledGuestDisplayArea.Height)
                {
                    state.Position.Y = scaledGuestDisplayArea.Height;
                }
                if (this.IsForegroundApplication())
                {
                    Rectangle rectangle = this.mConsole.RectangleToScreen(scaledGuestDisplayArea);
                    int x2 = state.Position.X + rectangle.Left - state.Pointer.GetBitmap().Width / 2;
                    int y2 = state.Position.Y + rectangle.Top - state.Pointer.GetBitmap().Height / 2;
                    state.Pointer.Update(x2, y2);
                    state.Pointer.Show();
                }
                else
                {
                    state.Pointer.Hide();
                }
                InputMapper.TouchPoint[] array = new InputMapper.TouchPoint[1]
				{
					default(InputMapper.TouchPoint)
				};
                array[0].X = (float)state.Position.X / (float)scaledGuestDisplayArea.Width;
                array[0].Y = (float)state.Position.Y / (float)scaledGuestDisplayArea.Height;
                array[0].Down = state.Clicked;
                this.mInputMapper.TouchHandlerImpl(array, state.SlotId * 4, false);
            }
        }

        private void InternalClick(int identity, bool down)
        {
            State state = this.LookupCursor(identity);
            if (state == null)
            {
                Logger.Warning("Cannot find cursor slot for identity {0}", identity);
            }
            else
            {
                state.Clicked = down;
                if (!down)
                {
                    state.Pointer.SetBitmap(state.PrimaryImage);
                }
                else
                {
                    state.Pointer.SetBitmap(state.SecondaryImage);
                }
                this.Move(identity, 0f, 0f, false);
            }
        }

        private State LookupCursor(int identity)
        {
            int num = -1;
            if (identity >= 0 && identity < 4)
            {
                num = 3 - identity;
            }
            else if (identity >= 16)
            {
                num = identity - 16;
            }
            if (num >= 0 && num < 4)
            {
                return this.mCursors[num];
            }
            return null;
        }

        private bool IsForegroundApplication()
        {
            bool result = false;
            IntPtr foregroundWindow = Window.GetForegroundWindow();
            if (foregroundWindow != IntPtr.Zero)
            {
                uint num = 0u;
                Window.GetWindowThreadProcessId(foregroundWindow, ref num);
                if (num == Process.GetCurrentProcess().Id)
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
