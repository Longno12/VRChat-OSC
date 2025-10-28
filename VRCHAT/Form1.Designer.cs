using System;
using System.Drawing;
using System.Windows.Forms;

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
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.btnNavAdvanced = new NavButton();
            this.btnNavAppearance = new NavButton();
            this.btnNavSystem = new NavButton();
            this.btnNavMedia = new NavButton();
            this.btnNavDashboard = new NavButton();
            this.lblAppName = new System.Windows.Forms.Label();
            this.pnlTitleBar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.mainUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.animationTimer = new System.Windows.Forms.Timer(this.components);
            this.pnlMainContent = new System.Windows.Forms.Panel();
            this.pnlDashboard = new System.Windows.Forms.Panel();
            this.pnlLog = new ModernPanel();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.pnlPreview = new ModernPanel();
            this.lblLivePreview = new System.Windows.Forms.Label();
            this.pnlAfk = new ModernPanel();
            this.tglAfk = new ToggleSwitch();
            this.pnlTime = new ModernPanel();
            this.tglTime = new ToggleSwitch();
            this.pnlPersonalStatus = new ModernPanel();
            this.txtPersonalStatus = new System.Windows.Forms.TextBox();
            this.tglPersonalStatus = new ToggleSwitch();
            this.pnlSystem = new System.Windows.Forms.Panel();
            this.pnlSystem_Hardware = new ModernPanel();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.txtCpuFormat = new System.Windows.Forms.TextBox();
            this.txtRamFormat = new System.Windows.Forms.TextBox();
            this.txtGpuFormat = new System.Windows.Forms.TextBox();
            this.tglCpuInfo = new ToggleSwitch();
            this.tglRamInfo = new ToggleSwitch();
            this.tglGpuInfo = new ToggleSwitch();
            this.pnlMedia = new System.Windows.Forms.Panel();
            this.pnlMedia_Main = new ModernPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tglSpotify = new ToggleSwitch();
            this.tglYouTube = new ToggleSwitch();
            this.pnlAppearance = new System.Windows.Forms.Panel();
            this.pnlAppearance_Presets = new ModernPanel();
            this.cmbPresets = new System.Windows.Forms.ComboBox();
            this.btnSavePreset = new System.Windows.Forms.Button();
            this.btnDeletePreset = new System.Windows.Forms.Button();
            this.pnlAppearance_AnimatedText = new ModernPanel();
            this.tglAnimatedText = new ToggleSwitch();
            this.txtAnimatedTexts = new System.Windows.Forms.TextBox();
            this.lblAnimatedTextPreview = new System.Windows.Forms.Label();
            this.pnlAdvanced = new System.Windows.Forms.Panel();
            this.pnlAdvanced_VR = new ModernPanel();
            this.lblPlayspaceY = new System.Windows.Forms.Label();
            this.lblPlayspaceX = new System.Windows.Forms.Label();
            this.tglPlayspace = new ToggleSwitch();
            this.tglAutoAfk = new ToggleSwitch();
            this.pnlAdvanced_Misc = new ModernPanel();
            this.chkAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.pnlAdvanced_Countdown = new ModernPanel();
            this.tglCountdown = new ToggleSwitch();
            this.txtCountdownFinished = new System.Windows.Forms.TextBox();
            this.dtpCountdown = new System.Windows.Forms.DateTimePicker();
            this.pnlAdvanced_Shutdown = new ModernPanel();
            this.btnCancelShutdown = new System.Windows.Forms.Button();
            this.btnShutdown = new System.Windows.Forms.Button();
            this.numShutdown = new System.Windows.Forms.NumericUpDown();
            this.hardwareUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlSidebar.SuspendLayout();
            this.pnlTitleBar.SuspendLayout();
            this.pnlMainContent.SuspendLayout();
            this.pnlDashboard.SuspendLayout();
            this.pnlLog.SuspendLayout();
            this.pnlPreview.SuspendLayout();
            this.pnlAfk.SuspendLayout();
            this.pnlTime.SuspendLayout();
            this.pnlPersonalStatus.SuspendLayout();
            this.pnlSystem.SuspendLayout();
            this.pnlSystem_Hardware.SuspendLayout();
            this.pnlMedia.SuspendLayout();
            this.pnlMedia_Main.SuspendLayout();
            this.pnlAppearance.SuspendLayout();
            this.pnlAppearance_Presets.SuspendLayout();
            this.pnlAppearance_AnimatedText.SuspendLayout();
            this.pnlAdvanced.SuspendLayout();
            this.pnlAdvanced_VR.SuspendLayout();
            this.pnlAdvanced_Misc.SuspendLayout();
            this.pnlAdvanced_Countdown.SuspendLayout();
            this.pnlAdvanced_Shutdown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numShutdown)).BeginInit();
            this.trayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlSidebar.Controls.Add(this.btnStartStop);
            this.pnlSidebar.Controls.Add(this.btnNavAdvanced);
            this.pnlSidebar.Controls.Add(this.btnNavAppearance);
            this.pnlSidebar.Controls.Add(this.btnNavSystem);
            this.pnlSidebar.Controls.Add(this.btnNavMedia);
            this.pnlSidebar.Controls.Add(this.btnNavDashboard);
            this.pnlSidebar.Controls.Add(this.lblAppName);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 30);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(200, 570);
            this.pnlSidebar.TabIndex = 0;
            // 
            // btnStartStop
            // 
            this.btnStartStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.btnStartStop.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnStartStop.FlatAppearance.BorderSize = 0;
            this.btnStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartStop.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnStartStop.ForeColor = System.Drawing.Color.White;
            this.btnStartStop.Location = new System.Drawing.Point(0, 520);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(200, 50);
            this.btnStartStop.TabIndex = 2;
            this.btnStartStop.Text = "▶ Start";
            this.btnStartStop.UseVisualStyleBackColor = false;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // btnNavAdvanced
            // 
            this.btnNavAdvanced.FlatAppearance.BorderSize = 0;
            this.btnNavAdvanced.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
            this.btnNavAdvanced.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnNavAdvanced.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavAdvanced.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            this.btnNavAdvanced.ForeColor = System.Drawing.Color.Gainsboro;
            this.btnNavAdvanced.Icon = '';
            this.btnNavAdvanced.IsActive = false;
            this.btnNavAdvanced.Location = new System.Drawing.Point(0, 240);
            this.btnNavAdvanced.Name = "btnNavAdvanced";
            this.btnNavAdvanced.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnNavAdvanced.Size = new System.Drawing.Size(200, 40);
            this.btnNavAdvanced.TabIndex = 7;
            this.btnNavAdvanced.Text = "Advanced";
            this.btnNavAdvanced.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavAdvanced.UseVisualStyleBackColor = true;
            this.btnNavAdvanced.Click += new System.EventHandler(this.btnNavAdvanced_Click);
            // 
            // btnNavAppearance
            // 
            this.btnNavAppearance.FlatAppearance.BorderSize = 0;
            this.btnNavAppearance.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
            this.btnNavAppearance.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnNavAppearance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavAppearance.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            this.btnNavAppearance.ForeColor = System.Drawing.Color.Gainsboro;
            this.btnNavAppearance.Icon = '';
            this.btnNavAppearance.IsActive = false;
            this.btnNavAppearance.Location = new System.Drawing.Point(0, 200);
            this.btnNavAppearance.Name = "btnNavAppearance";
            this.btnNavAppearance.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnNavAppearance.Size = new System.Drawing.Size(200, 40);
            this.btnNavAppearance.TabIndex = 6;
            this.btnNavAppearance.Text = "Appearance";
            this.btnNavAppearance.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavAppearance.UseVisualStyleBackColor = true;
            this.btnNavAppearance.Click += new System.EventHandler(this.btnNavAppearance_Click);
            // 
            // btnNavSystem
            // 
            this.btnNavSystem.FlatAppearance.BorderSize = 0;
            this.btnNavSystem.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
            this.btnNavSystem.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnNavSystem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavSystem.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            this.btnNavSystem.ForeColor = System.Drawing.Color.Gainsboro;
            this.btnNavSystem.Icon = '';
            this.btnNavSystem.IsActive = false;
            this.btnNavSystem.Location = new System.Drawing.Point(0, 160);
            this.btnNavSystem.Name = "btnNavSystem";
            this.btnNavSystem.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnNavSystem.Size = new System.Drawing.Size(200, 40);
            this.btnNavSystem.TabIndex = 5;
            this.btnNavSystem.Text = "System";
            this.btnNavSystem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavSystem.UseVisualStyleBackColor = true;
            this.btnNavSystem.Click += new System.EventHandler(this.btnNavSystem_Click);
            // 
            // btnNavMedia
            // 
            this.btnNavMedia.FlatAppearance.BorderSize = 0;
            this.btnNavMedia.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
            this.btnNavMedia.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnNavMedia.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavMedia.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            this.btnNavMedia.ForeColor = System.Drawing.Color.Gainsboro;
            this.btnNavMedia.Icon = '';
            this.btnNavMedia.IsActive = false;
            this.btnNavMedia.Location = new System.Drawing.Point(0, 120);
            this.btnNavMedia.Name = "btnNavMedia";
            this.btnNavMedia.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnNavMedia.Size = new System.Drawing.Size(200, 40);
            this.btnNavMedia.TabIndex = 4;
            this.btnNavMedia.Text = "Media";
            this.btnNavMedia.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavMedia.UseVisualStyleBackColor = true;
            this.btnNavMedia.Click += new System.EventHandler(this.btnNavMedia_Click);
            // 
            // btnNavDashboard
            // 
            this.btnNavDashboard.FlatAppearance.BorderSize = 0;
            this.btnNavDashboard.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
            this.btnNavDashboard.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnNavDashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavDashboard.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            this.btnNavDashboard.ForeColor = System.Drawing.Color.Gainsboro;
            this.btnNavDashboard.Icon = '';
            this.btnNavDashboard.IsActive = false;
            this.btnNavDashboard.Location = new System.Drawing.Point(0, 80);
            this.btnNavDashboard.Name = "btnNavDashboard";
            this.btnNavDashboard.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnNavDashboard.Size = new System.Drawing.Size(200, 40);
            this.btnNavDashboard.TabIndex = 1;
            this.btnNavDashboard.Text = "Dashboard";
            this.btnNavDashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavDashboard.UseVisualStyleBackColor = true;
            this.btnNavDashboard.Click += new System.EventHandler(this.btnNavDashboard_Click);
            // 
            // lblAppName
            // 
            this.lblAppName.AutoSize = true;
            this.lblAppName.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.lblAppName.ForeColor = System.Drawing.Color.White;
            this.lblAppName.Location = new System.Drawing.Point(12, 16);
            this.lblAppName.Name = "lblAppName";
            this.lblAppName.Size = new System.Drawing.Size(170, 30);
            this.lblAppName.TabIndex = 0;
            this.lblAppName.Text = "VRChat OSC Pro";
            // 
            // pnlTitleBar
            // 
            this.pnlTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.pnlTitleBar.Controls.Add(this.lblTitle);
            this.pnlTitleBar.Controls.Add(this.btnClose);
            this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Location = new System.Drawing.Point(0, 0);
            this.pnlTitleBar.Name = "pnlTitleBar";
            this.pnlTitleBar.Size = new System.Drawing.Size(950, 30);
            this.pnlTitleBar.TabIndex = 1;
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
            this.btnClose.Location = new System.Drawing.Point(910, 0);
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
            // pnlMainContent
            // 
            this.pnlMainContent.Controls.Add(this.pnlSystem);
            this.pnlMainContent.Controls.Add(this.pnlMedia);
            this.pnlMainContent.Controls.Add(this.pnlAppearance);
            this.pnlMainContent.Controls.Add(this.pnlAdvanced);
            this.pnlMainContent.Controls.Add(this.pnlDashboard);
            this.pnlMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContent.Location = new System.Drawing.Point(200, 30);
            this.pnlMainContent.Name = "pnlMainContent";
            this.pnlMainContent.Size = new System.Drawing.Size(750, 570);
            this.pnlMainContent.TabIndex = 2;
            // 
            // pnlDashboard
            // 
            this.pnlDashboard.Controls.Add(this.pnlLog);
            this.pnlDashboard.Controls.Add(this.pnlPreview);
            this.pnlDashboard.Controls.Add(this.pnlAfk);
            this.pnlDashboard.Controls.Add(this.pnlTime);
            this.pnlDashboard.Controls.Add(this.pnlPersonalStatus);
            this.pnlDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashboard.Location = new System.Drawing.Point(0, 0);
            this.pnlDashboard.Name = "pnlDashboard";
            this.pnlDashboard.Size = new System.Drawing.Size(750, 570);
            this.pnlDashboard.TabIndex = 1;
            // 
            // pnlLog
            // 
            this.pnlLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlLog.Controls.Add(this.rtbLog);
            this.pnlLog.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlLog.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlLog.Location = new System.Drawing.Point(20, 414);
            this.pnlLog.Name = "pnlLog";
            this.pnlLog.Padding = new System.Windows.Forms.Padding(1);
            this.pnlLog.Size = new System.Drawing.Size(710, 136);
            this.pnlLog.TabIndex = 18;
            this.pnlLog.Title = "System Log";
            // 
            // rtbLog
            // 
            this.rtbLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.rtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.rtbLog.ForeColor = System.Drawing.Color.White;
            this.rtbLog.Location = new System.Drawing.Point(6, 25);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbLog.Size = new System.Drawing.Size(698, 100);
            this.rtbLog.TabIndex = 0;
            this.rtbLog.Text = "";
            // 
            // pnlPreview
            // 
            this.pnlPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlPreview.Controls.Add(this.lblLivePreview);
            this.pnlPreview.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlPreview.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlPreview.Location = new System.Drawing.Point(20, 283);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Padding = new System.Windows.Forms.Padding(1);
            this.pnlPreview.Size = new System.Drawing.Size(710, 125);
            this.pnlPreview.TabIndex = 17;
            this.pnlPreview.Title = "Live Preview";
            // 
            // lblLivePreview
            // 
            this.lblLivePreview.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.lblLivePreview.Location = new System.Drawing.Point(6, 25);
            this.lblLivePreview.Name = "lblLivePreview";
            this.lblLivePreview.Size = new System.Drawing.Size(698, 96);
            this.lblLivePreview.TabIndex = 0;
            this.lblLivePreview.Text = "Click Start to begin...";
            // 
            // pnlAfk
            // 
            this.pnlAfk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlAfk.Controls.Add(this.tglAfk);
            this.pnlAfk.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlAfk.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlAfk.Location = new System.Drawing.Point(20, 20);
            this.pnlAfk.Name = "pnlAfk";
            this.pnlAfk.Padding = new System.Windows.Forms.Padding(1);
            this.pnlAfk.Size = new System.Drawing.Size(710, 60);
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
            // pnlTime
            // 
            this.pnlTime.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlTime.Controls.Add(this.tglTime);
            this.pnlTime.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlTime.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlTime.Location = new System.Drawing.Point(20, 152);
            this.pnlTime.Name = "pnlTime";
            this.pnlTime.Padding = new System.Windows.Forms.Padding(1);
            this.pnlTime.Size = new System.Drawing.Size(710, 60);
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
            // pnlPersonalStatus
            // 
            this.pnlPersonalStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlPersonalStatus.Controls.Add(this.txtPersonalStatus);
            this.pnlPersonalStatus.Controls.Add(this.tglPersonalStatus);
            this.pnlPersonalStatus.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlPersonalStatus.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlPersonalStatus.Location = new System.Drawing.Point(20, 86);
            this.pnlPersonalStatus.Name = "pnlPersonalStatus";
            this.pnlPersonalStatus.Padding = new System.Windows.Forms.Padding(1);
            this.pnlPersonalStatus.Size = new System.Drawing.Size(710, 60);
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
            this.txtPersonalStatus.Size = new System.Drawing.Size(540, 23);
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
            // pnlSystem
            // 
            this.pnlSystem.AutoScroll = true;
            this.pnlSystem.Controls.Add(this.pnlSystem_Hardware);
            this.pnlSystem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSystem.Location = new System.Drawing.Point(0, 0);
            this.pnlSystem.Name = "pnlSystem";
            this.pnlSystem.Size = new System.Drawing.Size(750, 570);
            this.pnlSystem.TabIndex = 4;
            // 
            // pnlMedia
            // 
            this.pnlMedia.AutoScroll = true;
            this.pnlMedia.Controls.Add(this.pnlMedia_Main);
            this.pnlMedia.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMedia.Location = new System.Drawing.Point(0, 0);
            this.pnlMedia.Name = "pnlMedia";
            this.pnlMedia.Size = new System.Drawing.Size(750, 570);
            this.pnlMedia.TabIndex = 3;
            // 
            // pnlAppearance
            // 
            this.pnlAppearance.AutoScroll = true;
            this.pnlAppearance.Controls.Add(this.pnlAppearance_Presets);
            this.pnlAppearance.Controls.Add(this.pnlAppearance_AnimatedText);
            this.pnlAppearance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAppearance.Location = new System.Drawing.Point(0, 0);
            this.pnlAppearance.Name = "pnlAppearance";
            this.pnlAppearance.Size = new System.Drawing.Size(750, 570);
            this.pnlAppearance.TabIndex = 5;
            // 
            // pnlAdvanced
            // 
            this.pnlAdvanced.AutoScroll = true;
            this.pnlAdvanced.Controls.Add(this.pnlAdvanced_VR);
            this.pnlAdvanced.Controls.Add(this.pnlAdvanced_Misc);
            this.pnlAdvanced.Controls.Add(this.pnlAdvanced_Countdown);
            this.pnlAdvanced.Controls.Add(this.pnlAdvanced_Shutdown);
            this.pnlAdvanced.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAdvanced.Location = new System.Drawing.Point(0, 0);
            this.pnlAdvanced.Name = "pnlAdvanced";
            this.pnlAdvanced.Size = new System.Drawing.Size(750, 570);
            this.pnlAdvanced.TabIndex = 8;
            // 
            // pnlMedia_Main
            // 
            this.pnlMedia_Main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlMedia_Main.Controls.Add(this.label8);
            this.pnlMedia_Main.Controls.Add(this.label7);
            this.pnlMedia_Main.Controls.Add(this.tglYouTube);
            this.pnlMedia_Main.Controls.Add(this.tglSpotify);
            this.pnlMedia_Main.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlMedia_Main.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlMedia_Main.Location = new System.Drawing.Point(20, 20);
            this.pnlMedia_Main.Name = "pnlMedia_Main";
            this.pnlMedia_Main.Padding = new System.Windows.Forms.Padding(1);
            this.pnlMedia_Main.Size = new System.Drawing.Size(710, 100);
            this.pnlMedia_Main.TabIndex = 13;
            this.pnlMedia_Main.Title = "Media Integration";
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
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(66, 62);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(117, 17);
            this.label8.TabIndex = 4;
            this.label8.Text = "YouTube Detection";
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
            // pnlSystem_Hardware
            // 
            this.pnlSystem_Hardware.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlSystem_Hardware.Controls.Add(this.label13);
            this.pnlSystem_Hardware.Controls.Add(this.label12);
            this.pnlSystem_Hardware.Controls.Add(this.label11);
            this.pnlSystem_Hardware.Controls.Add(this.tglRamInfo);
            this.pnlSystem_Hardware.Controls.Add(this.tglGpuInfo);
            this.pnlSystem_Hardware.Controls.Add(this.tglCpuInfo);
            this.pnlSystem_Hardware.Controls.Add(this.txtGpuFormat);
            this.pnlSystem_Hardware.Controls.Add(this.txtCpuFormat);
            this.pnlSystem_Hardware.Controls.Add(this.txtRamFormat);
            this.pnlSystem_Hardware.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.pnlSystem_Hardware.ForeColor = System.Drawing.Color.Gainsboro;
            this.pnlSystem_Hardware.Location = new System.Drawing.Point(20, 20);
            this.pnlSystem_Hardware.Name = "pnlSystem_Hardware";
            this.pnlSystem_Hardware.Padding = new System.Windows.Forms.Padding(1);
            this.pnlSystem_Hardware.Size = new System.Drawing.Size(710, 120);
            this.pnlSystem_Hardware.TabIndex = 14;
            this.pnlSystem_Hardware.Title = "System Stats";
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
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(66, 59);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(91, 17);
            this.label12.TabIndex = 9;
            this.label12.Text = "Show RAM Info";
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
            // txtCpuFormat
            // 
            this.txtCpuFormat.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.txtCpuFormat.ForeColor = Color.White;
            this.txtCpuFormat.Location = new System.Drawing.Point(160, 29);
            this.txtCpuFormat.Size = new System.Drawing.Size(530, 25);
            this.txtCpuFormat.Text = "CPU: {NAME} @ {LOAD}% ({TEMP})";
            // 
            // txtRamFormat
            // 
            this.txtRamFormat.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.txtRamFormat.ForeColor = Color.White;
            this.txtRamFormat.Location = new System.Drawing.Point(160, 57);
            this.txtRamFormat.Size = new System.Drawing.Size(530, 25);
            this.txtRamFormat.Text = "RAM Used: {USED} GB / {TOTAL} GB";
            // 
            // txtGpuFormat
            // 
            this.txtGpuFormat.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.txtGpuFormat.ForeColor = Color.White;
            this.txtGpuFormat.Location = new System.Drawing.Point(160, 85);
            this.txtGpuFormat.Size = new System.Drawing.Size(530, 25);
            this.txtGpuFormat.Text = "GPU: {NAME} @ {LOAD}% ({TEMP})";
            // 
            // tglCpuInfo
            // 
            this.tglCpuInfo.Location = new System.Drawing.Point(15, 29); this.tglCpuInfo.Tag = "CPU Info"; this.tglCpuInfo.CheckedChanged += new EventHandler(AnyToggle_CheckedChanged);
            // 
            // tglRamInfo
            // 
            this.tglRamInfo.Location = new System.Drawing.Point(15, 57); this.tglRamInfo.Tag = "RAM Info"; this.tglRamInfo.CheckedChanged += new EventHandler(AnyToggle_CheckedChanged);
            // 
            // tglGpuInfo
            // 
            this.tglGpuInfo.Location = new System.Drawing.Point(15, 85); this.tglGpuInfo.Tag = "GPU Info"; this.tglGpuInfo.CheckedChanged += new EventHandler(AnyToggle_CheckedChanged);
            // 
            // pnlAppearance_Presets
            // 
            this.pnlAppearance_Presets.Location = new System.Drawing.Point(20, 20); this.pnlAppearance_Presets.Size = new System.Drawing.Size(710, 70); this.pnlAppearance_Presets.Title = "Presets";
            // 
            // cmbPresets
            // 
            this.cmbPresets.Location = new System.Drawing.Point(15, 30);
            this.cmbPresets.Size = new System.Drawing.Size(200, 25);
            this.cmbPresets.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbPresets.SelectedIndexChanged += new EventHandler(cmbPresets_SelectedIndexChanged);
            // 
            // btnSavePreset
            // 
            this.btnSavePreset.Location = new System.Drawing.Point(225, 30);
            this.btnSavePreset.Size = new System.Drawing.Size(100, 25);
            this.btnSavePreset.Text = "Save Current";
            this.btnSavePreset.Click += new EventHandler(btnSavePreset_Click);
            // 
            // btnDeletePreset
            // 
            this.btnDeletePreset.Location = new System.Drawing.Point(335, 30);
            this.btnDeletePreset.Size = new System.Drawing.Size(100, 25);
            this.btnDeletePreset.Text = "Delete";
            this.btnDeletePreset.Click += new EventHandler(btnDeletePreset_Click);
            // 
            // pnlAppearance_AnimatedText
            // 
            this.pnlAppearance_AnimatedText.Location = new System.Drawing.Point(20, 100);
            this.pnlAppearance_AnimatedText.Size = new System.Drawing.Size(710, 160);
            this.pnlAppearance_AnimatedText.Title = "Animated Text";
            // 
            // tglAnimatedText
            // 
            this.tglAnimatedText.Location = new System.Drawing.Point(15, 30); this.tglAnimatedText.Tag = "Animated Text"; this.tglAnimatedText.CheckedChanged += new EventHandler(AnyToggle_CheckedChanged);
            // 
            // txtAnimatedTexts
            // 
            this.txtAnimatedTexts.Location = new System.Drawing.Point(15, 58); this.txtAnimatedTexts.Size = new System.Drawing.Size(680, 65); this.txtAnimatedTexts.Multiline = true; this.txtAnimatedTexts.ScrollBars = ScrollBars.Vertical; this.txtAnimatedTexts.BackColor = System.Drawing.Color.FromArgb(30, 30, 30); this.txtAnimatedTexts.ForeColor = Color.White;
            // 
            // lblAnimatedTextPreview
            // 
            this.lblAnimatedTextPreview.Location = new System.Drawing.Point(12, 130);
            // 
            // pnlAdvanced_Shutdown
            // 
            this.pnlAdvanced_Shutdown.Location = new System.Drawing.Point(20, 20); this.pnlAdvanced_Shutdown.Size = new System.Drawing.Size(710, 70); this.pnlAdvanced_Shutdown.Title = "PC Shutdown / Sleep Timer";
            // 
            // numShutdown
            // 
            this.numShutdown.Location = new System.Drawing.Point(15, 30);
            this.numShutdown.Maximum = 1440;
            this.numShutdown.Minimum = 1;
            // 
            // btnShutdown
            // 
            this.btnShutdown.Location = new System.Drawing.Point(125, 30);
            this.btnShutdown.Text = "Shutdown in (minutes)";
            this.btnShutdown.Click += new EventHandler(btnShutdown_Click);
            // 
            // btnCancelShutdown
            // 
            this.btnCancelShutdown.Location = new System.Drawing.Point(235, 30);
            this.btnCancelShutdown.Text = "Cancel Shutdown";
            this.btnCancelShutdown.Click += new EventHandler(btnCancelShutdown_Click);
            // 
            // pnlAdvanced_Countdown
            // 
            this.pnlAdvanced_Countdown.Location = new System.Drawing.Point(20, 100);
            this.pnlAdvanced_Countdown.Size = new System.Drawing.Size(710, 100);
            this.pnlAdvanced_Countdown.Title = "Countdown Timer";
            // 
            // tglCountdown
            // 
            this.tglCountdown.Location = new System.Drawing.Point(15, 30);
            this.tglCountdown.CheckedChanged += new EventHandler(AnyToggle_CheckedChanged);
            // 
            // dtpCountdown
            // 
            this.dtpCountdown.Location = new System.Drawing.Point(65, 30);
            this.dtpCountdown.Format = DateTimePickerFormat.Custom;
            this.dtpCountdown.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            // 
            // txtCountdownFinished
            // 
            this.txtCountdownFinished.Location = new System.Drawing.Point(65, 60);
            this.txtCountdownFinished.Text = "Countdown Finished!"; this.txtCountdownFinished.BackColor = System.Drawing.Color.FromArgb(30, 30, 30); this.txtCountdownFinished.ForeColor = Color.White;
            // 
            // pnlAdvanced_Misc
            // 
            this.pnlAdvanced_Misc.Location = new System.Drawing.Point(20, 210);
            this.pnlAdvanced_Misc.Size = new System.Drawing.Size(710, 70);
            this.pnlAdvanced_Misc.Title = "Misc";
            // 
            // chkAlwaysOnTop
            // 
            this.chkAlwaysOnTop.Location = new System.Drawing.Point(15, 30);
            this.chkAlwaysOnTop.Text = "Always on Top";
            this.chkAlwaysOnTop.AutoSize = true;
            this.chkAlwaysOnTop.CheckedChanged += new EventHandler(chkAlwaysOnTop_CheckedChanged);
            // 
            // pnlAdvanced_VR
            // 
            this.pnlAdvanced_VR.Location = new System.Drawing.Point(20, 290);
            this.pnlAdvanced_VR.Size = new System.Drawing.Size(710, 100);
            this.pnlAdvanced_VR.Title = "VR Integration (Not Implemented)";
            // 
            // tglAutoAfk
            // 
            this.tglAutoAfk.Location = new System.Drawing.Point(15, 30);
            this.tglAutoAfk.Text = "Enable Auto-AFK detection from VRChat";
            this.tglAutoAfk.AutoSize = true;
            this.tglAutoAfk.Enabled = false;
            // 
            // tglPlayspace
            // 
            this.tglPlayspace.Location = new System.Drawing.Point(15, 60);
            this.tglPlayspace.Text = "Show Playspace Coordinates";
            this.tglPlayspace.AutoSize = true;
            this.tglPlayspace.Enabled = false;
            // 
            // lblPlayspaceX
            // 
            this.lblPlayspaceX.Location = new System.Drawing.Point(250, 60);
            this.lblPlayspaceX.Text = "X: 0.00"; this.lblPlayspaceX.AutoSize = true;
            // 
            // lblPlayspaceY
            // 
            this.lblPlayspaceY.Location = new System.Drawing.Point(350, 60);
            this.lblPlayspaceY.Text = "Y: 0.00"; this.lblPlayspaceY.AutoSize = true;

            this.pnlSystem.Controls.Add(this.pnlSystem_Hardware);
            this.pnlMedia.Controls.Add(this.pnlMedia_Main);
            this.pnlAppearance.Controls.Add(this.pnlAppearance_Presets);
            this.pnlAppearance.Controls.Add(this.pnlAppearance_AnimatedText);
            this.pnlAdvanced.Controls.Add(this.pnlAdvanced_Shutdown);
            this.pnlAdvanced.Controls.Add(this.pnlAdvanced_Countdown);
            this.pnlAdvanced.Controls.Add(this.pnlAdvanced_Misc);
            this.pnlAdvanced.Controls.Add(this.pnlAdvanced_VR);
            this.pnlSystem_Hardware.Controls.Add(label11);
            this.pnlSystem_Hardware.Controls.Add(label12);
            this.pnlSystem_Hardware.Controls.Add(label13);
            this.pnlSystem_Hardware.Controls.Add(txtCpuFormat);
            this.pnlSystem_Hardware.Controls.Add(txtRamFormat);
            this.pnlSystem_Hardware.Controls.Add(txtGpuFormat);
            this.pnlSystem_Hardware.Controls.Add(tglCpuInfo);
            this.pnlSystem_Hardware.Controls.Add(tglRamInfo);
            this.pnlSystem_Hardware.Controls.Add(tglGpuInfo);
            this.pnlAppearance_Presets.Controls.Add(cmbPresets);
            this.pnlAppearance_Presets.Controls.Add(btnSavePreset);
            this.pnlAppearance_Presets.Controls.Add(btnDeletePreset);
            this.pnlAppearance_AnimatedText.Controls.Add(tglAnimatedText);
            this.pnlAppearance_AnimatedText.Controls.Add(txtAnimatedTexts);
            this.pnlAppearance_AnimatedText.Controls.Add(lblAnimatedTextPreview);
            this.pnlAdvanced_Shutdown.Controls.Add(numShutdown);
            this.pnlAdvanced_Shutdown.Controls.Add(btnShutdown);
            this.pnlAdvanced_Shutdown.Controls.Add(btnCancelShutdown);
            this.pnlAdvanced_Countdown.Controls.Add(tglCountdown);
            this.pnlAdvanced_Countdown.Controls.Add(dtpCountdown);
            this.pnlAdvanced_Countdown.Controls.Add(txtCountdownFinished);
            this.pnlAdvanced_Misc.Controls.Add(chkAlwaysOnTop);
            this.pnlAdvanced_VR.Controls.Add(tglAutoAfk);
            this.pnlAdvanced_VR.Controls.Add(tglPlayspace);
            this.pnlAdvanced_VR.Controls.Add(lblPlayspaceX);
            this.pnlAdvanced_VR.Controls.Add(lblPlayspaceY);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(950, 600);
            this.Controls.Add(this.pnlMainContent);
            this.Controls.Add(this.pnlSidebar);
            this.Controls.Add(this.pnlTitleBar);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VRChat OSC Pro C#";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlSidebar.ResumeLayout(false);
            this.pnlSidebar.PerformLayout();
            this.pnlTitleBar.ResumeLayout(false);
            this.pnlTitleBar.PerformLayout();
            this.pnlMainContent.ResumeLayout(false);
            this.pnlDashboard.ResumeLayout(false);
            this.pnlLog.ResumeLayout(false);
            this.pnlPreview.ResumeLayout(false);
            this.pnlAfk.ResumeLayout(false);
            this.pnlAfk.PerformLayout();
            this.pnlTime.ResumeLayout(false);
            this.pnlTime.PerformLayout();
            this.pnlPersonalStatus.ResumeLayout(false);
            this.pnlPersonalStatus.PerformLayout();
            this.pnlSystem.ResumeLayout(false);
            this.pnlSystem_Hardware.ResumeLayout(false);
            this.pnlSystem_Hardware.PerformLayout();
            this.pnlMedia.ResumeLayout(false);
            this.pnlMedia_Main.ResumeLayout(false);
            this.pnlMedia_Main.PerformLayout();
            this.pnlAppearance.ResumeLayout(false);
            this.pnlAppearance_Presets.ResumeLayout(false);
            this.pnlAppearance_AnimatedText.ResumeLayout(false);
            this.pnlAppearance_AnimatedText.PerformLayout();
            this.pnlAdvanced.ResumeLayout(false);
            this.pnlAdvanced_Shutdown.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numShutdown)).EndInit();
            this.pnlAdvanced_Countdown.ResumeLayout(false);
            this.pnlAdvanced_Countdown.PerformLayout();
            this.pnlAdvanced_Misc.ResumeLayout(false);
            this.pnlAdvanced_Misc.PerformLayout();
            this.pnlAdvanced_VR.ResumeLayout(false);
            this.pnlAdvanced_VR.PerformLayout();
            this.trayMenu.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.Panel pnlSidebar;
        private NavButton btnNavAppearance;
        private NavButton btnNavSystem;
        private NavButton btnNavMedia;
        private NavButton btnNavDashboard;
        private NavButton btnNavAdvanced;
        private System.Windows.Forms.Label lblAppName;
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Timer mainUpdateTimer;
        private System.Windows.Forms.Timer animationTimer;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Panel pnlMainContent;
        private System.Windows.Forms.Panel pnlSystem;
        private System.Windows.Forms.Panel pnlMedia;
        private System.Windows.Forms.Panel pnlAppearance;
        private System.Windows.Forms.Panel pnlAdvanced;
        private System.Windows.Forms.Panel pnlDashboard;
        private System.Windows.Forms.Timer hardwareUpdateTimer;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private ModernPanel pnlPersonalStatus;
        private System.Windows.Forms.TextBox txtPersonalStatus;
        private ToggleSwitch tglPersonalStatus;
        private ModernPanel pnlLog;
        private System.Windows.Forms.RichTextBox rtbLog;
        private ModernPanel pnlPreview;
        private System.Windows.Forms.Label lblLivePreview;
        private ModernPanel pnlAfk;
        private ToggleSwitch tglAfk;
        private ModernPanel pnlTime;
        private ToggleSwitch tglTime;
        private ModernPanel pnlMedia_Main;
        private ToggleSwitch tglSpotify;
        private ToggleSwitch tglYouTube;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private ModernPanel pnlSystem_Hardware;
        private ToggleSwitch tglCpuInfo;
        private ToggleSwitch tglRamInfo;
        private ToggleSwitch tglGpuInfo;
        private System.Windows.Forms.TextBox txtCpuFormat;
        private System.Windows.Forms.TextBox txtRamFormat;
        private System.Windows.Forms.TextBox txtGpuFormat;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private ModernPanel pnlAppearance_Presets;
        private System.Windows.Forms.ComboBox cmbPresets;
        private System.Windows.Forms.Button btnSavePreset;
        private System.Windows.Forms.Button btnDeletePreset;
        private ModernPanel pnlAppearance_AnimatedText;
        private ToggleSwitch tglAnimatedText;
        private System.Windows.Forms.TextBox txtAnimatedTexts;
        private System.Windows.Forms.Label lblAnimatedTextPreview;
        private ModernPanel pnlAdvanced_Shutdown;
        private System.Windows.Forms.NumericUpDown numShutdown;
        private System.Windows.Forms.Button btnShutdown;
        private System.Windows.Forms.Button btnCancelShutdown;
        private ModernPanel pnlAdvanced_Countdown;
        private ToggleSwitch tglCountdown;
        private System.Windows.Forms.DateTimePicker dtpCountdown;
        private System.Windows.Forms.TextBox txtCountdownFinished;
        private ModernPanel pnlAdvanced_Misc;
        private System.Windows.Forms.CheckBox chkAlwaysOnTop;
        private ModernPanel pnlAdvanced_VR;
        private ToggleSwitch tglAutoAfk;
        private ToggleSwitch tglPlayspace;
        private System.Windows.Forms.Label lblPlayspaceX;
        private System.Windows.Forms.Label lblPlayspaceY;
    }
}