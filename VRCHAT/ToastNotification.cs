using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VrcOscChatbox
{
    public class ToastNotification : Form
    {
        public const int DefaultWidth = 320;
        private const int DefaultHeight = 88;

        private readonly Timer _fadeTimer = new Timer();
        private readonly Timer _lifeTimer = new Timer();
        private double _phase = 0; // 0=fadeIn, 1=steady, 2=fadeOut

        public ToastNotification(string title, string message, Color accent, int durationMs)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Opacity = 0.0;
            BackColor = Color.FromArgb(37, 37, 38);
            Size = new Size(DefaultWidth, DefaultHeight);
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) ?.SetValue(this, true, null);
            var accentPanel = new Panel
            {
                BackColor = accent,
                Location = new Point(0, 0),
                Size = new Size(6, DefaultHeight)
            };
            Controls.Add(accentPanel);
            var lblTitle = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                AutoSize = false,
                Location = new Point(14, 10),
                Size = new Size(DefaultWidth - 24, 22)
            };
            Controls.Add(lblTitle);
            var lblMsg = new Label
            {
                Text = message,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 9f),
                AutoSize = false,
                Location = new Point(14, 32),
                Size = new Size(DefaultWidth - 24, 44)
            };
            Controls.Add(lblMsg);
            var btnX = new Button
            {
                Text = "✕",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(28, 24),
                Location = new Point(DefaultWidth - 34, 6),
                TabStop = false
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.Click += (s, e) => BeginFadeOut();
            Controls.Add(btnX);
            SetRoundRegion(12);
            _fadeTimer.Interval = 15;
            _fadeTimer.Tick += FadeTick;
            _lifeTimer.Interval = durationMs;
            _lifeTimer.Tick += (s, e) => BeginFadeOut();
            _phase = 0;
            _fadeTimer.Start();
            _lifeTimer.Start();
            this.Click += (s, e) => BeginFadeOut();
            foreach (Control c in Controls) c.Click += (s, e) => BeginFadeOut();
        }

        private void FadeTick(object sender, EventArgs e)
        {
            if (_phase == 0) // fade in
            {
                Opacity += 0.08;
                if (Opacity >= 0.98) { Opacity = 1; _phase = 1; }
            }
            else if (_phase == 2) // fade out
            {
                Opacity -= 0.08;
                if (Opacity <= 0) { _fadeTimer.Stop(); Close(); }
            }
        }

        private void BeginFadeOut()
        {
            _phase = 2;
            _lifeTimer.Stop();
        }

        private void SetRoundRegion(int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d = radius * 2;
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(Width - d, 0, d, d, 270, 90);
            path.AddArc(Width - d, Height - d, d, d, 0, 90);
            path.AddArc(0, Height - d, d, d, 90, 90);
            path.CloseAllFigures();
            Region = new Region(path);
        }
        protected override bool ShowWithoutActivation => true;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_TOPMOST = 0x00000008;
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
                return cp;
            }
        }
    }
}
