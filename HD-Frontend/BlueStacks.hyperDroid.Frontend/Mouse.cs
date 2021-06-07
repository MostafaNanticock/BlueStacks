using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
	public class Mouse
	{
		private uint x;

		private uint y;

		private bool b0;

		private bool b1;

		private bool b2;

		public uint X
		{
			get
			{
				return this.x;
			}
		}

		public uint Y
		{
			get
			{
				return this.y;
			}
		}

		public uint Mask
		{
			get
			{
				uint num = 0u;
				if (this.b0)
				{
					num |= 1;
				}
				if (this.b1)
				{
					num |= 2;
				}
				if (this.b2)
				{
					num |= 4;
				}
				return num;
			}
		}

		public Mouse()
		{
			this.x = 0u;
			this.y = 0u;
			this.b0 = false;
			this.b1 = false;
			this.b2 = false;
		}

		public void UpdateCursor(uint x, uint y)
		{
			this.x = x;
			this.y = y;
		}

		public void UpdateButton(uint x, uint y, MouseButtons button, bool pressed)
		{
			this.x = x;
			this.y = y;
			switch (button)
			{
			case MouseButtons.Left:
				this.b0 = pressed;
				break;
			case MouseButtons.Right:
				this.b1 = pressed;
				break;
			case MouseButtons.Middle:
				this.b2 = pressed;
				break;
			}
		}
	}
}
