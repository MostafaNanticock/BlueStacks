using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend.Interop;
using System;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
	public class VmxChecker
	{
		private Thread mThread;

		private Form mParent;

		private string mTitle;

		private string mText;

		public VmxChecker(Form parent, string title, string text)
		{
			this.mThread = new Thread(this.ThreadEntry);
			this.mThread.IsBackground = true;
			this.mParent = parent;
			this.mTitle = title;
			this.mText = text;
		}

		public void Start()
		{
			this.mThread.Start();
		}

		private void ThreadEntry()
		{
			try
			{
				this.ThreadEntryInternal();
			}
			catch (ObjectDisposedException)
			{
			}
		}

		private void ThreadEntryInternal()
		{
			Logger.Info("Starting VMX checker thread");
			Thread.Sleep(5000);
			while (true)
			{
				if (!this.IsParentVisible())
				{
					Thread.Sleep(1000);
					continue;
				}
				if (!Manager.IsVmxActive())
				{
					Thread.Sleep(1000);
					continue;
				}
				break;
			}
			Logger.Info("VMX is active");
			this.WarnAndQuit();
		}

		private bool IsParentVisible()
		{
			bool visible = false;
			UIHelper.RunOnUIThread(this.mParent, delegate
			{
				visible = this.mParent.Visible;
			});
			return visible;
		}

		private void WarnAndQuit()
		{
			UIHelper.RunOnUIThread(this.mParent, delegate
			{
				MessageBox.Show(this.mText, this.mTitle);
				this.mParent.Close();
			});
		}
	}
}
