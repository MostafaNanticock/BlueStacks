using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
	public class Keyboard
	{
		private Dictionary<Keys, bool> escapeSet;

		[DllImport("user32.dll")]
		private static extern uint MapVirtualKey(uint code, uint mapType);

		public Keyboard()
		{
			this.escapeSet = new Dictionary<Keys, bool>();
			this.escapeSet.Add(Keys.LWin, true);
			this.escapeSet.Add(Keys.RWin, true);
			this.escapeSet.Add(Keys.Apps, true);
			this.escapeSet.Add(Keys.Home, true);
			this.escapeSet.Add(Keys.End, true);
			this.escapeSet.Add(Keys.Prior, true);
			this.escapeSet.Add(Keys.Next, true);
			this.escapeSet.Add(Keys.Left, true);
			this.escapeSet.Add(Keys.Right, true);
			this.escapeSet.Add(Keys.Up, true);
			this.escapeSet.Add(Keys.Down, true);
		}

		public uint NativeToScanCodes(Keys key)
		{
			uint code = (uint)(key & Keys.KeyCode);
			uint num = Keyboard.MapVirtualKey(code, 0u);
			if (this.NeedEscape(key))
			{
				return 0xE000 | num;
			}
			return num;
		}

		private bool NeedEscape(Keys key)
		{
			return this.escapeSet.ContainsKey(key);
		}

		public bool IsAltDepressed()
		{
			if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
			{
				return true;
			}
			return false;
		}
	}
}
