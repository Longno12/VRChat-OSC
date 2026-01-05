

// FIXED IN NEXT UPDATE














/*using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VrcOscChatbox
{
    public class ModernTutorialForm : Form
    {
        private class TutorialStep
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Icon { get; set; }
            public Color Color { get; set; }
            public string[] Tips { get; set; }
            public bool HasInteractiveDemo { get; set; }
        }

        private readonly List<TutorialStep> _steps = new List<TutorialStep>
        {
            new TutorialStep
            {
                Title = "Welcome to VRChat OSC Pro",
                Description = "Transform your VRChat experience with real-time system stats, media integration, and animated messages.",
                Icon = "🚀",
                Color = Color.FromArgb(124, 58, 237),
                Tips = new[] { "Works with any VRChat client", "No mods required" }
            },
            new TutorialStep
            {
                Title = "Dashboard & Live Preview",
                Description = "Start/Stop broadcasting with one click. Watch your message build in real-time before it's sent.",
                Icon = "📊",
                Color = Color.FromArgb(56, 189, 248),
                HasInteractiveDemo = true
            },
            new TutorialStep
            {
                Title = "Media Integration",
                Description = "Automatically display Spotify tracks and YouTube videos. Works with most media players.",
                Icon = "🎵",
                Color = Color.FromArgb(255, 107, 129),
                Tips = new[] { "Spotify must be running", "YouTube in browser works" }
            },
            new TutorialStep
            {
                Title = "System Monitoring",
                Description = "Show CPU, GPU, and RAM usage with customizable formatting. Updates in real-time.",
                Icon = "⚙️",
                Color = Color.FromArgb(46, 204, 113),
                Tips = new[] { "Temperature monitoring", "Custom formatting available" }
            },
            new TutorialStep
            {
                Title = "Creative Features",
                Description = "Animated text, AFK mode, countdowns, and personal status. Make your chatbox unique.",
                Icon = "✨",
                Color = Color.FromArgb(241, 196, 15),
                Tips = new[] { "Create scrolling messages", "Set custom countdowns" }
            },
            new TutorialStep
            {
                Title = "Presets & Settings",
                Description = "Save different configurations as presets. Toggle features on/off with one click.",
                Icon = "💾",
                Color = Color.FromArgb(155, 89, 182),
                Tips = new[] { "Create gaming/work presets", "Export/import settings" }
            }
        };

        private int _currentStep = 0;
        private float _transitionProgress = 0f;
        private Timer _transitionTimer;
        private Timer _particleTimer;
        private List<Particle> _particles = new List<Particle>();
        private bool _animationsEnabled = true;

        // UI Controls
        private Panel _titleBar;
        private Label _stepCounter;
        private FlowLayoutPanel _stepDots;
        private Label _stepTitle;
        private Label _stepDescription;
        private Label _stepIcon;
        private Panel _tipsPanel;
        private Button _prevButton;
        private Button _nextButton;
        private Panel _demoPanel;

        public ModernTutorialForm(TutorialState state)
        {
            _animationsEnabled = state.EnableAnimations;
            InitializeForm();
            InitializeControls();
            SetupAnimations();
            UpdateStep(animate: false);
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(800, 600);
            BackColor = Color.FromArgb(15, 15, 22);
            Text = "Interactive Tutorial";
            DoubleBuffered = true;
            TopMost = true;

            Region = CreateRoundedRegion(ClientRectangle, 15);
        }

        private void InitializeControls()
        {
            // Title Bar
            _titleBar = new Panel { Height = 70, Dock = DockStyle.Top, BackColor = Color.Transparent };
            _titleBar.Paint += PaintTitleBar;
            _titleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };

            var title = new Label
            {
                Text = "Interactive Tutorial",
                Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 22),
                AutoSize = true
            };

            _stepCounter = new Label
            {
                Text = $"Step 1 of {_steps.Count}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(180, 180, 200),
                Location = new Point(650, 25),
                AutoSize = true
            };

            var closeButton = CreateCloseButton();
            _titleBar.Controls.AddRange(new Control[] { title, _stepCounter, closeButton });

            // Main content panel
            var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(40, 20, 40, 20) };

            _stepDots = new FlowLayoutPanel { Location = new Point(0, 0), Size = new Size(720, 40), BackColor = Color.Transparent };
            _stepIcon = new Label { Size = new Size(120, 120), Location = new Point(340, 60), Font = new Font("Segoe UI Emoji", 48), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            _stepTitle = new Label { Size = new Size(720, 50), Location = new Point(40, 200), Font = new Font("Segoe UI Semibold", 24, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
            _stepDescription = new Label { Size = new Size(720, 80), Location = new Point(40, 260), Font = new Font("Segoe UI", 14), ForeColor = Color.FromArgb(200, 200, 220), TextAlign = ContentAlignment.MiddleCenter };
            _tipsPanel = new Panel { Size = new Size(720, 60), Location = new Point(40, 350), BackColor = Color.Transparent };
            _demoPanel = new Panel { Size = new Size(400, 80), Location = new Point(200, 420), BackColor = Color.FromArgb(30, 30, 40), Visible = false };
            _demoPanel.Paint += PaintDemoPanel;

            _prevButton = new ModernButton { Text = "◄ Previous", Size = new Size(120, 45), Location = new Point(40, 520), BackColor = Color.FromArgb(50, 50, 65), ForeColor = Color.FromArgb(180, 180, 200), CornerRadius = 10, Font = new Font("Segoe UI", 10) };
            _prevButton.Click += (s, e) => PreviousStep();

            _nextButton = new ModernButton { Text = "Next ►", Size = new Size(120, 45), Location = new Point(640, 520), BackColor = _steps[0].Color, ForeColor = Color.White, CornerRadius = 10, Font = new Font("Segoe UI Semibold", 11) };
            _nextButton.Click += (s, e) => NextStep();

            var skipButton = new ModernButton { Text = "Skip Tutorial", Size = new Size(100, 35), Location = new Point(350, 525), BackColor = Color.Transparent, ForeColor = Color.FromArgb(150, 150, 170), CornerRadius = 8, Font = new Font("Segoe UI", 9) };
            skipButton.Click += (s, e) => CloseWithAnimation();

            contentPanel.Controls.AddRange(new Control[] { _stepDots, _stepIcon, _stepTitle, _stepDescription, _tipsPanel, _demoPanel, _prevButton, _nextButton, skipButton });
            Controls.Add(contentPanel);
            Controls.Add(_titleBar);
        }

        private void SetupAnimations()
        {
            if (_animationsEnabled)
            {
                _transitionTimer = new Timer { Interval = 16 };
                _transitionTimer.Tick += (s, e) =>
                {
                    _transitionProgress = Math.Min(_transitionProgress + 0.08f, 1f);
                    UpdateTransition();
                    if (_transitionProgress >= 1f) _transitionTimer.Stop();
                };

                _particleTimer = new Timer { Interval = 30 };
                _particleTimer.Tick += (s, e) => UpdateParticles();
            }
        }

        private void UpdateStep(bool animate = true)
        {
            var step = _steps[_currentStep];

            if (animate && _animationsEnabled) { _transitionProgress = 0f; _transitionTimer?.Start(); }

            _stepCounter.Text = $"Step {_currentStep + 1} of {_steps.Count}";
            _stepIcon.Text = step.Icon;
            _stepTitle.Text = step.Title;
            _stepDescription.Text = step.Description;
            _nextButton.Text = _currentStep == _steps.Count - 1 ? "Finish ★" : "Next ►";
            _nextButton.BackColor = step.Color;
            _prevButton.Enabled = _currentStep > 0;

            UpdateStepDots();
            UpdateTips();
            UpdateDemoPanel();

            if (_currentStep == _steps.Count - 1 && _animationsEnabled) CreateCompletionParticles();
        }

        private void UpdateStepDots()
        {
            _stepDots.Controls.Clear();
            for (int i = 0; i < _steps.Count; i++)
            {
                var dot = new Panel { Size = i == _currentStep ? new Size(20, 20) : new Size(12, 12), Margin = new Padding(8, 10, 8, 10), Cursor = Cursors.Hand };
                int index = i;
                dot.Click += (s, e) => { if (index != _currentStep) { _currentStep = index; UpdateStep(); } };
                dot.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    if (i == _currentStep)
                    {
                        using (var brush = new SolidBrush(_steps[i].Color)) e.Graphics.FillEllipse(brush, 0, 0, dot.Width, dot.Height);
                        using (var pen = new Pen(Color.White, 2)) e.Graphics.DrawEllipse(pen, 1, 1, dot.Width - 3, dot.Height - 3);
                    }
                    else if (i < _currentStep) using (var brush = new SolidBrush(Color.FromArgb(100, _steps[i].Color.R, _steps[i].Color.G, _steps[i].Color.B))) e.Graphics.FillEllipse(brush, 0, 0, dot.Width, dot.Height);
                    else using (var brush = new SolidBrush(Color.FromArgb(60, 60, 80))) e.Graphics.FillEllipse(brush, 0, 0, dot.Width, dot.Height);
                };
                _stepDots.Controls.Add(dot);
            }
        }

        private void UpdateTips()
        {
            _tipsPanel.Controls.Clear();
            var step = _steps[_currentStep];
            if (step.Tips != null && step.Tips.Length > 0)
            {
                int y = 0;
                foreach (var tip in step.Tips)
                {
                    var tipLabel = new Label
                    {
                        Text = $"💡 {tip}",
                        Font = new Font("Segoe UI", 10),
                        ForeColor = Color.FromArgb(180, 220, 255),
                        Location = new Point(0, y),
                        Size = new Size(720, 25),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    _tipsPanel.Controls.Add(tipLabel);
                    y += 25;
                }
            }
        }

        private void UpdateDemoPanel()
        {
            var step = _steps[_currentStep];
            _demoPanel.Visible = step.HasInteractiveDemo;

            if (_demoPanel.Visible)
            {
                _demoPanel.Controls.Clear();
                _demoPanel.Location = new Point((this.ClientSize.Width - _demoPanel.Width) / 2, 420);
                var demoLabel = new Label
                {
                    Text = "Try it:",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(180, 180, 200),
                    AutoSize = true
                };
                demoLabel.Location = new Point((_demoPanel.Width - demoLabel.Width) / 2, 10);
                var demoButton = new ModernButton
                {
                    Text = "Click me!",
                    Size = new Size(100, 35),
                    BackColor = step.Color,
                    ForeColor = Color.White,
                    CornerRadius = 8
                };
                demoButton.Location = new Point((_demoPanel.Width - demoButton.Width) / 2, 35);
                demoButton.Click += (s, e) =>
                {
                    System.Media.SystemSounds.Beep.Play();
                    ToastManager.Show("Demo", "Great! You're learning fast!", ToastType.Success, 1500);
                };
                _demoPanel.Controls.AddRange(new Control[] { demoLabel, demoButton });
            }
        }


        private void UpdateTransition()
        {
            float ease = (float)(Math.Sin(_transitionProgress * Math.PI - Math.PI / 2) + 1) / 2;
            _stepIcon.ForeColor = Color.FromArgb((int)(ease * 255), Color.White);
            _stepTitle.ForeColor = Color.FromArgb((int)(ease * 255), Color.White);
            _stepDescription.ForeColor = Color.FromArgb((int)(ease * 180), Color.FromArgb(200, 200, 220));
            float scale = 0.7f + ease * 0.3f;
            _stepIcon.Font = new Font(_stepIcon.Font.FontFamily, 48 * scale);
            _stepIcon.Location = new Point(340 + (int)((1 - scale) * 60), 60 + (int)((1 - scale) * 60));
        }

        private void PreviousStep() { if (_currentStep > 0) { _currentStep--; UpdateStep(); } }
        private void NextStep() { if (_currentStep < _steps.Count - 1) { _currentStep++; UpdateStep(); } else CloseWithAnimation(); }

        private void CreateCompletionParticles()
        {
            var random = new Random();
            for (int i = 0; i < 50; i++)
            {
                _particles.Add(new Particle
                {
                    X = random.Next(100, 700),
                    Y = 200,
                    Size = random.Next(4, 10),
                    Color = Color.FromArgb(
                        random.Next(150, 255),
                        random.Next(100, 255),
                        random.Next(100, 255),
                        random.Next(100, 255)),
                    VelocityX = (float)(random.NextDouble() - 0.5) * 6,
                    VelocityY = -random.Next(4, 12),
                    Life = 1.0f
                });
            }
            _particleTimer?.Start();
        }

        private void UpdateParticles()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.X += p.VelocityX;
                p.Y += p.VelocityY;
                p.VelocityY += 0.15f;
                p.Life -= 0.015f;
                if (p.Life <= 0) _particles.RemoveAt(i);
                else _particles[i] = new Particle { X = p.X, Y = p.Y, Size = p.Size, Color = Color.FromArgb((int)(p.Life * 255), p.Color.R, p.Color.G, p.Color.B), VelocityX = p.VelocityX, VelocityY = p.VelocityY, Life = p.Life };
            }
            if (_particles.Count == 0) _particleTimer?.Stop();
            Invalidate();
        }

        private void PaintTitleBar(object sender, PaintEventArgs e)
        {
            var gradient = new Rectangle(0, 0, _titleBar.Width, _titleBar.Height);
            using (var brush = new LinearGradientBrush(gradient, Color.FromArgb(30, 30, 40), Color.Transparent, 90f)) e.Graphics.FillRectangle(brush, gradient);
        }

        private void PaintDemoPanel(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CreateRoundedPath(_demoPanel.ClientRectangle, 10))
            using (var brush = new SolidBrush(Color.FromArgb(30, 30, 40)))
                e.Graphics.FillPath(brush, path);
            using (var path = CreateRoundedPath(_demoPanel.ClientRectangle, 10))
            using (var pen = new Pen(Color.FromArgb(60, 60, 80), 1))
                e.Graphics.DrawPath(pen, path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(15, 15, 22), Color.FromArgb(25, 25, 35), 135f))
                e.Graphics.FillRectangle(brush, ClientRectangle);
            using (var pen = new Pen(Color.FromArgb(40, 40, 60), 1))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            if (_animationsEnabled)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                lock (_particles)
                {
                    foreach (var particle in _particles)
                        using (var brush = new SolidBrush(particle.Color))
                            e.Graphics.FillEllipse(brush, particle.X, particle.Y, particle.Size, particle.Size);
                }
            }
        }

        private async void CloseWithAnimation()
        {
            if (_animationsEnabled) for (int i = 0; i < 20; i++) { Opacity = 1 - (i * 0.05f); await System.Threading.Tasks.Task.Delay(20); }
            DialogResult = DialogResult.OK;
            Close();
        }

        private PictureBox CreateCloseButton()
        {
            var button = new PictureBox { Size = new Size(32, 32), Location = new Point(Width - 40, 19), Cursor = Cursors.Hand };
            button.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(180, 180, 200), 2))
                {
                    e.Graphics.DrawLine(pen, 8, 8, 24, 24);
                    e.Graphics.DrawLine(pen, 24, 8, 8, 24);
                }
            };
            button.Click += (s, e) => CloseWithAnimation();
            return button;
        }

        private void ReleaseCapture() {}
        private void SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam) {}
        private Region CreateRoundedRegion(Rectangle rect, int radius) => new Region(CreateRoundedPath(rect, radius));
        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        public class TutorialState
        {
            public bool EnableAnimations { get; set; } = true;
            public bool MaybeAskSoundPref { get; set; } = true;
            public DateTime? FirstRunUtc { get; set; }
            public DateTime LastPromptUtc { get; set; }

            public static void AskPreferences(Form parent)
            {
                MessageBox.Show(parent, "Set your sound preferences now.", "Preferences", MessageBoxButtons.OK);
            }
        }


        public static class ToastManager { public static void Show(string title, string message, ToastType type, int duration) { } }
        public enum ToastType { Success, Info, Warning, Error }
        private struct Particle { public float X, Y, Size, VelocityX, VelocityY, Life; public Color Color; }
        public class ModernButton : Button { public int CornerRadius { get; set; } }
    }
}
*/