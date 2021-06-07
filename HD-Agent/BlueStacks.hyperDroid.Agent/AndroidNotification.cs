using System;

namespace BlueStacks.hyperDroid.Agent
{
	public class AndroidNotification
	{
		private string mPackageName;

		private string mMessage;

		private bool mNotificationSent;

		private DateTime mNotificationTime;

		public bool NotificationSent
		{
			get
			{
				return this.mNotificationSent;
			}
			set
			{
				this.mNotificationSent = value;
			}
		}

		public bool OldNotificationFlag
		{
			get
			{
				TimeSpan t = DateTime.Now.Subtract(this.mNotificationTime);
				TimeSpan t2 = new TimeSpan(0, 0, 5);
				if (TimeSpan.Compare(t, t2) > -1)
				{
					return true;
				}
				return false;
			}
		}

		public string Package
		{
			get
			{
				return this.mPackageName;
			}
		}

		public string Message
		{
			get
			{
				return this.mMessage;
			}
		}

		public AndroidNotification(string pkg, string msg)
		{
			this.mPackageName = pkg;
			this.mMessage = msg;
			this.mNotificationSent = false;
			this.mNotificationTime = DateTime.Now;
		}
	}
}
