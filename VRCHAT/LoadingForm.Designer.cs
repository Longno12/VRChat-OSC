using System.Drawing;
using System.Windows.Forms;

namespace VrcOscChatbox
{
    partial class LoadingForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblStatus;
        private ModernProgressBar progressBar;
        private RichTextBox rtbBoot;
        private Panel header;
        private Label lblVersion;
        private PictureBox pbIcon;
        private Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.header = new System.Windows.Forms.Panel();
            this.pbIcon = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.progressBar = new VrcOscChatbox.ModernProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.rtbBoot = new System.Windows.Forms.RichTextBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.header.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.SuspendLayout();

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = Color.FromArgb(22, 22, 24);
            this.ClientSize = new System.Drawing.Size(520, 360);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LoadingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VRChat OSC Pro C#";

            this.Padding = new Padding(1);

            this.header.Dock = DockStyle.Top;
            this.header.Height = 92;
            this.header.BackColor = Color.FromArgb(36, 14, 66);
            this.header.Paint += (s, e) => { using (var lg = new System.Drawing.Drawing2D.LinearGradientBrush(this.header.ClientRectangle, Color.FromArgb(124, 58, 237), Color.FromArgb(39, 19, 79), 0f)) { e.Graphics.FillRectangle(lg, this.header.ClientRectangle); } };
            this.header.MouseDown += Header_MouseDown;

            this.pbIcon.Location = new System.Drawing.Point(16, 18);
            this.pbIcon.Size = new System.Drawing.Size(56, 56);
            this.pbIcon.SizeMode = PictureBoxSizeMode.Zoom;
            this.pbIcon.Image = SystemIcons.Information.ToBitmap();

            this.lblTitle.AutoSize = true;
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.Font = new Font("Segoe UI Semibold", 20.5F, FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(84, 20);
            this.lblTitle.Text = "VRChat OSC Pro C#";

            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.ForeColor = Color.FromArgb(216, 197, 255);
            this.lblSubtitle.Font = new Font("Segoe UI", 10F);
            this.lblSubtitle.Location = new System.Drawing.Point(86, 56);
            this.lblSubtitle.Text = "Optimizing modules, initializing OSC, and preparing UI…";

            this.btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.ForeColor = Color.White;
            this.btnClose.Text = "✕";
            this.btnClose.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnClose.BackColor = Color.Transparent;
            this.btnClose.Location = new System.Drawing.Point(480, 8);
            this.btnClose.Size = new System.Drawing.Size(30, 30);
            this.btnClose.Click += (s, e) => this.Close();

            this.progressBar.Location = new System.Drawing.Point(26, 116);
            this.progressBar.Size = new System.Drawing.Size(468, 14);
            this.progressBar.Maximum = 100;
            this.progressBar.Value = 0;

            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.ForeColor = Color.Gainsboro;
            this.lblStatus.Font = new Font("Segoe UI", 9.5F);
            this.lblStatus.Location = new System.Drawing.Point(26, 136);
            this.lblStatus.Size = new System.Drawing.Size(468, 20);
            this.lblStatus.Text = "Initializing…";

            this.rtbBoot.BackColor = Color.FromArgb(28, 28, 30);
            this.rtbBoot.BorderStyle = BorderStyle.None;
            this.rtbBoot.ForeColor = Color.Gainsboro;
            this.rtbBoot.Location = new System.Drawing.Point(26, 162);
            this.rtbBoot.ReadOnly = true;
            this.rtbBoot.ShortcutsEnabled = false;
            this.rtbBoot.Size = new System.Drawing.Size(468, 160);
            this.rtbBoot.Font = new Font("Consolas", 9F);
            this.rtbBoot.TabIndex = 3;
            this.rtbBoot.Text = "";

            this.lblVersion.ForeColor = Color.FromArgb(140, 140, 160);
            this.lblVersion.Font = new Font("Segoe UI", 8.5F);
            this.lblVersion.Location = new System.Drawing.Point(26, 327);
            this.lblVersion.Size = new System.Drawing.Size(468, 20);
            this.lblVersion.Text = "v1.0.0  •  Tip: You can minimize to tray from Settings.";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.header.Controls.Add(this.pbIcon);
            this.header.Controls.Add(this.lblTitle);
            this.header.Controls.Add(this.lblSubtitle);
            this.header.Controls.Add(this.btnClose);
            this.Controls.Add(this.header);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.rtbBoot);
            this.Controls.Add(this.lblVersion);

            this.ResumeLayout(false);
        }
        #endregion
    }
}
