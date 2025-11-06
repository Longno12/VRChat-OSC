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

        [DefaultValue(0)]
        public int Value
        {
            get => _value;
            set { _value = Math.Max(0, Math.Min(_maximum, value)); Invalidate(); }
        }

        [DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set { _maximum = Math.Max(1, value); _value = Math.Min(_value, _maximum); Invalidate(); }
        }

        [DefaultValue(typeof(Color), "#7C3AED")] public Color StartColor { get; set; } = Color.FromArgb(0x7C, 0x3A, 0xED);
        [DefaultValue(typeof(Color), "#A78BFA")] public Color EndColor { get; set; } = Color.FromArgb(0xA7, 0x8B, 0xFA);
        [DefaultValue(typeof(Color), "#2A2A2A")] public Color TrackColor { get; set; } = Color.FromArgb(0x2A, 0x2A, 0x2A);

        public ModernProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            Height = 12;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            float radius = Height; // fully rounded
            using (GraphicsPath track = RoundRect(ClientRectangle, radius))
            using (SolidBrush trackBrush = new SolidBrush(TrackColor))
            {
                e.Graphics.FillPath(trackBrush, track);
            }

            if (_value <= 0) return;

            float pct = (float)_value / _maximum;
            int fillW = Math.Max(4, (int)(pct * Width));

            Rectangle fillRect = new Rectangle(0, 0, fillW, Height);
            using (GraphicsPath fill = RoundRect(fillRect, radius))
            using (LinearGradientBrush lg = new LinearGradientBrush(fillRect, StartColor, EndColor, LinearGradientMode.Horizontal))
            {
                e.Graphics.FillPath(lg, fill);
            }


            using (GraphicsPath gloss = RoundRect(new Rectangle(2, 2, Math.Max(0, fillW - 4), Math.Max(0, Height / 2 - 3)), radius))
            using (SolidBrush glossBrush = new SolidBrush(Color.FromArgb(40, Color.White)))
            {
                if (gloss.PointCount > 0) e.Graphics.FillPath(glossBrush, gloss);
            }
        }

        private static GraphicsPath RoundRect(Rectangle r, float radius)
        {
            float d = radius;
            GraphicsPath path = new GraphicsPath();
            float r2 = d / 2f;

            if (r.Width <= 0 || r.Height <= 0)
                return path;

            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
