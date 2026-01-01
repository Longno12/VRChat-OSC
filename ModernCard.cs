using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VrcOscChatbox
{
    public class ModernCard : Panel
    {
        [Browsable(true)] public string Title { get; set; } = "";
        [Browsable(true)] public int CornerRadius { get; set; } = 12;

        public ModernCard()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(37, 37, 38);
            ForeColor = Color.Gainsboro;
            Padding = new Padding(16);
            Margin = new Padding(0, 0, 0, 16);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = ClientRectangle;
            rect.Inflate(-1, -1);

            var shadow = rect; shadow.Offset(0, 3);
            using (var sb = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            using (var shadowPath = RoundRect(shadow, CornerRadius + 2)) e.Graphics.FillPath(sb, shadowPath);

            using (var path = RoundRect(rect, CornerRadius))
            using (var bg = new SolidBrush(BackColor))
            using (var pen = new Pen(Color.FromArgb(60, 255, 255, 255)))
            {
                e.Graphics.FillPath(bg, path);
                e.Graphics.DrawPath(pen, path);
            }

            using (var stripe = new LinearGradientBrush(
                new Rectangle(rect.X, rect.Y, rect.Width, 22),
                Color.FromArgb(124, 58, 237), Color.FromArgb(39, 19, 79), 0f))
            {
                e.Graphics.FillRectangle(stripe, rect.X, rect.Y, rect.Width, 22);
            }

            if (!string.IsNullOrEmpty(Title))
            {
                using (var f = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold))
                using (var br = new SolidBrush(Color.White))
                    e.Graphics.DrawString(Title, f, br, rect.X + 10, rect.Y + 3);
            }
        }

        private static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            int d = Math.Max(1, radius * 2);
            var gp = new GraphicsPath();
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }
}
