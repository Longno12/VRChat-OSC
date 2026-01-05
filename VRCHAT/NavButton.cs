using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;

public class NavButton : Button
{
    private char _icon;
    private bool _isActive;
    private Color _activeColor = Color.FromArgb(0, 120, 215);
    private Color _inactiveColor = Color.FromArgb(45, 45, 48);
    private Color _hoverColor = Color.FromArgb(62, 62, 64);
    private Color _textColor = Color.FromArgb(241, 241, 241);
    private Color _activeTextColor = Color.White;

    public char Icon
    {
        get => _icon;
        set { _icon = value; Invalidate(); }
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                Invalidate();
            }
        }
    }

    public Color ActiveColor
    {
        get => _activeColor;
        set { _activeColor = value; Invalidate(); }
    }

    public Color InactiveColor
    {
        get => _inactiveColor;
        set { _inactiveColor = value; Invalidate(); }
    }

    public NavButton()
    {
        this.FlatStyle = FlatStyle.Flat;
        this.FlatAppearance.BorderSize = 0;
        this.FlatAppearance.MouseOverBackColor = _hoverColor;
        this.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 80, 175);
        this.TextAlign = ContentAlignment.MiddleLeft;
        this.ForeColor = _textColor;
        this.Font = new Font("Segoe UI Semilight", 11.25F);
        this.Padding = new Padding(20, 0, 0, 0);
        this.Height = 42;
        this.Cursor = Cursors.Hand;
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        Color backColor = IsActive ? _activeColor : _inactiveColor;
        using (var brush = new SolidBrush(backColor))
        {
            pevent.Graphics.FillRectangle(brush, ClientRectangle);
        }
        if (IsActive)
        {
            using (var accentBrush = new SolidBrush(Color.FromArgb(255, 241, 148)))
            {
                pevent.Graphics.FillRectangle(accentBrush, 0, 0, 4, this.Height);
            }
        }
        using (var iconFont = new Font("Segoe MDL2 Assets", 12F, FontStyle.Regular))
        {
            Color iconColor = IsActive ? _activeTextColor : _textColor;
            pevent.Graphics.DrawString(_icon.ToString(), iconFont, new SolidBrush(iconColor), 20, (this.Height - iconFont.Height) / 2);
        }
        Color textColor = IsActive ? _activeTextColor : _textColor;
        using (var textBrush = new SolidBrush(textColor))
        {
            pevent.Graphics.DrawString(this.Text, this.Font, textBrush, 50, (this.Height - this.Font.Height) / 2 + 1);
        }
    }
}