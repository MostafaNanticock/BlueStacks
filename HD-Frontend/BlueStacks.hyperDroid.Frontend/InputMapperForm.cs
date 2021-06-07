using System.Drawing;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
	public class InputMapperForm : Form
	{
		public delegate void EditHandler(string package);

		public delegate void ManageHandler(string package);

		private const int WIDTH = 400;

		private const int HEIGHT = 250;

		private const int PADDING = 10;

		private const int TEXT_HEIGHT = 50;

		private string mPackage;

		private EditHandler mEditHandler;

		private ManageHandler mManageHandler;

		public InputMapperForm(string package, EditHandler editHandler, ManageHandler manageHandler)
		{
			this.mPackage = package;
			this.mEditHandler = editHandler;
			this.mManageHandler = manageHandler;
			this.Text = "Input Mapper Tool";
			this.CreateLayout();
		}

		private void CreateLayout()
		{
			base.Size = new Size(400, 250);
			base.FormBorderStyle = FormBorderStyle.FixedSingle;
			base.MinimizeBox = false;
			base.MaximizeBox = false;
			Label label = new Label();
			label.Text = "Current app: " + ((this.mPackage != null) ? this.mPackage : "none");
			label.Location = new Point(10, 10);
			label.Width = base.ClientSize.Width - 10;
			Button button = new Button();
			button.Text = "Edit";
			button.Location = new Point(10, label.Bottom + 10);
			if (this.mPackage == null)
			{
				button.Enabled = false;
			}
			button.Click += delegate
			{
				this.mEditHandler(this.mPackage);
				base.Close();
			};
			Label label2 = new Label();
			label2.Text = "Edit the input mapper configuration for the current app.  If the current app does not yet have a configuration Prebundled, then create one from a template.";
			label2.Location = new Point(button.Right + 10, button.Top);
			label2.Size = new Size(base.ClientSize.Width - label2.Left - 10, 50);
			Button button2 = new Button();
			button2.Text = "Manage";
			button2.Location = new Point(10, label2.Bottom + 10);
			button2.Click += delegate
			{
				this.mManageHandler(this.mPackage);
				base.Close();
			};
			Label label3 = new Label();
			label3.Text = "Manage all the existing input mapper configurations.  Opens the input mapper folder in Windows Explorer.";
			label3.Location = new Point(button2.Right + 10, button2.Top);
			label3.Size = new Size(base.ClientSize.Width - label3.Left - 10, 50);
			Button button3 = new Button();
			button3.Text = "Cancel";
			button3.Location = new Point(10, label3.Bottom + 10);
			button3.Click += delegate
			{
				base.Close();
			};
			Label label4 = new Label();
			label4.Text = "Close this window.";
			label4.Location = new Point(button3.Right + 10, button3.Top);
			label4.Size = new Size(base.ClientSize.Width - label4.Left - 10, 50);
			base.Controls.AddRange(new Control[7]
			{
				label,
				button,
				label2,
				button2,
				label3,
				button3,
				label4
			});
		}
	}
}
