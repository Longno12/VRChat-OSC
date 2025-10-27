using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

public class ModernPanel : Panel
{
    public string Title { get; set; }
    public ModernPanel() { this.BackColor = Color.FromArgb(37, 37, 38); this.ForeColor = Color.Gainsboro; this.Font = new Font("Segoe UI", 9.75F); this.Padding = new Padding(1); }
    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using (var pen = new Pen(Color.FromArgb(80, 80, 80))) { e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1); } if (!string.IsNullOrEmpty(this.Title)) { e.Graphics.DrawString($" {this.Title} —", this.Font, new SolidBrush(this.ForeColor), 4, 4); } }
}