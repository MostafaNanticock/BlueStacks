using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common
{
	internal class CustomAlert : Balloon
	{
		private ReflectionImage reflectionImage1;

		private Bar displayBar;

		private ButtonItem buttonItem3;

		private LabelX lblTitle;

		private LabelX lblMsg;

		private static int s_numAlerts = 0;

		public static Rectangle s_screenSize = Screen.PrimaryScreen.WorkingArea;

		private static string s_FontName = Utils.GetSystemFontName();

		private static Image ResizeImage(Image src)
		{
			int width = 64;
			int height = 64;
			Image image = new Bitmap(width, height);
			Graphics graphics = Graphics.FromImage(image);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.DrawImage(src, 0, 0, image.Width, image.Height);
			src.Dispose();
			return image;
		}

		public CustomAlert(Image image, string title, string displayMsg, bool autoClose, EventHandler clickHandler)
		{
			string text = "<b>" + title + "</b>";
			base.ShowInTaskbar = false;
			base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			this.reflectionImage1 = new ReflectionImage();
			this.displayBar = new Bar();
			this.buttonItem3 = new ButtonItem();
			this.lblTitle = new LabelX();
			this.lblMsg = new LabelX();
			((ISupportInitialize)this.displayBar).BeginInit();
			base.SuspendLayout();
			this.reflectionImage1.BackColor = Color.Transparent;
			this.reflectionImage1.Image = CustomAlert.ResizeImage(image);
			this.reflectionImage1.Location = new Point(8, 8);
			this.reflectionImage1.Name = "reflectionImage1";
			this.reflectionImage1.Size = new Size(64, 100);
			this.reflectionImage1.TabIndex = 0;
			this.displayBar.BackColor = Color.Transparent;
			this.displayBar.Dock = DockStyle.Bottom;
			this.displayBar.Location = new Point(0, 111);
			this.displayBar.Name = "displayBar";
			this.displayBar.Size = new Size(280, 25);
			this.displayBar.Stretch = true;
			this.displayBar.Style = eDotNetBarStyle.Office2007;
			this.displayBar.TabIndex = 1;
			this.displayBar.TabStop = false;
			this.displayBar.Text = "displayBar";
			this.lblTitle.BackColor = Color.Transparent;
			this.lblTitle.Location = new Point(80, 20);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new Size(260, 60);
			this.lblTitle.TabIndex = 2;
			this.lblTitle.Text = text;
			this.lblMsg.WordWrap = true;
			this.lblTitle.Font = new Font(CustomAlert.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.lblMsg.BackColor = Color.Transparent;
			this.lblMsg.Location = new Point(80, 78);
			this.lblMsg.Name = "lblMsg";
			this.lblMsg.Size = new Size(260, 75);
			this.lblMsg.TabIndex = 3;
			this.lblMsg.Text = displayMsg;
			this.lblMsg.TextLineAlignment = StringAlignment.Near;
			this.lblMsg.WordWrap = true;
			this.lblMsg.Font = new Font(CustomAlert.s_FontName, 10f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.AutoScaleBaseSize = new Size(5, 13);
			this.BackColor = Color.FromArgb(227, 239, 255);
			base.BackColor2 = Color.FromArgb(175, 210, 255);
			base.BorderColor = Color.FromArgb(101, 147, 207);
			base.CaptionFont = new Font(CustomAlert.s_FontName, 12f, FontStyle.Bold);
			base.ClientSize = new Size(350, 160);
			base.Controls.AddRange(new Control[4]
			{
				this.lblMsg,
				this.lblTitle,
				this.displayBar,
				this.reflectionImage1
			});
			foreach (Control control in base.Controls)
			{
				if (clickHandler != null)
				{
					control.Click += clickHandler;
				}
				control.Click += delegate
				{
					base.Close();
				};
			}
			base.TopMost = true;
			this.ForeColor = Color.FromArgb(8, 55, 114);
			base.Location = new Point(0, 0);
			base.Name = "CustomAlert";
			base.Style = eBallonStyle.Office2007Alert;
			((ISupportInitialize)this.displayBar).EndInit();
			base.ResumeLayout(false);
			base.FormClosing += this.AlertFormClosing;
			if (autoClose)
			{
				base.AutoCloseTimeOut = 10;
			}
			else
			{
				base.AutoCloseTimeOut = 86400;
			}
			base.AutoClose = true;
			base.AlertAnimation = eAlertAnimation.RightToLeft;
			base.AlertAnimationDuration = 300;
			CustomAlert.s_numAlerts++;
			base.Location = new Point(CustomAlert.s_screenSize.Right - base.Width, CustomAlert.s_screenSize.Bottom - base.Height * CustomAlert.s_numAlerts);
			base.Show(false);
		}

		public CustomAlert(string imagePath, string title, string displayMsg, bool autoClose, EventHandler clickHandler)
			: this(Image.FromFile(imagePath), title, displayMsg, autoClose, clickHandler)
		{
		}

		private void AlertFormClosing(object sender, FormClosingEventArgs e)
		{
			CustomAlert.s_numAlerts--;
		}

		public static void ShowAlert(string imagePath, string title, string displayMsg, bool autoClose, EventHandler clickHandler)
		{
			CustomAlert.ShowAlert(Image.FromFile(imagePath), title, displayMsg, autoClose, clickHandler);
		}

		public static void ShowAlert(Image image, string title, string displayMsg, bool autoClose, EventHandler clickHandler)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				Application.Run(new CustomAlert(image, title, displayMsg, autoClose, clickHandler));
			});
			thread.IsBackground = true;
			thread.Start();
		}

		public static void ShowInstallAlert(string imagePath, string title, string displayMsg, EventHandler clickHandler)
		{
			if (Features.IsFeatureEnabled(2u))
			{
				CustomAlert.ShowAlert(imagePath, title, displayMsg, true, clickHandler);
			}
		}

		public static void ShowUninstallAlert(string imagePath, string title, string displayMsg)
		{
			if (Features.IsFeatureEnabled(4u))
			{
				CustomAlert.ShowAlert(imagePath, title, displayMsg, true, null);
			}
		}

		public static void ShowCloudConnectedAlert(string imagePath, string title, string displayMsg)
		{
			CustomAlert.ShowAlert(imagePath, title, displayMsg, true, null);
		}

		public static void ShowCloudDisconnectedAlert(string imagePath, string title, string displayMsg, EventHandler clickHandler)
		{
			CustomAlert.ShowAlert(imagePath, title, displayMsg, true, clickHandler);
		}

		public static void ShowCloudAnnouncement(string imagePath, string title, string displayMsg, bool autoClose, EventHandler clickHandler)
		{
			CustomAlert.ShowAlert(imagePath, title, displayMsg, autoClose, clickHandler);
		}

		public static void ShowCloudAnnouncement(Image image, string title, string displayMsg, bool autoClose, EventHandler clickHandler)
		{
			CustomAlert.ShowAlert(image, title, displayMsg, autoClose, clickHandler);
		}

		public static void ShowSMSMessage(string imagePath, string title, string displayMsg)
		{
			CustomAlert.ShowAlert(imagePath, title, displayMsg, false, null);
		}

		public static void ShowAndroidNotification(string imagePath, string title, string displayMsg, EventHandler clickHandler)
		{
			if (CustomAlert.s_numAlerts >= 1)
			{
				Logger.Info("Another alert already being displayed. Not showing another one.", title);
			}
			else
			{
				CustomAlert.ShowAlert(imagePath, title, displayMsg, true, clickHandler);
			}
		}
	}
}
