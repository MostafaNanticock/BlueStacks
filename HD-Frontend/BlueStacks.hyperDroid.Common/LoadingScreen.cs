using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common
{
	internal class LoadingScreen : UserControl
	{
		public delegate void ToggleFullScreen();

		public class NewProgressBar : ProgressBar
		{
			private enum BarType
			{
				Progress,
				Marquee
			}

			private BarType barType;

			private SolidBrush baseBrush;

			private SolidBrush backBrush;

			private SolidBrush foreBrush;

			private int marqueeStart;

			public NewProgressBar(string type)
			{
				base.SetStyle(ControlStyles.UserPaint, true);
				base.SetStyle(ControlStyles.DoubleBuffer, true);
				base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				this.barType = (BarType)Enum.Parse(typeof(BarType), type, true);
				Color color = Color.FromArgb(35, 147, 213);
				this.baseBrush = new SolidBrush(color);
				Color color2 = Color.FromArgb(195, 195, 193);
				this.backBrush = new SolidBrush(color2);
				Color color3 = Color.FromArgb(21, 83, 120);
				this.foreBrush = new SolidBrush(color3);
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				Rectangle clipRectangle = e.ClipRectangle;
				this.FillRectangle(e, this.baseBrush, 0, 0, clipRectangle.Width, 1);
				this.FillRectangle(e, this.backBrush, 0, 1, clipRectangle.Width, clipRectangle.Height - 2);
				this.FillRectangle(e, this.baseBrush, 0, clipRectangle.Height - 1, clipRectangle.Width, 1);
				switch (this.barType)
				{
				case BarType.Progress:
				{
					int width = (int)((double)clipRectangle.Width * ((double)base.Value / (double)base.Maximum));
					this.FillRectangle(e, this.foreBrush, 0, 0, width, clipRectangle.Height);
					break;
				}
				case BarType.Marquee:
					this.FillRectangle(e, this.foreBrush, this.marqueeStart, 0, 96, clipRectangle.Height);
					this.marqueeStart += 10;
					if (this.marqueeStart >= clipRectangle.Width - 20)
					{
						this.marqueeStart = 0;
					}
					break;
				}
			}

			private void FillRectangle(PaintEventArgs e, Brush brush, int x, int y, int width, int height)
			{
				e.Graphics.FillRectangle(brush, x, y, width, height);
			}
		}

		public class AppNameText : Label
		{
			public AppNameText()
			{
				this.Font = new Font(Utils.GetSystemFontName(), 18f, FontStyle.Bold);
				base.Height = 36;
			}

			protected override void OnPaint(PaintEventArgs evt)
			{
				evt.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
				base.OnPaint(evt);
			}
		}

		public class StatusText : Label
		{
			public StatusText()
			{
				this.Font = new Font(Utils.GetSystemFontName(), 12f, FontStyle.Regular);
				base.Height = 24;
			}

			protected override void OnPaint(PaintEventArgs evt)
			{
				evt.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
				base.OnPaint(evt);
			}
		}

		private Image splashLogoImage;

		private Image whiteLogoImage;

		private Image closeButtonImage;

		private Image fullScreenButtonImage;

		private NewProgressBar progressBar;

		private Label statusText;

		private Label fullScreenButton;

		private Label closeButton;

		private string imageDir;

		private Form parentForm;

		private bool isFullScreen;

		public LoadingScreen(Size loadingScreenSize, Image appIconImage, string appName, string barType, ToggleFullScreen toggleFullScreen)
		{
			Logger.Info("LoadingScreen({0}, {1}, {2})", loadingScreenSize, appIconImage, appName);
			this.parentForm = base.FindForm();
			this.SetImageDir();
			this.LoadImages();
			base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
			base.Width = loadingScreenSize.Width;
			base.Height = loadingScreenSize.Height;
			this.BackColor = Color.FromArgb(35, 147, 213);
			if (base.Width == Screen.PrimaryScreen.Bounds.Width && base.Height == Screen.PrimaryScreen.Bounds.Height)
			{
				this.isFullScreen = true;
			}
			else
			{
				this.isFullScreen = false;
			}
			int num = loadingScreenSize.Width / 2;
			int num2 = loadingScreenSize.Height / 2;
			Logger.Info("centerX: {0}, centerY: {1}", num, num2);
			Label label = new Label();
			if (appName != null && appName.Trim() != "")
			{
				Logger.Info("Using app icon");
				label.BackgroundImage = appIconImage;
			}
			else
			{
				Logger.Info("Using splash logo");
				label.BackgroundImage = this.splashLogoImage;
			}
			label.Width = label.BackgroundImage.Width;
			label.Height = label.BackgroundImage.Height;
			label.BackColor = Color.Transparent;
			Label label2 = new AppNameText();
			if (appName == "")
			{
				appName = "BlueStacks";
			}
			label2.Text = appName;
			label2.TextAlign = ContentAlignment.MiddleCenter;
			label2.Width = loadingScreenSize.Width;
			label2.UseMnemonic = false;
			label2.ForeColor = Color.White;
			label2.BackColor = Color.Transparent;
			this.statusText = new StatusText();
			this.statusText.TextAlign = ContentAlignment.MiddleCenter;
			this.statusText.Width = loadingScreenSize.Width;
			this.statusText.UseMnemonic = false;
			this.statusText.ForeColor = Color.White;
			this.statusText.BackColor = Color.Transparent;
			this.progressBar = new NewProgressBar(barType);
			this.progressBar.Width = 336;
			this.progressBar.Height = 10;
			this.progressBar.Value = 0;
			if (barType == "Marquee")
			{
				Timer timer = new Timer();
				timer.Interval = 50;
				timer.Tick += delegate
				{
					this.progressBar.Invalidate();
				};
				timer.Start();
			}
			Label label3 = new Label
			{
				BackgroundImage = this.whiteLogoImage,
				BackgroundImageLayout = ImageLayout.Stretch,
				Width = 48,
				Height = 44,
				BackColor = Color.Transparent
			};
			this.fullScreenButton = new Label();
			this.fullScreenButton.BackgroundImage = this.fullScreenButtonImage;
			this.fullScreenButton.BackgroundImageLayout = ImageLayout.Stretch;
			this.fullScreenButton.Width = 24;
			this.fullScreenButton.Height = 24;
			this.fullScreenButton.BackColor = Color.Transparent;
			this.fullScreenButton.Click += delegate
			{
				if (toggleFullScreen != null)
				{
					toggleFullScreen();
					this.FullScreenToggled();
				}
			};
			if (toggleFullScreen == null)
			{
				this.fullScreenButton.Visible = false;
			}
			this.closeButton = new Label();
			this.closeButton.BackgroundImage = this.closeButtonImage;
			this.closeButton.BackgroundImageLayout = ImageLayout.Stretch;
			this.closeButton.Width = 24;
			this.closeButton.Height = 24;
			this.closeButton.BackColor = Color.Transparent;
			this.closeButton.Click += delegate
			{
				this.parentForm.Close();
			};
			if (!this.isFullScreen)
			{
				this.closeButton.Visible = false;
			}
			int num3 = label.Height + 30 + label2.Height + 50 + this.progressBar.Height + 20 + this.statusText.Height;
			int y = num2 - num3 / 2;
			label.Location = new Point(num - label.Width / 2, y);
			label2.Location = new Point(0, label.Bottom + 30);
			this.progressBar.Location = new Point(num - this.progressBar.Width / 2, label2.Bottom + 50);
			this.statusText.Location = new Point(0, this.progressBar.Bottom + 20);
			label3.Location = new Point(num - label3.Width / 2, base.Height - label3.Height - 20);
			this.closeButton.Location = new Point(base.Width - this.closeButton.Width - 30, 30);
			this.fullScreenButton.Location = new Point(this.closeButton.Left - 10 - this.fullScreenButton.Width, 30);
			base.Controls.Add(label);
			base.Controls.Add(label2);
			base.Controls.Add(this.progressBar);
			base.Controls.Add(this.statusText);
			base.Controls.Add(label3);
			base.Controls.Add(this.closeButton);
			base.Controls.Add(this.fullScreenButton);
		}

		public void SetStatusText(string text)
		{
			this.statusText.Text = text;
		}

		private void SetImageDir()
		{
			string name = "Software\\BlueStacks";
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
			if (registryKey == null)
			{
				string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				string path = Path.Combine(folderPath, "BlueStacksSetup");
				this.imageDir = Path.Combine(path, "Images");
			}
			else
			{
				this.imageDir = (string)registryKey.GetValue("InstallDir");
			}
		}

		private void LoadImages()
		{
			Logger.Info("imageDir = " + this.imageDir);
			Image original = new Bitmap(Path.Combine(this.imageDir, "ProductLogo.png"));
			this.splashLogoImage = new Bitmap(original, new Size(128, 128));
			this.whiteLogoImage = new Bitmap(Path.Combine(this.imageDir, "WhiteLogo.png"));
			this.closeButtonImage = new Bitmap(Path.Combine(this.imageDir, "XButton.png"));
			this.fullScreenButtonImage = new Bitmap(Path.Combine(this.imageDir, "WhiteFullScreen.png"));
		}

		private void FullScreenToggled()
		{
			this.isFullScreen = !this.isFullScreen;
			this.closeButton.Visible = !this.closeButton.Visible;
		}

		public void UpdateProgressBar(int val)
		{
			this.progressBar.Value = val;
		}
	}
}
