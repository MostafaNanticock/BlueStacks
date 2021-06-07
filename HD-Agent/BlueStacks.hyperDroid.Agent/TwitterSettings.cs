using BlueStacks.hyperDroid.Common;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Agent
{
	public class TwitterSettings : Form
	{
		public static Label s_FollowLabel;

		public static Label s_FetchIntervalLabel;

		public static TextBoxX s_FollowNameTxtBox;

		public static TextBoxX s_FetchIntervalTxtBox;

		public static ButtonX s_DoneBtn;

		public static TwitterSettings s_ThisForm;

		public static string s_FontName = Utils.GetSystemFontName();

		public TwitterSettings()
		{
			this.InitializeComponent();
		}

		public void InitializeComponent()
		{
			TwitterSettings.s_FollowLabel = new Label();
			TwitterSettings.s_FollowLabel.AutoSize = true;
			TwitterSettings.s_FollowLabel.Font = new Font(TwitterSettings.s_FontName, 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
			TwitterSettings.s_FollowLabel.Location = new Point(29, 32);
			TwitterSettings.s_FollowLabel.Size = new Size(115, 13);
			TwitterSettings.s_FollowLabel.TextAlign = ContentAlignment.MiddleCenter;
			TwitterSettings.s_FollowLabel.Text = "Show tweets for:";
			TwitterSettings.s_FetchIntervalLabel = new Label();
			TwitterSettings.s_FetchIntervalLabel.AutoSize = true;
			TwitterSettings.s_FetchIntervalLabel.Font = new Font(TwitterSettings.s_FontName, 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
			TwitterSettings.s_FetchIntervalLabel.Location = new Point(29, 71);
			TwitterSettings.s_FetchIntervalLabel.Size = new Size(115, 13);
			TwitterSettings.s_FetchIntervalLabel.TextAlign = ContentAlignment.MiddleCenter;
			TwitterSettings.s_FetchIntervalLabel.Text = "Fetch interval (in minutes):";
			TwitterSettings.s_FollowNameTxtBox = new TextBoxX();
			TwitterSettings.s_FollowNameTxtBox.Border.Class = "TextBoxBorder";
			TwitterSettings.s_FollowNameTxtBox.Border.CornerType = eCornerType.Square;
			TwitterSettings.s_FollowNameTxtBox.Font = new Font(TwitterSettings.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
			TwitterSettings.s_FollowNameTxtBox.Location = new Point(250, 32);
			TwitterSettings.s_FollowNameTxtBox.Size = new Size(190, 20);
			TwitterSettings.s_FollowNameTxtBox.TabIndex = 1;
			TwitterSettings.s_FetchIntervalTxtBox = new TextBoxX();
			TwitterSettings.s_FetchIntervalTxtBox.Border.Class = "TextBoxBorder";
			TwitterSettings.s_FetchIntervalTxtBox.Border.CornerType = eCornerType.Square;
			TwitterSettings.s_FetchIntervalTxtBox.Font = new Font(TwitterSettings.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
			TwitterSettings.s_FetchIntervalTxtBox.Location = new Point(250, 71);
			TwitterSettings.s_FetchIntervalTxtBox.Size = new Size(190, 20);
			TwitterSettings.s_FetchIntervalTxtBox.TabIndex = 2;
			TwitterSettings.s_DoneBtn = new ButtonX();
			TwitterSettings.s_DoneBtn.AccessibleRole = AccessibleRole.PushButton;
			TwitterSettings.s_DoneBtn.ColorTable = eButtonColor.OrangeWithBackground;
			TwitterSettings.s_DoneBtn.Location = new Point(350, 110);
			TwitterSettings.s_DoneBtn.Size = new Size(90, 35);
			TwitterSettings.s_DoneBtn.Style = eDotNetBarStyle.StyleManagerControlled;
			TwitterSettings.s_DoneBtn.TabIndex = 3;
			TwitterSettings.s_DoneBtn.Text = "Done";
			TwitterSettings.s_DoneBtn.Click += TwitterSettings.s_DoneBtnClick;
			base.ClientSize = new Size(500, 160);
			base.MaximizeBox = false;
			base.FormBorderStyle = FormBorderStyle.Fixed3D;
			this.Text = "Twitter Feeds Settings";
			base.Controls.Add(TwitterSettings.s_FollowLabel);
			base.Controls.Add(TwitterSettings.s_FetchIntervalLabel);
			base.Controls.Add(TwitterSettings.s_FollowNameTxtBox);
			base.Controls.Add(TwitterSettings.s_FetchIntervalTxtBox);
			base.Controls.Add(TwitterSettings.s_DoneBtn);
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
			TwitterSettings.s_FollowNameTxtBox.Text = (string)registryKey.GetValue("TwitterName", "");
			TwitterSettings.s_FetchIntervalTxtBox.Text = (string)registryKey.GetValue("TwitterFetchInterval", "");
			base.ResumeLayout(false);
			TwitterSettings.s_ThisForm = this;
		}

		public static void s_DoneBtnClick(object sender, EventArgs e)
		{
			string value = TwitterSettings.s_FetchIntervalTxtBox.Text;
			string text = TwitterSettings.s_FollowNameTxtBox.Text;
			if (string.IsNullOrEmpty(text))
			{
				MessageBox.Show("Please provide the Twitter Name to follow");
			}
			else
			{
				if (string.IsNullOrEmpty(value))
				{
					value = "5";
				}
				RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.RegBasePath);
				registryKey.SetValue("TwitterFetchInterval", value);
				registryKey.SetValue("TwitterName", text);
				TwitterSettings.s_ThisForm.Dispose();
			}
		}
	}
}
