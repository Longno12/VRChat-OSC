using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VrcOscChatbox
{
    public partial class LoadingForm : Form
    {
        private Timer _uiTimer;
        private int _dots = 0;

        public LoadingForm()
        {
            InitializeComponent();
            ApplyRoundedCorners();
            EnableDropShadow();

            this.Shown += LoadingForm_Shown;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            _uiTimer = new Timer { Interval = 500 };
            _uiTimer.Tick += (s, e) =>
            {
                _dots = (_dots + 1) % 4;
                var suffix = new string('.', _dots);
                lblStatus.Text = _cachedStatus + suffix;
            };
        }

        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) DragMove();
        }

        private void ApplyRoundedCorners()
        {
            var rgn = CreateRoundRectRgn(0, 0, Width + 1, Height + 1, 20, 20);
            Region = Region.FromHrgn(rgn);
            DeleteObject(rgn);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyRoundedCorners();
        }

        private void EnableDropShadow()
        {
            const int CS_DROPSHADOW = 0x00020000;
        }

        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private void DragMove()
        {
            ReleaseCapture();
            SendMessage(Handle, 0xA1, 0x2, 0);
        }

        [DllImport("gdi32.dll")] static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr hObject);

        private string _cachedStatus = "Initializing";

        public void UpdateStatus(string message)
        {
            _cachedStatus = message;
            lblStatus.Text = message;
        }

        private void LogBoot(string message, Color? color = null)
        {
            int start = rtbBoot.TextLength;
            rtbBoot.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            rtbBoot.Select(start, 10); rtbBoot.SelectionColor = Color.Gray; // timestamp
            rtbBoot.Select(start + 12, message.Length); rtbBoot.SelectionColor = color ?? Color.Gainsboro;
            rtbBoot.Select(rtbBoot.TextLength, 0);
            rtbBoot.ScrollToCaret();
        }

        private async void LoadingForm_Shown(object sender, EventArgs e)
        {
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            _uiTimer.Start();

            await StepTo(5, "Loading modules");
            LogBoot("Core: UI framework online");

            await StepTo(12, "Reading config");
            LogBoot("Config: locating config.json");
            LogBoot("Config: settings loaded", Color.Aquamarine);

            await StepTo(22, "Preparing presets");
            LogBoot("Presets: Default profile ready");

            await StepTo(35, "Wiring module toggles");
            LogBoot("Modules: Spotify/YouTube toggles", Color.LightGreen);
            LogBoot("Modules: CPU/RAM/GPU info toggles", Color.LightGreen);

            await StepTo(48, "Starting OSC client");
            LogBoot("OSC: UDPSender(127.0.0.1:9000)", Color.LightGreen);

            await StepTo(60, "Scanning media sources");
            LogBoot("Media: Spotify window hook");
            LogBoot("Media: YouTube title hook");

            await StepTo(72, "Initializing hardware monitor");
            LogBoot("HW: CPU/RAM/GPU providers bound");

            await StepTo(84, "Preparing animated text");
            LogBoot("UX: Animated text controller primed");

            await StepTo(92, "Applying VR/AFK settings");
            LogBoot("VR: Auto-AFK & Playspace options", Color.LightSkyBlue);

            await StepTo(97, "Loading system tools");
            LogBoot("System: Countdown & Shutdown hooks");

            await StepTo(100, "Finalizing");
            LogBoot("Tray: notify icon & menu online");
            LogBoot("All systems nominal", Color.LightGreen);

            var main = new Form1();
            main.FormClosed += (_, __) => this.Close();
            main.Show();
            this.Hide();
            _uiTimer.Stop();
        }

        private async Task StepTo(int target, string statusText)
        {
            UpdateStatus(statusText);
            if (target < progressBar.Value) target = progressBar.Value;

            while (progressBar.Value < target)
            {
                progressBar.Value += 1;
                await Task.Delay(18);
            }

            await Task.Delay(180);
        }
    }
}
