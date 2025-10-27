namespace VrcOscChatbox
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pnlTitleBar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.mainUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.animationTimer = new System.Windows.Forms.Timer(this.components);
            this.btnStartStop = new System.Windows.Forms.Button();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlMedia = new ModernPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tglYouTube = new ToggleSwitch();
            this.tglSpotify = new ToggleSwitch();
            this.pnlSystem = new ModernPanel();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.tglRamInfo = new ToggleSwitch();
            this.tglGpuInfo = new ToggleSwitch();
            this.tglCpuInfo = new ToggleSwitch();
            this.txtGpuFormat = new System.Windows.Forms.TextBox();
            this.txtCpuFormat = new System.Windows.Forms.TextBox();
            this.txtRamFormat = new System.Windows.Forms.TextBox();
            this.pnlAnimated = new ModernPanel();
            this.tglAnimatedText = new ToggleSwitch();
            this.txtAnimatedTexts = new System.Windows.Forms.TextBox();
            this.lblAnimatedTextPreview = new System.Windows.Forms.Label();
            this.pnlPersonalStatus = new ModernPanel();
            this.txtPersonalStatus = new System.Windows.Forms.TextBox();
            this.tglPersonalStatus = new ToggleSwitch();
            this.pnlTime = new ModernPanel();
            this.tglTime = new ToggleSwitch();
            this.pnlPreview = new ModernPanel();
            this.lblLivePreview = new System.Windows.Forms.Label();
            this.pnlLog = new ModernPanel();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.pnlAfk = new ModernPanel();
            this.tglAfk = new ToggleSwitch();
            this.hardwareUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlTitleBar.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlMedia.SuspendLayout();
            this.pnlSystem.SuspendLayout();
            this.pnlAnimated.SuspendLayout();
            this.pnlPersonalStatus.SuspendLayout();
            this.pnlTime.SuspendLayout();
            this.pnlPreview.SuspendLayout();
            this.pnlLog.SuspendLayout();
            this.pnlAfk.SuspendLayout();
            this.trayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTitleBar
            // 
            this.pnlTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.pnlTitleBar.Controls.Add(this.lblTitle);
            this.pnlTitleBar.Controls.Add(this.btnClose);
            this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Location = new System.Drawing.Point(0, 0);
            this.pnlTitleBar.Name = "pnlTitleBar";
            this.pnlTitleBar.Size = new System.Drawing.Size(484, 30);
            this.pnlTitleBar.TabIndex = 10;
            this.pnlTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseDown);
            this.pnlTitleBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseMove);
            this.pnlTitleBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseUp);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(12, 6);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(127, 17);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "VRChat OSC Pro C#";
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(444, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(40, 30);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // mainUpdateTimer
            // 
            this.mainUpdateTimer.Interval = 1000;
            this.mainUpdateTimer.Tick += new System.EventHandler(this.mainUpdateTimer_Tick);
            // 
            // animationTimer
            // 
            this.animationTimer.Interval = 150;
            this.animationTimer.Tick += new System.EventHandler(this.animationTimer_Tick);
            // 
            // btnStartStop
            // 
            this.btnStartStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.btnStartStop.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnStartStop.FlatAppearance.BorderSize = 0;
            this.btnStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartStop.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnStartStop.ForeColor = System.Drawing.Color.White;
            this.btnStartStop.Location = new System.Drawing.Point(0, 781);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(484, 50);
            this.btnStartStop.TabIndex = 11;
            this.btnStartStop.Text = "▶ Start";
            this.btnStartStop.UseVisualStyleBackColor = false;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.pnlMedia);
            this.pnlMain.Controls.Add(this.pnlSystem);
            this.pnlMain.Controls.Add(this.pnlAnimated);
            this.pnlMain.Controls.Add(this.pnlPersonalStatus);
            this.pnlMain.Controls.Add(this.pnlTime);
            this.pnlMain.Location = new System.Drawing.Point(0, 107);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(484, 521);
            this.pnlMain.TabIndex = 17;
            // 
            // pnlMedia
            // 
            this.pnlMedia.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlMedia.Controls.Add(this.label8);
            this.pnlMedia.Controls.Add(this.label7);
            this.pnlMedia.Controls.Add(this.tglYouTube);
            this.pnlMedia.Controls.Add(this.tglSpotify);
            this.pnlMedia.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlMedia.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlMedia.Location = new System.Drawing.Point(12, 12);
            this.pnlMedia.Name = "pnlMedia";
            this.pnlMedia.Padding = new System.Windows.Forms.Padding(1);
            this.pnlMedia.Size = new System.Drawing.Size(460, 95);
            this.pnlMedia.TabIndex = 12;
            this.pnlMedia.Title = "Media Integration";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(66, 62);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(117, 17);
            this.label8.TabIndex = 4;
            this.label8.Text = "YouTube Detection";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(66, 32);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(126, 17);
            this.label7.TabIndex = 3;
            this.label7.Text = "Spotify Desktop App";
            // 
            // tglYouTube
            // 
            this.tglYouTube.Location = new System.Drawing.Point(15, 60);
            this.tglYouTube.MinimumSize = new System.Drawing.Size(45, 22);
            this.tglYouTube.Name = "tglYouTube";
            this.tglYouTube.Size = new System.Drawing.Size(45, 22);
            this.tglYouTube.TabIndex = 1;
            this.tglYouTube.Tag = "YouTube";
            this.tglYouTube.UseVisualStyleBackColor = true;
            this.tglYouTube.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);
            // 
            // tglSpotify
            // 
            this.tglSpotify.Location = new System.Drawing.Point(15, 30);
            this.tglSpotify.MinimumSize = new System.Drawing.Size(45, 22);
            this.tglSpotify.Name = "tglSpotify";
            this.tglSpotify.Size = new System.Drawing.Size(45, 22);
            this.tglSpotify.TabIndex = 0;
            this.tglSpotify.Tag = "Spotify";
            this.tglSpotify.UseVisualStyleBackColor = true;
            this.tglSpotify.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);
            // 
            // pnlSystem
            // 
            this.pnlSystem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlSystem.Controls.Add(this.label13);
            this.pnlSystem.Controls.Add(this.label12);
            this.pnlSystem.Controls.Add(this.label11);
            this.pnlSystem.Controls.Add(this.tglRamInfo);
            this.pnlSystem.Controls.Add(this.tglGpuInfo);
            this.pnlSystem.Controls.Add(this.tglCpuInfo);
            this.pnlSystem.Controls.Add(this.txtGpuFormat);
            this.pnlSystem.Controls.Add(this.txtCpuFormat);
            this.pnlSystem.Controls.Add(this.txtRamFormat);
            this.pnlSystem.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlSystem.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlSystem.Location = new System.Drawing.Point(12, 234);
            this.pnlSystem.Name = "pnlSystem";
            this.pnlSystem.Padding = new System.Windows.Forms.Padding(1);
            this.pnlSystem.Size = new System.Drawing.Size(460, 120);
            this.pnlSystem.TabIndex = 13;
            this.pnlSystem.Title = "System Stats";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(66, 87);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(89, 17);
            this.label13.TabIndex = 10;
            this.label13.Text = "Show GPU Info";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(66, 59);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(91, 17);
            this.label12.TabIndex = 9;
            this.label12.Text = "Show RAM Info";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(66, 31);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(89, 17);
            this.label11.TabIndex = 8;
            this.label11.Text = "Show CPU Info";
            // 
            // tglRamInfo
            // 
            this.tglRamInfo.Location = new System.Drawing.Point(15, 57);
            this.tglRamInfo.MinimumSize = new System.Drawing.Size(45, 22);
            this.tglRamInfo.Name = "tglRamInfo";
            this.tglRamInfo.Size = new System.Drawing.Size(45, 22);
            this.tglRamInfo.TabIndex = 6;
            this.tglRamInfo.Tag = "RAM Info";
            this.tglRamInfo.UseVisualStyleBackColor = true;
            this.tglRamInfo.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);
            // 
            // tglGpuInfo
            // 
            this.tglGpuInfo.Location = new System.Drawing.Point(15, 85);
            this.tglGpuInfo.MinimumSize = new System.Drawing.Size(45, 22);
            this.tglGpuInfo.Name = "tglGpuInfo";
            this.tglGpuInfo.Size = new System.Drawing.Size(45, 22);
            this.tglGpuInfo.TabIndex = 7;
            this.tglGpuInfo.Tag = "GPU Info";
            this.tglGpuInfo.UseVisualStyleBackColor = true;
            this.tglGpuInfo.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);
            // 
            // tglCpuInfo
            // 
            this.tglCpuInfo.Location = new System.Drawing.Point(15, 29);
            this.tglCpuInfo.MinimumSize = new System.Drawing.Size(45, 22);
            this.tglCpuInfo.Name = "tglCpuInfo";
            this.tglCpuInfo.Size = new System.Drawing.Size(45, 22);
            this.tglCpuInfo.TabIndex = 5;
            this.tglCpuInfo.Tag = "CPU Info";
            this.tglCpuInfo.UseVisualStyleBackColor = true;
            this.tglCpuInfo.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);
            // 
            // txtGpuFormat
            // 
            this.txtGpuFormat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtGpuFormat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtGpuFormat.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtGpuFormat.ForeColor = System.Drawing.Color.White;
            this.txtGpuFormat.Location = new System.Drawing.Point(160, 85);
            this.txtGpuFormat.Name = "txtGpuFormat";
            this.txtGpuFormat.Size = new System.Drawing.Size(280, 22);
            this.txtGpuFormat.TabIndex = 3;
            this.txtGpuFormat.Text = "GPU: {NAME} @ {LOAD}% ({TEMP})";
            // 
            // txtCpuFormat
            // 
            this.txtCpuFormat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtCpuFormat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCpuFormat.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtCpuFormat.ForeColor = System.Drawing.Color.White;
            this.txtCpuFormat.Location = new System.Drawing.Point(160, 29);
            this.txtCpuFormat.Name = "txtCpuFormat";
            this.txtCpuFormat.Size = new System.Drawing.Size(280, 22);
            this.txtCpuFormat.TabIndex = 1;
            this.txtCpuFormat.Text = "CPU: {NAME} @ {LOAD}% ({TEMP})";
            // 
            // txtRamFormat
            // 
            this.txtRamFormat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtRamFormat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRamFormat.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtRamFormat.ForeColor = System.Drawing.Color.White;
            this.txtRamFormat.Location = new System.Drawing.Point(160, 57);
            this.txtRamFormat.Name = "txtRamFormat";
            this.txtRamFormat.Size = new System.Drawing.Size(280, 22);
            this.txtRamFormat.TabIndex = 2;
            this.txtRamFormat.Text = "RAM Used: {USED} GB / {TOTAL} GB";
            // 
            // pnlAnimated
            // 
            this.pnlAnimated.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlAnimated.Controls.Add(this.tglAnimatedText);
            this.pnlAnimated.Controls.Add(this.txtAnimatedTexts);
            this.pnlAnimated.Controls.Add(this.lblAnimatedTextPreview);
            this.pnlAnimated.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlAnimated.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlAnimated.Location = new System.Drawing.Point(12, 360);
            this.pnlAnimated.Name = "pnlAnimated";
            this.pnlAnimated.Padding = new System.Windows.Forms.Padding(1);
            this.pnlAnimated.Size = new System.Drawing.Size(460, 160);
            this.pnlAnimated.TabIndex = 14;
            this.pnlAnimated.Title = "Animated Text";
            // 
            // tglAnimatedText
            // 
            this.tglAnimatedText.Location = new System.Drawing.Point(15, 30);
            this.tglAnimatedText.MinimumSize = new System.Drawing.Size(45, 22);
            this.tglAnimatedText.Name = "tglAnimatedText";
            this.tglAnimatedText.Size = new System.Drawing.Size(45, 22);
            this.tglAnimatedText.TabIndex = 0;
            this.tglAnimatedText.Tag = "Animated Text";
            this.tglAnimatedText.UseVisualStyleBackColor = true;
            this.tglAnimatedText.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);
            // 
            // txtAnimatedTexts
            // 
            this.txtAnimatedTexts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtAnimatedTexts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAnimatedTexts.ForeColor = System.Drawing.Color.White;
            this.txtAnimatedTexts.Location = new System.Drawing.Point(15, 58);
            this.txtAnimatedTexts.Multiline = true;
            this.txtAnimatedTexts.Name = "txtAnimatedTexts";
            this.txtAnimatedTexts.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAnimatedTexts.Size = new System.Drawing.Size(425, 65);
            this.txtAnimatedTexts.TabIndex = 1;
            this.txtAnimatedTexts.Text = "VRChat OSC Pro C#\r\nby Veron";
            // 
            // lblAnimatedTextPreview
            // 
            this.lblAnimatedTextPreview.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblAnimatedTextPreview.Location = new System.Drawing.Point(12, 130);
            this.lblAnimatedTextPreview.Name = "lblAnimatedTextPreview";
            this.lblAnimatedTextPreview.Size = new System.Drawing.Size(428, 23);
            this.lblAnimatedTextPreview.TabIndex = 2;
            this.lblAnimatedTextPreview.Text = "Preview...";
            // 
            // pnlPersonalStatus
            // 
            this.pnlPersonalStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlPersonalStatus.Controls.Add(this.txtPersonalStatus);
            this.pnlPersonalStatus.Controls.Add(this.tglPersonalStatus);
            this.pnlPersonalStatus.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlPersonalStatus.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlPersonalStatus.Location = new System.Drawing.Point(12, 113);
            this.pnlPersonalStatus.Name = "pnlPersonalStatus";
            this.pnlPersonalStatus.Padding = new System.Windows.Forms.Padding(1);
            this.pnlPersonalStatus.Size = new System.Drawing.Size(460, 60);
            this.pnlPersonalStatus.TabIndex = 2;
            this.pnlPersonalStatus.Title = "Personal Status";
            // 
            // txtPersonalStatus
            // 
            this.txtPersonalStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtPersonalStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPersonalStatus.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPersonalStatus.ForeColor = System.Drawing.Color.White;
            this.txtPersonalStatus.Location = new System.Drawing.Point(155, 25);
            this.txtPersonalStatus.Name = "txtPersonalStatus";
            this.txtPersonalStatus.Size = new System.Drawing.Size(285, 23);
            this.txtPersonalStatus.TabIndex = 1;
            this.txtPersonalStatus.Text = "My Status";
            // 
            // tglPersonalStatus
            // 
            this.tglPersonalStatus.AutoSize = true;
            this.tglPersonalStatus.Location = new System.Drawing.Point(15, 25);
            this.tglPersonalStatus.MinimumSize = new System.Drawing.Size(45, 22);
            this.tglPersonalStatus.Name = "tglPersonalStatus";
            this.tglPersonalStatus.Size = new System.Drawing.Size(134, 22);
            this.tglPersonalStatus.TabIndex = 0;
            this.tglPersonalStatus.Tag = "Personal Status";
            this.tglPersonalStatus.Text = "Enable Custom Status";
            this.tglPersonalStatus.UseVisualStyleBackColor = true;
            this.tglPersonalStatus.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);
            // 
            // pnlTime
            // 
            this.pnlTime.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlTime.Controls.Add(this.tglTime);
            this.pnlTime.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlTime.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlTime.Location = new System.Drawing.Point(12, 179);
            this.pnlTime.Name = "pnlTime";
            this.pnlTime.Padding = new System.Windows.Forms.Padding(1);
            this.pnlTime.Size = new System.Drawing.Size(460, 60);
            this.pnlTime.TabIndex = 1;
            this.pnlTime.Title = "Current Time";
            // 
            // tglTime
            // 
            this.tglTime.AutoSize = true;
            this.tglTime.Location = new System.Drawing.Point(15, 25);
            this.tglTime.MinimumSize = new System.Drawing.Size(45, 22);
            this.tglTime.Name = "tglTime";
            this.tglTime.Size = new System.Drawing.Size(183, 22);
            this.tglTime.TabIndex = 0;
            this.tglTime.Tag = "Time";
            this.tglTime.Text = "Show Current Time (HH:mm)";
            this.tglTime.UseVisualStyleBackColor = true;
            this.tglTime.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);
            // 
            // pnlPreview
            // 
            this.pnlPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlPreview.Controls.Add(this.lblLivePreview);
            this.pnlPreview.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlPreview.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlPreview.Location = new System.Drawing.Point(12, 526);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Padding = new System.Windows.Forms.Padding(1);
            this.pnlPreview.Size = new System.Drawing.Size(460, 125);
            this.pnlPreview.TabIndex = 15;
            this.pnlPreview.Title = "Live Preview";
            // 
            // pnlLog
            // 
            this.pnlLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlLog.Controls.Add(this.rtbLog);
            this.pnlLog.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlLog.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlLog.Location = new System.Drawing.Point(12, 657);
            this.pnlLog.Name = "pnlLog";
            this.pnlLog.Padding = new System.Windows.Forms.Padding(1);
            this.pnlLog.Size = new System.Drawing.Size(460, 118);
            this.pnlLog.TabIndex = 16;
            this.pnlLog.Title = "System Log";
            // 
            // pnlAfk
            // 
            this.pnlAfk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlAfk.Controls.Add(this.tglAfk);
            this.pnlAfk.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlAfk.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlAfk.Location = new System.Drawing.Point(12, 45);
            this.pnlAfk.Name = "pnlAfk";
            this.pnlAfk.Padding = new System.Windows.Forms.Padding(1);
            this.pnlAfk.Size = new System.Drawing.Size(460, 60);
            this.pnlAfk.TabIndex = 0;
            this.pnlAfk.Title = "AFK Mode";
            // 
            // tglAfk
            // 
            this.tglAfk.AutoSize = true;
            this.tglAfk.Location = new System.Drawing.Point(15, 25);
            this.tglAfk.MinimumSize = new System.Drawing.Size(45, 22);
            this.tglAfk.Name = "tglAfk";
            this.tglAfk.Size = new System.Drawing.Size(123, 22);
            this.tglAfk.TabIndex = 0;
            this.tglAfk.Tag = "AFK";
            this.tglAfk.Text = "Enable AFK Mode";
            this.tglAfk.UseVisualStyleBackColor = true;
            this.tglAfk.CheckedChanged += new System.EventHandler(this.tglAfk_CheckedChanged);
            // 
            // hardwareUpdateTimer
            // 
            this.hardwareUpdateTimer.Interval = 2000;
            this.hardwareUpdateTimer.Tick += new System.EventHandler(this.hardwareUpdateTimer_Tick);
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.trayMenu;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "VRChat OSC Pro C#";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.showToolStripMenuItem, this.exitToolStripMenuItem });
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(104, 48);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.showToolStripMenuItem.Text = "Show";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.showToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(484, 831);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlLog);
            this.Controls.Add(this.pnlPreview);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.pnlTitleBar);
            this.Controls.Add(this.pnlAfk);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VRChat OSC Pro C#";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlTitleBar.ResumeLayout(false);
            this.pnlTitleBar.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.pnlMedia.ResumeLayout(false);
            this.pnlMedia.PerformLayout();
            this.pnlSystem.ResumeLayout(false);
            this.pnlSystem.PerformLayout();
            this.pnlAnimated.ResumeLayout(false);
            this.pnlAnimated.PerformLayout();
            this.pnlPersonalStatus.ResumeLayout(false);
            this.pnlPersonalStatus.PerformLayout();
            this.pnlTime.ResumeLayout(false);
            this.pnlTime.PerformLayout();
            this.pnlPreview.ResumeLayout(false);
            this.pnlLog.ResumeLayout(false);
            this.pnlAfk.ResumeLayout(false);
            this.pnlAfk.PerformLayout();
            this.trayMenu.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Timer mainUpdateTimer;
        private System.Windows.Forms.Timer animationTimer;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.Label lblLivePreview;
        private ToggleSwitch tglSpotify;
        private ToggleSwitch tglYouTube;
        private ToggleSwitch tglCpuInfo;
        private System.Windows.Forms.TextBox txtCpuFormat;
        private System.Windows.Forms.TextBox txtRamFormat;
        private System.Windows.Forms.TextBox txtGpuFormat;
        private ToggleSwitch tglAnimatedText;
        private System.Windows.Forms.TextBox txtAnimatedTexts;
        private System.Windows.Forms.Label lblAnimatedTextPreview;
        private ModernPanel pnlMedia;
        private ModernPanel pnlSystem;
        private ModernPanel pnlAnimated;
        private ModernPanel pnlPreview;
        private ModernPanel pnlLog;
        private ToggleSwitch tglRamInfo;
        private ToggleSwitch tglGpuInfo;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Timer hardwareUpdateTimer;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Panel pnlMain;
        private ModernPanel pnlPersonalStatus;
        private System.Windows.Forms.TextBox txtPersonalStatus;
        private ToggleSwitch tglPersonalStatus;
        private ModernPanel pnlTime;
        private ToggleSwitch tglTime;
        private ModernPanel pnlAfk;
        private ToggleSwitch tglAfk;
    }
}