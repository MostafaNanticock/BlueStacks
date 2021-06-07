using BlueStacks.hyperDroid.Common;
using DevComponents.DotNetBar;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Frontend
{
    public class InputMapperTool : Form
    {
        private System.Windows.Forms.TabControl tabControl1;

        private TabPage tabPage1;

        private TabPage tabPage2;

        private TabPage tabPage3;

        private GroupBox groupBox1;

        private Label swipeLeftLabel;

        private Label swipeRightLabel;

        private Label swipeUpLabel;

        private Label swipeDownLabel;

        private ComboBox swipeLeftKeyCombo;

        private ComboBox swipeRightKeyCombo;

        private ComboBox swipeUpKeyCombo;

        private ComboBox swipeDownKeyCombo;

        private GroupBox groupBox2;

        private HScrollBar DownHScroll;

        private Label DownTiltLabel;

        private ComboBox TiltDownKeyCombo;

        private Label TiltDownLabel;

        private HScrollBar UpHScroll;

        private Label UpTiltLabel;

        private ComboBox TiltUpKeyCombo;

        private Label TiltUpLabel;

        private HScrollBar RightHScroll;

        private Label RightTiltLabel;

        private ComboBox TiltRightKeyCombo;

        private Label TiltRightLabel;

        private HScrollBar LeftHScroll;

        private Label LeftTiltLabel;

        private ComboBox TiltLeftKeyCombo;

        private Label TiltLeftLabel;

        private GroupBox groupBox3;

        private ButtonX buttonX1;

        private Label[] TapAtLocLabel;

        private PictureBox[] ConnectingArrowsPBox;

        private ComboBox[] TapAtLocKeyCombo;

        private ButtonX buttonX2;

        private ButtonX buttonX3;

        private Panel MainPanel;

        private Label tapHelpTextLabel;

        public static string sCurrentAppPackage = "com.bluestacks.FOO";

        private System.Windows.Forms.ToolTip tapLocationTooltip = new System.Windows.Forms.ToolTip();

        private IContainer components;

        private int mLeft;

        private int mTop;

        private int mWidth;

        private int mHeight;

        private int mOrigWidth = 465;

        private int mOrigHeight = 450;

        private int mTapLocMappingIndex;

        private int mMaxMappingsSupported = 6;

        private bool mIsModifyingMapping;

        private float mTapX;

        private float mTapY;

        private string mKeysTemplate = "[keys]";

        private string mOpenSensorTemplate = "[opensensor]";

        public InputMapperTool(int left, int top, int width, int height)
        {
            if (InputMapperTool.sCurrentAppPackage == null || InputMapperTool.sCurrentAppPackage.StartsWith("com.bluestacks"))
            {
                MessageBox.Show("Please start the App for which you want to set keyboard mappings.");
            }
            else
            {
                Logger.Error("pkgName = " + InputMapperTool.sCurrentAppPackage);
                this.mLeft = left;
                this.mTop = top;
                this.mWidth = width;
                this.mHeight = height;
                this.InitializeComponent();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new TabPage();
            this.groupBox3 = new GroupBox();
            this.buttonX1 = new ButtonX();
            this.buttonX2 = new ButtonX();
            this.buttonX3 = new ButtonX();
            this.TapAtLocLabel = new Label[this.mMaxMappingsSupported];
            this.TapAtLocKeyCombo = new ComboBox[this.mMaxMappingsSupported];
            this.ConnectingArrowsPBox = new PictureBox[this.mMaxMappingsSupported];
            this.tabPage2 = new TabPage();
            this.groupBox1 = new GroupBox();
            this.swipeLeftLabel = new Label();
            this.swipeRightLabel = new Label();
            this.swipeUpLabel = new Label();
            this.swipeDownLabel = new Label();
            this.swipeLeftKeyCombo = new ComboBox();
            this.swipeRightKeyCombo = new ComboBox();
            this.swipeUpKeyCombo = new ComboBox();
            this.swipeDownKeyCombo = new ComboBox();
            this.tabPage3 = new TabPage();
            this.groupBox2 = new GroupBox();
            this.DownHScroll = new HScrollBar();
            this.DownTiltLabel = new Label();
            this.TiltDownKeyCombo = new ComboBox();
            this.TiltDownLabel = new Label();
            this.UpHScroll = new HScrollBar();
            this.UpTiltLabel = new Label();
            this.TiltUpKeyCombo = new ComboBox();
            this.TiltUpLabel = new Label();
            this.RightHScroll = new HScrollBar();
            this.RightTiltLabel = new Label();
            this.TiltRightKeyCombo = new ComboBox();
            this.TiltRightLabel = new Label();
            this.LeftHScroll = new HScrollBar();
            this.LeftTiltLabel = new Label();
            this.TiltLeftKeyCombo = new ComboBox();
            this.TiltLeftLabel = new Label();
            this.tapHelpTextLabel = new Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            base.SuspendLayout();
            this.MainPanel = new Panel();
            this.MainPanel.Location = new Point(0, 0);
            this.MainPanel.Size = new Size(this.mWidth, this.mHeight);
            this.MainPanel.Visible = false;
            this.MainPanel.Click += this.MainPanelClick;
            this.MainPanel.Cursor = Cursors.Hand;
            this.tapHelpTextLabel.Text = "Tap the area for which you want to add keyboard mapping";
            this.tapHelpTextLabel.Location = new Point(this.mWidth / 2 - 200, 10);
            this.tapHelpTextLabel.Size = new Size(450, 100);
            this.tapHelpTextLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.tapHelpTextLabel.Visible = false;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.tabControl1.Location = new Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new Size(560, 399);
            this.tabControl1.TabIndex = 0;
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.tabPage1.Location = new Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new Size(552, 266);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Tap";
            this.tabPage1.ToolTipText = "Set keyboard mappings for TAP action on screen";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.groupBox3.Controls.Add(this.buttonX1);
            this.groupBox3.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.groupBox3.Location = new Point(8, 16);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new Size(441, 334);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Tap Screen Actions";
            this.buttonX1.AccessibleRole = AccessibleRole.PushButton;
            this.buttonX1.ColorTable = eButtonColor.OrangeWithBackground;
            this.buttonX1.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.buttonX1.Location = new Point(277, 293);
            this.buttonX1.Name = "buttonX1";
            this.buttonX1.Size = new Size(148, 33);
            this.buttonX1.Style = eDotNetBarStyle.StyleManagerControlled;
            this.buttonX1.TabIndex = 26;
            this.buttonX1.Text = "Add New Mapping";
            this.buttonX1.Click += this.AddNewMappingBtnClick;
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Location = new Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new Size(552, 266);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Swipe";
            this.tabPage2.ToolTipText = "Set keyboard mappings for SWIPE actions";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.groupBox1.Controls.Add(this.swipeLeftLabel);
            this.groupBox1.Controls.Add(this.swipeRightLabel);
            this.groupBox1.Controls.Add(this.swipeUpLabel);
            this.groupBox1.Controls.Add(this.swipeDownLabel);
            this.groupBox1.Controls.Add(this.swipeLeftKeyCombo);
            this.groupBox1.Controls.Add(this.swipeRightKeyCombo);
            this.groupBox1.Controls.Add(this.swipeUpKeyCombo);
            this.groupBox1.Controls.Add(this.swipeDownKeyCombo);
            this.groupBox1.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.groupBox1.Location = new Point(8, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(441, 242);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Emulate swipe actions";
            this.swipeLeftKeyCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.swipeLeftKeyCombo.FormattingEnabled = true;
            this.swipeLeftKeyCombo.Items.AddRange(new object[43]
			{
				"Disabled",
				"1",
				"2",
				"3",
				"4",
				"5",
				"6",
				"7",
				"8",
				"9",
				"0",
				"A",
				"B",
				"C",
				"D",
				"E",
				"F",
				"G",
				"H",
				"I",
				"J",
				"K",
				"L",
				"M",
				"N",
				"O",
				"P",
				"Q",
				"R",
				"S",
				"T",
				"U",
				"V",
				"W",
				"X",
				"Y",
				"Z",
				"Space",
				"Enter",
				"Up-Arrow",
				"Down-Arrow",
				"Left-Arrow",
				"Right-Arrow"
			});
            this.swipeLeftKeyCombo.Location = new Point(242, 42);
            this.swipeLeftKeyCombo.Name = "swipeLeftKeyCombo";
            this.swipeLeftKeyCombo.Size = new Size(158, 28);
            this.swipeRightKeyCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.swipeRightKeyCombo.FormattingEnabled = true;
            this.swipeRightKeyCombo.Location = new Point(242, 85);
            this.swipeRightKeyCombo.Name = "swipeRightKeyCombo";
            this.swipeRightKeyCombo.Size = new Size(158, 28);
            this.swipeRightKeyCombo.Items.AddRange(new object[43]
			{
				"Disabled",
				"1",
				"2",
				"3",
				"4",
				"5",
				"6",
				"7",
				"8",
				"9",
				"0",
				"A",
				"B",
				"C",
				"D",
				"E",
				"F",
				"G",
				"H",
				"I",
				"J",
				"K",
				"L",
				"M",
				"N",
				"O",
				"P",
				"Q",
				"R",
				"S",
				"T",
				"U",
				"V",
				"W",
				"X",
				"Y",
				"Z",
				"Space",
				"Enter",
				"Up-Arrow",
				"Down-Arrow",
				"Left-Arrow",
				"Right-Arrow"
			});
            this.swipeUpKeyCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.swipeUpKeyCombo.FormattingEnabled = true;
            this.swipeUpKeyCombo.Location = new Point(242, 130);
            this.swipeUpKeyCombo.Name = "swipeUpKeyCombo";
            this.swipeUpKeyCombo.Size = new Size(158, 28);
            this.swipeUpKeyCombo.Items.AddRange(new object[43]
			{
				"Disabled",
				"1",
				"2",
				"3",
				"4",
				"5",
				"6",
				"7",
				"8",
				"9",
				"0",
				"A",
				"B",
				"C",
				"D",
				"E",
				"F",
				"G",
				"H",
				"I",
				"J",
				"K",
				"L",
				"M",
				"N",
				"O",
				"P",
				"Q",
				"R",
				"S",
				"T",
				"U",
				"V",
				"W",
				"X",
				"Y",
				"Z",
				"Space",
				"Enter",
				"Up-Arrow",
				"Down-Arrow",
				"Left-Arrow",
				"Right-Arrow"
			});
            this.swipeDownKeyCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.swipeDownKeyCombo.FormattingEnabled = true;
            this.swipeDownKeyCombo.Location = new Point(242, 175);
            this.swipeDownKeyCombo.Name = "swipeDownKeyCombo";
            this.swipeDownKeyCombo.Size = new Size(158, 28);
            this.swipeDownKeyCombo.Items.AddRange(new object[43]
			{
				"Disabled",
				"1",
				"2",
				"3",
				"4",
				"5",
				"6",
				"7",
				"8",
				"9",
				"0",
				"A",
				"B",
				"C",
				"D",
				"E",
				"F",
				"G",
				"H",
				"I",
				"J",
				"K",
				"L",
				"M",
				"N",
				"O",
				"P",
				"Q",
				"R",
				"S",
				"T",
				"U",
				"V",
				"W",
				"X",
				"Y",
				"Z",
				"Space",
				"Enter",
				"Up-Arrow",
				"Down-Arrow",
				"Left-Arrow",
				"Right-Arrow"
			});
            this.swipeLeftLabel.Text = "Swipe Left";
            this.swipeLeftLabel.Location = new Point(62, 45);
            this.swipeLeftLabel.Name = "SwipeLeftLabel";
            this.swipeLeftLabel.Size = new Size(121, 28);
            this.swipeLeftLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.swipeRightLabel.Text = "Swipe Right";
            this.swipeRightLabel.Location = new Point(62, 90);
            this.swipeRightLabel.Name = "SwipeRightLabel";
            this.swipeRightLabel.Size = new Size(121, 28);
            this.swipeRightLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.swipeUpLabel.Text = "Swipe Up";
            this.swipeUpLabel.Location = new Point(62, 135);
            this.swipeUpLabel.Name = "SwipeUpLabel";
            this.swipeUpLabel.Size = new Size(121, 28);
            this.swipeUpLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.swipeDownLabel.Text = "Swipe Down";
            this.swipeDownLabel.Location = new Point(62, 180);
            this.swipeDownLabel.Name = "SwipeDownLabel";
            this.swipeDownLabel.Size = new Size(121, 28);
            this.swipeDownLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.tabPage3.Controls.Add(this.groupBox2);
            this.tabPage3.Location = new Point(4, 29);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new Size(552, 266);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Tilt";
            this.tabPage3.ToolTipText = "Set keyboard settings for TILT actions";
            this.tabPage3.UseVisualStyleBackColor = true;
            this.groupBox2.Controls.Add(this.DownHScroll);
            this.groupBox2.Controls.Add(this.DownTiltLabel);
            this.groupBox2.Controls.Add(this.TiltDownKeyCombo);
            this.groupBox2.Controls.Add(this.TiltDownLabel);
            this.groupBox2.Controls.Add(this.UpHScroll);
            this.groupBox2.Controls.Add(this.UpTiltLabel);
            this.groupBox2.Controls.Add(this.TiltUpKeyCombo);
            this.groupBox2.Controls.Add(this.TiltUpLabel);
            this.groupBox2.Controls.Add(this.RightHScroll);
            this.groupBox2.Controls.Add(this.RightTiltLabel);
            this.groupBox2.Controls.Add(this.TiltRightKeyCombo);
            this.groupBox2.Controls.Add(this.TiltRightLabel);
            this.groupBox2.Controls.Add(this.LeftHScroll);
            this.groupBox2.Controls.Add(this.LeftTiltLabel);
            this.groupBox2.Controls.Add(this.TiltLeftKeyCombo);
            this.groupBox2.Controls.Add(this.TiltLeftLabel);
            this.groupBox2.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.groupBox2.Location = new Point(8, 16);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new Size(441, 254);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Device Tilt Actions";
            this.DownHScroll.Location = new Point(108, 190);
            this.DownHScroll.Maximum = 98;
            this.DownHScroll.Name = "DownHScroll";
            this.DownHScroll.Size = new Size(106, 17);
            this.DownHScroll.TabIndex = 38;
            this.DownHScroll.Scroll += this.DownHScroll_Scroll;
            this.DownTiltLabel.AutoSize = true;
            this.DownTiltLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.DownTiltLabel.Location = new Point(222, 190);
            this.DownTiltLabel.Name = "DownTiltLabel";
            this.DownTiltLabel.Size = new Size(41, 20);
            this.DownTiltLabel.TabIndex = 37;
            this.DownTiltLabel.Text = "0 Degrees";
            this.TiltDownKeyCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.TiltDownKeyCombo.FormattingEnabled = true;
            this.TiltDownKeyCombo.Items.AddRange(new object[43]
			{
				"Disabled",
				"1",
				"2",
				"3",
				"4",
				"5",
				"6",
				"7",
				"8",
				"9",
				"0",
				"A",
				"B",
				"C",
				"D",
				"E",
				"F",
				"G",
				"H",
				"I",
				"J",
				"K",
				"L",
				"M",
				"N",
				"O",
				"P",
				"Q",
				"R",
				"S",
				"T",
				"U",
				"V",
				"W",
				"X",
				"Y",
				"Z",
				"Space",
				"Enter",
				"Up-Arrow",
				"Down-Arrow",
				"Left-Arrow",
				"Right-Arrow"
			});
            this.TiltDownKeyCombo.Location = new Point(319, 187);
            this.TiltDownKeyCombo.Name = "TiltDownKeyCombo";
            this.TiltDownKeyCombo.Size = new Size(116, 28);
            this.TiltDownKeyCombo.TabIndex = 36;
            this.TiltDownLabel.AutoSize = true;
            this.TiltDownLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.TiltDownLabel.Location = new Point(7, 187);
            this.TiltDownLabel.Name = "TiltDownLabel";
            this.TiltDownLabel.Size = new Size(74, 20);
            this.TiltDownLabel.TabIndex = 35;
            this.TiltDownLabel.Text = "Tilt Down";
            this.UpHScroll.Location = new Point(108, 141);
            this.UpHScroll.Maximum = 98;
            this.UpHScroll.Name = "UpHScroll";
            this.UpHScroll.Size = new Size(106, 17);
            this.UpHScroll.TabIndex = 34;
            this.UpHScroll.Scroll += this.UpHScroll_Scroll;
            this.UpTiltLabel.AutoSize = true;
            this.UpTiltLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.UpTiltLabel.Location = new Point(222, 141);
            this.UpTiltLabel.Name = "UpTiltLabel";
            this.UpTiltLabel.Size = new Size(41, 20);
            this.UpTiltLabel.TabIndex = 33;
            this.UpTiltLabel.Text = "0 Degrees";
            this.TiltUpKeyCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.TiltUpKeyCombo.FormattingEnabled = true;
            this.TiltUpKeyCombo.Items.AddRange(new object[43]
			{
				"Disabled",
				"1",
				"2",
				"3",
				"4",
				"5",
				"6",
				"7",
				"8",
				"9",
				"0",
				"A",
				"B",
				"C",
				"D",
				"E",
				"F",
				"G",
				"H",
				"I",
				"J",
				"K",
				"L",
				"M",
				"N",
				"O",
				"P",
				"Q",
				"R",
				"S",
				"T",
				"U",
				"V",
				"W",
				"X",
				"Y",
				"Z",
				"Space",
				"Enter",
				"Up-Arrow",
				"Down-Arrow",
				"Left-Arrow",
				"Right-Arrow"
			});
            this.TiltUpKeyCombo.Location = new Point(319, 138);
            this.TiltUpKeyCombo.Name = "TiltUpKeyCombo";
            this.TiltUpKeyCombo.Size = new Size(116, 28);
            this.TiltUpKeyCombo.TabIndex = 32;
            this.TiltUpLabel.AutoSize = true;
            this.TiltUpLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.TiltUpLabel.Location = new Point(8, 138);
            this.TiltUpLabel.Name = "TiltUpLabel";
            this.TiltUpLabel.Size = new Size(54, 20);
            this.TiltUpLabel.TabIndex = 31;
            this.TiltUpLabel.Text = "Tilt Up";
            this.RightHScroll.Location = new Point(108, 90);
            this.RightHScroll.Maximum = 98;
            this.RightHScroll.Name = "RightHScroll";
            this.RightHScroll.Size = new Size(106, 17);
            this.RightHScroll.TabIndex = 30;
            this.RightHScroll.Scroll += this.RightHScroll_Scroll;
            this.RightTiltLabel.AutoSize = true;
            this.RightTiltLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.RightTiltLabel.Location = new Point(222, 90);
            this.RightTiltLabel.Name = "RightTiltLabel";
            this.RightTiltLabel.Size = new Size(41, 20);
            this.RightTiltLabel.TabIndex = 29;
            this.RightTiltLabel.Text = "0 Degrees";
            this.TiltRightKeyCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.TiltRightKeyCombo.FormattingEnabled = true;
            this.TiltRightKeyCombo.Items.AddRange(new object[43]
			{
				"Disabled",
				"1",
				"2",
				"3",
				"4",
				"5",
				"6",
				"7",
				"8",
				"9",
				"0",
				"A",
				"B",
				"C",
				"D",
				"E",
				"F",
				"G",
				"H",
				"I",
				"J",
				"K",
				"L",
				"M",
				"N",
				"O",
				"P",
				"Q",
				"R",
				"S",
				"T",
				"U",
				"V",
				"W",
				"X",
				"Y",
				"Z",
				"Space",
				"Enter",
				"Up-Arrow",
				"Down-Arrow",
				"Left-Arrow",
				"Right-Arrow"
			});
            this.TiltRightKeyCombo.Location = new Point(319, 87);
            this.TiltRightKeyCombo.Name = "TiltRightKeyCombo";
            this.TiltRightKeyCombo.Size = new Size(116, 28);
            this.TiltRightKeyCombo.TabIndex = 28;
            this.TiltRightLabel.AutoSize = true;
            this.TiltRightLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.TiltRightLabel.Location = new Point(7, 93);
            this.TiltRightLabel.Name = "TiltRightLabel";
            this.TiltRightLabel.Size = new Size(71, 20);
            this.TiltRightLabel.TabIndex = 27;
            this.TiltRightLabel.Text = "Tilt Right";
            this.LeftHScroll.Location = new Point(108, 45);
            this.LeftHScroll.Maximum = 98;
            this.LeftHScroll.Name = "LeftHScroll";
            this.LeftHScroll.Size = new Size(106, 17);
            this.LeftHScroll.TabIndex = 26;
            this.LeftHScroll.Scroll += this.LeftHScroll_Scroll;
            this.LeftTiltLabel.AutoSize = true;
            this.LeftTiltLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.LeftTiltLabel.Location = new Point(222, 40);
            this.LeftTiltLabel.Name = "LeftTiltLabel";
            this.LeftTiltLabel.Size = new Size(41, 20);
            this.LeftTiltLabel.TabIndex = 25;
            this.LeftTiltLabel.Text = "0 Degrees";
            this.TiltLeftKeyCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.TiltLeftKeyCombo.FormattingEnabled = true;
            this.TiltLeftKeyCombo.Items.AddRange(new object[43]
			{
				"Disabled",
				"1",
				"2",
				"3",
				"4",
				"5",
				"6",
				"7",
				"8",
				"9",
				"0",
				"A",
				"B",
				"C",
				"D",
				"E",
				"F",
				"G",
				"H",
				"I",
				"J",
				"K",
				"L",
				"M",
				"N",
				"O",
				"P",
				"Q",
				"R",
				"S",
				"T",
				"U",
				"V",
				"W",
				"X",
				"Y",
				"Z",
				"Space",
				"Enter",
				"Up-Arrow",
				"Down-Arrow",
				"Left-Arrow",
				"Right-Arrow"
			});
            this.TiltLeftKeyCombo.Location = new Point(319, 37);
            this.TiltLeftKeyCombo.Name = "TiltLeftKeyCombo";
            this.TiltLeftKeyCombo.Size = new Size(116, 28);
            this.TiltLeftKeyCombo.TabIndex = 24;
            this.TiltLeftLabel.AutoSize = true;
            this.TiltLeftLabel.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.TiltLeftLabel.Location = new Point(7, 42);
            this.TiltLeftLabel.Name = "TiltLeftLabel";
            this.TiltLeftLabel.Size = new Size(61, 20);
            this.TiltLeftLabel.TabIndex = 23;
            this.TiltLeftLabel.Text = "Tilt Left";
            this.buttonX2.AccessibleRole = AccessibleRole.PushButton;
            this.buttonX2.ColorTable = eButtonColor.OrangeWithBackground;
            this.buttonX2.Location = new Point(371, 408);
            this.buttonX2.Name = "buttonX2";
            this.buttonX2.Size = new Size(85, 33);
            this.buttonX2.Style = eDotNetBarStyle.StyleManagerControlled;
            this.buttonX2.TabIndex = 27;
            this.buttonX2.Text = "Save";
            this.buttonX2.Click += this.SaveBtnClick;
            this.buttonX3.AccessibleRole = AccessibleRole.PushButton;
            this.buttonX3.ColorTable = eButtonColor.OrangeWithBackground;
            this.buttonX3.Location = new Point(272, 408);
            this.buttonX3.Name = "buttonX3";
            this.buttonX3.Size = new Size(85, 33);
            this.buttonX3.Style = eDotNetBarStyle.StyleManagerControlled;
            this.buttonX3.TabIndex = 28;
            this.buttonX3.Text = "Cancel";
            this.buttonX3.Click += this.CancelBtnClick;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(this.mOrigWidth, this.mOrigHeight);
            base.Controls.Add(this.buttonX3);
            base.Controls.Add(this.buttonX2);
            base.Controls.Add(this.tabControl1);
            base.Controls.Add(this.tapHelpTextLabel);
            base.Controls.Add(this.MainPanel);
            base.Name = "InputMapperForm";
            this.Text = "Input Mapper";
            base.Load += this.FormLoad;
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.ShowInTaskbar = false;
            base.ResumeLayout(false);
        }

        private void FormLoad(object sender, EventArgs e)
        {
            this.swipeLeftKeyCombo.SelectedIndex = 0;
            this.swipeRightKeyCombo.SelectedIndex = 0;
            this.swipeUpKeyCombo.SelectedIndex = 0;
            this.swipeDownKeyCombo.SelectedIndex = 0;
            this.TiltLeftKeyCombo.SelectedIndex = 0;
            this.TiltRightKeyCombo.SelectedIndex = 0;
            this.TiltUpKeyCombo.SelectedIndex = 0;
            this.TiltDownKeyCombo.SelectedIndex = 0;
        }

        private void LeftHScroll_Scroll(object sender, ScrollEventArgs e)
        {
            this.LeftTiltLabel.Text = e.NewValue + " Degrees";
        }

        private void RightHScroll_Scroll(object sender, ScrollEventArgs e)
        {
            this.RightTiltLabel.Text = e.NewValue + " Degrees";
        }

        private void UpHScroll_Scroll(object sender, ScrollEventArgs e)
        {
            this.UpTiltLabel.Text = e.NewValue + " Degrees";
        }

        private void DownHScroll_Scroll(object sender, ScrollEventArgs e)
        {
            this.DownTiltLabel.Text = e.NewValue + " Degrees";
        }

        private void AddNewMappingBtnClick(object sender, EventArgs e)
        {
            this.tabControl1.Visible = false;
            this.buttonX2.Visible = false;
            this.buttonX3.Visible = false;
            this.MainPanel.Visible = true;
            this.tapHelpTextLabel.Visible = true;
            base.FormBorderStyle = FormBorderStyle.None;
            base.ClientSize = new Size(this.mWidth, this.mHeight);
            base.Location = new Point(this.mLeft, this.mTop);
            base.Opacity = 0.7;
        }

        private void MainPanelClick(object sender, EventArgs e)
        {
            MouseEventArgs mouseEventArgs = e as MouseEventArgs;
            float x = (float)(mouseEventArgs.X * 100 / this.mWidth);
            float y = (float)(mouseEventArgs.Y * 100 / this.mHeight);
            this.tabControl1.Visible = true;
            this.buttonX2.Visible = true;
            this.buttonX3.Visible = true;
            this.MainPanel.Visible = false;
            this.tapHelpTextLabel.Visible = false;
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.ClientSize = new Size(this.mOrigWidth, this.mOrigHeight);
            base.Opacity = 1.0;
            this.mTapX = x;
            this.mTapY = y;
            if (!this.mIsModifyingMapping)
            {
                this.AddNewMapping(x, y);
            }
            this.mIsModifyingMapping = false;
        }

        private void CancelBtnClick(object sender, EventArgs e)
        {
            base.Dispose();
        }

        private void AddNewMapping(float x, float y)
        {
            int num = this.mTapLocMappingIndex;
            if (num >= this.mMaxMappingsSupported)
            {
                MessageBox.Show("Only " + this.mMaxMappingsSupported + " new mappings are supported");
            }
            else
            {
                this.TapAtLocLabel[num] = new Label();
                this.TapAtLocLabel[num].AutoSize = true;
                this.TapAtLocLabel[num].Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
                this.TapAtLocLabel[num].Location = new Point(30, 15 + 20 * (num + 1) + 20 * num);
                this.TapAtLocLabel[num].Name = "TapAtLoc" + num + "Label";
                this.TapAtLocLabel[num].Size = new Size(110, 20);
                this.TapAtLocLabel[num].Text = "Tap at (" + x + "," + y + ")";
                this.TapAtLocLabel[num].Cursor = Cursors.Hand;
                this.TapAtLocLabel[num].Click += this.ChangeTapLocation;
                this.tapLocationTooltip.SetToolTip(this.TapAtLocLabel[num], "Click to change tap position");
                this.ConnectingArrowsPBox[num] = new PictureBox();
                this.ConnectingArrowsPBox[num].Location = new Point(this.TapAtLocLabel[num].Right + 30, this.TapAtLocLabel[num].Top);
                this.ConnectingArrowsPBox[num].Size = new Size(100, 20);
                Image image = Image.FromFile("C:\\arrows.png");
                this.ConnectingArrowsPBox[num].Visible = true;
                this.ConnectingArrowsPBox[num].Image = image;
                this.ConnectingArrowsPBox[num].BackgroundImage = image;
                this.ConnectingArrowsPBox[num].SizeMode = PictureBoxSizeMode.Zoom;
                this.TapAtLocKeyCombo[num] = new ComboBox();
                this.TapAtLocKeyCombo[num].DropDownStyle = ComboBoxStyle.DropDownList;
                this.TapAtLocKeyCombo[num].FormattingEnabled = true;
                this.TapAtLocKeyCombo[num].Location = new Point(289, 15 + 20 * (num + 1) + 20 * num);
                this.TapAtLocKeyCombo[num].Name = "TapAtLoc" + num + "KeyCombo";
                this.TapAtLocKeyCombo[num].Size = new Size(116, 20);
                this.TapAtLocKeyCombo[num].Items.AddRange(new object[43]
				{
					"Disabled",
					"1",
					"2",
					"3",
					"4",
					"5",
					"6",
					"7",
					"8",
					"9",
					"0",
					"A",
					"B",
					"C",
					"D",
					"E",
					"F",
					"G",
					"H",
					"I",
					"J",
					"K",
					"L",
					"M",
					"N",
					"O",
					"P",
					"Q",
					"R",
					"S",
					"T",
					"U",
					"V",
					"W",
					"X",
					"Y",
					"Z",
					"Space",
					"Enter",
					"Up-Arrow",
					"Down-Arrow",
					"Left-Arrow",
					"Right-Arrow"
				});
                this.TapAtLocKeyCombo[num].SelectedIndex = 0;
                this.groupBox3.Controls.Add(this.TapAtLocKeyCombo[num]);
                this.groupBox3.Controls.Add(this.ConnectingArrowsPBox[num]);
                this.groupBox3.Controls.Add(this.TapAtLocLabel[num]);
                this.mTapLocMappingIndex++;
            }
        }

        private void ChangeTapLocation(object sender, EventArgs e)
        {
            this.mIsModifyingMapping = true;
            this.AddNewMappingBtnClick(sender, e);
            Label label = sender as Label;
            label.Text = "Tap at (" + this.mTapX + "," + this.mTapY + ")";
        }

        private void SaveBtnClick(object sender, EventArgs e)
        {
            string path = InputMapperTool.sCurrentAppPackage + ".cfg";
            string path2 = Path.Combine(Strings.InputMapperFolder, path);
            string newTapMappings = this.GetNewTapMappings();
            string newTiltMappings = this.GetNewTiltMappings();
            string newSwipeMappings = this.GetNewSwipeMappings();
            StreamWriter streamWriter = new StreamWriter(path2);
            streamWriter.WriteLine(this.mKeysTemplate);
            streamWriter.WriteLine(newTapMappings);
            streamWriter.WriteLine(newTiltMappings);
            streamWriter.WriteLine(newSwipeMappings);
            streamWriter.WriteLine(this.mOpenSensorTemplate);
            streamWriter.Close();
            Console.s_Console.SetupInputMapper();
            MessageBox.Show("Keyboard mappings saved. Please restart the app for the changes to take effect");
            base.Dispose();
        }

        private string GetNewTapMappings()
        {
            string text = "\r\n";
            if (this.TapAtLocLabel == null)
            {
                return text;
            }
            int num = 0;
            for (num = 0; num < this.mMaxMappingsSupported; num++)
            {
                try
                {
                    if (this.TapAtLocLabel[num] != null && this.TapAtLocKeyCombo[num].SelectedItem.ToString() != "Disabled")
                    {
                        string arg = this.TapAtLocLabel[num].Text.Substring(this.TapAtLocLabel[num].Text.IndexOf('('));
                        text += this.TapAtLocKeyCombo[num].SelectedItem.ToString() + "\t= Tap " + arg + "\r\n";
                        num++;
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to add Tap mappings for index {0}. err: {1}", num, ex.ToString());
                    continue;
                }
                break;
            }
            return text.Replace("-Arrow", "");
        }

        private string GetNewTiltMappings()
        {
            string text = "\r\n";
            string text2 = "";
            string text3 = this.TiltLeftKeyCombo.SelectedItem.ToString();
            string text4 = this.TiltRightKeyCombo.SelectedItem.ToString();
            string text5 = this.TiltUpKeyCombo.SelectedItem.ToString();
            string text6 = this.TiltDownKeyCombo.SelectedItem.ToString();
            if (text3 != "Disabled")
            {
                text2 = this.LeftTiltLabel.Text.Substring(0, this.LeftTiltLabel.Text.IndexOf(' '));
                text += text3 + "\t= Tilt Absolute (0,-" + text2 + ") Return\r\n";
            }
            if (text4 != "Disabled")
            {
                text2 = this.RightTiltLabel.Text.Substring(0, this.RightTiltLabel.Text.IndexOf(' '));
                text += text4 + "\t= Tilt Absolute (0," + text2 + ") Return\r\n";
            }
            if (text5 != "Disabled")
            {
                text2 = this.UpTiltLabel.Text.Substring(0, this.UpTiltLabel.Text.IndexOf(' '));
                text += text5 + "\t= Tilt Absolute (-" + text2 + ",0) Return\r\n";
            }
            if (text6 != "Disabled")
            {
                text2 = this.DownTiltLabel.Text.Substring(0, this.DownTiltLabel.Text.IndexOf(' '));
                text += text6 + "\t= Tilt Absolute (" + text2 + ",0) Return\r\n";
            }
            return text.Replace("-Arrow", "");
        }

        private string GetNewSwipeMappings()
        {
            string text = "\r\n";
            string text2 = this.swipeLeftKeyCombo.SelectedItem.ToString();
            string text3 = this.swipeRightKeyCombo.SelectedItem.ToString();
            string text4 = this.swipeUpKeyCombo.SelectedItem.ToString();
            string text5 = this.swipeDownKeyCombo.SelectedItem.ToString();
            if (text2 != "Disabled")
            {
                text += text2 + "\t= Swipe Left\r\n";
            }
            if (text3 != "Disabled")
            {
                text += text3 + "\t= Swipe Right\r\n";
            }
            if (text4 != "Disabled")
            {
                text += text4 + "\t= Swipe Up\r\n";
            }
            if (text5 != "Disabled")
            {
                text += text5 + "\t= Swipe Down\r\n";
            }
            return text.Replace("-Arrow", "");
        }

        private void GetExistingMappings()
        {
            string path = InputMapperTool.sCurrentAppPackage + ".cfg";
            string path2 = Path.Combine(Strings.InputMapperFolder, path);
            File.Exists(path2);
        }
    }
}
