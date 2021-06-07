using BlueStacks.hyperDroid.Locale;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Agent
{
	public class GpsSettings : Form
	{
		private const int GPS_SOURCE_HW_DEVICE = 1;

		private const int GPS_SOURCE_WIFI = 2;

		private const int GPS_SOURCE_IP = 4;

		private const int GPS_SOURCE_USER = 8;

		private Button btnCancel;

		private Button btnSave;

		private Label lblLatitude;

		private Label lblLongitude;

		private TextBox txtLatitude;

		private TextBox txtLongitude;

		private Label lblComPort;

		private ComboBox cmbComPort;

		private GroupBox groupBox1;

		private RadioButton rdbMock;

		private RadioButton rdbHwDevice;

		private RadioButton rdbGeoIp;

		private RadioButton rdbWifi;

		private GroupBox groupBox2;

		private void rdbMock_CheckedChanged(object sender, EventArgs e)
		{
			this.cmbComPort.Enabled = false;
			this.txtLatitude.Enabled = true;
			this.txtLongitude.Enabled = true;
		}

		private void rdbGeoIp_CheckedChanged(object sender, EventArgs e)
		{
			this.cmbComPort.Enabled = false;
			this.txtLatitude.Enabled = false;
			this.txtLongitude.Enabled = false;
		}

		private void rdbWifi_CheckedChanged(object sender, EventArgs e)
		{
			this.cmbComPort.Enabled = false;
			this.txtLatitude.Enabled = false;
			this.txtLongitude.Enabled = false;
		}

		private void rdbHwDevice_CheckedChanged(object sender, EventArgs e)
		{
			this.cmbComPort.Enabled = true;
			this.txtLatitude.Enabled = false;
			this.txtLongitude.Enabled = false;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
			try
			{
				this.txtLatitude.Text = registryKey.GetValue("GpsLatitude").ToString();
				this.txtLongitude.Text = registryKey.GetValue("GpsLongitude").ToString();
				this.cmbComPort.Text = registryKey.GetValue("GpsComPort").ToString();
			}
			catch (Exception ex)
			{
				string message = ex.Message;
			}
			string[] portNames = SerialPort.GetPortNames();
			string[] array = portNames;
			foreach (string item in array)
			{
				this.cmbComPort.Items.Add(item);
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Dispose(false);
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			decimal num = 0m;
			decimal num2 = 0m;
			bool flag = false;
			if (this.rdbHwDevice.Checked)
			{
				if (this.cmbComPort.Text.Length == 0)
				{
					MessageBox.Show("No COM port has been specified. Your GPS device would not be used.", "Gps Form warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				else
				{
					RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
					registryKey.SetValue("GpsComPort", this.cmbComPort.Text);
					registryKey.SetValue("GpsSource", 1);
					this.Dispose(false);
				}
			}
			else if (this.rdbWifi.Checked)
			{
				RegistryKey registryKey2 = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
				registryKey2.SetValue("GpsSource", 2);
				this.Dispose(false);
			}
			else if (this.rdbGeoIp.Checked)
			{
				RegistryKey registryKey3 = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
				registryKey3.SetValue("GpsSource", 4);
				this.Dispose(false);
			}
			else if (this.rdbMock.Checked)
			{
				if (this.txtLatitude.Text.Length == 0)
				{
					MessageBox.Show("Latitude can not be blank.", "Gps Form error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				else if (this.txtLongitude.Text.Length == 0)
				{
					MessageBox.Show("Longitude can not be blank.", "Gps Form error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				else if (!decimal.TryParse(this.txtLatitude.Text, out num))
				{
					MessageBox.Show("Latitude can only be a decimal value", "Gps Form error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				else if (!decimal.TryParse(this.txtLongitude.Text, out num2))
				{
					MessageBox.Show("Longitude can only be a decimal value", "Gps Form error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				else if (num > 90m || num < -90m)
				{
					MessageBox.Show("Latitude can have values between -90 and 90", "Gps Form error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				else if (num2 > 180m || num2 < -180m)
				{
					MessageBox.Show("Longitude can have values between -180 and 180", "Gps Form error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				else
				{
					RegistryKey registryKey4 = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
					registryKey4.SetValue("GpsLatitude", num);
					registryKey4.SetValue("GpsLongitude", num2);
					registryKey4.SetValue("GpsSource", 8);
					this.Dispose(false);
				}
			}
		}

		public GpsSettings()
		{
			this.InitializeComponents();
		}

		private void InitializeComponents()
		{
			base.StartPosition = FormStartPosition.CenterScreen;
			base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			base.SizeGripStyle = SizeGripStyle.Hide;
			base.ShowIcon = true;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.ShowInTaskbar = true;
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.Text = Strings.GpsWindowTitle;
			this.btnCancel = new Button();
			this.btnSave = new Button();
			this.lblLatitude = new Label();
			this.lblLongitude = new Label();
			this.txtLatitude = new TextBox();
			this.txtLongitude = new TextBox();
			this.lblComPort = new Label();
			this.cmbComPort = new ComboBox();
			this.groupBox1 = new GroupBox();
			this.groupBox2 = new GroupBox();
			this.rdbHwDevice = new RadioButton();
			this.rdbGeoIp = new RadioButton();
			this.rdbWifi = new RadioButton();
			this.rdbMock = new RadioButton();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			base.SuspendLayout();
			this.btnCancel.Location = new Point(113, 285);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new Size(75, 23);
			this.btnCancel.TabIndex = 0;
			this.btnCancel.Text = "&Close";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += this.btnCancel_Click;
			this.btnSave.Location = new Point(213, 285);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new Size(75, 23);
			this.btnSave.TabIndex = 1;
			this.btnSave.Text = "&Save";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += this.btnSave_Click;
			this.lblLatitude.AutoSize = true;
			this.lblLatitude.Location = new Point(27, 62);
			this.lblLatitude.Name = "lblLatitude";
			this.lblLatitude.Size = new Size(75, 13);
			this.lblLatitude.TabIndex = 3;
			this.lblLatitude.Text = "Latitude";
			this.lblLongitude.AutoSize = true;
			this.lblLongitude.Location = new Point(27, 98);
			this.lblLongitude.Name = "lblLongitude";
			this.lblLongitude.Size = new Size(84, 13);
			this.lblLongitude.TabIndex = 4;
			this.lblLongitude.Text = "Longitude";
			this.txtLatitude.Location = new Point(128, 62);
			this.txtLatitude.Enabled = false;
			this.txtLatitude.Name = "txtLatitude";
			this.txtLatitude.Size = new Size(100, 20);
			this.txtLatitude.TabIndex = 6;
			this.txtLongitude.Location = new Point(128, 95);
			this.txtLongitude.Enabled = false;
			this.txtLongitude.Name = "txtLongitude";
			this.txtLongitude.Size = new Size(100, 20);
			this.txtLongitude.TabIndex = 7;
			this.lblComPort.AutoSize = true;
			this.lblComPort.Location = new Point(27, 30);
			this.lblComPort.Name = "lblComPort";
			this.lblComPort.Size = new Size(90, 13);
			this.lblComPort.TabIndex = 8;
			this.lblComPort.Text = "Device COM Port";
			this.cmbComPort.Enabled = false;
			this.cmbComPort.FormattingEnabled = true;
			this.cmbComPort.Location = new Point(128, 27);
			this.cmbComPort.Name = "cmbComPort";
			this.cmbComPort.Size = new Size(100, 21);
			this.cmbComPort.TabIndex = 9;
			this.groupBox1.Controls.Add(this.rdbMock);
			this.groupBox1.Controls.Add(this.rdbHwDevice);
			this.groupBox1.Controls.Add(this.rdbGeoIp);
			this.groupBox1.Controls.Add(this.rdbWifi);
			this.groupBox1.Location = new Point(23, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new Size(265, 115);
			this.groupBox1.TabIndex = 10;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "GPS Location Source";
			this.groupBox2.Controls.Add(this.lblComPort);
			this.groupBox2.Controls.Add(this.lblLatitude);
			this.groupBox2.Controls.Add(this.cmbComPort);
			this.groupBox2.Controls.Add(this.lblLongitude);
			this.groupBox2.Controls.Add(this.txtLatitude);
			this.groupBox2.Controls.Add(this.txtLongitude);
			this.groupBox2.Location = new Point(23, 135);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new Size(265, 138);
			this.groupBox2.TabIndex = 11;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "GPS Settings";
			this.rdbHwDevice.AutoSize = true;
			this.rdbHwDevice.Location = new Point(26, 19);
			this.rdbHwDevice.Name = "rdbHwDevice";
			this.rdbHwDevice.Size = new Size(106, 17);
			this.rdbHwDevice.TabIndex = 0;
			this.rdbHwDevice.Text = "Hardware device";
			this.rdbHwDevice.UseVisualStyleBackColor = true;
			this.rdbHwDevice.CheckedChanged += this.rdbHwDevice_CheckedChanged;
			this.rdbWifi.AutoSize = true;
			this.rdbWifi.Location = new Point(26, 42);
			this.rdbWifi.Name = "rdbWifi";
			this.rdbWifi.Size = new Size(92, 17);
			this.rdbWifi.TabIndex = 0;
			this.rdbWifi.Text = "Wifi triangulation";
			this.rdbWifi.UseVisualStyleBackColor = true;
			this.rdbWifi.CheckedChanged += this.rdbWifi_CheckedChanged;
			this.rdbGeoIp.AutoSize = true;
			this.rdbGeoIp.Checked = true;
			this.rdbGeoIp.Location = new Point(26, 65);
			this.rdbGeoIp.Name = "rdbGeoIp";
			this.rdbGeoIp.Size = new Size(106, 17);
			this.rdbGeoIp.TabIndex = 0;
			this.rdbGeoIp.Text = "IP based GeoLocation";
			this.rdbGeoIp.UseVisualStyleBackColor = true;
			this.rdbGeoIp.CheckedChanged += this.rdbGeoIp_CheckedChanged;
			this.rdbMock.AutoSize = true;
			this.rdbMock.Location = new Point(26, 88);
			this.rdbMock.Name = "rdbMock";
			this.rdbMock.Size = new Size(120, 17);
			this.rdbMock.TabIndex = 1;
			this.rdbMock.TabStop = true;
			this.rdbMock.Text = "User specified location";
			this.rdbMock.UseVisualStyleBackColor = true;
			this.rdbMock.CheckedChanged += this.rdbMock_CheckedChanged;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(310, 318);
			base.Controls.Add(this.groupBox2);
			base.Controls.Add(this.groupBox1);
			base.Controls.Add(this.btnSave);
			base.Controls.Add(this.btnCancel);
			base.Name = "gpsForm";
			this.Text = "Bluestacks GPS settings";
			base.Load += this.Form1_Load;
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
