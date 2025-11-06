using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace VrcOscChatbox
{
    internal class TutorialState
    {
        public DateTime? FirstRunUtc { get; set; }
        public DateTime? LastPromptUtc { get; set; }
        public bool Suppress { get; set; }
        public bool? ToastSoundEnabled { get; set; }
        private static string Dir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VrcOscChatbox");
        private static string FilePath => Path.Combine(Dir, "tutorial.json");
        private static TutorialState _cached;

        public static TutorialState Load()
        {
            if (_cached != null) return _cached;
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    _cached = JsonSerializer.Deserialize<TutorialState>(json) ?? new TutorialState();
                }
                else _cached = new TutorialState();
            }
            catch { _cached = new TutorialState(); }
            return _cached;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch { }
        }

        public static void MaybeAskSoundPref(IWin32Window owner)
        {
            var s = Load();
            if (s.ToastSoundEnabled.HasValue)
            {
                ToastManager.EnableSound = s.ToastSoundEnabled.Value;
                return;
            }

            var dr = MessageBox.Show(owner,
                "Would you like sound effects for notifications (toasts)?\n\nYou can change this later in Advanced.",
                "Enable sound effects?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            bool enable = (dr == DialogResult.Yes);
            ToastManager.EnableSound = enable;
            s.ToastSoundEnabled = enable;
            s.Save();
        }

        public static void MaybePrompt(IWin32Window owner, Action onStart, Action<string> log = null)
        {
            var s = Load();
            bool firstRun = !s.FirstRunUtc.HasValue;
            bool due = firstRun || !s.LastPromptUtc.HasValue || (DateTime.UtcNow - s.LastPromptUtc.Value) >= TimeSpan.FromDays(7);
            log?.Invoke($"Tutorial check: firstRun={firstRun}, due={due}, suppress={s.Suppress}");
            if (s.Suppress || !due) return;
            if (firstRun) s.FirstRunUtc = DateTime.UtcNow;
            using (var prompt = new TutorialPromptForm(firstRun))
            {
                if (prompt.ShowDialog(owner) == DialogResult.OK)
                {
                    s.LastPromptUtc = DateTime.UtcNow;
                    if (prompt.Result == TutorialPromptResult.Start)
                    {
                        s.Save();
                        MaybeAskSoundPref(owner);
                        onStart?.Invoke();
                        return;
                    }
                    if (prompt.Result == TutorialPromptResult.Never) s.Suppress = true;
                    s.Save();
                }
            }
        }

        public static void Reset()
        {
            var s = Load();
            s.Suppress = false;
            s.LastPromptUtc = null;
            s.FirstRunUtc = null;
            s.Save();
        }
    }
}
