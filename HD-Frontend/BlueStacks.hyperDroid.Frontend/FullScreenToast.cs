using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend.Interop;
using BlueStacks.hyperDroid.Locale;
using System;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
	public class FullScreenToast
	{
		private Form mParent;

		private Toast mToast;

		private Timer mTimer;

		public FullScreenToast(Form parent)
		{
			this.mParent = parent;
			this.mTimer = new Timer();
			this.mTimer.Interval = 5000;
			this.mTimer.Tick += this.Timeout;
		}

		public void Show()
		{
			this.Hide();
			this.mToast = new Toast(this.mParent, BlueStacks.hyperDroid.Locale.Strings.FullScreenToastText);
			int dwFlags = 262148;
			Animate.AnimateWindow(this.mToast.Handle, 500, dwFlags);
			this.mToast.Show();
			this.mTimer.Start();
		}

		public void Hide()
		{
			this.mTimer.Stop();
			if (this.mToast != null)
			{
				this.mToast.Hide();
				this.mToast = null;
			}
		}

		private void Timeout(object obj, EventArgs evt)
		{
			this.mTimer.Stop();
			int dwFlags = 327688;
			if (this.mToast != null)
			{
				Animate.AnimateWindow(this.mToast.Handle, 500, dwFlags);
				this.mToast.Hide();
			}
		}
	}
}
