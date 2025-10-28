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
using CoreOSC;

namespace VrcOscChatbox
{
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
        #endregion

        #region Module States & Helpers
        private bool _isSpotifyEnabled, _isYouTubeEnabled, _isAnimatedTextEnabled;
        private bool _isCpuEnabled, _isRamEnabled, _isGpuEnabled;
        private bool _isPersonalStatusEnabled, _isTimeEnabled, _isAfkEnabled, _isCountdownEnabled;
        private DateTime _afkStartTime;
        private DateTime _shutdownTime = DateTime.MinValue;
        private class AnimationState { public int ListIndex = 0, CharIndex = 0; public bool Forward = true; public DateTime PauseUntil = DateTime.MinValue; }
        private AnimationState _animState = new AnimationState();
        #endregion

        public Form1() { InitializeComponent(); _hardwareManager = new HardwareManager(); }

        private void Form1_Load(object sender, EventArgs e)
        {
            try { _oscSender = new UDPSender("127.0.0.1", 9000); Log("OSC Sender Initialized.", Color.LimeGreen); }
            catch (Exception ex) { Log($"OSC Sender FAILED: {ex.Message}", Color.Red); }

            _contentPanels = new List<Panel> { pnlDashboard, pnlMedia, pnlSystem, pnlAppearance, pnlAdvanced };
            _navButtons = new List<NavButton> { btnNavDashboard, btnNavMedia, btnNavSystem, btnNavAppearance, btnNavAdvanced };

            LoadSettings();
            UpdateAllModuleStates();
            Log("Application Ready.", Color.Cyan);
            btnNavDashboard.PerformClick();
            this.FormClosing += OnFormClosing;
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (mainUpdateTimer.Enabled) { mainUpdateTimer.Stop(); animationTimer.Stop(); hardwareUpdateTimer.Stop(); btnStartStop.Text = "▶ Start"; SendChatboxMessage(""); Log("OSC Broadcasting Stopped.", Color.OrangeRed); }
            else { mainUpdateTimer.Start(); animationTimer.Start(); hardwareUpdateTimer.Start(); btnStartStop.Text = "■ Stop"; Log("OSC Broadcasting Started.", Color.LimeGreen); }
        }

        private void mainUpdateTimer_Tick(object sender, EventArgs e) => BuildAndSendMessage();
        private void hardwareUpdateTimer_Tick(object sender, EventArgs e) => _hardwareManager.Update();
        private void animationTimer_Tick(object sender, EventArgs e)
        {
            if (!_isAnimatedTextEnabled || _isAfkEnabled) { lblAnimatedTextPreview.Text = ""; return; }
            string[] texts = txtAnimatedTexts.Lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            if (texts.Length == 0) return; string activeText = texts[_animState.ListIndex % texts.Length];
            if (DateTime.Now < _animState.PauseUntil) { lblAnimatedTextPreview.Text = activeText.Substring(0, _animState.CharIndex); return; }
            if (_animState.Forward) { if (_animState.CharIndex < activeText.Length) _animState.CharIndex++; else { _animState.PauseUntil = DateTime.Now.AddSeconds(2.5); _animState.Forward = false; } }
            else { if (_animState.CharIndex > 0) _animState.CharIndex--; else { _animState.Forward = true; _animState.ListIndex++; _animState.PauseUntil = DateTime.Now.AddSeconds(1.0); } }
            lblAnimatedTextPreview.Text = activeText.Substring(0, _animState.CharIndex);
        }

        private void BuildAndSendMessage()
        {
            var messageParts = new List<string>();
            if (_isAfkEnabled) { messageParts.Add(BuildAfkLine()); }
            else { string mediaLine = BuildMediaLine(); if (!string.IsNullOrEmpty(mediaLine)) messageParts.Add(mediaLine); messageParts.AddRange(BuildSystemLines()); if (_isTimeEnabled) messageParts.Add($"🕒 {DateTime.Now:HH:mm}"); if (_isPersonalStatusEnabled && !string.IsNullOrWhiteSpace(txtPersonalStatus.Text)) messageParts.Add(txtPersonalStatus.Text); if (_isAnimatedTextEnabled) messageParts.Add(lblAnimatedTextPreview.Text); if (_isCountdownEnabled) messageParts.Add(BuildCountdownLine()); }
            if (_shutdownTime > DateTime.Now) { TimeSpan remaining = _shutdownTime - DateTime.Now; messageParts.Add($"PC Shutting Down in {remaining.Minutes:00}:{remaining.Seconds:00}..."); }
            string currentMessage = string.Join("\n", messageParts.Where(s => !string.IsNullOrEmpty(s)));
            lblLivePreview.Text = currentMessage;
            if (currentMessage != _lastSentMessage) { _lastSentMessage = currentMessage; SendChatboxMessage(currentMessage); }
        }

        #region Line Builders
        private string BuildMediaLine()
        {
            if (_isSpotifyEnabled) { string t = FindWindowTitle("spotify", " - "); if (!string.IsNullOrEmpty(t)) return $"🎵 {t}"; }
            if (_isYouTubeEnabled) { string t = FindWindowTitle(null, " - YouTube"); if (!string.IsNullOrEmpty(t)) return $"📺 {t.Replace(" - YouTube", "")}"; }
            return null;
        }
        private string[] BuildSystemLines() { var lines = new List<string>(); if (_isCpuEnabled) lines.Add(txtCpuFormat.Text.Replace("{NAME}", _hardwareManager.CpuName).Replace("{LOAD}", $"{_hardwareManager.CpuLoad:F0}").Replace("{TEMP}", $"{_hardwareManager.CpuTemp:F0}")); if (_isRamEnabled) lines.Add(txtRamFormat.Text.Replace("{USED}", $"{_hardwareManager.RamUsed:F1}").Replace("{TOTAL}", $"{_hardwareManager.RamTotal:F1}")); if (_isGpuEnabled) lines.Add(txtGpuFormat.Text.Replace("{NAME}", _hardwareManager.GpuName).Replace("{LOAD}", $"{_hardwareManager.GpuLoad:F0}").Replace("{TEMP}", $"{_hardwareManager.GpuTemp:F0}")); return lines.ToArray(); }
        private string BuildAfkLine() { TimeSpan afkDuration = DateTime.Now - _afkStartTime; if (afkDuration.TotalMinutes < 1) return "AFK - Be right back..."; return $"AFK ({afkDuration.Hours}h {afkDuration.Minutes}m ago)"; }
        private string BuildCountdownLine() { try { TimeSpan remaining = dtpCountdown.Value - DateTime.Now; if (remaining.TotalSeconds > 0) return $"Countdown: {remaining.Days}d {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}"; return txtCountdownFinished.Text; } catch { return "Invalid Countdown"; } }
        #endregion

        #region UI & Settings
        private void UpdateAllModuleStates() { _isSpotifyEnabled = tglSpotify.Checked; _isYouTubeEnabled = tglYouTube.Checked; _isCpuEnabled = tglCpuInfo.Checked; _isRamEnabled = tglRamInfo.Checked; _isGpuEnabled = tglGpuInfo.Checked; _isAnimatedTextEnabled = tglAnimatedText.Checked; _isPersonalStatusEnabled = tglPersonalStatus.Checked; _isTimeEnabled = tglTime.Checked; _isAfkEnabled = tglAfk.Checked; _isCountdownEnabled = tglCountdown.Checked; }
        private void AnyToggle_CheckedChanged(object s, EventArgs e) => UpdateAllModuleStates();
        private void tglAfk_CheckedChanged(object s, EventArgs e) { UpdateAllModuleStates(); bool isAfk = tglAfk.Checked; pnlMainContent.Enabled = !isAfk; if (isAfk) _afkStartTime = DateTime.Now; }
        private void chkAlwaysOnTop_CheckedChanged(object s, EventArgs e) => this.TopMost = chkAlwaysOnTop.Checked;
        private void btnShutdown_Click(object s, EventArgs e) { _shutdownTime = DateTime.Now.AddMinutes((double)numShutdown.Value); Process.Start("shutdown", $"/s /t {(int)numShutdown.Value * 60}"); Log($"PC will shut down in {numShutdown.Value} minutes.", Color.Orange); }
        private void btnCancelShutdown_Click(object s, EventArgs e) { _shutdownTime = DateTime.MinValue; Process.Start("shutdown", "/a"); Log("PC shutdown cancelled.", Color.LightBlue); }

        private void LoadSettings()
        {
            _settings = new AppSettings();
            try { if (File.Exists(SETTINGS_FILE)) { string json = File.ReadAllText(SETTINGS_FILE); if (!string.IsNullOrWhiteSpace(json)) { var loadedSettings = JsonSerializer.Deserialize<AppSettings>(json); if (loadedSettings != null) { _settings = loadedSettings; Log("Settings loaded.", Color.Aquamarine); } } } }
            catch (Exception ex) { Log($"Settings load failed, using defaults: {ex.Message}", Color.Red); }
            if (_settings.Presets == null) _settings.Presets = new Dictionary<string, PresetSettings>();
            if (_settings.Presets.Count == 0) { _settings.Presets["Default"] = new PresetSettings(); _settings.LastPreset = "Default"; }
            cmbPresets.Items.Clear(); foreach (var key in _settings.Presets.Keys) cmbPresets.Items.Add(key);
            if (!string.IsNullOrEmpty(_settings.LastPreset) && _settings.Presets.ContainsKey(_settings.LastPreset)) cmbPresets.SelectedItem = _settings.LastPreset; else if (cmbPresets.Items.Count > 0) cmbPresets.SelectedIndex = 0;
            if (cmbPresets.SelectedItem != null) LoadPreset((string)cmbPresets.SelectedItem);
        }
        private void SaveSettings() { try { SavePreset((string)cmbPresets.SelectedItem); _settings.LastPreset = (string)cmbPresets.SelectedItem; File.WriteAllText(SETTINGS_FILE, JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true })); } catch (Exception ex) { MessageBox.Show($"Error saving settings: {ex.Message}"); } }
        private void LoadPreset(string name) { if (!_settings.Presets.TryGetValue(name, out var preset)) return; tglSpotify.Checked = preset.SpotifyEnabled; tglYouTube.Checked = preset.YouTubeEnabled; tglCpuInfo.Checked = preset.CpuInfoEnabled; tglRamInfo.Checked = preset.RamInfoEnabled; tglGpuInfo.Checked = preset.GpuInfoEnabled; tglAnimatedText.Checked = preset.AnimatedTextEnabled; tglPersonalStatus.Checked = preset.PersonalStatusEnabled; tglTime.Checked = preset.TimeEnabled; tglCountdown.Checked = preset.CountdownEnabled; tglAutoAfk.Checked = preset.AutoAfkEnabled; tglPlayspace.Checked = preset.PlayspaceEnabled; txtCpuFormat.Text = preset.CpuFormat; txtRamFormat.Text = preset.RamFormat; txtGpuFormat.Text = preset.GpuFormat; txtPersonalStatus.Text = preset.PersonalStatus; txtAnimatedTexts.Lines = preset.AnimatedTexts.ToArray(); dtpCountdown.Value = preset.CountdownTarget; txtCountdownFinished.Text = preset.CountdownFinished; UpdateAllModuleStates(); Log($"Loaded preset: {name}", Color.LightGreen); }
        private void SavePreset(string name) { if (string.IsNullOrWhiteSpace(name)) return; if (!_settings.Presets.ContainsKey(name)) _settings.Presets[name] = new PresetSettings(); var preset = _settings.Presets[name]; preset.SpotifyEnabled = tglSpotify.Checked; preset.YouTubeEnabled = tglYouTube.Checked; preset.CpuInfoEnabled = tglCpuInfo.Checked; preset.RamInfoEnabled = tglRamInfo.Checked; preset.GpuInfoEnabled = tglGpuInfo.Checked; preset.AnimatedTextEnabled = tglAnimatedText.Checked; preset.PersonalStatusEnabled = tglPersonalStatus.Checked; preset.TimeEnabled = tglTime.Checked; preset.CountdownEnabled = tglCountdown.Checked; preset.AutoAfkEnabled = tglAutoAfk.Checked; preset.PlayspaceEnabled = tglPlayspace.Checked; preset.CpuFormat = txtCpuFormat.Text; preset.RamFormat = txtRamFormat.Text; preset.GpuFormat = txtGpuFormat.Text; preset.PersonalStatus = txtPersonalStatus.Text; preset.AnimatedTexts = txtAnimatedTexts.Lines.ToList(); preset.CountdownTarget = dtpCountdown.Value; preset.CountdownFinished = txtCountdownFinished.Text; }
        private void cmbPresets_SelectedIndexChanged(object s, EventArgs e) => LoadPreset((string)cmbPresets.SelectedItem);
        private void btnSavePreset_Click(object s, EventArgs e) { string name = cmbPresets.Text; if (string.IsNullOrWhiteSpace(name)) { MessageBox.Show("Please enter a preset name."); return; } if (!_settings.Presets.ContainsKey(name)) { _settings.Presets[name] = new PresetSettings(); cmbPresets.Items.Add(name); } cmbPresets.SelectedItem = name; SavePreset(name); SaveSettings(); Log($"Saved settings to preset '{name}'", Color.LightGreen); }
        private void btnDeletePreset_Click(object s, EventArgs e) { string name = (string)cmbPresets.SelectedItem; if (name == "Default" || !_settings.Presets.ContainsKey(name)) return; if (MessageBox.Show($"Are you sure you want to delete the '{name}' preset?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes) { _settings.Presets.Remove(name); cmbPresets.Items.Remove(name); cmbPresets.SelectedItem = "Default"; SaveSettings(); Log($"Deleted preset '{name}'", Color.OrangeRed); } }
        #endregion

        #region System & Utilities
        private void OnFormClosing(object s, FormClosingEventArgs e) { if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; this.Hide(); } else { SaveSettings(); _hardwareManager.Close(); } }
        private void ShowPage(Panel p, NavButton b) { foreach (var panel in _contentPanels) panel.Visible = false; p.Visible = true; foreach (var btn in _navButtons) btn.IsActive = false; b.IsActive = true; }
        private void btnNavDashboard_Click(object s, EventArgs e) => ShowPage(pnlDashboard, btnNavDashboard); private void btnNavMedia_Click(object s, EventArgs e) => ShowPage(pnlMedia, btnNavMedia); private void btnNavSystem_Click(object s, EventArgs e) => ShowPage(pnlSystem, btnNavSystem); private void btnNavAppearance_Click(object s, EventArgs e) => ShowPage(pnlAppearance, btnNavAppearance); private void btnNavAdvanced_Click(object s, EventArgs e) => ShowPage(pnlAdvanced, btnNavAdvanced);
        private void notifyIcon_MouseDoubleClick(object s, MouseEventArgs e) { this.Show(); this.WindowState = FormWindowState.Normal; }
        private void showToolStripMenuItem_Click(object s, EventArgs e) { this.Show(); this.WindowState = FormWindowState.Normal; }
        private void exitToolStripMenuItem_Click(object s, EventArgs e) { notifyIcon.Visible = false; Application.Exit(); }
        private void Log(string message, Color color) { if (rtbLog.InvokeRequired) { rtbLog.Invoke(new Action(() => Log(message, color))); return; } rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n"); rtbLog.Find($"[{DateTime.Now:HH:mm:ss}]"); rtbLog.SelectionColor = Color.Gray; rtbLog.Find(message); rtbLog.SelectionColor = color; rtbLog.ScrollToCaret(); }
        private void SendChatboxMessage(string message) { if (_oscSender == null) return; try { _oscSender.Send(new OscMessage("/chatbox/input", message, true, false)); } catch (Exception ex) { Log($"OSC Send Error: {ex.Message}", Color.Red); } }
        private string FindWindowTitle(string pName, string sub) { string fTitle = null; EnumWindows((hWnd, lParam) => { GetWindowThreadProcessId(hWnd, out uint pId); try { Process p = Process.GetProcessById((int)pId); if (pName == null || p.ProcessName.ToLower() == pName) { int len = GetWindowTextLength(hWnd); if (len > 0) { var sb = new StringBuilder(len + 1); GetWindowText(hWnd, sb, sb.Capacity); string t = sb.ToString(); if (t.Contains(sub)) { fTitle = t; return false; } } } } catch { } return true; }, IntPtr.Zero); return fTitle; }
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
    }
}