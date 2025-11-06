using System;
using System.Drawing;
using System.Windows.Forms;

namespace VrcOscChatbox
{
    public class TutorialForm : Form
    {
        private readonly string[] _steps = new[]
        {
            "Welcome! This app sends messages to the VRChat chatbox using OSC.",
            "Dashboard: Start/Stop broadcasting. The live preview shows what will be sent.",
            "Media: Enable Spotify/YouTube to display the current track or video title.",
            "System: Show CPU/RAM/GPU lines using your chosen formats.",
            "Appearance & Advanced: Animated text, presets, AFK mode, countdowns, and more."
        };
        private int _i = 0;
        private readonly Label _title, _content;
        private readonly Button _next, _back, _done;
        private readonly Panel _dots;

        public TutorialForm()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            Width = 640; Height = 360; Text = "Quick Tutorial";
            BackColor = Color.FromArgb(37, 37, 38); ForeColor = Color.Gainsboro;

            _title = new Label
            {
                Text = "Step 1 of 5",
                Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Location = new Point(22, 18),
                Size = new Size(580, 32)
            };
            _content = new Label
            {
                Text = _steps[0],
                Font = new Font("Segoe UI", 11f),
                AutoSize = false,
                Location = new Point(24, 64),
                Size = new Size(580, 170)
            };
            _back = new Button
            {
                Text = "Back",
                Enabled = false,
                Location = new Point(24, 280),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.Gainsboro
            };
            _back.FlatAppearance.BorderSize = 0; _back.Click += (s, e) => { if (_i > 0) { _i--; UpdateStep(); } };
            _next = new Button
            {
                Text = "Next",
                Location = new Point(444, 280),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(124, 58, 237),
                ForeColor = Color.White
            };
            _next.FlatAppearance.BorderSize = 0; _next.Click += (s, e) => { if (_i < _steps.Length - 1) { _i++; UpdateStep(); } };
            _done = new Button
            {
                Text = "Done",
                Location = new Point(532, 280),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.Gainsboro
            };
            _done.FlatAppearance.BorderSize = 0; _done.Click += (s, e) => Close();
            _dots = new Panel { Location = new Point(24, 244), Size = new Size(580, 20) };
            _dots.Paint += (s, e) => {
                for (int k = 0; k < _steps.Length; k++)
                {
                    var r = new Rectangle(10 + k * 24, 2, 12, 12);
                    e.Graphics.FillEllipse(new SolidBrush(k == _i ? Color.White : Color.FromArgb(120, 200, 200, 200)), r);
                }
            };

            Controls.AddRange(new Control[] { _title, _content, _back, _next, _done, _dots });
            UpdateStep();
        }
        private void UpdateStep()
        {
            _title.Text = $"Step {_i + 1} of {_steps.Length}";
            _content.Text = _steps[_i];
            _back.Enabled = _i > 0;
            _next.Enabled = _i < _steps.Length - 1;
            _dots.Invalidate();
        }
    }
}
