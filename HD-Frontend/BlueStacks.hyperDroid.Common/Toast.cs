using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common
{
	internal class Toast : Form
	{
		private const int WS_EX_TOOLWINDOW = 128;

		private const int WS_EX_NOACTIVATE = 134217728;

		private const int WS_CHILD = 1073741824;

		private Font font = new Font(Utils.GetSystemFontName(), 12f);

		private SizeF stringSize;

		private string toastText;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.Style = 1073741824;
				createParams.ExStyle |= 134217856;
				return createParams;
			}
		}

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);

		public Toast(Form parent, string toastText)
		{
			this.toastText = toastText;
			Graphics graphics = base.CreateGraphics();
			this.stringSize = graphics.MeasureString(this.toastText, this.font);
			base.StartPosition = FormStartPosition.Manual;
			base.FormBorderStyle = FormBorderStyle.None;
			base.ShowInTaskbar = false;
			base.Paint += this.ShowToast;
			base.Width = (int)this.stringSize.Width + 20;
			base.Height = (int)this.stringSize.Height + 20;
			int x = parent.Left + (parent.Width - base.Width) / 2;
			int y = parent.Top + 5;
			base.Location = new Point(x, y);
			base.Owner = parent;
			IntPtr intPtr = Toast.CreateRoundRectRgn(0, 0, base.Width, base.Height, 5, 5);
			base.Region = Region.FromHrgn(intPtr);
			Toast.DeleteObject(intPtr);
		}

		private void ShowToast(object sender, PaintEventArgs e)
		{
			RectangleF rect = new RectangleF(0f, 0f, (float)base.Width, (float)base.Height);
			Pen pen = new Pen(Color.Black);
			e.Graphics.DrawRectangle(pen, 0, 0, base.Width, base.Height);
			SolidBrush brush = new SolidBrush(Color.White);
			e.Graphics.FillRectangle(brush, rect);
			float x = ((float)base.Width - this.stringSize.Width) / 2f + 5f;
			float y = ((float)base.Height - this.stringSize.Height) / 2f;
			RectangleF layoutRectangle = new RectangleF(x, y, this.stringSize.Width, this.stringSize.Height);
			SolidBrush brush2 = new SolidBrush(Color.Black);
			e.Graphics.DrawString(this.toastText, this.font, brush2, layoutRectangle);
			base.Owner.Focus();
		}
	}
}
