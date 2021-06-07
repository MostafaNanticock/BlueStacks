using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.Samples.TabletPC.MTScratchpad.WMTouch
{
	public class WMTouchForm : Form
	{
		public delegate void LoggerCallback(string msg);

		protected class TouchPoint
		{
			private int x;

			private int y;

			private int id;

			private int slot;

			public int X
			{
				get
				{
					return this.x;
				}
				set
				{
					this.x = value;
				}
			}

			public int Y
			{
				get
				{
					return this.y;
				}
				set
				{
					this.y = value;
				}
			}

			public int Id
			{
				get
				{
					return this.id;
				}
				set
				{
					this.id = value;
				}
			}

			public int Slot
			{
				get
				{
					return this.slot;
				}
			}

			public TouchPoint(int slot)
			{
				this.Clear();
				this.slot = slot;
			}

			public void Clear()
			{
				this.x = -1;
				this.y = -1;
				this.id = -1;
			}
		}

		protected class WMTouchEventArgs : EventArgs
		{
			private WMTouchForm form;

			public int GetPointCount()
			{
				return this.form.touchPointArray.Length;
			}

			public TouchPoint GetPoint(int ndx)
			{
				return this.form.touchPointArray[ndx];
			}

			public WMTouchEventArgs(WMTouchForm form)
			{
				this.form = form;
			}
		}

		private struct TOUCHINPUT
		{
			public int x;

			public int y;

			public IntPtr hSource;

			public int dwID;

			public int dwFlags;

			public int dwMask;

			public int dwTime;

			public IntPtr dwExtraInfo;

			public int cxContact;

			public int cyContact;
		}

		private struct POINTS
		{
			public short x;

			public short y;
		}

		private const int WM_TOUCHMOVE = 576;

		private const int WM_TOUCHDOWN = 577;

		private const int WM_TOUCHUP = 578;

		private const int TOUCHEVENTF_MOVE = 1;

		private const int TOUCHEVENTF_DOWN = 2;

		private const int TOUCHEVENTF_UP = 4;

		private const int TOUCHEVENTF_INRANGE = 8;

		private const int TOUCHEVENTF_PRIMARY = 16;

		private const int TOUCHEVENTF_NOCOALESCE = 32;

		private const int TOUCHEVENTF_PEN = 64;

		private const int TOUCHINPUTMASKF_TIMEFROMSYSTEM = 1;

		private const int TOUCHINPUTMASKF_EXTRAINFO = 2;

		private const int TOUCHINPUTMASKF_CONTACTAREA = 4;

		private const int TWF_FINETOUCH = 1;

		private const int TWF_WANTPALM = 2;

		private LoggerCallback loggerCallback;

		private TOUCHINPUT[] touchInputArray;

		private TouchPoint[] touchPointArray;

		private WMTouchEventArgs touchEventArgs;

		private int touchInputSize;

		protected event EventHandler<WMTouchEventArgs> TouchEvent;

		public WMTouchForm(int maxInputs, LoggerCallback loggerCallback)
		{
			this.loggerCallback = loggerCallback;
			try
			{
				base.Load += this.OnLoadHandler;
			}
			catch (Exception ex)
			{
				this.Log("ERROR: Could not add form load handler");
				this.Log(ex.ToString());
			}
			this.touchInputArray = new TOUCHINPUT[maxInputs];
			for (int i = 0; i < maxInputs; i++)
			{
				this.touchInputArray[i] = default(TOUCHINPUT);
			}
			this.touchPointArray = new TouchPoint[maxInputs];
			for (int j = 0; j < maxInputs; j++)
			{
				this.touchPointArray[j] = new TouchPoint(j);
			}
			this.touchEventArgs = new WMTouchEventArgs(this);
			this.touchInputSize = Marshal.SizeOf(default(TOUCHINPUT));
		}

		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool RegisterTouchWindow(IntPtr hWnd, ulong ulFlags);

		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetTouchInputInfo(IntPtr hTouchInput, int cInputs, [In] [Out] TOUCHINPUT[] pInputs, int cbSize);

		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern void CloseTouchInputHandle(IntPtr lParam);

		private void OnLoadHandler(object sender, EventArgs e)
		{
			ulong ulFlags = 2uL;
			try
			{
				if (!WMTouchForm.RegisterTouchWindow(base.Handle, ulFlags))
				{
					this.Log("ERROR: Could not register window for touch");
				}
			}
			catch (Exception ex)
			{
				this.Log("ERROR: RegisterTouchWindow API not available");
				this.Log(ex.ToString());
			}
		}

		private void Log(string fmt, params object[] args)
		{
			this.loggerCallback(string.Format(fmt, args));
		}

		protected override void WndProc(ref Message m)
		{
			bool flag;
			switch (m.Msg)
			{
			case 576:
			case 577:
			case 578:
				flag = this.DecodeTouch(ref m);
				break;
			default:
				flag = false;
				break;
			}
			base.WndProc(ref m);
			if (flag)
			{
				try
				{
					m.Result = new IntPtr(1);
				}
				catch (Exception ex)
				{
					this.Log("ERROR: Could not allocate result ptr");
					this.Log(ex.ToString());
				}
			}
		}

		private static int LoWord(int number)
		{
			return number & 0xFFFF;
		}

		private bool DecodeTouch(ref Message m)
		{
			if (this.TouchEvent == null)
			{
				return false;
			}
			int num = WMTouchForm.LoWord(m.WParam.ToInt32());
			if (num > this.touchInputArray.Length)
			{
				num = this.touchInputArray.Length;
			}
			if (!WMTouchForm.GetTouchInputInfo(m.LParam, num, this.touchInputArray, this.touchInputSize))
			{
				return false;
			}
			for (int i = 0; i < this.touchPointArray.Length; i++)
			{
				this.touchPointArray[i].Clear();
			}
			for (int j = 0; j < num; j++)
			{
				TOUCHINPUT tOUCHINPUT = this.touchInputArray[j];
				TouchPoint touchPoint = this.touchPointArray[j];
				if ((tOUCHINPUT.dwFlags & 2) != 0 || (tOUCHINPUT.dwFlags & 1) != 0)
				{
					Point point = base.PointToClient(new Point(tOUCHINPUT.x / 100, tOUCHINPUT.y / 100));
					touchPoint.Id = tOUCHINPUT.dwID;
					touchPoint.X = point.X;
					touchPoint.Y = point.Y;
				}
			}
			this.TouchEvent(this, this.touchEventArgs);
			WMTouchForm.CloseTouchInputHandle(m.LParam);
			return true;
		}
	}
}
