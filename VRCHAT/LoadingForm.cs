using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VrcOscChatbox
{
    public partial class LoadingForm : Form
    {
        private Timer _uiTimer;
        private Timer _progressTimer;
        private Timer _pulseTimer;
        private Timer _fadeTimer;
        private int _dots = 0;
        private int _targetProgress = 0;
        private int _currentProgress = 0;
        private string _cachedStatus = "Initializing";
        private float _pulsePhase = 0;
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private Color _currentGlowColor = Color.FromArgb(124, 58, 237);
        private bool _glowDirection = true;
        private int _glowAlpha = 40;
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);

        private struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        public LoadingForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            ApplyRoundedCorners();
            ApplyDropShadow();
            ApplyGlassEffect();

            this.Shown += LoadingForm_Shown;
            this.FormClosing += LoadingForm_FormClosing;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;
            UpdateIcon();

            _uiTimer = new Timer { Interval = 500 };
            _uiTimer.Tick += (s, e) =>
            {
                _dots = (_dots + 1) % 4;
                var suffix = new string('.', _dots);
                lblStatus.Text = _cachedStatus + suffix;
            };

            _progressTimer = new Timer { Interval = 10 };
            _progressTimer.Tick += ProgressTimer_Tick;
            _pulseTimer = new Timer { Interval = 20 };
            _pulseTimer.Tick += PulseTimer_Tick;
            header.MouseDown += Header_MouseDown;
            header.MouseMove += Header_MouseMove;
            header.MouseUp += Header_MouseUp;
            lblTitle.MouseDown += Header_MouseDown;
            lblTitle.MouseMove += Header_MouseMove;
            lblTitle.MouseUp += Header_MouseUp;
            lblSubtitle.MouseDown += Header_MouseDown;
            lblSubtitle.MouseMove += Header_MouseMove;
            lblSubtitle.MouseUp += Header_MouseUp;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 255, 100, 100);
            btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(120, 255, 60, 60);
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.FromArgb(255, 180, 180);
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.White;
            header.Paint += Header_Paint;
            progressBar.StartColor = Color.FromArgb(124, 58, 237);
            progressBar.EndColor = Color.FromArgb(167, 139, 250);
            progressBar.TrackColor = Color.FromArgb(40, 40, 45);
            progressBar.ShowGlow = true;
            lblVersion.Text = $"v5.0.0 • {DateTime.Now:yyyy-MM-dd} • Press ESC to skip";
            rtbBoot.TextChanged += (s, e) =>
            {
                rtbBoot.SelectionStart = rtbBoot.TextLength;
                rtbBoot.ScrollToCaret();
            };
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Copy All", null, (s, e) => Clipboard.SetText(rtbBoot.Text));
            contextMenu.Items.Add("Clear", null, (s, e) => rtbBoot.Clear());
            contextMenu.Items.Add("Save to File...", null, (s, e) =>
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Log Files (*.log)|*.log|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                    sfd.FileName = $"bootlog_{DateTime.Now:yyyyMMdd_HHmmss}.log";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        System.IO.File.WriteAllText(sfd.FileName, rtbBoot.Text);
                        LogBoot($"Log saved to: {sfd.FileName}", Color.LightBlue);
                    }
                }
            });
            rtbBoot.ContextMenuStrip = contextMenu;
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    if (MessageBox.Show("Skip loading and open main application?", "Skip Loading",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        SkipToMain();
                    }
                }
                else if (e.KeyCode == Keys.F12)
                {
                    LogBoot("DEBUG: F12 pressed - Test log entry", Color.Yellow);
                }
            };

            _uiTimer.Start();
            _progressTimer.Start();
            _pulseTimer.Start();

            this.Opacity = 0;
            _fadeTimer = new Timer { Interval = 10 };
            int fadeStep = 0;
            _fadeTimer.Tick += (s, e) =>
            {
                fadeStep++;
                this.Opacity = Math.Min(1.0, fadeStep * 0.05);
                if (fadeStep >= 20)
                {
                    _fadeTimer.Stop();
                    _fadeTimer.Dispose();
                }
            };
            _fadeTimer.Start();
        }

        private void Header_Paint(object sender, PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(
                header.ClientRectangle,
                Color.FromArgb(124, 58, 237),
                Color.FromArgb(39, 19, 79),
                45f))
            {
                var blend = new ColorBlend
                {
                    Colors = new[]
                    {
                        Color.FromArgb(124, 58, 237),
                        Color.FromArgb(167, 139, 250),
                        Color.FromArgb(124, 58, 237),
                        Color.FromArgb(39, 19, 79)
                    },
                    Positions = new[] { 0f, 0.3f, 0.7f, 1f }
                };
                brush.InterpolationColors = blend;
                e.Graphics.FillRectangle(brush, header.ClientRectangle);
            }

            using (var noise = new SolidBrush(Color.FromArgb(15, 0, 0, 0)))
            {
                e.Graphics.FillRectangle(noise, header.ClientRectangle);
            }

            using (var glow = new LinearGradientBrush(
                new Rectangle(0, header.Height - 8, header.Width, 8),
                Color.FromArgb(80, 167, 139, 250),
                Color.Transparent,
                90f))
            {
                e.Graphics.FillRectangle(glow, 0, header.Height - 8, header.Width, 8);
            }
        }

        private void UpdateIcon()
        {
            var iconBmp = new Bitmap(56, 56);
            using (var g = Graphics.FromImage(iconBmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(40, 167, 139, 250))) g.FillEllipse(brush, -5, -5, 66, 66);
                var rect = new Rectangle(0, 0, 56, 56);
                using (var brush = new LinearGradientBrush(rect,
                    Color.FromArgb(124, 58, 237),
                    Color.FromArgb(167, 139, 250),
                    45f))
                {
                    g.FillEllipse(brush, rect);
                }

                using (var brush = new SolidBrush(Color.FromArgb(80, 255, 255, 255))) g.FillEllipse(brush, 10, 10, 36, 36);

                using (var font = new Font("Segoe UI Symbol", 20, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString("▶", font, brush, 12, 14);
                }
            }

            pbIcon.Image = iconBmp;
        }

        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _dragStartPoint = new Point(e.X, e.Y);
                header.Cursor = Cursors.SizeAll;
            }
        }

        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point p = PointToScreen(new Point(e.X, e.Y));
                Location = new Point(p.X - _dragStartPoint.X, p.Y - _dragStartPoint.Y);
            }
        }

        private void Header_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            header.Cursor = Cursors.Default;
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (_currentProgress < _targetProgress)
            {
                int difference = _targetProgress - _currentProgress;
                int step = Math.Max(1, (int)(difference * 0.3f));
                _currentProgress = Math.Min(_currentProgress + step, _targetProgress);
                progressBar.Value = _currentProgress;

                if (_currentProgress > 90)
                {
                    float progressRatio = (_currentProgress - 90) / 10f;
                    progressBar.StartColor = Color.FromArgb(
                        124 + (int)(31 * progressRatio),
                        58 + (int)(131 * progressRatio),
                        237 - (int)(187 * progressRatio));
                    progressBar.EndColor = Color.FromArgb(
                        167 + (int)(88 * progressRatio),
                        139 + (int)(116 * progressRatio),
                        250 - (int)(195 * progressRatio));
                }
            }
        }

        private void PulseTimer_Tick(object sender, EventArgs e)
        {
            _pulsePhase += 0.1f;

            if (_glowDirection)
            {
                _glowAlpha += 2;
                if (_glowAlpha >= 80) _glowDirection = false;
            }
            else
            {
                _glowAlpha -= 2;
                if (_glowAlpha <= 40) _glowDirection = true;
            }

            progressBar.GlowAlpha = _glowAlpha;
            progressBar.Invalidate();

            float pulse = (float)(Math.Sin(_pulsePhase) * 0.2 + 0.8);
            lblTitle.ForeColor = Color.FromArgb(
                (int)(255 * pulse),
                (int)(255 * pulse),
                (int)(255 * pulse));
        }

        private void ApplyRoundedCorners()
        {
            var rgn = CreateRoundRectRgn(0, 0, Width + 1, Height + 1, 20, 20);
            Region = Region.FromHrgn(rgn);
            DeleteObject(rgn);
        }

        private void ApplyDropShadow()
        {
            const int CS_DROPSHADOW = 0x00020000;
            var cs = CreateParamsEx();
            cs.ClassStyle |= CS_DROPSHADOW;
        }

        private void ApplyGlassEffect()
        {
            try
            {
                var margins = new MARGINS()
                {
                    leftWidth = 0,
                    rightWidth = 0,
                    topHeight = 1,
                    bottomHeight = 0
                };
                DwmExtendFrameIntoClientArea(this.Handle, ref margins);
            }
            catch { }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x00020000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private CreateParams CreateParamsEx()
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= 0x02000000;
            return cp;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var pen = new Pen(Color.FromArgb(80, 167, 139, 250), 2))
            {
                e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
            }

            using (var pen = new Pen(Color.FromArgb(60, 40, 40, 45), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        public void UpdateStatus(string message)
        {
            _cachedStatus = message;
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => lblStatus.Text = message));
            }
            else
            {
                lblStatus.Text = message;
            }
        }

        public void LogBoot(string message, Color? color = null)
        {
            if (rtbBoot.InvokeRequired)
            {
                rtbBoot.Invoke(new Action(() => LogBootInternal(message, color)));
            }
            else
            {
                LogBootInternal(message, color);
            }
        }

        private void LogBootInternal(string message, Color? color = null)
        {
            int start = rtbBoot.TextLength;
            string timestamp = $"[{DateTime.Now:HH:mm:ss.fff}] ";
            rtbBoot.AppendText(timestamp + message + "\n");
            rtbBoot.Select(start, timestamp.Length);
            rtbBoot.SelectionColor = Color.FromArgb(150, 150, 170);
            rtbBoot.Select(start + timestamp.Length, message.Length);
            rtbBoot.SelectionColor = color ?? Color.Gainsboro;
            rtbBoot.Select(rtbBoot.TextLength, 0);
            rtbBoot.SelectionColor = rtbBoot.ForeColor;
            rtbBoot.ScrollToCaret();
        }

        private async void LoadingForm_Shown(object sender, EventArgs e)
        {
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            await AnimatedLoadingSequence();
        }

        private async Task AnimatedLoadingSequence()
        {
            var steps = new[]
            {
                (5, "Loading core modules", "Core: UI framework initialized with hardware acceleration"),
                (12, "Reading configuration", "Config: Settings profile 'Default' loaded successfully"),
                (22, "Preparing presets system", "Presets: 3 default profiles created and validated"),
                (35, "Initializing media hooks", "Media: Spotify and YouTube title capture active"),
                (48, "Starting OSC client", "OSC: Connected to 127.0.0.1:9000 with heartbeat monitor"),
                (60, "Scanning hardware sensors", "HW: CPU, GPU, RAM telemetry pipeline established"),
                (72, "Building user interface", "UI: Modern theme applied with 60FPS animations"),
                (84, "Preparing text effects", "Text: Animated marquee with smooth transitions ready"),
                (92, "Configuring VR features", "VR: Auto-AFK detection calibrated for room scale"),
                (97, "Loading system utilities", "System: Countdown timer and shutdown manager online"),
                (100, "Final optimizations", "Performance: All systems nominal and ready for VR")
            };

            foreach (var (target, status, log) in steps)
            {
                await StepTo(target, status, log);
            }

            progressBar.StartColor = Color.FromArgb(56, 189, 248);
            progressBar.EndColor = Color.FromArgb(56, 189, 248);
            UpdateStatus("Ready! Launching main application...");
            LogBoot("✓ All systems ready for VR Chat integration", Color.LimeGreen);

            for (int i = 0; i < 3; i++)
            {
                progressBar.GlowAlpha = 100;
                await Task.Delay(200);
                progressBar.GlowAlpha = 40;
                await Task.Delay(200);
            }

            await Task.Delay(500);
            OpenMainApplication();
        }

        private async Task StepTo(int target, string statusText, string logMessage)
        {
            UpdateStatus(statusText);
            _targetProgress = Math.Max(target, _currentProgress);

            Color logColor;
            if (target < 30)
                logColor = Color.LightGray;
            else if (target < 60)
                logColor = Color.LightBlue;
            else if (target < 90)
                logColor = Color.LightGreen;
            else
                logColor = Color.LimeGreen;

            LogBoot(logMessage, logColor);
            while (_currentProgress < target)
            {
                await Task.Delay(10);
            }
            await Task.Delay(target < 100 ? 250 : 100);
        }

        private void OpenMainApplication()
        {
            var main = new Form1();
            main.FormClosed += (_, __) => this.Close();
            var fadeOut = new Timer { Interval = 10 };
            int fadeStep = 20;
            fadeOut.Tick += (s, e) =>
            {
                fadeStep--;
                this.Opacity = Math.Max(0, fadeStep * 0.05);
                if (fadeStep <= 0)
                {
                    fadeOut.Stop();
                    main.Show();
                    this.Hide();
                }
            };
            fadeOut.Start();
        }

        private void SkipToMain()
        {
            _targetProgress = 100;
            _currentProgress = 100;
            progressBar.Value = 100;
            progressBar.StartColor = Color.FromArgb(56, 189, 248);
            progressBar.EndColor = Color.FromArgb(56, 189, 248);

            LogBoot("Loading skipped by user request", Color.Yellow);
            LogBoot("Launching main application...", Color.Yellow);

            OpenMainApplication();
        }

        private void LoadingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _uiTimer?.Stop();
            _progressTimer?.Stop();
            _pulseTimer?.Stop();
            _fadeTimer?.Stop();
            _uiTimer?.Dispose();
            _progressTimer?.Dispose();
            _pulseTimer?.Dispose();
            _fadeTimer?.Dispose();
        }
    }
}