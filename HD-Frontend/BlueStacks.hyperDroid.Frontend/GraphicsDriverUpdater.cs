using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
	internal class GraphicsDriverUpdater : Form
	{
		private Console s_Console;

		private Label statusControl;

		private ProgressBar progressControl;

		private Button exitControl;

		[CompilerGenerated]
		private static EventHandler _003C_003E9__CachedAnonymousMethodDelegate1;

		public GraphicsDriverUpdater(Console console)
		{
			this.s_Console = console;
			base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			this.Text = string.Format("Graphics Driver Updater");
			base.ClientSize = new Size(300, 120);
			this.statusControl = new Label();
			this.statusControl.AutoSize = true;
			this.statusControl.Location = new Point(20, 15);
			this.statusControl.Text = "Updating Graphics Driver";
			this.progressControl = new ProgressBar();
			this.progressControl.Width = 260;
			this.progressControl.Location = new Point(20, 45);
			this.progressControl.MarqueeAnimationSpeed = 25;
			this.exitControl = new Button();
			this.exitControl.Text = "Cancel";
			this.exitControl.Location = new Point(280 - this.exitControl.Width, 85);
			this.exitControl.Click += delegate
			{
				Environment.Exit(0);
			};
			base.Controls.Add(this.statusControl);
			base.Controls.Add(this.progressControl);
			base.Controls.Add(this.exitControl);
		}

		private void UpdateStatus(string status)
		{
			this.statusControl.Text = status;
		}

		private void SetProgressBarStyle(ProgressBarStyle style)
		{
			this.progressControl.Style = style;
		}

		private void UpdateDownloadProgress(int progress)
		{
			this.progressControl.Value = progress;
		}

		public void Update(string downloadUrl)
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			string path = Path.Combine(folderPath, "BlueStacksSetup");
			string fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
			string filePath = Path.Combine(path, fileName);
			Thread thread = new Thread((ThreadStart)delegate
			{
				UIHelper.RunOnUIThread(this, delegate
				{
					this.UpdateStatus("Downloading graphics driver");
					this.SetProgressBarStyle(ProgressBarStyle.Continuous);
				});
				bool downloaded = false;
				int num = 5;
				while (num-- > 0 && !downloaded)
				{
					Downloader.Download(3, downloadUrl, filePath, delegate(int percent)
					{
						UIHelper.RunOnUIThread(this, delegate
						{
							this.UpdateDownloadProgress(percent);
						});
					}, delegate(string file)
					{
						try
						{
							this.InstallDriver(file);
							downloaded = true;
						}
						catch (Exception ex2)
						{
							Logger.Error("Exception in CompleteGraphicsDriverSetup: " + ex2.ToString());
							Thread.Sleep(10000);
							downloaded = false;
						}
					}, delegate(Exception ex)
					{
						downloaded = false;
						Logger.Error("DownloadGraphicsDriver error: " + ex.ToString());
						Thread.Sleep(10000);
					});
				}
			});
			thread.IsBackground = true;
			thread.Start();
		}

		private void InstallDriver(string filePath)
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			string path = Path.Combine(folderPath, "BlueStacksSetup");
			string text = Path.Combine(path, "GraphicsDriver");
			int num = Utils.Unzip(filePath, text);
			Logger.Info("Unzip runtime exited with error code: " + num);
			if (num != 0)
			{
				Logger.Error("Failed to unzip graphics driver. Aborting");
				try
				{
					Logger.Info("Deleting corrupted downloaded Prebundled...");
					File.Delete(filePath);
				}
				catch
				{
				}
			}
			else
			{
				string setupPath = Path.Combine(text, "Setup.exe");
				Logger.Info("Installing graphics driver: {0}", setupPath);
				UIHelper.RunOnUIThread(this, delegate
				{
					this.UpdateStatus("Installing graphics driver");
					this.SetProgressBarStyle(ProgressBarStyle.Marquee);
				});
				Thread thread = new Thread((ThreadStart)delegate
				{
					string text2 = "-over4id -nowinsat -s";
					Logger.Info("Launching {0} with args {1}", setupPath, text2);
					Process process = Process.Start(setupPath, text2);
					process.WaitForExit();
					Logger.Info("Installation completed. ExitCode: {0}", process.ExitCode);
					base.Close();
					DialogResult dialogResult = MessageBox.Show(BlueStacks.hyperDroid.Locale.Strings.GraphicsDriverUpdatedMessage, "Graphics Driver Updater", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
					Logger.Info("Retrying IsGraphicsDriverUptodate check");
					string text3 = default(string);
					string text4 = default(string);
					bool flag = Utils.IsGraphicsDriverUptodate(out text3, out text4, (string)null);
					Logger.Info("isDriverUptodate: " + flag);
					if (dialogResult == DialogResult.Yes)
					{
						Process.Start("shutdown.exe", "-r -t 0");
					}
					else
					{
						RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath);
						string path2 = (string)registryKey.GetValue("InstallDir");
						string fileName = Path.Combine(path2, "HD-Restart.exe");
						Process process2 = new Process();
						process2.StartInfo.FileName = fileName;
						process2.StartInfo.Arguments = "Android";
						process2.Start();
						Environment.Exit(0);
					}
				});
				thread.IsBackground = true;
				thread.Start();
			}
		}
	}
}
