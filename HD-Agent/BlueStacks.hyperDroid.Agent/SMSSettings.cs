using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using CodeTitans.JSon;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Agent
{
    public class SMSSettings : Form
    {
        public static SlidePanel s_HomeScreenPanel;

        public static Label s_PhoneQuestionLabel;

        public static Label s_MainDescLabel;

        public static RadioButton s_YesRadioBtn;

        public static RadioButton s_NoRadioBtn;

        public static PictureBox s_ImagePBox;

        public static ButtonX s_NextBtn;

        public static SlidePanel s_RegScreenPanel;

        public static Label s_EmailLabel;

        public static TextBoxX s_EmailTxtBox;

        public static Label s_EmailDescLabel;

        public static Label s_PhoneNumLabel;

        public static Label s_PlusLabel;

        public static TextBoxX s_CountryCodeTxtBox;

        public static TextBoxX s_MobileNumTxtBox;

        public static Label s_PhoneDescLabel;

        public static ButtonX s_BackBtn;

        public static ButtonX s_RegisterBtn;

        public static SlidePanel s_PasswordScreenPanel;

        public static Label s_LoginEmailLabel;

        public static TextBoxX s_LoginEmailTxtBox;

        public static Label s_LoginPasswordLabel;

        public static TextBoxX s_LoginPasswordTxtBox;

        public static ButtonX s_LoginBtn;

        public static ButtonX s_ForgotPasswordBtn;

        public static ButtonX s_LoginBackBtn;

        public static SMSSettings s_ThisForm;

        public static string s_FontName = BlueStacks.hyperDroid.Common.Utils.GetSystemFontName();

        public SMSSettings()
        {
            this.InitializeComponent();
        }

        public void InitializeComponent()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string path = (string)registryKey.GetValue("InstallDir");
            string filename = Path.Combine(path, "cc.png");
            base.ClientSize = new Size(673, 479);
            SMSSettings.s_HomeScreenPanel = new SlidePanel();
            SMSSettings.s_HomeScreenPanel.SlideOutButtonVisible = false;
            SMSSettings.s_HomeScreenPanel.BackColor = SystemColors.ControlDark;
            SMSSettings.s_HomeScreenPanel.SlideSide = eSlideSide.Left;
            SMSSettings.s_HomeScreenPanel.Location = new Point(0, 0);
            SMSSettings.s_HomeScreenPanel.Size = new Size(675, 482);
            SMSSettings.s_HomeScreenPanel.TabIndex = 0;
            SMSSettings.s_HomeScreenPanel.Text = "s_HomeScreenPanel";
            SMSSettings.s_NextBtn = new ButtonX();
            SMSSettings.s_NextBtn.AccessibleRole = AccessibleRole.PushButton;
            SMSSettings.s_NextBtn.ColorTable = eButtonColor.OrangeWithBackground;
            SMSSettings.s_NextBtn.Location = new Point(545, 434);
            SMSSettings.s_NextBtn.Size = new Size(88, 33);
            SMSSettings.s_NextBtn.Style = eDotNetBarStyle.StyleManagerControlled;
            SMSSettings.s_NextBtn.TabIndex = 5;
            SMSSettings.s_NextBtn.Text = "Next";
            SMSSettings.s_NextBtn.Click += SMSSettings.s_NextBtnClick;
            SMSSettings.s_NoRadioBtn = new RadioButton();
            SMSSettings.s_NoRadioBtn.AutoSize = true;
            SMSSettings.s_NoRadioBtn.Font = new Font(SMSSettings.s_FontName, 14.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
            SMSSettings.s_NoRadioBtn.ForeColor = Color.White;
            SMSSettings.s_NoRadioBtn.Location = new Point(282, 153);
            SMSSettings.s_NoRadioBtn.Size = new Size(53, 26);
            SMSSettings.s_NoRadioBtn.TabIndex = 4;
            SMSSettings.s_NoRadioBtn.TabStop = true;
            SMSSettings.s_NoRadioBtn.Text = "No";
            SMSSettings.s_NoRadioBtn.UseVisualStyleBackColor = true;
            SMSSettings.s_YesRadioBtn = new RadioButton();
            SMSSettings.s_YesRadioBtn.AutoSize = true;
            SMSSettings.s_YesRadioBtn.Font = new Font(SMSSettings.s_FontName, 14.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
            SMSSettings.s_YesRadioBtn.ForeColor = Color.White;
            SMSSettings.s_YesRadioBtn.Location = new Point(282, 115);
            SMSSettings.s_YesRadioBtn.Size = new Size(62, 26);
            SMSSettings.s_YesRadioBtn.TabIndex = 3;
            SMSSettings.s_YesRadioBtn.TabStop = true;
            SMSSettings.s_YesRadioBtn.Text = "Yes";
            SMSSettings.s_YesRadioBtn.UseVisualStyleBackColor = true;
            SMSSettings.s_MainDescLabel = new Label();
            SMSSettings.s_MainDescLabel.AutoSize = false;
            SMSSettings.s_MainDescLabel.Font = new Font(SMSSettings.s_FontName, 10f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_MainDescLabel.ForeColor = Color.White;
            SMSSettings.s_MainDescLabel.Location = new Point(0, 77);
            SMSSettings.s_MainDescLabel.Size = new Size(base.ClientSize.Width, 40);
            SMSSettings.s_MainDescLabel.TabIndex = 2;
            SMSSettings.s_MainDescLabel.TextAlign = ContentAlignment.MiddleCenter;
            SMSSettings.s_MainDescLabel.Text = "Notification Center can automatically sync your SMS and notifications from your phone to PC";
            SMSSettings.s_PhoneQuestionLabel = new Label();
            SMSSettings.s_PhoneQuestionLabel.AutoSize = false;
            SMSSettings.s_PhoneQuestionLabel.Font = new Font(SMSSettings.s_FontName, 18f, FontStyle.Bold, GraphicsUnit.Point, 0);
            SMSSettings.s_PhoneQuestionLabel.ForeColor = Color.White;
            SMSSettings.s_PhoneQuestionLabel.Location = new Point(0, 38);
            SMSSettings.s_PhoneQuestionLabel.Size = new Size(base.ClientSize.Width, 29);
            SMSSettings.s_PhoneQuestionLabel.TabIndex = 1;
            SMSSettings.s_PhoneQuestionLabel.TextAlign = ContentAlignment.MiddleCenter;
            SMSSettings.s_PhoneQuestionLabel.Text = "Do you have an Android phone?";
            SMSSettings.s_ImagePBox = new PictureBox();
            SMSSettings.s_ImagePBox.BackgroundImage = Image.FromFile(filename);
            SMSSettings.s_ImagePBox.BackgroundImageLayout = ImageLayout.Stretch;
            SMSSettings.s_ImagePBox.Location = new Point(24, 199);
            SMSSettings.s_ImagePBox.Size = new Size(636, 215);
            SMSSettings.s_ImagePBox.TabIndex = 0;
            SMSSettings.s_ImagePBox.TabStop = false;
            SMSSettings.s_RegScreenPanel = new SlidePanel();
            SMSSettings.s_RegScreenPanel.SlideOutButtonVisible = false;
            SMSSettings.s_RegScreenPanel.BackColor = SystemColors.ControlDark;
            SMSSettings.s_RegScreenPanel.Location = new Point(0, 0);
            SMSSettings.s_RegScreenPanel.Size = SMSSettings.s_HomeScreenPanel.Size;
            SMSSettings.s_RegScreenPanel.SlideSide = eSlideSide.Right;
            SMSSettings.s_RegScreenPanel.TabIndex = 6;
            SMSSettings.s_RegScreenPanel.Text = "s_RegScreenPanel";
            SMSSettings.s_RegScreenPanel.IsOpen = false;
            SMSSettings.s_EmailLabel = new Label();
            SMSSettings.s_EmailLabel.AutoSize = false;
            SMSSettings.s_EmailLabel.Font = new Font(SMSSettings.s_FontName, 21.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_EmailLabel.ForeColor = Color.White;
            SMSSettings.s_EmailLabel.Location = new Point(0, 57);
            SMSSettings.s_EmailLabel.Size = new Size(base.ClientSize.Width, 33);
            SMSSettings.s_EmailLabel.TabIndex = 3;
            SMSSettings.s_EmailLabel.TextAlign = ContentAlignment.MiddleCenter;
            SMSSettings.s_EmailLabel.Text = "Your Email address:";
            SMSSettings.s_LoginEmailLabel = new Label();
            SMSSettings.s_LoginEmailLabel.AutoSize = false;
            SMSSettings.s_LoginEmailLabel.Font = new Font(SMSSettings.s_FontName, 21.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_LoginEmailLabel.ForeColor = Color.White;
            SMSSettings.s_LoginEmailLabel.Location = new Point(0, 57);
            SMSSettings.s_LoginEmailLabel.Size = new Size(base.ClientSize.Width, 33);
            SMSSettings.s_LoginEmailLabel.TabIndex = 3;
            SMSSettings.s_LoginEmailLabel.TextAlign = ContentAlignment.MiddleCenter;
            SMSSettings.s_LoginEmailLabel.Text = "Your Email address:";
            SMSSettings.s_RegisterBtn = new ButtonX();
            SMSSettings.s_RegisterBtn.AccessibleRole = AccessibleRole.PushButton;
            SMSSettings.s_RegisterBtn.ColorTable = eButtonColor.OrangeWithBackground;
            SMSSettings.s_RegisterBtn.Location = new Point(545, 434);
            SMSSettings.s_RegisterBtn.Size = new Size(88, 33);
            SMSSettings.s_RegisterBtn.Style = eDotNetBarStyle.StyleManagerControlled;
            SMSSettings.s_RegisterBtn.TabIndex = 7;
            SMSSettings.s_RegisterBtn.Text = "Register";
            SMSSettings.s_RegisterBtn.Click += SMSSettings.RegisterBtnClick;
            SMSSettings.s_BackBtn = new ButtonX();
            SMSSettings.s_BackBtn.AccessibleRole = AccessibleRole.PushButton;
            SMSSettings.s_BackBtn.ColorTable = eButtonColor.OrangeWithBackground;
            SMSSettings.s_BackBtn.Location = new Point(455, 434);
            SMSSettings.s_BackBtn.Size = new Size(88, 33);
            SMSSettings.s_BackBtn.Style = eDotNetBarStyle.StyleManagerControlled;
            SMSSettings.s_BackBtn.TabIndex = 7;
            SMSSettings.s_BackBtn.Text = "Go Back";
            SMSSettings.s_BackBtn.Click += SMSSettings.ShowHomeScreen;
            SMSSettings.s_LoginBackBtn = new ButtonX();
            SMSSettings.s_LoginBackBtn.AccessibleRole = AccessibleRole.PushButton;
            SMSSettings.s_LoginBackBtn.ColorTable = eButtonColor.OrangeWithBackground;
            SMSSettings.s_LoginBackBtn.Location = new Point(455, 434);
            SMSSettings.s_LoginBackBtn.Size = new Size(88, 33);
            SMSSettings.s_LoginBackBtn.Style = eDotNetBarStyle.StyleManagerControlled;
            SMSSettings.s_LoginBackBtn.TabIndex = 7;
            SMSSettings.s_LoginBackBtn.Text = "Go Back";
            SMSSettings.s_LoginBackBtn.Click += SMSSettings.ShowHomeScreen;
            SMSSettings.s_PhoneNumLabel = new Label();
            SMSSettings.s_PhoneNumLabel.AutoSize = false;
            SMSSettings.s_PhoneNumLabel.Font = new Font(SMSSettings.s_FontName, 21.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_PhoneNumLabel.ForeColor = Color.White;
            SMSSettings.s_PhoneNumLabel.Location = new Point(0, 242);
            SMSSettings.s_PhoneNumLabel.Size = new Size(base.ClientSize.Width, 33);
            SMSSettings.s_PhoneNumLabel.TabIndex = 4;
            SMSSettings.s_PhoneNumLabel.TextAlign = ContentAlignment.MiddleCenter;
            SMSSettings.s_PhoneNumLabel.Text = "Your country code and phone number:";
            SMSSettings.s_EmailTxtBox = new TextBoxX();
            SMSSettings.s_EmailTxtBox.Border.Class = "TextBoxBorder";
            SMSSettings.s_EmailTxtBox.Border.CornerType = eCornerType.Square;
            SMSSettings.s_EmailTxtBox.Font = new Font(SMSSettings.s_FontName, 24f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_EmailTxtBox.Location = new Point(52, 113);
            SMSSettings.s_EmailTxtBox.Size = new Size(580, 44);
            SMSSettings.s_EmailTxtBox.TabIndex = 5;
            SMSSettings.s_LoginEmailTxtBox = new TextBoxX();
            SMSSettings.s_LoginEmailTxtBox.Border.Class = "TextBoxBorder";
            SMSSettings.s_LoginEmailTxtBox.Border.CornerType = eCornerType.Square;
            SMSSettings.s_LoginEmailTxtBox.Font = new Font(SMSSettings.s_FontName, 24f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_LoginEmailTxtBox.Location = new Point(52, 113);
            SMSSettings.s_LoginEmailTxtBox.Size = new Size(580, 44);
            SMSSettings.s_LoginEmailTxtBox.TabIndex = 5;
            SMSSettings.s_EmailDescLabel = new Label();
            SMSSettings.s_EmailDescLabel.AutoSize = false;
            SMSSettings.s_EmailDescLabel.Font = new Font(SMSSettings.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_EmailDescLabel.ForeColor = Color.White;
            SMSSettings.s_EmailDescLabel.Location = new Point(0, 176);
            SMSSettings.s_EmailDescLabel.Size = new Size(base.ClientSize.Width, 23);
            SMSSettings.s_EmailDescLabel.TabIndex = 6;
            SMSSettings.s_EmailDescLabel.TextAlign = ContentAlignment.MiddleCenter;
            SMSSettings.s_EmailDescLabel.Text = "Your Email address will be used to login to BlueStacks account.";
            SMSSettings.s_MobileNumTxtBox = new TextBoxX();
            SMSSettings.s_MobileNumTxtBox.Border.Class = "TextBoxBorder";
            SMSSettings.s_MobileNumTxtBox.Border.CornerType = eCornerType.Square;
            SMSSettings.s_MobileNumTxtBox.Font = new Font(SMSSettings.s_FontName, 24f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_MobileNumTxtBox.Location = new Point(139, 305);
            SMSSettings.s_MobileNumTxtBox.Size = new Size(490, 44);
            SMSSettings.s_MobileNumTxtBox.TabIndex = 8;
            SMSSettings.s_CountryCodeTxtBox = new TextBoxX();
            SMSSettings.s_CountryCodeTxtBox.Border.Class = "TextBoxBorder";
            SMSSettings.s_CountryCodeTxtBox.Border.CornerType = eCornerType.Square;
            SMSSettings.s_CountryCodeTxtBox.Font = new Font(SMSSettings.s_FontName, 24f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_CountryCodeTxtBox.Location = new Point(79, 305);
            SMSSettings.s_CountryCodeTxtBox.Size = new Size(54, 44);
            SMSSettings.s_CountryCodeTxtBox.TabIndex = 9;
            SMSSettings.s_PlusLabel = new Label();
            SMSSettings.s_PlusLabel.AutoSize = true;
            SMSSettings.s_PlusLabel.Font = new Font(SMSSettings.s_FontName, 21.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_PlusLabel.ForeColor = Color.White;
            SMSSettings.s_PlusLabel.Location = new Point(47, 309);
            SMSSettings.s_PlusLabel.Size = new Size(29, 33);
            SMSSettings.s_PlusLabel.TabIndex = 10;
            SMSSettings.s_PlusLabel.Text = "+";
            SMSSettings.s_PhoneDescLabel = new Label();
            SMSSettings.s_PhoneDescLabel.AutoSize = false;
            SMSSettings.s_PhoneDescLabel.Font = new Font(SMSSettings.s_FontName, 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_PhoneDescLabel.ForeColor = Color.White;
            SMSSettings.s_PhoneDescLabel.Location = new Point(0, 368);
            SMSSettings.s_PhoneDescLabel.Size = new Size(base.ClientSize.Width, 23);
            SMSSettings.s_PhoneDescLabel.TabIndex = 12;
            SMSSettings.s_PhoneDescLabel.TextAlign = ContentAlignment.MiddleCenter;
            SMSSettings.s_PhoneDescLabel.Text = "You will receive an SMS on your phone with instructions to pair it with your PC";
            SMSSettings.s_LoginPasswordLabel = new Label();
            SMSSettings.s_LoginPasswordLabel.AutoSize = false;
            SMSSettings.s_LoginPasswordLabel.Font = new Font(SMSSettings.s_FontName, 21.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_LoginPasswordLabel.ForeColor = Color.White;
            SMSSettings.s_LoginPasswordLabel.Location = new Point(0, 242);
            SMSSettings.s_LoginPasswordLabel.Size = new Size(base.ClientSize.Width, 33);
            SMSSettings.s_LoginPasswordLabel.TabIndex = 12;
            SMSSettings.s_LoginPasswordLabel.TextAlign = ContentAlignment.MiddleCenter;
            SMSSettings.s_LoginPasswordLabel.Text = "Your password:";
            SMSSettings.s_LoginPasswordTxtBox = new TextBoxX();
            SMSSettings.s_LoginPasswordTxtBox.Border.Class = "TextBoxBorder";
            SMSSettings.s_LoginPasswordTxtBox.Border.CornerType = eCornerType.Square;
            SMSSettings.s_LoginPasswordTxtBox.Font = new Font(SMSSettings.s_FontName, 24f, FontStyle.Regular, GraphicsUnit.Point, 0);
            SMSSettings.s_LoginPasswordTxtBox.Location = new Point(52, 305);
            SMSSettings.s_LoginPasswordTxtBox.Size = new Size(580, 44);
            SMSSettings.s_LoginPasswordTxtBox.TabIndex = 9;
            SMSSettings.s_LoginPasswordTxtBox.PasswordChar = '*';
            SMSSettings.s_LoginBtn = new ButtonX();
            SMSSettings.s_LoginBtn.AccessibleRole = AccessibleRole.PushButton;
            SMSSettings.s_LoginBtn.ColorTable = eButtonColor.OrangeWithBackground;
            SMSSettings.s_LoginBtn.Location = new Point(545, 434);
            SMSSettings.s_LoginBtn.Size = new Size(88, 33);
            SMSSettings.s_LoginBtn.Style = eDotNetBarStyle.StyleManagerControlled;
            SMSSettings.s_LoginBtn.TabIndex = 7;
            SMSSettings.s_LoginBtn.Text = "Login";
            SMSSettings.s_LoginBtn.Click += SMSSettings.LoginBtnClick;
            SMSSettings.s_ForgotPasswordBtn = new ButtonX();
            SMSSettings.s_ForgotPasswordBtn.AccessibleRole = AccessibleRole.PushButton;
            SMSSettings.s_ForgotPasswordBtn.ColorTable = eButtonColor.OrangeWithBackground;
            SMSSettings.s_ForgotPasswordBtn.Location = new Point(52, 434);
            SMSSettings.s_ForgotPasswordBtn.Size = new Size(108, 33);
            SMSSettings.s_ForgotPasswordBtn.Style = eDotNetBarStyle.StyleManagerControlled;
            SMSSettings.s_ForgotPasswordBtn.TabIndex = 8;
            SMSSettings.s_ForgotPasswordBtn.Text = "Forgot Password?";
            SMSSettings.s_ForgotPasswordBtn.Click += SMSSettings.ForgotPasswordBtnClick;
            SMSSettings.s_PasswordScreenPanel = new SlidePanel();
            SMSSettings.s_PasswordScreenPanel.SlideOutButtonVisible = false;
            SMSSettings.s_PasswordScreenPanel.BackColor = SystemColors.ControlDark;
            SMSSettings.s_PasswordScreenPanel.Location = new Point(0, 0);
            SMSSettings.s_PasswordScreenPanel.Size = SMSSettings.s_HomeScreenPanel.Size;
            SMSSettings.s_PasswordScreenPanel.SlideSide = eSlideSide.Right;
            SMSSettings.s_PasswordScreenPanel.Text = "PasswordScreenPanel";
            SMSSettings.s_PasswordScreenPanel.IsOpen = false;
            SMSSettings.s_HomeScreenPanel.Controls.Add(SMSSettings.s_NextBtn);
            SMSSettings.s_HomeScreenPanel.Controls.Add(SMSSettings.s_NoRadioBtn);
            SMSSettings.s_HomeScreenPanel.Controls.Add(SMSSettings.s_YesRadioBtn);
            SMSSettings.s_HomeScreenPanel.Controls.Add(SMSSettings.s_MainDescLabel);
            SMSSettings.s_HomeScreenPanel.Controls.Add(SMSSettings.s_PhoneQuestionLabel);
            SMSSettings.s_HomeScreenPanel.Controls.Add(SMSSettings.s_ImagePBox);
            SMSSettings.s_HomeScreenPanel.ResumeLayout(false);
            SMSSettings.s_HomeScreenPanel.PerformLayout();
            SMSSettings.s_RegScreenPanel.Controls.Add(SMSSettings.s_PhoneDescLabel);
            SMSSettings.s_RegScreenPanel.Controls.Add(SMSSettings.s_PlusLabel);
            SMSSettings.s_RegScreenPanel.Controls.Add(SMSSettings.s_CountryCodeTxtBox);
            SMSSettings.s_RegScreenPanel.Controls.Add(SMSSettings.s_MobileNumTxtBox);
            SMSSettings.s_RegScreenPanel.Controls.Add(SMSSettings.s_EmailDescLabel);
            SMSSettings.s_RegScreenPanel.Controls.Add(SMSSettings.s_EmailTxtBox);
            SMSSettings.s_RegScreenPanel.Controls.Add(SMSSettings.s_PhoneNumLabel);
            SMSSettings.s_RegScreenPanel.Controls.Add(SMSSettings.s_EmailLabel);
            SMSSettings.s_RegScreenPanel.Controls.Add(SMSSettings.s_RegisterBtn);
            SMSSettings.s_RegScreenPanel.Controls.Add(SMSSettings.s_BackBtn);
            SMSSettings.s_RegScreenPanel.ResumeLayout(false);
            SMSSettings.s_RegScreenPanel.PerformLayout();
            SMSSettings.s_PasswordScreenPanel.Controls.Add(SMSSettings.s_LoginEmailLabel);
            SMSSettings.s_PasswordScreenPanel.Controls.Add(SMSSettings.s_LoginEmailTxtBox);
            SMSSettings.s_PasswordScreenPanel.Controls.Add(SMSSettings.s_LoginPasswordLabel);
            SMSSettings.s_PasswordScreenPanel.Controls.Add(SMSSettings.s_LoginPasswordTxtBox);
            SMSSettings.s_PasswordScreenPanel.Controls.Add(SMSSettings.s_LoginBtn);
            SMSSettings.s_PasswordScreenPanel.Controls.Add(SMSSettings.s_ForgotPasswordBtn);
            SMSSettings.s_PasswordScreenPanel.Controls.Add(SMSSettings.s_LoginBackBtn);
            SMSSettings.s_PasswordScreenPanel.ResumeLayout(false);
            SMSSettings.s_PasswordScreenPanel.PerformLayout();
            base.Controls.Add(SMSSettings.s_HomeScreenPanel);
            base.Controls.Add(SMSSettings.s_RegScreenPanel);
            base.Controls.Add(SMSSettings.s_PasswordScreenPanel);
            base.MaximizeBox = false;
            base.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.Text = "Notification Center Registration";
            base.ResumeLayout(false);
            SMSSettings.s_ThisForm = this;
            RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(Strings.CloudRegKeyPath);
            string text = (string)registryKey2.GetValue("CCPin");
            if (!string.IsNullOrEmpty(text))
            {
                SMSSettings.ShowExistingPin(text);
            }
        }

        public static void ShowExistingPin(string pin)
        {
            SMSSettings.s_HomeScreenPanel.IsOpen = true;
            SMSSettings.s_PasswordScreenPanel.IsOpen = false;
            SMSSettings.s_RegScreenPanel.IsOpen = false;
            SMSSettings.s_NoRadioBtn.Visible = false;
            SMSSettings.s_YesRadioBtn.Visible = false;
            string text = string.Format("Your PIN is: " + SMSSettings.SplitPINInReadableFormat(pin));
            SMSSettings.s_PhoneQuestionLabel.Text = text;
            SMSSettings.s_MainDescLabel.Text = "Please check your email for instructions on how to pair up your Phone and PC.";
            SMSSettings.s_NextBtn.Text = "Edit";
        }

        public static void s_NextBtnClick(object sender, EventArgs e)
        {
            if (SMSSettings.s_NextBtn.Text == "Edit")
            {
                SMSSettings.s_NextBtn.Text = "Next";
                SMSSettings.s_NoRadioBtn.Visible = true;
                SMSSettings.s_YesRadioBtn.Visible = true;
                SMSSettings.s_PhoneQuestionLabel.Text = "Do you have an Android phone?";
                SMSSettings.s_MainDescLabel.Text = "Notification Center can automatically sync your SMS and notifications from your phone to PC";
            }
            else
            {
                if (SMSSettings.s_NoRadioBtn.Checked)
                {
                    SMSSettings.s_MobileNumTxtBox.Enabled = false;
                    SMSSettings.s_CountryCodeTxtBox.Enabled = false;
                    SMSSettings.s_MobileNumTxtBox.WatermarkText = "Available for Android phones only";
                }
                else
                {
                    SMSSettings.s_MobileNumTxtBox.Enabled = true;
                    SMSSettings.s_CountryCodeTxtBox.Enabled = true;
                    SMSSettings.s_MobileNumTxtBox.WatermarkText = "";
                }
                SMSSettings.s_HomeScreenPanel.IsOpen = false;
                SMSSettings.s_PasswordScreenPanel.IsOpen = false;
                SMSSettings.s_RegScreenPanel.IsOpen = true;
            }
        }

        public static void RegisterBtnClick(object sender, EventArgs e)
        {
            try
            {
                SMSSettings.DoEmailRegistration();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to register. err: " + ex.ToString());
                MessageBox.Show("Could not complete registration. Please check your internet connection and try again.", "Notification center registration failed.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public static void DoEmailRegistration()
        {
            if (!BlueStacks.hyperDroid.Common.Utils.IsValidEmail(SMSSettings.s_EmailTxtBox.Text))
            {
                MessageBox.Show("Invalid email address. Please check for spelling mistakes and try again.", "Notification center registration failed.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.CloudRegKeyPath);
                string arg = (string)registryKey.GetValue("Host");
                string url = arg + "/" + Strings.RegisterEmailUrl;
                string value = Regex.Replace(SMSSettings.s_CountryCodeTxtBox.Text + SMSSettings.s_MobileNumTxtBox.Text, "[^0-9]", "");
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("email", SMSSettings.s_EmailTxtBox.Text);
                dictionary.Add("phone", value);
                string text = Client.Post(url, dictionary, null, true);
                if (text == null)
                {
                    MessageBox.Show("Could not complete registration. Please check your internet connection and try again.", "Notification center registration failed.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    JSonReader jSonReader = new JSonReader();
                    IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(text);
                    string stringValue = iJSonObject["success"].StringValue;
                    if (string.Compare(stringValue, "false") == 0)
                    {
                        MessageBox.Show("Could not complete registration. Error: " + iJSonObject["reason"].StringValue, "Notification center registration failed.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else
                    {
                        string stringValue2 = iJSonObject["is_new"].StringValue;
                        if (string.Compare(stringValue2, "false") == 0)
                        {
                            MessageBox.Show("This email address is already registered with us. Please login with your password", "Notification center", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            SMSSettings.ShowLoginScreen();
                        }
                        else
                        {
                            string stringValue3 = iJSonObject["pin"].StringValue;
                            registryKey.SetValue("CCPin", iJSonObject["pin"].StringValue);
                            registryKey.SetValue("Email", iJSonObject["email"].StringValue);
                            byte[] inArray = SecureUserData.Encrypt(iJSonObject["bst_auth_key"].StringValue);
                            registryKey.SetValue("Key", Convert.ToBase64String(inArray));
                            byte[] inArray2 = SecureUserData.Encrypt(iJSonObject["bst_auth_secret"].StringValue);
                            registryKey.SetValue("Secret", Convert.ToBase64String(inArray2));
                            SMSSettings.ShowExistingPin(stringValue3);
                        }
                    }
                }
            }
        }

        public static void ShowLoginScreen()
        {
            SMSSettings.s_RegScreenPanel.IsOpen = false;
            SMSSettings.s_HomeScreenPanel.IsOpen = false;
            SMSSettings.s_PasswordScreenPanel.IsOpen = true;
            SMSSettings.s_LoginEmailTxtBox.Text = SMSSettings.s_EmailTxtBox.Text;
        }

        public static void ShowHomeScreen(object sender, EventArgs e)
        {
            SMSSettings.s_PasswordScreenPanel.IsOpen = false;
            SMSSettings.s_RegScreenPanel.IsOpen = false;
            SMSSettings.s_HomeScreenPanel.IsOpen = true;
        }

        public static void ForgotPasswordBtnClick(object sender, EventArgs e)
        {
            try
            {
                SMSSettings.SendForgotPasswordReq();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send forgot password request. err: " + ex.ToString());
            }
        }

        public static void SendForgotPasswordReq()
        {
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.CloudRegKeyPath);
            string arg = (string)registryKey.GetValue("Host");
            string url = arg + "/" + Strings.ForgotPasswordUrl;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("email", SMSSettings.s_LoginEmailTxtBox.Text);
            string text = Client.Post(url, dictionary, null, true);
            if (text == null)
            {
                MessageBox.Show("Could not send password request.\nPlease check your internet connection and try again.", "Notification center", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                JSonReader jSonReader = new JSonReader();
                IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(text);
                string stringValue = iJSonObject["success"].StringValue;
                if (string.Compare(stringValue, "false") == 0)
                {
                    MessageBox.Show("Could not send password request.\nError: " + iJSonObject["reason"].StringValue, "Notification center", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    string stringValue2 = iJSonObject["reason"].StringValue;
                    MessageBox.Show(stringValue2, "Notification center", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
        }

        public static void LoginBtnClick(object sender, EventArgs e)
        {
            try
            {
                SMSSettings.DoLogin();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to do login. Err: " + ex.ToString());
                MessageBox.Show("Could not login. Please check your internet connection and try again.", "Notification center registration failed.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public static void DoLogin()
        {
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.CloudRegKeyPath);
            string arg = (string)registryKey.GetValue("Host");
            string url = arg + "/" + Strings.LoginUrl;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("email", SMSSettings.s_LoginEmailTxtBox.Text);
            dictionary.Add("password", SMSSettings.s_LoginPasswordTxtBox.Text);
            string text = Client.Post(url, dictionary, null, true);
            if (text == null)
            {
                MessageBox.Show("Could not complete registration. Please check your internet connection and try again.", "Notification center signup failed.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                JSonReader jSonReader = new JSonReader();
                IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(text);
                string stringValue = iJSonObject["success"].StringValue;
                if (string.Compare(stringValue, "false") == 0)
                {
                    MessageBox.Show("Could not complete registration. Error: " + iJSonObject["reason"].StringValue, "Notification center registration failed.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    string stringValue2 = iJSonObject["pin"].StringValue;
                    registryKey.SetValue("CCPin", iJSonObject["pin"].StringValue);
                    registryKey.SetValue("Email", iJSonObject["email"].StringValue);
                    byte[] inArray = SecureUserData.Encrypt(iJSonObject["bst_auth_key"].StringValue);
                    registryKey.SetValue("Key", Convert.ToBase64String(inArray));
                    byte[] inArray2 = SecureUserData.Encrypt(iJSonObject["bst_auth_secret"].StringValue);
                    registryKey.SetValue("Secret", Convert.ToBase64String(inArray2));
                    SMSSettings.ShowExistingPin(stringValue2);
                }
            }
        }

        public static string SplitPINInReadableFormat(string pin)
        {
            return pin.Substring(0, 3) + " " + pin.Substring(3, 3) + " " + pin.Substring(6, 3);
        }
    }
}
