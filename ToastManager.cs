using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using VRCHAT.Properties;

namespace VrcOscChatbox
{
    public enum ToastType { Info, Success, Warning, Error }

    public static class ToastManager
    {
        private static readonly List<ToastNotification> _active = new List<ToastNotification>();
        private const int Margin = 12;

        public static bool EnableSound = true;
        public static int SoundThrottleMs = 150;
        private static DateTime _lastSound = DateTime.MinValue;
        private static readonly Dictionary<ToastType, string> _custom = new Dictionary<ToastType, string>();

        public static void SetCustomSound(ToastType type, string wavPath)
        {
            if (File.Exists(wavPath)) _custom[type] = wavPath;
        }

        public static void Show(string title, string message, ToastType type = ToastType.Info, int durationMs = 2800)
        {
            Color accent = Color.DeepSkyBlue;
            if (type == ToastType.Success) accent = Color.FromArgb(16, 185, 129);
            else if (type == ToastType.Warning) accent = Color.FromArgb(245, 158, 11);
            else if (type == ToastType.Error) accent = Color.IndianRed;

            var toast = new ToastNotification(title, message, accent, durationMs);
            toast.FormClosed += (s, e) =>
            {
                _active.Remove(toast);
                LayoutToasts();
            };

            _active.Add(toast);
            LayoutToasts();
            toast.Show();

            PlaySound(type);
        }

        private static void LayoutToasts()
        {
            var area = Screen.PrimaryScreen.WorkingArea;
            int x = area.Right - ToastNotification.DefaultWidth - Margin;
            int y = area.Top + Margin;

            foreach (var t in _active)
            {
                t.Location = new Point(x, y);
                y += t.Height + Margin;
            }
        }

        private static void PlaySound(ToastType type)
        {
            if (!EnableSound) return;
            if ((DateTime.Now - _lastSound).TotalMilliseconds < SoundThrottleMs) return;
            _lastSound = DateTime.Now;

            try
            {
                string path;
                if (_custom.TryGetValue(type, out path) && File.Exists(path))
                {
                    using (var sp = new SoundPlayer(path)) sp.Play();
                    return;
                }
                var ums = GetEmbeddedWave();
                if (ums != null)
                {
                    ums.Position = 0;
                    using (var sp = new SoundPlayer(ums)) sp.Play();
                    return;
                }
                SystemSounds.Asterisk.Play();
            }
            catch { }
        }

        private static System.IO.UnmanagedMemoryStream GetEmbeddedWave()
        {
            return Resources.sound;
        }
    }
}
