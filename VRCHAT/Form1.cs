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
        #endregion

        #region Module States & Helpers
        private bool _isSpotifyEnabled, _isYouTubeEnabled, _isAnimatedTextEnabled;
        private bool _isCpuEnabled, _isRamEnabled, _isGpuEnabled;
        private bool _isPersonalStatusEnabled, _isTimeEnabled, _isAfkEnabled;
        private DateTime _afkStartTime;
        private class AnimationState { public int ListIndex = 0, CharIndex = 0; public bool Forward = true; public DateTime PauseUntil = DateTime.MinValue; }
        private AnimationState _animState = new AnimationState();
        #endregion

        public Form1() { InitializeComponent(); _hardwareManager = new HardwareManager(); }

        private void Form1_Load(object sender, EventArgs e)
        {
            try { _oscSender = new UDPSender("127.0.0.1", 9000); Log("OSC Sender Initialized.", Color.LimeGreen); }
            catch (Exception ex) { Log($"OSC Init FAILED: {ex.Message}", Color.Red); MessageBox.Show($"OSC Init Error: {ex.Message}"); this.Close(); }

            LoadSettings();
            UpdateAllModuleStates();
            Log("Application Ready.", Color.Cyan);

            this.FormClosing += OnFormClosing;
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (mainUpdateTimer.Enabled)
            {
                mainUpdateTimer.Stop(); animationTimer.Stop(); hardwareUpdateTimer.Stop();
                btnStartStop.Text = "▶ Start"; SendChatboxMessage(""); Log("OSC Broadcasting Stopped.", Color.OrangeRed);
            }
            else
            {
                mainUpdateTimer.Start(); animationTimer.Start(); hardwareUpdateTimer.Start();
                btnStartStop.Text = "■ Stop"; Log("OSC Broadcasting Started.", Color.LimeGreen);
            }
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
            else { string mediaLine = BuildMediaLine(); if (!string.IsNullOrEmpty(mediaLine)) messageParts.Add(mediaLine); messageParts.AddRange(BuildSystemLines()); if (_isTimeEnabled) messageParts.Add($"🕒 {DateTime.Now:HH:mm}"); if (_isPersonalStatusEnabled && !string.IsNullOrWhiteSpace(txtPersonalStatus.Text)) messageParts.Add(txtPersonalStatus.Text); if (_isAnimatedTextEnabled) messageParts.Add(lblAnimatedTextPreview.Text); }
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
        #endregion

        #region UI, Settings & Utilities
        private void AnyToggle_CheckedChanged(object s, EventArgs e) => UpdateAllModuleStates();
        private void tglAfk_CheckedChanged(object s, EventArgs e) { UpdateAllModuleStates(); bool isAfk = tglAfk.Checked; pnlMain.Enabled = !isAfk; if (isAfk) _afkStartTime = DateTime.Now; }
        private void UpdateAllModuleStates() { _isSpotifyEnabled = tglSpotify.Checked; _isYouTubeEnabled = tglYouTube.Checked; _isCpuEnabled = tglCpuInfo.Checked; _isRamEnabled = tglRamInfo.Checked; _isGpuEnabled = tglGpuInfo.Checked; _isAnimatedTextEnabled = tglAnimatedText.Checked; _isPersonalStatusEnabled = tglPersonalStatus.Checked; _isTimeEnabled = tglTime.Checked; _isAfkEnabled = tglAfk.Checked; }

        private void LoadSettings()
        {
            _settings = new AppSettings();
            try { if (File.Exists(SETTINGS_FILE)) { string json = File.ReadAllText(SETTINGS_FILE); if (!string.IsNullOrWhiteSpace(json)) { _settings = JsonSerializer.Deserialize<AppSettings>(json); Log("Settings loaded.", Color.Aquamarine); } } }
            catch (Exception ex) { Log($"Settings load failed, using defaults: {ex.Message}", Color.Red); }
            tglSpotify.Checked = _settings.SpotifyEnabled; tglYouTube.Checked = _settings.YouTubeEnabled; tglCpuInfo.Checked = _settings.CpuInfoEnabled; tglRamInfo.Checked = _settings.RamInfoEnabled; tglGpuInfo.Checked = _settings.GpuInfoEnabled; tglAnimatedText.Checked = _settings.AnimatedTextEnabled; tglPersonalStatus.Checked = _settings.PersonalStatusEnabled; tglTime.Checked = _settings.TimeEnabled;
            txtCpuFormat.Text = _settings.CpuFormat; txtRamFormat.Text = _settings.RamFormat; txtGpuFormat.Text = _settings.GpuFormat; txtPersonalStatus.Text = _settings.PersonalStatus; txtAnimatedTexts.Lines = _settings.AnimatedTexts.ToArray();
        }
        private void SaveSettings()
        {
            try
            {
                if (_settings == null) _settings = new AppSettings();
                _settings.SpotifyEnabled = tglSpotify.Checked; _settings.YouTubeEnabled = tglYouTube.Checked; _settings.CpuInfoEnabled = tglCpuInfo.Checked; _settings.RamInfoEnabled = tglRamInfo.Checked; _settings.GpuInfoEnabled = tglGpuInfo.Checked; _settings.AnimatedTextEnabled = tglAnimatedText.Checked; _settings.PersonalStatusEnabled = tglPersonalStatus.Checked; _settings.TimeEnabled = tglTime.Checked;
                _settings.CpuFormat = txtCpuFormat.Text; _settings.RamFormat = txtRamFormat.Text; _settings.GpuFormat = txtGpuFormat.Text; _settings.PersonalStatus = txtPersonalStatus.Text; _settings.AnimatedTexts = txtAnimatedTexts.Lines.ToList();
                File.WriteAllText(SETTINGS_FILE, JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex) { MessageBox.Show($"Error saving settings: {ex.Message}"); }
        }

        private void OnFormClosing(object s, FormClosingEventArgs e) { if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; this.Hide(); Log("App minimized to tray.", Color.Gray); } else { SaveSettings(); _hardwareManager.Close(); } }
        private void notifyIcon_MouseDoubleClick(object s, MouseEventArgs e) => this.Show();
        private void showToolStripMenuItem_Click(object s, EventArgs e) => this.Show();
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