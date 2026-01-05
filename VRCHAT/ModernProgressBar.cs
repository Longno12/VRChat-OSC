using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VrcOscChatbox
{
    public class ModernProgressBar : Control
    {
        private int _value = 0;
        private int _maximum = 100;
        private int _minimum = 0;
        private int _glowAlpha = 40;
        private bool _showGlow = true;

        [DefaultValue(0)]
        public int Value
        {
            get => _value;
            set
            {
                int newValue = Math.Max(_minimum, Math.Min(_maximum, value));
                if (_value != newValue)
                {
                    _value = newValue;
                    Invalidate();
                    OnValueChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(0)]
        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = Math.Max(0, value);
                if (_minimum > _maximum) _maximum = _minimum + 1;
                _value = Math.Max(_minimum, Math.Min(_maximum, _value));
                Invalidate();
            }
        }

        [DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = Math.Max(_minimum + 1, value);
                _value = Math.Min(_value, _maximum);
                Invalidate();
            }
        }

        [DefaultValue(typeof(Color), "#7C3AED")]
        public Color StartColor { get; set; } = Color.FromArgb(0x7C, 0x3A, 0xED);

        [DefaultValue(typeof(Color), "#A78BFA")]
        public Color EndColor { get; set; } = Color.FromArgb(0xA7, 0x8B, 0xFA);

        [DefaultValue(typeof(Color), "#2A2A2A")]
        public Color TrackColor { get; set; } = Color.FromArgb(0x2A, 0x2A, 0x2A);

        [DefaultValue(40)]
        public int GlowAlpha
        {
            get => _glowAlpha;
            set { _glowAlpha = Math.Max(0, Math.Min(255, value)); Invalidate(); }
        }

        [DefaultValue(true)]
        public bool ShowGlow { get; set; } = true;

        public event EventHandler ValueChanged;

        public ModernProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
            Height = 14;
            BackColor = Color.Transparent;
            DoubleBuffered = true;
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float percentage = 0f;
            if (_maximum > _minimum)
            {
                percentage = (float)(_value - _minimum) / (_maximum - _minimum);
            }

            using (GraphicsPath trackPath = RoundedRect(ClientRectangle, Height))
            using (LinearGradientBrush trackBrush = new LinearGradientBrush(
                ClientRectangle,
                Color.FromArgb(40, 40, 45),
                Color.FromArgb(35, 35, 40),
                0f))
            {
                e.Graphics.FillPath(trackBrush, trackPath);

                using (Pen borderPen = new Pen(Color.FromArgb(80, 60, 60, 65), 1))
                {
                    e.Graphics.DrawPath(borderPen, trackPath);
                }
            }

            if (_value > _minimum)
            {
                int fillWidth = Math.Max(8, (int)(percentage * Width));
                Rectangle fillRect = new Rectangle(0, 0, fillWidth, Height);

                using (GraphicsPath fillPath = RoundedRect(fillRect, Height))
                {
                    using (LinearGradientBrush gradientBrush = new LinearGradientBrush(
                        fillRect,
                        StartColor,
                        EndColor,
                        LinearGradientMode.Horizontal))
                    {
                        ColorBlend colorBlend = new ColorBlend(4);
                        colorBlend.Colors = new Color[]
                        {
                            Color.FromArgb(255, StartColor),
                            Color.FromArgb(255, EndColor),
                            Color.FromArgb(200, EndColor),
                            Color.FromArgb(150, EndColor)
                        };
                        colorBlend.Positions = new float[] { 0f, 0.6f, 0.9f, 1f };
                        gradientBrush.InterpolationColors = colorBlend;

                        e.Graphics.FillPath(gradientBrush, fillPath);
                    }

                    if (ShowGlow && fillWidth > 10)
                    {
                        using (GraphicsPath glowPath = RoundedRect(
                            new Rectangle(2, 2, Math.Max(0, fillWidth - 4), Math.Max(0, Height - 4)),
                            Height - 4))
                        using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(_glowAlpha, 255, 255, 255)))
                        {
                            e.Graphics.FillPath(glowBrush, glowPath);
                        }
                    }

                    if (fillWidth > 20)
                    {
                        using (SolidBrush highlightBrush = new SolidBrush(Color.FromArgb(80, 255, 255, 255)))
                        {
                            e.Graphics.FillEllipse(highlightBrush, fillWidth - 12, 3, 8, 8);
                        }
                    }

                    using (Pen progressBorder = new Pen(Color.FromArgb(100, 255, 255, 255), 1))
                    {
                        e.Graphics.DrawPath(progressBorder, fillPath);
                    }
                }
            }

            if (percentage > 0.3 && Width > 100)
            {
                string text = $"{percentage:P0}";
                using (StringFormat sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                using (Font textFont = new Font("Segoe UI Semibold", 8f, FontStyle.Bold))
                {
                    RectangleF textRect = new RectangleF(0, 0, Width, Height);
                    RectangleF shadowRect = new RectangleF(1, 1, Width, Height);
                    e.Graphics.DrawString(text, textFont, shadowBrush, shadowRect, sf);
                    e.Graphics.DrawString(text, textFont, textBrush, textRect, sf);
                }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (rect.Width <= 0 || rect.Height <= 0 || radius <= 0)
                return path;

            float diameter = radius;
            RectangleF arcRect = new RectangleF(rect.Location, new SizeF(diameter, diameter));

            path.AddArc(arcRect, 180, 90);
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}