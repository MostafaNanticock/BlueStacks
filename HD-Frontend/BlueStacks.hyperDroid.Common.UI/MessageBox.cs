using System.Drawing;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common.UI
{
	internal class MessageBox : Form
	{
		public static DialogResult ShowMessageBox(string title, string message, string leftBtnLbl, string rightBtnLbl, Image pic)
		{
			using (MessageBox messageBox = new MessageBox(title, message, leftBtnLbl, rightBtnLbl, pic))
			{
				return messageBox.ShowDialog();
			}
		}

		private MessageBox(string title, string message, string leftBtnLbl, string rightBtnLbl, Image pic)
		{
			base.Size = new Size(360, 160);
			base.KeyPreview = true;
			base.ShowIcon = false;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.ShowInTaskbar = false;
			base.SizeGripStyle = SizeGripStyle.Hide;
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = title;
			Button button = new Button();
			button.Text = rightBtnLbl;
			button.DialogResult = DialogResult.Cancel;
			button.Width = this.GetTextWidth(rightBtnLbl) + 20;
			button.Location = new Point(base.ClientSize.Width - button.Width - 10, base.ClientSize.Height - button.Height - 10);
			Button button2 = new Button();
			button2.Text = leftBtnLbl;
			button2.DialogResult = DialogResult.OK;
			button2.Width = this.GetTextWidth(leftBtnLbl) + 20;
			button2.Location = new Point(button.Left - button2.Width - 10, base.ClientSize.Height - button2.Height - 10);
			Label value = new Label
			{
				Text = message,
				Width = base.ClientSize.Width - 60,
				Height = button.Top - 30,
				Location = new Point(30, 30)
			};
			base.Controls.Add(button);
			base.Controls.Add(button2);
			base.Controls.Add(value);
		}

		private int GetTextWidth(string text)
		{
			using (Graphics graphics = base.CreateGraphics())
			{
				return (int)graphics.MeasureString(text, this.Font).Width;
			}
		}
	}
}
