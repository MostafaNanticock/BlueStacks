using BlueStacks.hyperDroid.Locale;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
	internal class ControlBar : UserControl
	{
		private delegate void ClickHandler();

		private class NoneStripRenderer : ToolStripSystemRenderer
		{
			protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
			{
			}
		}

		public const int Depth = 48;

		public static int NR_LEFT_BUTTONS = 2;

		public static int NR_RIGHT_BUTTONS = 3;

		public static int BUTTON_WIDTH = 60;

		public static int BUTTON_HEIGHT = 48;

		public Size BUTTON_SIZE = new Size(ControlBar.BUTTON_WIDTH, ControlBar.BUTTON_HEIGHT);

		private ToolStrip mStrip;

		private ToolStripButton mBack;

		private ToolStripButton mMenu;

		private ToolStripButton mInputMapper;

		private ToolStripButton mHome;

		private ToolStripButton mShare;

		private ToolStripButton mSettings;

		private ToolStripButton mFullScreen;

		private ToolStripButton mClose;

		public ControlBar(string imgDir, Size parent, IControlHandler handler, bool showHomeButton, bool showShareButton, bool showSettingsButton, bool showFullScreenButton, bool showCloseButton)
		{
			string name = "Software\\BlueStacks\\Guests\\Android\\FrameBuffer\\0";
			Registry.LocalMachine.OpenSubKey(name);
			this.mStrip = new ToolStrip();
			this.mStrip.Dock = DockStyle.Bottom;
			this.mStrip.RenderMode = ToolStripRenderMode.System;
			this.mStrip.Renderer = new NoneStripRenderer();
			this.mStrip.AutoSize = false;
			this.mStrip.BackColor = Color.Transparent;
			this.mStrip.Padding = new Padding(0);
			this.mStrip.GripStyle = ToolStripGripStyle.Hidden;
			this.mStrip.Size = new Size(parent.Width, 48);
			this.mBack = this.CreateButton(imgDir + "BackButton.png", Strings.BackButtonToolTip, handler.Back, ToolStripItemAlignment.Left);
			this.mMenu = this.CreateButton(imgDir + "MenuButton.png", Strings.MenuButtonToolTip, handler.Menu, ToolStripItemAlignment.Left);
			this.mInputMapper = this.CreateButton(imgDir + "keyboard.png", "Play with Keyboard", handler.InputMapper, ToolStripItemAlignment.Left);
			this.mStrip.Items.AddRange(new ToolStripItem[2]
			{
				this.mBack,
				this.mMenu
			});
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
			int num = (int)registryKey.GetValue("InputMapping", 0);
			if (num == 1)
			{
				this.mStrip.Items.AddRange(new ToolStripItem[1]
				{
					this.mInputMapper
				});
			}
			this.mHome = this.CreateButton(imgDir + "HomeButton.png", Strings.HomeButtonToolTip, handler.Home, ToolStripItemAlignment.Left);
			int left = parent.Width / 2 - ControlBar.NR_LEFT_BUTTONS * this.BUTTON_SIZE.Width - ControlBar.BUTTON_WIDTH / 2;
			this.mHome.Margin = new Padding(left, 0, 0, 0);
			if (showHomeButton)
			{
				this.mStrip.Items.AddRange(new ToolStripItem[1]
				{
					this.mHome
				});
			}
			if (showCloseButton)
			{
				this.mClose = this.CreateButton(imgDir + "CloseButton.png", Strings.CloseButtonToolTip, handler.Close, ToolStripItemAlignment.Right);
				this.mStrip.Items.Add(this.mClose);
			}
			if (showFullScreenButton)
			{
				this.mFullScreen = this.CreateButton(imgDir + "FullScreenButton.png", Strings.FullScreenButtonToolTip, handler.FullScreen, ToolStripItemAlignment.Right);
				this.mStrip.Items.Add(this.mFullScreen);
			}
			if (showShareButton)
			{
				this.mShare = this.CreateButton(imgDir + "ShareImage.png", Strings.ShareButtonToolTip, handler.Share, ToolStripItemAlignment.Right);
				this.mStrip.Items.Add(this.mShare);
			}
			if (showSettingsButton)
			{
				this.mSettings = this.CreateButton(imgDir + "SettingsButton.png", Strings.SettingsButtonToolTip, handler.Settings, ToolStripItemAlignment.Right);
				this.mStrip.Items.Add(this.mSettings);
			}
			base.Controls.Add(this.mStrip);
			base.Load += this.OnControlBarLoad;
			base.TabStop = false;
		}

		private ToolStripButton CreateButton(string imagePath, string toolTip, ClickHandler clickHandler, ToolStripItemAlignment alignment)
		{
			ToolStripButton toolStripButton = new ToolStripButton();
			toolStripButton.Alignment = alignment;
			toolStripButton.AutoSize = false;
			toolStripButton.BackgroundImage = new Bitmap(imagePath);
			toolStripButton.BackgroundImageLayout = ImageLayout.Zoom;
			toolStripButton.Size = this.BUTTON_SIZE;
			toolStripButton.ToolTipText = toolTip;
			toolStripButton.Click += delegate
			{
				clickHandler();
			};
			return toolStripButton;
		}

		private void OnControlBarLoad(object obj, EventArgs args)
		{
			this.Dock = DockStyle.Bottom;
			this.AutoSize = true;
			base.TabStop = false;
		}
	}
}
