using System.Windows.Forms;
using System.Drawing;

namespace VrcOscChatbox
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // ==== Top-level layout ====
        private Panel pnlTitleBar;
        private Label lblTitle;
        private Button btnClose;
        private Panel pnlSidebar;
        private NavButton btnNavDashboard;
        private NavButton btnNavMedia;
        private NavButton btnNavSystem;
        private NavButton btnNavAppearance;
        private NavButton btnNavAdvanced;
        private Panel pnlMainContent;

        // ==== Pages ====
        private Panel pnlDashboard;
        private Panel pnlMedia;
        private Panel pnlSystem;
        private Panel pnlAppearance;
        private Panel pnlAdvanced;

        // ==== Shared / Dashboard ====
        private Button btnStartStop;
        private Label lblLivePreview;
        private RichTextBox rtbLog;

        // ==== Media ====
        private CheckBox tglSpotify;
        private CheckBox tglYouTube;

        // ==== System ====
        private CheckBox tglCpuInfo;
        private CheckBox tglRamInfo;
        private CheckBox tglGpuInfo;
        private TextBox txtCpuFormat;
        private TextBox txtRamFormat;
        private TextBox txtGpuFormat;

        // ==== Appearance ====
        private CheckBox tglPersonalStatus;
        private CheckBox tglTime;
        private CheckBox tglAnimatedText;
        private TextBox txtPersonalStatus;
        private Label lblAnimatedTextPreview;
        private RichTextBox txtAnimatedTexts;
        private ComboBox cmbPresets;
        private Button btnSavePreset;
        private Button btnDeletePreset;
        private TextBox txtCountdownFinished;

        // ==== Advanced ====
        private CheckBox tglAfk;
        private CheckBox tglCountdown;
        private CheckBox tglAutoAfk;
        private CheckBox tglPlayspace;
        private DateTimePicker dtpCountdown;
        private NumericUpDown numShutdown;
        private Button btnShutdown;
        private Button btnCancelShutdown;
        private CheckBox chkAlwaysOnTop;

        // ==== Tray + timers ====
        private NotifyIcon notifyIcon;
        private ContextMenuStrip trayMenu;
        private ToolStripMenuItem showToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private Timer mainUpdateTimer;
        private Timer animationTimer;
        private Timer hardwareUpdateTimer;

        private System.Windows.Forms.Button btnRunTutorial;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // === Root form ===
            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.None;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(22, 22, 24);
            this.ClientSize = new Size(1120, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "VRChat OSC Pro C#";
            this.Load += new System.EventHandler(this.Form1_Load); // wire Load

            // ===== Title Bar =====
            this.pnlTitleBar = new Panel();
            this.pnlTitleBar.Name = "pnlTitleBar";
            this.pnlTitleBar.Dock = DockStyle.Top;
            this.pnlTitleBar.Height = 54;
            this.pnlTitleBar.BackColor = Color.FromArgb(36, 14, 66);
            this.pnlTitleBar.MouseDown += new MouseEventHandler(this.pnlTitleBar_MouseDown);
            this.pnlTitleBar.MouseMove += new MouseEventHandler(this.pnlTitleBar_MouseMove);
            this.pnlTitleBar.MouseUp += new MouseEventHandler(this.pnlTitleBar_MouseUp);

            this.lblTitle = new Label();
            this.lblTitle.Text = "VRChat OSC Pro C#";
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.Location = new Point(16, 12);

            this.btnClose = new Button();
            this.btnClose.Name = "btnClose";
            this.btnClose.Text = "✕";
            this.btnClose.ForeColor = Color.White;
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.BackColor = Color.Transparent;
            this.btnClose.Size = new Size(40, 40);
            this.btnClose.Location = new Point(1070, 7);
            this.btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.pnlTitleBar.Controls.Add(this.lblTitle);
            this.pnlTitleBar.Controls.Add(this.btnClose);
            this.Controls.Add(this.pnlTitleBar);

            // ===== Sidebar =====
            this.pnlSidebar = new Panel();
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Dock = DockStyle.Left;
            this.pnlSidebar.Width = 200;
            this.pnlSidebar.BackColor = Color.FromArgb(33, 33, 36);
            this.Controls.Add(this.pnlSidebar);

            this.btnNavDashboard = new NavButton();
            this.btnNavDashboard.Name = "btnNavDashboard";
            this.btnNavDashboard.Text = "Dashboard";
            this.btnNavDashboard.Size = new Size(200, 48);
            this.btnNavDashboard.Location = new Point(0, 20);
            this.btnNavDashboard.FlatStyle = FlatStyle.Flat;
            this.btnNavDashboard.FlatAppearance.BorderSize = 0;
            this.btnNavDashboard.ForeColor = Color.Gainsboro;
            this.btnNavDashboard.Click += new System.EventHandler(this.btnNavDashboard_Click);

            this.btnNavMedia = new NavButton();
            this.btnNavMedia.Name = "btnNavMedia";
            this.btnNavMedia.Text = "Media";
            this.btnNavMedia.Size = new Size(200, 48);
            this.btnNavMedia.Location = new Point(0, 68);
            this.btnNavMedia.FlatStyle = FlatStyle.Flat;
            this.btnNavMedia.FlatAppearance.BorderSize = 0;
            this.btnNavMedia.ForeColor = Color.Gainsboro;
            this.btnNavMedia.Click += new System.EventHandler(this.btnNavMedia_Click);

            this.btnNavSystem = new NavButton();
            this.btnNavSystem.Name = "btnNavSystem";
            this.btnNavSystem.Text = "System";
            this.btnNavSystem.Size = new Size(200, 48);
            this.btnNavSystem.Location = new Point(0, 116);
            this.btnNavSystem.FlatStyle = FlatStyle.Flat;
            this.btnNavSystem.FlatAppearance.BorderSize = 0;
            this.btnNavSystem.ForeColor = Color.Gainsboro;
            this.btnNavSystem.Click += new System.EventHandler(this.btnNavSystem_Click);

            this.btnNavAppearance = new NavButton();
            this.btnNavAppearance.Name = "btnNavAppearance";
            this.btnNavAppearance.Text = "Appearance";
            this.btnNavAppearance.Size = new Size(200, 48);
            this.btnNavAppearance.Location = new Point(0, 164);
            this.btnNavAppearance.FlatStyle = FlatStyle.Flat;
            this.btnNavAppearance.FlatAppearance.BorderSize = 0;
            this.btnNavAppearance.ForeColor = Color.Gainsboro;
            this.btnNavAppearance.Click += new System.EventHandler(this.btnNavAppearance_Click);

            this.btnNavAdvanced = new NavButton();
            this.btnNavAdvanced.Name = "btnNavAdvanced";
            this.btnNavAdvanced.Text = "Advanced";
            this.btnNavAdvanced.Size = new Size(200, 48);
            this.btnNavAdvanced.Location = new Point(0, 212);
            this.btnNavAdvanced.FlatStyle = FlatStyle.Flat;
            this.btnNavAdvanced.FlatAppearance.BorderSize = 0;
            this.btnNavAdvanced.ForeColor = Color.Gainsboro;
            this.btnNavAdvanced.Click += new System.EventHandler(this.btnNavAdvanced_Click);

            this.pnlSidebar.Controls.Add(this.btnNavDashboard);
            this.pnlSidebar.Controls.Add(this.btnNavMedia);
            this.pnlSidebar.Controls.Add(this.btnNavSystem);
            this.pnlSidebar.Controls.Add(this.btnNavAppearance);
            this.pnlSidebar.Controls.Add(this.btnNavAdvanced);

            // ===== Main content host =====
            this.pnlMainContent = new Panel();
            this.pnlMainContent.Name = "pnlMainContent";
            this.pnlMainContent.Dock = DockStyle.Fill;
            this.pnlMainContent.BackColor = Color.FromArgb(24, 24, 26);
            this.Controls.Add(this.pnlMainContent);
            this.Controls.SetChildIndex(this.pnlMainContent, 0); // ensure under title bar

            // ===== Dashboard (Modern layout with cards) =====
            this.pnlDashboard = new Panel();
            this.pnlDashboard.Name = "pnlDashboard";
            this.pnlDashboard.Dock = DockStyle.Fill;
            this.pnlDashboard.BackColor = Color.Transparent;

            // Grid container
            var tlpDash = new TableLayoutPanel();
            tlpDash.Name = "tlpDash";
            tlpDash.Dock = DockStyle.Fill;
            tlpDash.BackColor = Color.Transparent;
            tlpDash.ColumnCount = 2;
            tlpDash.RowCount = 2;
            tlpDash.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));
            tlpDash.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            tlpDash.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpDash.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            this.pnlDashboard.Controls.Add(tlpDash);

            // Left Top: Start + Preview
            var cardStart = new ModernCard() { Title = "Broadcast", Dock = DockStyle.Fill };
            this.btnStartStop = new Button();
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Text = "▶ Start";
            this.btnStartStop.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            this.btnStartStop.FlatStyle = FlatStyle.Flat;
            this.btnStartStop.FlatAppearance.BorderSize = 0;
            this.btnStartStop.BackColor = Color.FromArgb(124, 58, 237);
            this.btnStartStop.ForeColor = Color.White;
            this.btnStartStop.Size = new Size(170, 46);
            this.btnStartStop.Location = new Point(20, 40);
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);

            this.lblLivePreview = new Label();
            this.lblLivePreview.Name = "lblLivePreview";
            this.lblLivePreview.Text = "";
            this.lblLivePreview.Font = new Font("Segoe UI", 10F);
            this.lblLivePreview.ForeColor = Color.Gainsboro;
            this.lblLivePreview.AutoEllipsis = false;
            this.lblLivePreview.Location = new Point(210, 40);
            this.lblLivePreview.Size = new Size(420, 60);
            this.lblLivePreview.BorderStyle = BorderStyle.FixedSingle;
            this.lblLivePreview.Padding = new Padding(8);
            this.lblLivePreview.BackColor = Color.FromArgb(30, 30, 30);

            cardStart.Controls.Add(this.btnStartStop);
            cardStart.Controls.Add(this.lblLivePreview);

            // Right Top: System summary (labels found by name in Form1.cs)
            var cardHw = new ModernCard() { Title = "System", Dock = DockStyle.Fill };
            var lblCpu = new Label()
            {
                Name = "lblCpuSummary",
                AutoSize = false,
                Location = new Point(16, 46),
                Size = new Size(350, 22),
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 9F)
            };
            var lblGpu = new Label()
            {
                Name = "lblGpuSummary",
                AutoSize = false,
                Location = new Point(16, 72),
                Size = new Size(350, 22),
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 9F)
            };
            cardHw.Controls.Add(lblCpu); cardHw.Controls.Add(lblGpu);

            // Bottom Left: Log (fills)
            var cardLog = new ModernCard() { Title = "Activity Log", Dock = DockStyle.Fill };
            this.rtbLog = new RichTextBox();
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.ShortcutsEnabled = false;
            this.rtbLog.Font = new Font("Consolas", 9F);
            this.rtbLog.BackColor = Color.FromArgb(30, 30, 30);
            this.rtbLog.ForeColor = Color.Gainsboro;
            this.rtbLog.BorderStyle = BorderStyle.None;
            this.rtbLog.Dock = DockStyle.Fill;
            cardLog.Controls.Add(this.rtbLog);

            // Bottom Right: Tips / placeholder
            var cardTips = new ModernCard() { Title = "Tips", Dock = DockStyle.Fill };
            var tip = new Label()
            {
                Text = "Use Presets in Appearance to save your setup.\nDouble-click the tray icon to restore.",
                AutoSize = false,
                ForeColor = Color.Gainsboro,
                Location = new Point(16, 46),
                Size = new Size(330, 60)
            };
            cardTips.Controls.Add(tip);

            // place into grid
            tlpDash.Controls.Add(cardStart, 0, 0);
            tlpDash.Controls.Add(cardHw, 1, 0);
            tlpDash.Controls.Add(cardLog, 0, 1);
            tlpDash.Controls.Add(cardTips, 1, 1);

            // ===== Media =====
            this.pnlMedia = new Panel();
            this.pnlMedia.Name = "pnlMedia";
            this.pnlMedia.Dock = DockStyle.Fill;
            this.pnlMedia.Visible = false;

            this.tglSpotify = new CheckBox();
            this.tglSpotify.Name = "tglSpotify";
            this.tglSpotify.Text = "Enable Spotify";
            this.tglSpotify.Location = new Point(24, 24);
            this.tglSpotify.AutoSize = true;
            this.tglSpotify.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);

            this.tglYouTube = new CheckBox();
            this.tglYouTube.Name = "tglYouTube";
            this.tglYouTube.Text = "Enable YouTube";
            this.tglYouTube.Location = new Point(24, 54);
            this.tglYouTube.AutoSize = true;
            this.tglYouTube.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);

            this.pnlMedia.Controls.Add(this.tglSpotify);
            this.pnlMedia.Controls.Add(this.tglYouTube);

            // ===== System =====
            this.pnlSystem = new Panel();
            this.pnlSystem.Name = "pnlSystem";
            this.pnlSystem.Dock = DockStyle.Fill;
            this.pnlSystem.Visible = false;

            this.tglCpuInfo = new CheckBox();
            this.tglCpuInfo.Name = "tglCpuInfo";
            this.tglCpuInfo.Text = "CPU Line";
            this.tglCpuInfo.Location = new Point(24, 24);
            this.tglCpuInfo.AutoSize = true;
            this.tglCpuInfo.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);

            this.tglRamInfo = new CheckBox();
            this.tglRamInfo.Name = "tglRamInfo";
            this.tglRamInfo.Text = "RAM Line";
            this.tglRamInfo.Location = new Point(24, 54);
            this.tglRamInfo.AutoSize = true;
            this.tglRamInfo.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);

            this.tglGpuInfo = new CheckBox();
            this.tglGpuInfo.Name = "tglGpuInfo";
            this.tglGpuInfo.Text = "GPU Line";
            this.tglGpuInfo.Location = new Point(24, 84);
            this.tglGpuInfo.AutoSize = true;
            this.tglGpuInfo.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);

            this.txtCpuFormat = new TextBox();
            this.txtCpuFormat.Name = "txtCpuFormat";
            this.txtCpuFormat.Text = "{NAME} {LOAD}% {TEMP}°C";
            this.txtCpuFormat.Location = new Point(180, 22);
            this.txtCpuFormat.Size = new Size(320, 24);

            this.txtRamFormat = new TextBox();
            this.txtRamFormat.Name = "txtRamFormat";
            this.txtRamFormat.Text = "{USED}/{TOTAL} GB RAM";
            this.txtRamFormat.Location = new Point(180, 52);
            this.txtRamFormat.Size = new Size(320, 24);

            this.txtGpuFormat = new TextBox();
            this.txtGpuFormat.Name = "txtGpuFormat";
            this.txtGpuFormat.Text = "{NAME} {LOAD}% {TEMP}°C";
            this.txtGpuFormat.Location = new Point(180, 82);
            this.txtGpuFormat.Size = new Size(320, 24);

            this.pnlSystem.Controls.Add(this.tglCpuInfo);
            this.pnlSystem.Controls.Add(this.tglRamInfo);
            this.pnlSystem.Controls.Add(this.tglGpuInfo);
            this.pnlSystem.Controls.Add(this.txtCpuFormat);
            this.pnlSystem.Controls.Add(this.txtRamFormat);
            this.pnlSystem.Controls.Add(this.txtGpuFormat);

            // ===== Appearance =====
            this.pnlAppearance = new Panel();
            this.pnlAppearance.Name = "pnlAppearance";
            this.pnlAppearance.Dock = DockStyle.Fill;
            this.pnlAppearance.Visible = false;

            this.tglPersonalStatus = new CheckBox();
            this.tglPersonalStatus.Name = "tglPersonalStatus";
            this.tglPersonalStatus.Text = "Personal Status";
            this.tglPersonalStatus.Location = new Point(24, 24);
            this.tglPersonalStatus.AutoSize = true;
            this.tglPersonalStatus.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);

            this.txtPersonalStatus = new TextBox();
            this.txtPersonalStatus.Name = "txtPersonalStatus";
            this.txtPersonalStatus.Location = new Point(180, 22);
            this.txtPersonalStatus.Size = new Size(500, 24);

            this.tglTime = new CheckBox();
            this.tglTime.Name = "tglTime";
            this.tglTime.Text = "Clock (🕒 HH:mm)";
            this.tglTime.Location = new Point(24, 58);
            this.tglTime.AutoSize = true;
            this.tglTime.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);

            this.tglAnimatedText = new CheckBox();
            this.tglAnimatedText.Name = "tglAnimatedText";
            this.tglAnimatedText.Text = "Animated Text";
            this.tglAnimatedText.Location = new Point(24, 92);
            this.tglAnimatedText.AutoSize = true;
            this.tglAnimatedText.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);

            this.lblAnimatedTextPreview = new Label();
            this.lblAnimatedTextPreview.Name = "lblAnimatedTextPreview";
            this.lblAnimatedTextPreview.Text = "";
            this.lblAnimatedTextPreview.AutoSize = false;
            this.lblAnimatedTextPreview.Location = new Point(180, 90);
            this.lblAnimatedTextPreview.Size = new Size(500, 24);
            this.lblAnimatedTextPreview.ForeColor = Color.Gainsboro;

            this.txtAnimatedTexts = new RichTextBox();
            this.txtAnimatedTexts.Name = "txtAnimatedTexts";
            this.txtAnimatedTexts.Location = new Point(24, 132);
            this.txtAnimatedTexts.Size = new Size(656, 120);
            this.txtAnimatedTexts.Font = new Font("Consolas", 9F);
            this.txtAnimatedTexts.BackColor = Color.FromArgb(30, 30, 30);
            this.txtAnimatedTexts.ForeColor = Color.Gainsboro;

            this.cmbPresets = new ComboBox();
            this.cmbPresets.Name = "cmbPresets";
            this.cmbPresets.Location = new Point(24, 270);
            this.cmbPresets.Size = new Size(240, 24);
            this.cmbPresets.DropDownStyle = ComboBoxStyle.DropDown;
            this.cmbPresets.SelectedIndexChanged += new System.EventHandler(this.cmbPresets_SelectedIndexChanged);

            this.btnSavePreset = new Button();
            this.btnSavePreset.Name = "btnSavePreset";
            this.btnSavePreset.Text = "Save Preset";
            this.btnSavePreset.Location = new Point(272, 268);
            this.btnSavePreset.Size = new Size(120, 28);
            this.btnSavePreset.Click += new System.EventHandler(this.btnSavePreset_Click);

            this.btnDeletePreset = new Button();
            this.btnDeletePreset.Name = "btnDeletePreset";
            this.btnDeletePreset.Text = "Delete Preset";
            this.btnDeletePreset.Location = new Point(398, 268);
            this.btnDeletePreset.Size = new Size(120, 28);
            this.btnDeletePreset.Click += new System.EventHandler(this.btnDeletePreset_Click);

            this.txtCountdownFinished = new TextBox();
            this.txtCountdownFinished.Name = "txtCountdownFinished";
            this.txtCountdownFinished.Location = new Point(24, 310);
            this.txtCountdownFinished.Size = new Size(656, 24);
            this.txtCountdownFinished.Text = "Countdown finished!";

            this.pnlAppearance.Controls.Add(this.tglPersonalStatus);
            this.pnlAppearance.Controls.Add(this.txtPersonalStatus);
            this.pnlAppearance.Controls.Add(this.tglTime);
            this.pnlAppearance.Controls.Add(this.tglAnimatedText);
            this.pnlAppearance.Controls.Add(this.lblAnimatedTextPreview);
            this.pnlAppearance.Controls.Add(this.txtAnimatedTexts);
            this.pnlAppearance.Controls.Add(this.cmbPresets);
            this.pnlAppearance.Controls.Add(this.btnSavePreset);
            this.pnlAppearance.Controls.Add(this.btnDeletePreset);
            this.pnlAppearance.Controls.Add(this.txtCountdownFinished);

            // ===== Advanced =====
            this.pnlAdvanced = new Panel();
            this.pnlAdvanced.Name = "pnlAdvanced";
            this.pnlAdvanced.Dock = DockStyle.Fill;
            this.pnlAdvanced.Visible = false;

            this.tglAfk = new CheckBox();
            this.tglAfk.Name = "tglAfk";
            this.tglAfk.Text = "AFK Mode";
            this.tglAfk.AutoSize = true;
            this.tglAfk.Location = new Point(24, 24);
            this.tglAfk.CheckedChanged += new System.EventHandler(this.tglAfk_CheckedChanged);

            this.tglCountdown = new CheckBox();
            this.tglCountdown.Name = "tglCountdown";
            this.tglCountdown.Text = "Enable Countdown";
            this.tglCountdown.AutoSize = true;
            this.tglCountdown.Location = new Point(24, 54);
            this.tglCountdown.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);

            this.tglAutoAfk = new CheckBox();
            this.tglAutoAfk.Name = "tglAutoAfk";
            this.tglAutoAfk.Text = "Auto AFK";
            this.tglAutoAfk.AutoSize = true;
            this.tglAutoAfk.Location = new Point(24, 84);
            this.tglAutoAfk.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);

            this.tglPlayspace = new CheckBox();
            this.tglPlayspace.Name = "tglPlayspace";
            this.tglPlayspace.Text = "Playspace Options";
            this.tglPlayspace.AutoSize = true;
            this.tglPlayspace.Location = new Point(24, 114);
            this.tglPlayspace.CheckedChanged += new System.EventHandler(this.AnyToggle_CheckedChanged);

            this.dtpCountdown = new DateTimePicker();
            this.dtpCountdown.Name = "dtpCountdown";
            this.dtpCountdown.Location = new Point(24, 154);
            this.dtpCountdown.Size = new Size(260, 24);

            this.numShutdown = new NumericUpDown();
            this.numShutdown.Name = "numShutdown";
            this.numShutdown.Location = new Point(24, 194);
            this.numShutdown.Size = new Size(80, 24);
            this.numShutdown.Minimum = 1;
            this.numShutdown.Maximum = 240;
            this.numShutdown.Value = 15;

            this.btnShutdown = new Button();
            this.btnShutdown.Name = "btnShutdown";
            this.btnShutdown.Text = "Schedule Shutdown (min)";
            this.btnShutdown.Location = new Point(112, 192);
            this.btnShutdown.Size = new Size(220, 26);
            this.btnShutdown.Click += new System.EventHandler(this.btnShutdown_Click);

            this.btnCancelShutdown = new Button();
            this.btnCancelShutdown.Name = "btnCancelShutdown";
            this.btnCancelShutdown.Text = "Cancel Shutdown";
            this.btnCancelShutdown.Location = new Point(340, 192);
            this.btnCancelShutdown.Size = new Size(160, 26);
            this.btnCancelShutdown.Click += new System.EventHandler(this.btnCancelShutdown_Click);

            this.chkAlwaysOnTop = new CheckBox();
            this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
            this.chkAlwaysOnTop.Text = "Always on top";
            this.chkAlwaysOnTop.AutoSize = true;
            this.chkAlwaysOnTop.Location = new Point(24, 232);
            this.chkAlwaysOnTop.CheckedChanged += new System.EventHandler(this.chkAlwaysOnTop_CheckedChanged);

            this.pnlAdvanced.Controls.Add(this.tglAfk);
            this.pnlAdvanced.Controls.Add(this.tglCountdown);
            this.pnlAdvanced.Controls.Add(this.tglAutoAfk);
            this.pnlAdvanced.Controls.Add(this.tglPlayspace);
            this.pnlAdvanced.Controls.Add(this.dtpCountdown);
            this.pnlAdvanced.Controls.Add(this.numShutdown);
            this.pnlAdvanced.Controls.Add(this.btnShutdown);
            this.pnlAdvanced.Controls.Add(this.btnCancelShutdown);
            this.pnlAdvanced.Controls.Add(this.chkAlwaysOnTop);

            // ===== Add pages to host =====
            this.pnlMainContent.Controls.Add(this.pnlDashboard);
            this.pnlMainContent.Controls.Add(this.pnlMedia);
            this.pnlMainContent.Controls.Add(this.pnlSystem);
            this.pnlMainContent.Controls.Add(this.pnlAppearance);
            this.pnlMainContent.Controls.Add(this.pnlAdvanced);

            // ===== Tray + timers =====
            this.trayMenu = new ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new ToolStripMenuItem();
            this.exitToolStripMenuItem = new ToolStripMenuItem();

            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Text = "Show";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.showToolStripMenuItem_Click);

            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);

            this.trayMenu.Items.AddRange(new ToolStripItem[] { this.showToolStripMenuItem, this.exitToolStripMenuItem });

            this.notifyIcon = new NotifyIcon(this.components);
            this.notifyIcon.Text = "VRChat OSC Pro C#";
            this.notifyIcon.Visible = true;
            this.notifyIcon.ContextMenuStrip = this.trayMenu;
            this.notifyIcon.MouseDoubleClick += new MouseEventHandler(this.notifyIcon_MouseDoubleClick);

            this.mainUpdateTimer = new Timer(this.components);
            this.mainUpdateTimer.Interval = 500;
            this.mainUpdateTimer.Tick += new System.EventHandler(this.mainUpdateTimer_Tick);

            this.animationTimer = new Timer(this.components);
            this.animationTimer.Interval = 80;
            this.animationTimer.Tick += new System.EventHandler(this.animationTimer_Tick);

            this.hardwareUpdateTimer = new Timer(this.components);
            this.hardwareUpdateTimer.Interval = 1000;
            this.hardwareUpdateTimer.Tick += new System.EventHandler(this.hardwareUpdateTimer_Tick);

            // Default visibility (Dashboard on)
            this.pnlDashboard.Visible = true;
            this.pnlMedia.Visible = false;
            this.pnlSystem.Visible = false;
            this.pnlAppearance.Visible = false;
            this.pnlAdvanced.Visible = false;

            this.btnRunTutorial = new System.Windows.Forms.Button();
            this.btnRunTutorial.Name = "btnRunTutorial";
            this.btnRunTutorial.Text = "Run Tutorial…";
            this.btnRunTutorial.Location = new System.Drawing.Point(24, 300);
            this.btnRunTutorial.Size = new System.Drawing.Size(160, 28);
            this.btnRunTutorial.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunTutorial.FlatAppearance.BorderSize = 0;
            this.btnRunTutorial.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
            this.btnRunTutorial.ForeColor = System.Drawing.Color.Gainsboro;
            //this.btnRunTutorial.Click += new System.EventHandler(this.btnRunTutorial_Click);
            this.pnlAdvanced.Controls.Add(this.btnRunTutorial);

            // Done
            this.ResumeLayout(false);
        }
        #endregion
    }
}
