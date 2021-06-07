using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common
{
	public static class UIHelper
	{
		public delegate void Action();

		public static void RunOnUIThread(Control control, Action action)
		{
			if (control.InvokeRequired)
			{
				control.Invoke(action);
			}
			else
			{
				action();
			}
		}
	}
}
