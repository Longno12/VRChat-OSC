using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

public class ModernPanel : Panel
{
    private string _title;
    private Color _borderColor = Color.FromArgb(80, 80, 80);
    private Color _accentColor = Color.FromArgb(124, 58, 237);
    private int _titleHeight = 28;
    private bool _showTopAccent = true;

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            Invalidate();
        }
    }

    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            Invalidate();
        }
    }

    public Color AccentColor
    {
        get => _accentColor;
        set
        {
            _accentColor = value;
            Invalidate();
        }
    }

    public bool ShowTopAccent
    {
        get => _showTopAccent;
        set
        {
            _showTopAccent = value;
            Invalidate();
        }
    }

    public ModernPanel()
    {
        this.BackColor = Color.FromArgb(37, 37, 38);
        this.ForeColor = Color.FromArgb(220, 220, 220);
        this.Font = new Font("Segoe UI Semibold", 9.75F);
        this.Padding = new Padding(8, 32, 8, 8);
        this.DoubleBuffered = true;
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (var borderPen = new Pen(_borderColor, 1))
        {
            e.Graphics.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
        }
        if (!string.IsNullOrEmpty(_title))
        {
            if (_showTopAccent)
            {
                using (var accentBrush = new SolidBrush(_accentColor))
                {
                    e.Graphics.FillRectangle(accentBrush, 0, 0, this.Width, 2);
                }
            }
            using (var titleBgBrush = new SolidBrush(Color.FromArgb(30, 30, 32)))
            {
                e.Graphics.FillRectangle(titleBgBrush, 0, 0, this.Width, _titleHeight);
            }
            using (var titleFont = new Font(this.Font.FontFamily, 9.5f, FontStyle.Bold))
            using (var textBrush = new SolidBrush(this.ForeColor))
            {
                e.Graphics.DrawString($"  {_title}", titleFont, textBrush, 8, 6);

                using (var underlinePen = new Pen(_accentColor, 1))
                {
                    e.Graphics.DrawLine(underlinePen, 8, _titleHeight - 2, 80, _titleHeight - 2);
                }
            }
        }
        else if (_showTopAccent)
        {
            using (var accentBrush = new SolidBrush(_accentColor))
            {
                e.Graphics.FillRectangle(accentBrush, 0, 0, this.Width, 2);
            }
        }
    }
}