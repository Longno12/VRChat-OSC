using CoreOSC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
//using static VrcOscChatbox.ModernTutorialForm;

namespace VrcOscChatbox
{
    internal static class UiTheme
    {
        public static readonly Color Back = Color.FromArgb(22, 22, 24);
        public static readonly Color Surface = Color.FromArgb(37, 37, 38);
        public static readonly Color Surface2 = Color.FromArgb(30, 30, 30);
        public static readonly Color Text = Color.White;
        public static readonly Color Subtext = Color.FromArgb(180, 180, 200);
        public static readonly Color Accent1 = Color.FromArgb(124, 58, 237);
        public static readonly Color Accent2 = Color.FromArgb(167, 139, 250);
        public static readonly Color Accent3 = Color.FromArgb(39, 19, 79);
        public static readonly Color Good = Color.FromArgb(56, 189, 248);
        public static readonly Color Warn = Color.Orange;
        public static readonly Color Danger = Color.IndianRed;
    }

    public partial class Form1 : Form
    {
        #region Core & UI
        private UDPSender _oscSender;
        private string _lastSentMessage = "";
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);
        private HardwareManager _hardwareManager;
        private AppSettings _settings;
        private const string SETTINGS_FILE = "config.json";
        private List<Panel> _contentPanels;
        private List<NavButton> _navButtons;
        private DateTime _lastMessageSent = DateTime.MinValue;
        private const double MIN_MESSAGE_INTERVAL = 0.3;
        private string _pendingMessage = "";
        private Timer _messageThrottleTimer;
        #endregion

        #region Module States & Helpers
        private bool _isSpotifyEnabled, _isYouTubeEnabled, _isAnimatedTextEnabled;
        private bool _isCpuEnabled, _isRamEnabled, _isGpuEnabled;
        private bool _isPersonalStatusEnabled, _isTimeEnabled, _isAfkEnabled, _isCountdownEnabled;
        private bool _isAutoAfkEnabled, _isPlayspaceEnabled;
        private DateTime _afkStartTime;
        private DateTime _shutdownTime = DateTime.MinValue;
        private class AnimationState { public int ListIndex = 0, CharIndex = 0; public bool Forward = true; public DateTime PauseUntil = DateTime.MinValue; }
        private AnimationState _animState = new AnimationState();
        private DateTime _lastSystemUpdate = DateTime.MinValue;
        private const double SYSTEM_UPDATE_INTERVAL = 0.5;
        private DateTime _lastSuccessfulSend = DateTime.MinValue;
        private int _consecutiveSendFailures = 0;
        private const int MAX_FAILURES_BEFORE_BACKOFF = 3;
        #endregion

        public Form1()
        {
            InitializeComponent();
            _hardwareManager = new HardwareManager();
        }
        /*private void Form1_Shown(object sender, EventArgs e)
        {
            try
            {
                var tutorialState = new ModernTutorialForm.TutorialState
                {
                    EnableAnimations = true
                };
                using (var tour = new ModernTutorialForm(tutorialState))
                {
                    tour.ShowDialog(this);
                }
                ToastManager.Show("Tutorial", "You’re all set! 🙌", ToastType.Success, 2000);
            }
            catch (Exception ex)
            {
                Log($"Tutorial error: {ex.Message}", Color.Red);
                using (var tour = new ModernTutorialForm(new ModernTutorialForm.TutorialState { EnableAnimations = true }))
                {
                    tour.ShowDialog(this);
                }
            }
        }*/

        private void Form1_Load(object sender, EventArgs e)
        {
            ApplyModernTheme();

            try
            {
                _oscSender = new UDPSender("127.0.0.1", 9000);
                Log("OSC Sender Initialized.", Color.LimeGreen);
            }
            catch (Exception ex)
            {
                Log($"OSC Sender FAILED: {ex.Message}", Color.Red);
            }

            _contentPanels = new List<Panel> { pnlDashboard, pnlMedia, pnlSystem, pnlAppearance, pnlAdvanced };
            _navButtons = new List<NavButton> { btnNavDashboard, btnNavMedia, btnNavSystem, btnNavAppearance, btnNavAdvanced };
            _messageThrottleTimer = new Timer { Interval = (int)(MIN_MESSAGE_INTERVAL * 1000) };
            _messageThrottleTimer.Tick += (s, ev) =>
            {
                if (!string.IsNullOrEmpty(_pendingMessage))
                {
                    SendChatboxMessage(_pendingMessage);
                    _lastSentMessage = _pendingMessage;
                    _lastMessageSent = DateTime.Now;
                    _pendingMessage = "";
                }
                _messageThrottleTimer.Stop();
            };

            LoadSettings();
            UpdateAllModuleStates();
            UpdateTimerIntervalsBasedOnActivity();
            Log("Application Ready.", Color.Cyan);
            btnNavDashboard.PerformClick();
            this.FormClosing += OnFormClosing;

            /*this.BeginInvoke(new Action(() =>
            {
                try
                {
                    using (var tour = new ModernTutorialForm(new ModernTutorialForm.TutorialState { EnableAnimations = true }))
                    {
                        tour.ShowDialog(this);
                    }
                    ToastManager.Show("Tutorial", "You’re all set! 🙌", ToastType.Success, 2000);
                }
                catch (Exception ex)
                {
                    Log($"Tutorial error: {ex.Message}", Color.Red);
                    using (var tour = new ModernTutorialForm(new ModernTutorialForm.TutorialState { EnableAnimations = true }))
                    {
                        tour.ShowDialog(this);
                    }
                }
            }));*/

        }

        private void Notify(string title, string text, ToastType type = ToastType.Info)
        {
            ToastManager.Show(title, text, type);
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (mainUpdateTimer.Enabled)
            {
                mainUpdateTimer.Stop(); animationTimer.Stop(); hardwareUpdateTimer.Stop();
                btnStartStop.Text = "▶ Start"; SendChatboxMessage("");
                Log("OSC Broadcasting Stopped.", Color.OrangeRed);
                Notify("OSC", "Broadcasting stopped", ToastType.Warning);
            }
            else
            {
                mainUpdateTimer.Start(); animationTimer.Start(); hardwareUpdateTimer.Start();
                btnStartStop.Text = "■ Stop";
                Log("OSC Broadcasting Started.", Color.LimeGreen);
                Notify("OSC", "Broadcasting started", ToastType.Success);
            }
        }

        private void mainUpdateTimer_Tick(object sender, EventArgs e) => BuildAndSendMessage();
        private void hardwareUpdateTimer_Tick(object sender, EventArgs e)
        {
            _hardwareManager.Update();

            var cpuLbl = Controls.Find("lblCpuSummary", true).FirstOrDefault() as Label;
            var gpuLbl = Controls.Find("lblGpuSummary", true).FirstOrDefault() as Label;
            if (cpuLbl != null) cpuLbl.Text = $"CPU: {_hardwareManager.CpuName} @ {_hardwareManager.CpuLoad:F0}% ({_hardwareManager.CpuTemp:F0}°C)";
            if (gpuLbl != null) gpuLbl.Text = $"GPU: {_hardwareManager.GpuName} @ {_hardwareManager.GpuLoad:F0}% ({_hardwareManager.GpuTemp:F0}°C)";
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            if (!_isAnimatedTextEnabled || _isAfkEnabled) { lblAnimatedTextPreview.Text = ""; return; }
            string[] texts = txtAnimatedTexts.Lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            if (texts.Length == 0) return;
            string activeText = texts[_animState.ListIndex % texts.Length];

            if (DateTime.Now < _animState.PauseUntil)
            {
                lblAnimatedTextPreview.Text = activeText.Substring(0, _animState.CharIndex);
                return;
            }

            if (_animState.Forward)
            {
                if (_animState.CharIndex < activeText.Length) _animState.CharIndex++;
                else { _animState.PauseUntil = DateTime.Now.AddSeconds(2.5); _animState.Forward = false; }
            }
            else
            {
                if (_animState.CharIndex > 0) _animState.CharIndex--;
                else { _animState.Forward = true; _animState.ListIndex++; _animState.PauseUntil = DateTime.Now.AddSeconds(1.0); }
            }
            lblAnimatedTextPreview.Text = activeText.Substring(0, _animState.CharIndex);
        }

        private void BuildAndSendMessage()
        {
            var messageParts = new List<string>();

            if (_isTimeEnabled)
            {
                string dayAndYear = DateTime.Now.ToString("ddd, MMM d, yyyy");
                string time = DateTime.Now.ToString("HH:mm");
                messageParts.Add($"🕒 {dayAndYear} | {time}");
            }

            if (_isAfkEnabled)
            {
                messageParts.Add(BuildAfkLine());
            }
            else
            {
                string mediaLine = BuildMediaLine();
                if (!string.IsNullOrEmpty(mediaLine)) messageParts.Add(mediaLine);
                if ((DateTime.Now - _lastSystemUpdate).TotalSeconds >= SYSTEM_UPDATE_INTERVAL)
                {
                    _lastSystemUpdate = DateTime.Now;
                    messageParts.AddRange(BuildSystemLines());
                }

                if (_isPersonalStatusEnabled && !string.IsNullOrWhiteSpace(txtPersonalStatus.Text))
                    messageParts.Add(txtPersonalStatus.Text);
                if (_isAnimatedTextEnabled)
                    messageParts.Add(lblAnimatedTextPreview.Text);
                if (_isCountdownEnabled)
                    messageParts.Add(BuildCountdownLine());
            }

            if (_shutdownTime > DateTime.Now)
            {
                TimeSpan remaining = _shutdownTime - DateTime.Now;
                messageParts.Add($"PC Shutting Down in {remaining.Minutes:00}:{remaining.Seconds:00}...");
            }

            string currentMessage = string.Join("\n", messageParts.Where(s => !string.IsNullOrEmpty(s)));
            lblLivePreview.Text = currentMessage;

            if (currentMessage != _lastSentMessage)
            {
                _pendingMessage = currentMessage;
                ScheduleMessageSend();
            }
        }

        private void ScheduleMessageSend()
        {
            if (_messageThrottleTimer == null)
            {
                _messageThrottleTimer = new Timer { Interval = (int)(MIN_MESSAGE_INTERVAL * 1000), Enabled = false };
                _messageThrottleTimer.Tick += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(_pendingMessage))
                    {
                        SendChatboxMessage(_pendingMessage);
                        _lastSentMessage = _pendingMessage;
                        _lastMessageSent = DateTime.Now;
                        _pendingMessage = "";
                    }
                    _messageThrottleTimer.Stop();
                };
            }

            if (!_messageThrottleTimer.Enabled)
            {
                _messageThrottleTimer.Start();
            }
        }

        #region Line Builders
        private string BuildMediaLine()
        {
            if (_isSpotifyEnabled) { string t = FindWindowTitle("spotify", " - "); if (!string.IsNullOrEmpty(t)) return $"🎵 {t}"; }
            if (_isYouTubeEnabled) { string t = FindWindowTitle(null, " - YouTube"); if (!string.IsNullOrEmpty(t)) return $"📺 {t.Replace(" - YouTube", "")}"; }
            return null;
        }

        private string[] BuildSystemLines()
        {
            var lines = new List<string>();
            if (_isCpuEnabled || _isRamEnabled || _isGpuEnabled)
            {
                string cpuName = _hardwareManager.CpuName;
                float cpuLoad = _hardwareManager.CpuLoad;
                float cpuTemp = _hardwareManager.CpuTemp;
                float ramUsed = _hardwareManager.RamUsed;
                float ramTotal = _hardwareManager.RamTotal;
                string gpuName = _hardwareManager.GpuName;
                float gpuLoad = _hardwareManager.GpuLoad;
                float gpuTemp = _hardwareManager.GpuTemp;
                if (_isCpuEnabled) lines.Add(txtCpuFormat.Text.Replace("{NAME}", cpuName).Replace("{LOAD}", $"{cpuLoad:F0}").Replace("{TEMP}", $"{cpuTemp:F0}"));
                if (_isRamEnabled) lines.Add(txtRamFormat.Text.Replace("{USED}", $"{ramUsed:F1}").Replace("{TOTAL}", $"{ramTotal:F1}"));
                if (_isGpuEnabled) lines.Add(txtGpuFormat.Text.Replace("{NAME}", gpuName).Replace("{LOAD}", $"{gpuLoad:F0}").Replace("{TEMP}", $"{gpuTemp:F0}"));
            }
            return lines.ToArray();
        }

        private string BuildAfkLine()
        {
            TimeSpan afkDuration = DateTime.Now - _afkStartTime;
            if (afkDuration.TotalMinutes < 1) return "AFK - Be right back...";
            return $"AFK ({afkDuration.Hours}h {afkDuration.Minutes}m ago)";
        }

        private string BuildCountdownLine()
        {
            try
            {
                TimeSpan remaining = dtpCountdown.Value - DateTime.Now;
                if (remaining.TotalSeconds > 0) return $"Countdown: {remaining.Days}d {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
                return txtCountdownFinished.Text;
            }
            catch { return "Invalid Countdown"; }
        }
        #endregion

        #region UI & Settings
        private void UpdateAllModuleStates()
        {
            _isSpotifyEnabled = tglSpotify.Checked; _isYouTubeEnabled = tglYouTube.Checked;
            _isCpuEnabled = tglCpuInfo.Checked; _isRamEnabled = tglRamInfo.Checked; _isGpuEnabled = tglGpuInfo.Checked;
            _isAnimatedTextEnabled = tglAnimatedText.Checked; _isPersonalStatusEnabled = tglPersonalStatus.Checked;
            _isTimeEnabled = tglTime.Checked; _isAfkEnabled = tglAfk.Checked; _isCountdownEnabled = tglCountdown.Checked;
            _isAutoAfkEnabled = tglAutoAfk.Checked; _isPlayspaceEnabled = tglPlayspace.Checked;
        }
        /*private void btnRunTutorial_Click(object sender, EventArgs e)
        {
            try
            {
                var state = new TutorialState();

                if (state.MaybeAskSoundPref)
                {
                    TutorialState.AskPreferences(this);
                }

                using (var tour = new ModernTutorialForm(new ModernTutorialForm.TutorialState { EnableAnimations = true }))
                {
                    tour.ShowDialog(this);
                }

                state.LastPromptUtc = DateTime.UtcNow;
                if (!state.FirstRunUtc.HasValue) state.FirstRunUtc = DateTime.UtcNow;

                Notify("Tutorial", "Completed 👍", VrcOscChatbox.ToastType.Success);

            }
            catch (Exception ex)
            {
                Log($"Run Tutorial error: {ex.Message}", System.Drawing.Color.Red);
            }
        }*/


        private void UpdateTimerIntervalsBasedOnActivity()
        {
            bool hasActiveUpdates = _isTimeEnabled || _isMediaActive() || _isSystemActive();

            if (hasActiveUpdates)
            {
                mainUpdateTimer.Interval = 100;
            }
            else
            {
                mainUpdateTimer.Interval = 500;
            }
        }

        private bool _isMediaActive()
        {
            return _isSpotifyEnabled || _isYouTubeEnabled;
        }

        private bool _isSystemActive()
        {
            return _isCpuEnabled || _isRamEnabled || _isGpuEnabled;
        }

        private void AnyToggle_CheckedChanged(object s, EventArgs e)
        {
            UpdateAllModuleStates();
            UpdateTimerIntervalsBasedOnActivity();

            var cb = s as CheckBox;
            if (cb != null)
            {
                var nowOn = cb.Checked ? "Enabled" : "Disabled";
                var type = cb.Checked ? ToastType.Success : ToastType.Warning;
                Notify(cb.Text, nowOn, type);
            }
        }

        private void tglAfk_CheckedChanged(object s, EventArgs e)
        {
            UpdateAllModuleStates();
            bool isAfk = tglAfk.Checked;
            pnlMainContent.Enabled = !isAfk;
            if (isAfk) _afkStartTime = DateTime.Now;

            Notify("AFK Mode", isAfk ? "AFK enabled" : "AFK disabled",
                isAfk ? ToastType.Warning : ToastType.Info);
        }

        private void chkAlwaysOnTop_CheckedChanged(object s, EventArgs e) => this.TopMost = chkAlwaysOnTop.Checked;

        private void btnShutdown_Click(object s, EventArgs e)
        {
            _shutdownTime = DateTime.Now.AddMinutes((double)numShutdown.Value);
            Process.Start("shutdown", $"/s /t {(int)numShutdown.Value * 60}");
            Log($"PC will shut down in {numShutdown.Value} minutes.", Color.Orange);
        }

        private void btnCancelShutdown_Click(object s, EventArgs e)
        {
            _shutdownTime = DateTime.MinValue;
            Process.Start("shutdown", "/a");
            Log("PC shutdown cancelled.", Color.LightBlue);
        }

        private void LoadSettings()
        {
            _settings = new AppSettings();
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    _settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SETTINGS_FILE));
                    Log("Settings loaded.", Color.Aquamarine);
                }
            }
            catch (Exception ex) { Log($"Settings load failed: {ex.Message}", Color.Red); }

            if (_settings == null) _settings = new AppSettings();
            if (_settings.Presets == null) _settings.Presets = new Dictionary<string, PresetSettings>();
            if (_settings.Presets.Count == 0) { _settings.Presets["Default"] = new PresetSettings(); _settings.LastPreset = "Default"; }
            cmbPresets.Items.Clear();
            foreach (var key in _settings.Presets.Keys) cmbPresets.Items.Add(key);
            if (!string.IsNullOrEmpty(_settings.LastPreset) && _settings.Presets.ContainsKey(_settings.LastPreset)) cmbPresets.SelectedItem = _settings.LastPreset;
            else if (cmbPresets.Items.Count > 0) cmbPresets.SelectedIndex = 0;
            if (cmbPresets.SelectedItem != null) LoadPreset((string)cmbPresets.SelectedItem);
        }

        private void SaveSettings()
        {
            try
            {
                SavePreset((string)cmbPresets.SelectedItem);
                _settings.LastPreset = (string)cmbPresets.SelectedItem;
                File.WriteAllText(SETTINGS_FILE, JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex) { MessageBox.Show($"Error saving settings: {ex.Message}"); }
        }

        private void LoadPreset(string name)
        {
            if (!_settings.Presets.TryGetValue(name, out var preset)) return;

            tglSpotify.Checked = preset.SpotifyEnabled;
            tglYouTube.Checked = preset.YouTubeEnabled;
            tglCpuInfo.Checked = preset.CpuInfoEnabled;
            tglRamInfo.Checked = preset.RamInfoEnabled;
            tglGpuInfo.Checked = preset.GpuInfoEnabled;
            tglAnimatedText.Checked = preset.AnimatedTextEnabled;
            tglPersonalStatus.Checked = preset.PersonalStatusEnabled;
            tglTime.Checked = preset.TimeEnabled;
            tglCountdown.Checked = preset.CountdownEnabled;
            tglAutoAfk.Checked = preset.AutoAfkEnabled;
            tglPlayspace.Checked = preset.PlayspaceEnabled;
            txtCpuFormat.Text = preset.CpuFormat;
            txtRamFormat.Text = preset.RamFormat;
            txtGpuFormat.Text = preset.GpuFormat;
            txtPersonalStatus.Text = preset.PersonalStatus;
            txtAnimatedTexts.Lines = preset.AnimatedTexts.ToArray();
            dtpCountdown.Value = preset.CountdownTarget;
            txtCountdownFinished.Text = preset.CountdownFinished;
            UpdateAllModuleStates();
            Log($"Loaded preset: {name}", Color.LightGreen);
        }

        private void SavePreset(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (!_settings.Presets.ContainsKey(name)) _settings.Presets[name] = new PresetSettings();
            var preset = _settings.Presets[name];
            preset.SpotifyEnabled = tglSpotify.Checked;
            preset.YouTubeEnabled = tglYouTube.Checked;
            preset.CpuInfoEnabled = tglCpuInfo.Checked;
            preset.RamInfoEnabled = tglRamInfo.Checked;
            preset.GpuInfoEnabled = tglGpuInfo.Checked;
            preset.AnimatedTextEnabled = tglAnimatedText.Checked;
            preset.PersonalStatusEnabled = tglPersonalStatus.Checked;
            preset.TimeEnabled = tglTime.Checked;
            preset.CountdownEnabled = tglCountdown.Checked;
            preset.AutoAfkEnabled = tglAutoAfk.Checked;
            preset.PlayspaceEnabled = tglPlayspace.Checked;
            preset.CpuFormat = txtCpuFormat.Text;
            preset.RamFormat = txtRamFormat.Text;
            preset.GpuFormat = txtGpuFormat.Text;
            preset.PersonalStatus = txtPersonalStatus.Text;
            preset.AnimatedTexts = txtAnimatedTexts.Lines.ToList();
            preset.CountdownTarget = dtpCountdown.Value;
            preset.CountdownFinished = txtCountdownFinished.Text;
        }

        private void cmbPresets_SelectedIndexChanged(object s, EventArgs e) => LoadPreset((string)cmbPresets.SelectedItem);

        private void btnSavePreset_Click(object s, EventArgs e)
        {
            string name = cmbPresets.Text;
            if (string.IsNullOrWhiteSpace(name)) { MessageBox.Show("Please enter a preset name."); return; }
            if (!_settings.Presets.ContainsKey(name)) { _settings.Presets[name] = new PresetSettings(); cmbPresets.Items.Add(name); }
            cmbPresets.SelectedItem = name;
            SavePreset(name);
            SaveSettings();
            Log($"Saved settings to preset '{name}'", Color.LightGreen);
        }

        private void btnDeletePreset_Click(object s, EventArgs e)
        {
            string name = (string)cmbPresets.SelectedItem;
            if (name == "Default" || !_settings.Presets.ContainsKey(name)) return;

            if (MessageBox.Show($"Are you sure you want to delete the '{name}' preset?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _settings.Presets.Remove(name);
                cmbPresets.Items.Remove(name);
                cmbPresets.SelectedItem = "Default";
                SaveSettings();
                Log($"Deleted preset '{name}'", Color.OrangeRed);
            }
        }
        #endregion

        #region System & Utilities
        private void OnFormClosing(object s, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; this.Hide();
            }
            else
            {
                SaveSettings();
                _hardwareManager.Close();
            }
        }

        private void ShowPage(Panel p, NavButton b)
        {
            foreach (var panel in _contentPanels) panel.Visible = false;
            p.Visible = true;
            foreach (var btn in _navButtons) btn.IsActive = false;
            b.IsActive = true;
        }

        private void btnNavDashboard_Click(object s, EventArgs e) => ShowPage(pnlDashboard, btnNavDashboard);
        private void btnNavMedia_Click(object s, EventArgs e) => ShowPage(pnlMedia, btnNavMedia);
        private void btnNavSystem_Click(object s, EventArgs e) => ShowPage(pnlSystem, btnNavSystem);
        private void btnNavAppearance_Click(object s, EventArgs e) => ShowPage(pnlAppearance, btnNavAppearance);
        private void btnNavAdvanced_Click(object s, EventArgs e) => ShowPage(pnlAdvanced, btnNavAdvanced);

        private void notifyIcon_MouseDoubleClick(object s, MouseEventArgs e) { this.Show(); this.WindowState = FormWindowState.Normal; }
        private void showToolStripMenuItem_Click(object s, EventArgs e) { this.Show(); this.WindowState = FormWindowState.Normal; }
        private void exitToolStripMenuItem_Click(object s, EventArgs e) { notifyIcon.Visible = false; Application.Exit(); }

        private void Log(string message, Color color)
        {
            if (rtbLog.InvokeRequired) { rtbLog.Invoke(new Action(() => Log(message, color))); return; }
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            rtbLog.Find($"[{DateTime.Now:HH:mm:ss}]"); rtbLog.SelectionColor = Color.Gray;
            rtbLog.Find(message); rtbLog.SelectionColor = color;
            rtbLog.ScrollToCaret();
        }

        /*private void MaybeShowTutorialPrompt()
        {
            try
            {
                using (var tour = new ModernTutorialForm(new ModernTutorialForm.TutorialState()))
                {
                    tour.ShowDialog(this);
                }
                Notify("Tutorial", "You’re all set! 🙌", ToastType.Success);
            }
            catch (Exception ex)
            {
                Log($"Tutorial error: {ex.Message}", Color.Red);
                using (var tour = new ModernTutorialForm(new ModernTutorialForm.TutorialState()))
                {
                    tour.ShowDialog(this);
                }
            }
        }*/

        private void SendChatboxMessage(string message)
        {
            if (_oscSender == null) return;
            if (_consecutiveSendFailures >= MAX_FAILURES_BEFORE_BACKOFF)
            {
                double backoffSeconds = Math.Pow(2, _consecutiveSendFailures - MAX_FAILURES_BEFORE_BACKOFF);
                if ((DateTime.Now - _lastSuccessfulSend).TotalSeconds < backoffSeconds)
                {
                    Log($"Backing off OSC sends due to failures. Retry in {backoffSeconds:F1}s", Color.Orange);
                    return;
                }
            }
            try
            {
                _oscSender.Send(new OscMessage("/chatbox/input", message, true, false));
                _lastSuccessfulSend = DateTime.Now;
                _consecutiveSendFailures = 0;
            }
            catch (Exception ex)
            {
                _consecutiveSendFailures++;
                Log($"OSC Send Error ({_consecutiveSendFailures}): {ex.Message}", Color.Red);
                Notify("OSC Error", ex.Message, ToastType.Error);
                mainUpdateTimer.Interval = Math.Min(5000, mainUpdateTimer.Interval * 2);
            }
        }

        private string FindWindowTitle(string pName, string sub)
        {
            string fTitle = null;
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint pId);
                try
                {
                    Process p = Process.GetProcessById((int)pId);
                    if (pName == null || p.ProcessName.ToLower() == pName)
                    {
                        int len = GetWindowTextLength(hWnd);
                        if (len > 0)
                        {
                            var sb = new StringBuilder(len + 1);
                            GetWindowText(hWnd, sb, sb.Capacity);
                            string t = sb.ToString();
                            if (t.Contains(sub)) { fTitle = t; return false; }
                        }
                    }
                }
                catch { }
                return true;
            }, IntPtr.Zero);
            return fTitle;
        }

        private void pnlTitleBar_MouseDown(object s, MouseEventArgs e) { _dragging = true; _start_point = new Point(e.X, e.Y); }
        private void pnlTitleBar_MouseMove(object s, MouseEventArgs e) { if (_dragging) { Point p = PointToScreen(e.Location); Location = new Point(p.X - _start_point.X, p.Y - _start_point.Y); } }
        private void pnlTitleBar_MouseUp(object s, MouseEventArgs e) { _dragging = false; }
        private void btnClose_Click(object s, EventArgs e) { this.Close(); }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)] private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        #endregion

        private void ApplyModernTheme()
        {
            this.BackColor = UiTheme.Back;
            this.DoubleBuffered = true;
            ApplyRoundedCorners(14);

            pnlTitleBar.BackColor = UiTheme.Surface;
            pnlTitleBar.Paint += (s, e) =>
            {
                using (var lg = new System.Drawing.Drawing2D.LinearGradientBrush(
                    pnlTitleBar.ClientRectangle, UiTheme.Accent1, UiTheme.Accent3, 0f))
                {
                    e.Graphics.FillRectangle(lg, pnlTitleBar.ClientRectangle);
                }
                using (var glow = new SolidBrush(Color.FromArgb(60, UiTheme.Accent2)))
                {
                    e.Graphics.FillRectangle(glow, 0, pnlTitleBar.Height - 2, pnlTitleBar.Width, 2);
                }
            };

            StylePrimaryButton(btnStartStop);
            foreach (var card in new[] { pnlDashboard, pnlMedia, pnlSystem, pnlAppearance, pnlAdvanced, pnlMainContent }) StyleCard(card);
            StyleNavButton(btnNavDashboard);
            StyleNavButton(btnNavMedia);
            StyleNavButton(btnNavSystem);
            StyleNavButton(btnNavAppearance);
            StyleNavButton(btnNavAdvanced);
            StyleInput(txtPersonalStatus);
            StyleInput(txtCpuFormat);
            StyleInput(txtRamFormat);
            StyleInput(txtGpuFormat);
            StyleInput(txtAnimatedTexts);
            StyleInput(txtCountdownFinished);
            cmbPresets.BackColor = UiTheme.Surface2; cmbPresets.ForeColor = UiTheme.Text;
            rtbLog.BackColor = UiTheme.Surface2; rtbLog.ForeColor = UiTheme.Text;
            lblLivePreview.ForeColor = UiTheme.Subtext;
            pnlMainContent.Paint += (s, e) =>
            {
                using (var shade = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shade, 0, 0, pnlMainContent.Width, 6);
                }
            };
        }

        private void StyleNavButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 48, 52);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(28, 28, 30);
            b.ForeColor = Color.Gainsboro;
            b.Font = new Font("Segoe UI", 11f, FontStyle.Regular);

            if (b is NavButton nb)
            {
                nb.Paint += (s, e) =>
                {
                    if (nb.IsActive)
                    {
                        using (var accent = new SolidBrush(UiTheme.Accent1))
                        {
                            e.Graphics.FillRectangle(accent, 0, 0, 4, nb.Height);
                        }
                        nb.ForeColor = Color.White;
                    }
                    else nb.ForeColor = Color.Gainsboro;
                };
                nb.Invalidate();
            }
        }

        private void StylePrimaryButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold);
            b.BackColor = UiTheme.Accent1;
            b.ForeColor = Color.White;
            b.Height = Math.Max(44, b.Height);
            b.MouseEnter += (s, e) => b.BackColor = UiTheme.Accent2;
            b.MouseLeave += (s, e) => b.BackColor = UiTheme.Accent1;
        }

        private void StyleCard(Control c)
        {
            c.BackColor = UiTheme.Surface;
            c.ForeColor = Color.Gainsboro;
            c.Padding = new Padding(1);

            c.Paint += (s, e) =>
            {
                var rect = c.ClientRectangle;

                using (var bg = new SolidBrush(UiTheme.Surface))
                {
                    e.Graphics.FillRectangle(bg, rect);
                }

                using (var accent = new SolidBrush(Color.FromArgb(36, 14, 66)))
                {
                    e.Graphics.FillRectangle(accent, 1, 1, rect.Width - 2, 22);
                }

                using (var pen = new Pen(Color.FromArgb(70, 255, 255, 255)))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
                }
            };
            c.Invalidate();
        }

        private void StyleInput(Control c)
        {
            c.BackColor = UiTheme.Surface2;
            c.ForeColor = UiTheme.Text;
            if (c is TextBoxBase tb) tb.BorderStyle = BorderStyle.FixedSingle;
        }

        private void ApplyRoundedCorners(int radius)
        {
            try
            {
                var r = new System.Drawing.Drawing2D.GraphicsPath();
                r.AddArc(0, 0, radius, radius, 180, 90);
                r.AddArc(Width - radius, 0, radius, radius, 270, 90);
                r.AddArc(Width - radius, Height - radius, radius, radius, 0, 90);
                r.AddArc(0, Height - radius, radius, radius, 90, 90);
                r.CloseAllFigures();
                this.Region = new Region(r);
            }
            catch { }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyRoundedCorners(14);
        }
    }
}
