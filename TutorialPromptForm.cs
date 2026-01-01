using System;
using System.Drawing;
using System.Windows.Forms;

namespace VrcOscChatbox
{
    public enum TutorialPromptResult { Start, RemindLater, Never }

    public class TutorialPromptForm : Form
    {
        public TutorialPromptResult Result { get; private set; } = TutorialPromptResult.RemindLater;

        public TutorialPromptForm(bool isFirstRun)
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            Width = 520; Height = 260;
            Text = "Welcome";
            BackColor = Color.FromArgb(37, 37, 38);
            ForeColor = Color.Gainsboro;

            var title = new Label
            {
                Text = isFirstRun ? "Welcome to VRChat OSC Pro C#" : "Want a quick refresher?",
                Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Location = new Point(20, 18),
                Size = new Size(470, 32)
            };
            var body = new Label
            {
                Text = isFirstRun
                    ? "Would you like a short guided tour of the features? It takes about a minute."
                    : "It’s been a while. Do you want to run the quick tutorial again?",
                Font = new Font("Segoe UI", 10f),
                AutoSize = false,
                Location = new Point(22, 60),
                Size = new Size(470, 48)
            };
            var btnStart = new Button
            {
                Text = "Start tutorial",
                BackColor = Color.FromArgb(124, 58, 237),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(24, 160),
                Size = new Size(140, 34)
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += (s, e) => { Result = TutorialPromptResult.Start; DialogResult = DialogResult.OK; };

            var btnLater = new Button
            {
                Text = "Remind me later",
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.Gainsboro,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(176, 160),
                Size = new Size(160, 34)
            };
            btnLater.FlatAppearance.BorderSize = 0;
            btnLater.Click += (s, e) => { Result = TutorialPromptResult.RemindLater; DialogResult = DialogResult.OK; };

            var btnNever = new Button
            {
                Text = "Don’t show again",
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.Gainsboro,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(344, 160),
                Size = new Size(140, 34)
            };
            btnNever.FlatAppearance.BorderSize = 0;
            btnNever.Click += (s, e) => { Result = TutorialPromptResult.Never; DialogResult = DialogResult.OK; };

            Controls.AddRange(new Control[] { title, body, btnStart, btnLater, btnNever });
        }
    }
}
