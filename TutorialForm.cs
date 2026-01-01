using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
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

        private readonly string[] _stepTitles = new[]
        {
            "Welcome to VRChat OSC Pro",
            "Dashboard Controls",
            "Media Integration",
            "System Monitoring",
            "Advanced Features"
        };

        private readonly Icon[] _stepIcons = new Icon[]
        {
            Icon.Welcome,
            Icon.Dashboard,
            Icon.Media,
            Icon.System,
            Icon.Advanced
        };

        private int _currentStep = 0;
        private float _animationProgress = 0f;
        private Timer _animationTimer;
        private List<Particle> _particles = new List<Particle>();

        private readonly Color _primaryColor = Color.FromArgb(124, 58, 237);
        private readonly Color _secondaryColor = Color.FromArgb(100, 45, 200);
        private readonly Color _backgroundColor = Color.FromArgb(18, 18, 24);
        private readonly Color _surfaceColor = Color.FromArgb(28, 28, 36);
        private readonly Color _textColor = Color.FromArgb(240, 240, 255);
        private readonly Color _mutedTextColor = Color.FromArgb(160, 160, 180);

        private Panel _titleBar;
        private Label _titleLabel;
        private PictureBox _closeButton;
        private Panel _contentPanel;
        private Label _stepTitleLabel;
        private Label _stepContentLabel;
        private FlowLayoutPanel _dotsPanel;
        private Button _prevButton;
        private Button _nextButton;
        private PictureBox _stepIcon;
        private Panel _progressBar;
        private Timer _particleTimer;

        public TutorialForm()
        {
            InitializeForm();
            InitializeControls();
            SetupTimers();
            UpdateStep(animate: false);
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(720, 480);
            BackColor = _backgroundColor;
            Text = "Quick Tutorial";
            DoubleBuffered = true;
            ShowInTaskbar = false;
            TopMost = true;
            Padding = new Padding(1);

            Region = CreateRoundedRegion(ClientRectangle, 15);
        }

        private void InitializeControls()
        {
            _titleBar = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(25, 25, 35)
            };
            _titleBar.MouseDown += TitleBar_MouseDown;
            _titleBar.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    _titleBar.ClientRectangle,
                    Color.FromArgb(30, 30, 40),
                    Color.FromArgb(25, 25, 35),
                    90f))
                {
                    e.Graphics.FillRectangle(brush, _titleBar.ClientRectangle);
                }
            };

            _titleLabel = new Label
            {
                Text = "Getting Started Guide",
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = _textColor,
                Location = new Point(20, 15),
                AutoSize = true
            };

            _closeButton = new PictureBox
            {
                Size = new Size(30, 30),
                Location = new Point(Width - 40, 10),
                Image = CreateCloseIcon(),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Hand
            };
            _closeButton.Click += (s, e) => CloseWithAnimation();
            _closeButton.MouseEnter += (s, e) => _closeButton.BackColor = Color.FromArgb(255, 50, 50);
            _closeButton.MouseLeave += (s, e) => _closeButton.BackColor = Color.Transparent;

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(40, 30, 40, 30)
            };

            _stepIcon = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(30, 30),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            _stepTitleLabel = new Label
            {
                AutoSize = false,
                Size = new Size(550, 40),
                Location = new Point(120, 30),
                Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold),
                ForeColor = _textColor
            };

            _stepContentLabel = new Label
            {
                AutoSize = false,
                Size = new Size(550, 120),
                Location = new Point(120, 80),
                Font = new Font("Segoe UI", 12),
                ForeColor = _mutedTextColor,
                Padding = new Padding(0, 10, 0, 0)
            };

            _progressBar = new Panel
            {
                Height = 4,
                Location = new Point(40, 220),
                Size = new Size(640, 4),
                BackColor = Color.FromArgb(50, 50, 65)
            };

            _dotsPanel = new FlowLayoutPanel
            {
                Location = new Point(40, 240),
                Size = new Size(640, 40),
                BackColor = Color.Transparent
            };
            UpdateDots();

            _prevButton = new ModernButton
            {
                Text = "← Previous",
                Size = new Size(120, 40),
                Location = new Point(40, 300),
                BackColor = Color.FromArgb(60, 60, 75),
                ForeColor = _textColor,
                CornerRadius = 8
            };
            _prevButton.Click += (s, e) => PreviousStep();

            _nextButton = new ModernButton
            {
                Text = "Next →",
                Size = new Size(120, 40),
                Location = new Point(560, 300),
                BackColor = _primaryColor,
                ForeColor = Color.White,
                CornerRadius = 8,
                HoverColor = Color.FromArgb(140, 70, 255)
            };
            _nextButton.Click += (s, e) => NextStep();

            var skipButton = new ModernButton
            {
                Text = "Skip Tutorial",
                Size = new Size(100, 32),
                Location = new Point(310, 310),
                BackColor = Color.Transparent,
                ForeColor = _mutedTextColor,
                UseFlatStyle = true
            };
            skipButton.Click += (s, e) => CloseWithAnimation();

            _contentPanel.Controls.Add(_stepIcon);
            _contentPanel.Controls.Add(_stepTitleLabel);
            _contentPanel.Controls.Add(_stepContentLabel);
            _contentPanel.Controls.Add(_progressBar);
            _contentPanel.Controls.Add(_dotsPanel);
            _contentPanel.Controls.Add(_prevButton);
            _contentPanel.Controls.Add(_nextButton);
            _contentPanel.Controls.Add(skipButton);

            _titleBar.Controls.Add(_titleLabel);
            _titleBar.Controls.Add(_closeButton);

            Controls.Add(_contentPanel);
            Controls.Add(_titleBar);
        }

        private void SetupTimers()
        {
            _animationTimer = new Timer { Interval = 16 };
            _animationTimer.Tick += (s, e) =>
            {
                _animationProgress = Math.Min(_animationProgress + 0.1f, 1f);
                UpdateAnimation();
                if (_animationProgress >= 1f)
                    _animationTimer.Stop();
            };

            _particleTimer = new Timer { Interval = 16 };
            _particleTimer.Tick += (s, e) => UpdateParticles();
        }

        private void UpdateStep(bool animate = true)
        {
            if (animate)
            {
                _animationProgress = 0f;
                _animationTimer.Start();
            }

            _stepTitleLabel.Text = _stepTitles[_currentStep];
            _stepContentLabel.Text = _steps[_currentStep];
            _stepIcon.Image = GetStepIcon(_stepIcons[_currentStep]);

            UpdateDots();
            UpdateButtons();

            int progressWidth = (int)(640 * ((_currentStep + 1) / (float)_steps.Length));
            _progressBar.Width = progressWidth;
            _progressBar.BackColor = _primaryColor;

            if (_currentStep == _steps.Length - 1)
            {
                CreateCelebrationParticles();
            }
        }

        private void UpdateDots()
        {
            _dotsPanel.Controls.Clear();
            for (int i = 0; i < _steps.Length; i++)
            {
                var dot = new Panel
                {
                    Size = new Size(12, 12),
                    Margin = new Padding(10, 15, 10, 15)
                };

                if (i == _currentStep)
                {
                    dot.BackColor = _primaryColor;
                    dot.Size = new Size(16, 16);
                    dot.Margin = new Padding(8, 13, 8, 13);
                }
                else
                {
                    dot.BackColor = Color.FromArgb(80, 80, 100);
                }

                dot.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(new SolidBrush(dot.BackColor), 0, 0, dot.Width, dot.Height);

                    if (i == _currentStep)
                    {
                        using (var pen = new Pen(Color.White, 2))
                        {
                            e.Graphics.DrawEllipse(pen, 1, 1, dot.Width - 3, dot.Height - 3);
                        }
                    }
                };

                var index = i;
                dot.Click += (s, e) =>
                {
                    _currentStep = index;
                    UpdateStep();
                };
                dot.Cursor = Cursors.Hand;

                _dotsPanel.Controls.Add(dot);
            }
        }

        private void UpdateButtons()
        {
            _prevButton.Enabled = _currentStep > 0;
            _prevButton.BackColor = _currentStep > 0 ?
                Color.FromArgb(80, 80, 100) :
                Color.FromArgb(50, 50, 65);

            _nextButton.Text = _currentStep == _steps.Length - 1 ? "Finish" : "Next →";
            _nextButton.BackColor = _primaryColor;
        }

        private void PreviousStep()
        {
            if (_currentStep > 0)
            {
                _currentStep--;
                UpdateStep();
            }
        }

        private void NextStep()
        {
            if (_currentStep < _steps.Length - 1)
            {
                _currentStep++;
                UpdateStep();
            }
            else
            {
                CloseWithAnimation();
            }
        }

        private void UpdateAnimation()
        {
            float alpha = _animationProgress;
            _stepTitleLabel.ForeColor = Color.FromArgb((int)(alpha * 255), _textColor);
            _stepContentLabel.ForeColor = Color.FromArgb((int)(alpha * 180), _mutedTextColor);
            _stepTitleLabel.Left = (int)(120 - (1 - alpha) * 20);
            _stepContentLabel.Left = (int)(120 - (1 - alpha) * 20);
            float scale = 0.8f + alpha * 0.2f;
            _stepIcon.Size = new Size((int)(80 * scale), (int)(80 * scale));
            _stepIcon.Left = 30 + (int)((80 - _stepIcon.Width) / 2);
            _stepIcon.Top = 30 + (int)((80 - _stepIcon.Height) / 2);
        }

        private void CreateCelebrationParticles()
        {
            var random = new Random();
            for (int i = 0; i < 30; i++)
            {
                _particles.Add(new Particle
                {
                    X = random.Next(100, 600),
                    Y = 150,
                    Size = random.Next(3, 8),
                    Color = Color.FromArgb(random.Next(150, 255),
                        random.Next(100, 255),
                        random.Next(100, 255),
                        random.Next(100, 255)),
                    VelocityX = (float)(random.NextDouble() - 0.5) * 4,
                    VelocityY = -random.Next(3, 8),
                    Life = 1.0f
                });
            }
            _particleTimer.Start();
        }

        private void UpdateParticles()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.X += p.VelocityX;
                p.Y += p.VelocityY;
                p.VelocityY += 0.2f;
                p.Life -= 0.02f;

                if (p.Life <= 0)
                {
                    _particles.RemoveAt(i);
                }
                else
                {
                    p.Color = Color.FromArgb((int)(p.Life * 255), p.Color);
                    _particles[i] = p;
                }
            }

            if (_particles.Count == 0)
                _particleTimer.Stop();

            _contentPanel.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var brush = new LinearGradientBrush(
                ClientRectangle,
                Color.FromArgb(15, 15, 20),
                Color.FromArgb(25, 25, 35),
                135f))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            using (var pen = new Pen(Color.FromArgb(60, 60, 80), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            lock (_particles)
            {
                foreach (var particle in _particles)
                {
                    using (var brush = new SolidBrush(particle.Color))
                    {
                        e.Graphics.FillEllipse(brush, particle.X, particle.Y, particle.Size, particle.Size);
                    }
                }
            }
        }

        private Image GetStepIcon(Icon icon)
        {
            var bmp = new Bitmap(80, 80);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, 80, 80),
                    _primaryColor,
                    _secondaryColor,
                    135f))
                {
                    g.FillEllipse(brush, 5, 5, 70, 70);
                }
                string symbol;
                switch (icon)
                {
                    case Icon.Welcome:
                        symbol = "👋";
                        break;
                    case Icon.Dashboard:
                        symbol = "📊";
                        break;
                    case Icon.Media:
                        symbol = "🎵";
                        break;
                    case Icon.System:
                        symbol = "⚙️";
                        break;
                    case Icon.Advanced:
                        symbol = "✨";
                        break;
                    default:
                        symbol = "❓";
                        break;
                }

                using (var font = new Font("Segoe UI Symbol", 24, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var size = g.MeasureString(symbol, font);
                    g.DrawString(symbol, font, brush, 40 - size.Width / 2, 40 - size.Height / 2);
                }
            }
            return bmp;
        }

        private Image CreateCloseIcon()
        {
            var bmp = new Bitmap(20, 20);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(_textColor, 2))
                {
                    g.DrawLine(pen, 5, 5, 15, 15);
                    g.DrawLine(pen, 15, 5, 5, 15);
                }
            }
            return bmp;
        }

        private Region CreateRoundedRegion(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return new Region(path);
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        private async void CloseWithAnimation()
        {
            for (int i = 0; i < 20; i++)
            {
                this.Opacity = 1 - (i * 0.05f);
                await Task.Delay(20);
            }
            Close();
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ReleaseCapture();

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        }

        private class ModernButton : Button
        {
            public int CornerRadius { get; set; } = 6;
            public Color HoverColor { get; set; } = Color.FromArgb(80, 80, 100);
            public bool UseFlatStyle { get; set; } = false;

            private bool _isHovered = false;

            public ModernButton()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
                base.FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                Cursor = Cursors.Hand;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                var backColor = _isHovered ? HoverColor : BackColor;

                if (UseFlatStyle)
                {
                    using (var brush = new SolidBrush(backColor))
                    using (var pen = new Pen(ForeColor, 1))
                    {
                        e.Graphics.FillRectangle(brush, ClientRectangle);
                        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                    }
                }
                else
                {
                    using (var path = CreateRoundedPath(ClientRectangle, CornerRadius))
                    using (var brush = new SolidBrush(backColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                }

                TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                _isHovered = true;
                Invalidate();
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                _isHovered = false;
                Invalidate();
            }

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
        }

        private enum Icon { Welcome, Dashboard, Media, System, Advanced }

        private struct Particle
        {
            public float X;
            public float Y;
            public float Size;
            public Color Color;
            public float VelocityX;
            public float VelocityY;
            public float Life;
        }
    }
}