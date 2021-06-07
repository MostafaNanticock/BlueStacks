using BlueStacks.hyperDroid.Common;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
	public class StatusMessage : Label
	{
		public StatusMessage()
		{
			this.Font = new Font(Utils.GetSystemFontName(), 14f, FontStyle.Bold);
		}

		protected override void OnPaint(PaintEventArgs evt)
		{
			evt.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			base.OnPaint(evt);
		}
	}
}
