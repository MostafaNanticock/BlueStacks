using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;
using System.Drawing;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Agent
{
    public class UninstallerForm : Form
    {
        private Label m_Label;

        private ComboBox m_AppList;

        private ProgressBar m_ProgressBar;

        private Button m_okButton;

        private Button m_cancelButton;

        public UninstallerForm()
        {
            this.InitializeComponents();
            AppUninstaller.s_originalJson = JsonParser.GetAppList();
            this.m_AppList.BeginUpdate();
            this.m_AppList.Items.Add("Select App to Uninstall");
            this.m_AppList.SelectedIndex = 0;
            for (int i = AppUninstaller.s_systemApps; i < AppUninstaller.s_originalJson.Length; i++)
            {
                this.m_AppList.Items.Add(AppUninstaller.s_originalJson[i].name);
            }
            this.m_AppList.EndUpdate();
        }

        private void InitializeComponents()
        {
            int num = 100;
            int num2 = 200;
            base.SuspendLayout();
            base.StartPosition = FormStartPosition.CenterScreen;
            base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            base.SizeGripStyle = SizeGripStyle.Hide;
            base.ShowIcon = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.ShowInTaskbar = true;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.ClientSize = new Size(num2, num);
            this.Text = BlueStacks.hyperDroid.Locale.Strings.UninstallWindowTitle;
            this.m_Label = new Label();
            this.m_Label.Location = new Point(0, 5);
            this.m_Label.Size = new Size(num2, 25);
            this.m_Label.Text = "Select an App to Uninstall";
            this.m_AppList = new ComboBox();
            this.m_AppList.Location = new Point(5, 35);
            this.m_AppList.Size = new Size(num2 - 10, 35);
            this.m_AppList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.m_ProgressBar = new ProgressBar();
            this.m_ProgressBar.Location = new Point(num2 / 4, num / 2 - 10);
            this.m_ProgressBar.Size = new Size(num2 / 2, 20);
            this.m_ProgressBar.Style = ProgressBarStyle.Marquee;
            this.m_ProgressBar.MarqueeAnimationSpeed = 25;
            this.m_ProgressBar.Visible = false;
            this.m_okButton = new Button();
            this.m_okButton.Text = "Ok";
            this.m_okButton.DialogResult = DialogResult.OK;
            this.m_okButton.Width = 60;
            this.m_okButton.Height = 25;
            this.m_okButton.Location = new Point(30, 70);
            this.m_okButton.Click += delegate
            {
                this.Uninstall();
            };
            this.m_cancelButton = new Button();
            this.m_cancelButton.Text = "Cancel";
            this.m_cancelButton.DialogResult = DialogResult.Cancel;
            this.m_cancelButton.Width = 60;
            this.m_cancelButton.Height = 25;
            this.m_cancelButton.Location = new Point(110, 70);
            this.m_okButton.Click += delegate
            {
                base.Dispose();
            };
            base.Controls.Add(this.m_Label);
            base.Controls.Add(this.m_AppList);
            base.Controls.Add(this.m_ProgressBar);
            base.Controls.Add(this.m_okButton);
            base.Controls.Add(this.m_cancelButton);
            base.ResumeLayout(false);
            base.PerformLayout();
            Logger.Info("AppUninstaller: Components Initialized");
        }

        private void Uninstall()
        {
            int selectedIndex = this.m_AppList.SelectedIndex;
            Logger.Info("AppUninstaller: Uninstalling item " + selectedIndex.ToString());
            if (selectedIndex > 0)
            {
                base.ClientSize = new Size(200, 40);
                this.m_Label.Text = BlueStacks.hyperDroid.Locale.Strings.UninstallingWait;
                this.m_AppList.Visible = false;
                this.m_okButton.Visible = false;
                this.m_cancelButton.Visible = false;
                this.Refresh();
                string name = AppUninstaller.s_originalJson[selectedIndex - 1 + AppUninstaller.s_systemApps].name;
                string package = AppUninstaller.s_originalJson[selectedIndex - 1 + AppUninstaller.s_systemApps].package;
                Logger.Info("AppUninstaller: Uninstalling " + package);
                int num = AppUninstaller.UninstallApp(package);
                base.Visible = false;
                if (num == 0)
                {
                    MessageBox.Show(name + " " + BlueStacks.hyperDroid.Locale.Strings.UninstallSuccess, this.Text, MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                else
                {
                    MessageBox.Show(BlueStacks.hyperDroid.Locale.Strings.UninstallFailed, this.Text, MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                base.Dispose();
            }
            base.Dispose();
        }
    }
}
