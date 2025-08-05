import tkinter as tk
from tkinter import ttk, messagebox, font
import threading
import time
import datetime
import random
import json
import os
import webbrowser

try:
    from pythonosc import udp_client
    import spotipy
    from spotipy.oauth2 import SpotifyOAuth
    import psutil
    from ttkthemes import ThemedTk
except ImportError as e:
    messagebox.showerror(
        "Missing Library",
        f"A required library is missing: {e.name}.\n\nPlease install it by running:\n'pip install {e.name}'"
    )
    exit()

CONFIG_FILE = "config.json"
DEFAULT_CONFIG = {
    "module_spotify": True, "module_clock": True, "module_fps": True, "module_sys_stats": True,
    "module_heartbeat": True, "module_animated_text": True,
    "clock_show_seconds": False,
    "spotify_client_id": "YOUR_SPOTIFY_CLIENT_ID", "spotify_client_secret": "YOUR_SPOTIFY_CLIENT_SECRET",
    "spotify_redirect_uri": "http://localhost:8888/callback", "spotify_show_device": False,
    "spotify_show_song_name": True, "spotify_show_progress_bar": True, "spotify_show_timestamp": True, # NEW granular controls
    "watermark_text": "VRChat OSC", "progress_bar_length": 12, "progress_filled_char": "█",
    "progress_empty_char": "░", "separator_char": "|", "theme": "arc", "font_size": 10,
    "animated_texts": ["discord.gg/yourserver", "your-website.com"],
    "animation_speed": 1.5, "rewrite_pause": 2.0,
    "update_interval": 1.0
}

class ToolTip:
    def __init__(self, widget, text):
        self.widget = widget
        self.text = text
        self.tooltip_window = None
        widget.bind("<Enter>", self.show_tooltip)
        widget.bind("<Leave>", self.hide_tooltip)

    def show_tooltip(self, event):
        x, y, _, _ = self.widget.bbox("insert")
        x += self.widget.winfo_rootx() + 25
        y += self.widget.winfo_rooty() + 25
        self.tooltip_window = tk.Toplevel(self.widget)
        self.tooltip_window.wm_overrideredirect(True)
        self.tooltip_window.wm_geometry(f"+{x}+{y}")
        label = tk.Label(self.tooltip_window, text=self.text, justify='left',
                         background="#ffffe0", relief='solid', borderwidth=1,
                         font=("tahoma", "8", "normal"))
        label.pack(ipadx=1)

    def hide_tooltip(self, event):
        if self.tooltip_window:
            self.tooltip_window.destroy()
        self.tooltip_window = None

class VrcOscThread(threading.Thread):
    def __init__(self, config, log_callback):
        super().__init__()
        self.config = config
        self.log = log_callback
        self.is_running = True
        self.osc_client = udp_client.SimpleUDPClient("127.0.0.1", 9000)
        self.spotify_client = None
        self.anim_state = {"list_idx": 0, "char_idx": 0, "forward": True, "last_update": 0}
        self.last_heartbeat_flash = 0

    def setup_spotify(self):
        cid = self.config.get('spotify_client_id')
        if not cid or cid == 'YOUR_SPOTIFY_CLIENT_ID':
            self.log("Spotify Error: Client ID not set in GUI.", "red")
            return
        try:
            self.log("Authenticating Spotify...")
            auth_manager = SpotifyOAuth(
                client_id=self.config['spotify_client_id'],
                client_secret=self.config['spotify_client_secret'],
                redirect_uri=self.config['spotify_redirect_uri'],
                scope="user-read-currently-playing",
                open_browser=True
            )
            self.spotify_client = spotipy.Spotify(auth_manager=auth_manager)
            self.spotify_client.current_user() # Test call to trigger auth
            self.log("Spotify Authenticated Successfully!", "green")
        except Exception as e:
            self.spotify_client = None
            self.log(f"Spotify Auth Error: {str(e).splitlines()[0]}", "red")

    def get_spotify_info(self):
        if not self.spotify_client: return None
        try:
            track = self.spotify_client.current_user_playing_track()
            if not track or not track.get('item'):
                return {"name": "Nothing playing on Spotify", "is_playing": False, "progress_ms": 0, "duration_ms": 1}
            info = {
                "name": f"{track['item']['name']} - {track['item']['artists'][0]['name']}",
                "progress_ms": track.get('progress_ms', 0),
                "duration_ms": track['item'].get('duration_ms', 1),
                "is_playing": track.get('is_playing', False)
            }
            if self.config.get('spotify_show_device') and track.get('device'):
                info['device'] = track['device'].get('name', 'Unknown Device')
            return info
        except Exception:
            self.log("Spotify token may have expired. Re-authenticating...", "orange")
            self.setup_spotify()
            return {"name": "Re-authenticating...", "is_playing": False, "progress_ms": 0, "duration_ms": 1}

    def run(self):
        if self.config.get('module_spotify'):
            self.setup_spotify()
        else:
            self.log("Running without Spotify.")
        last_message = ""
        while self.is_running:
            try:
                current_message = self.build_message()
                if current_message != last_message:  # Only send if message changed
                    self.osc_client.send_message("/chatbox/input", [current_message, True])
                    last_message = current_message
                    time.sleep(self.config['update_interval'])
            except Exception as e:
                    self.log(f"OSC Loop Error: {e}", "red")
                    time.sleep(3)

    def build_message(self):
        lines = []
        sep = f" {self.config.get('separator_char', '|')} "
        line1_parts = []
        if self.config.get('module_clock'):
            time_format = "%I:%M:%S %p" if self.config.get('clock_show_seconds') else "%I:%M %p"
            line1_parts.append(f"🕒 {datetime.datetime.now().strftime(time_format)}")  
        if self.config.get('module_heartbeat'):
            now = time.time()
            if now - self.last_heartbeat_flash >= 5:
                self.last_heartbeat_flash = now
            if now - self.last_heartbeat_flash < 1.0:
                line1_parts.append("❤")   
        if self.config.get('module_fps'):
            line1_parts.append(f"🖥️ {random.randint(250, 300)} FPS")
        if self.config.get('module_sys_stats'):
            line1_parts.append(f"CPU: {psutil.cpu_percent():.0f}% | RAM: {psutil.virtual_memory().percent:.0f}%")   
        spotify_info = self.get_spotify_info() if self.config.get('module_spotify') else None
        if spotify_info and self.config.get('spotify_show_song_name'):
            prefix = "🎵"
            if not spotify_info['is_playing'] and spotify_info['name'] not in ["Nothing playing on Spotify", "Re-authenticating..."]:
                prefix = "⏸️"
            song_name = spotify_info['name']
            if self.config.get('spotify_show_device') and 'device' in spotify_info:
                song_name += f" ({spotify_info['device']})"
            line1_parts.append(f"{prefix} {song_name}") 
        if line1_parts:
            lines.append(sep.join(line1_parts))
        if spotify_info:
            line2_parts = []
            if self.config.get('spotify_show_progress_bar'):
                p_len = self.config.get('progress_bar_length', 12)
                ratio = spotify_info['progress_ms'] / spotify_info['duration_ms'] if spotify_info['duration_ms'] > 0 else 0
                filled = int(ratio * p_len)
                empty = p_len - filled
                bar = f"[{self.config.get('progress_filled_char', '█') * filled}{self.config.get('progress_empty_char', '░') * empty}]"
                line2_parts.append(bar)
            if self.config.get('spotify_show_timestamp'):
                p_time = f"{int(spotify_info['progress_ms']/60000):02}:{int((spotify_info['progress_ms']/1000)%60):02}"
                d_time = f"{int(spotify_info['duration_ms']/60000):02}:{int((spotify_info['duration_ms']/1000)%60):02}"
                line2_parts.append(f"{p_time}/{d_time}")

            if line2_parts:
                lines.append(" ".join(line2_parts))
        if self.config.get('module_animated_text'):
            now = time.time()
            texts = [t for t in self.config.get('animated_texts', []) if t]
            if texts and (now - self.anim_state['last_update']) > self.config.get('animation_speed', 1.5):
                self.anim_state['last_update'] = now
                active_text = texts[self.anim_state['list_idx'] % len(texts)]
                if self.anim_state['forward']:
                    if self.anim_state['char_idx'] < len(active_text):
                        self.anim_state['char_idx'] += 1
                    else:
                        time.sleep(self.config.get('rewrite_pause', 2.0))
                        self.anim_state['forward'] = False
                else:
                    if self.anim_state['char_idx'] > 0:
                        self.anim_state['char_idx'] -= 1
                    else:
                        time.sleep(self.config.get('rewrite_pause', 2.0))
                        self.anim_state['forward'] = True
                        self.anim_state['list_idx'] += 1
            if texts:
                current_text_to_display = texts[self.anim_state['list_idx'] % len(texts)][:self.anim_state['char_idx']]
                lines.append(current_text_to_display if current_text_to_display else '\u200b')
        if self.config.get('watermark_text'):
            lines.append(self.config.get('watermark_text'))       
        return "\n".join(lines)

    def stop(self):
        self.is_running = False
        try:
            self.osc_client.send_message("/chatbox/input", ["", True])
        except Exception as e:
            self.log(f"Could not clear chatbox: {e}", "orange")
        self.log("Stopped.")

class Application(ttk.Frame):
    def __init__(self, master=None):
        super().__init__(master)
        self.master = master
        self.config = self.load_config()
        self.master.title("VRChat OSC Pro Controller")
        self.master.geometry("750x600")       
        self.style = ttk.Style()
        available_themes = sorted(self.master.get_themes())
        if self.config.get('theme') in available_themes:
            try:
                self.master.set_theme(self.config.get('theme'))
            except Exception:
                self.master.set_theme("arc") # Fallback theme       
        self.pack(fill="both", expand=True)
        self.vars = {}
        self.osc_thread = None
        self.create_widgets()
        self.load_settings_to_gui()
        self.update_dependencies()
        self.update_preview()

    def load_config(self):
        if os.path.exists(CONFIG_FILE):
            try:
                with open(CONFIG_FILE, 'r') as f:
                    user_config = json.load(f)
                    config = DEFAULT_CONFIG.copy()
                    config.update(user_config)
                    return config
            except (json.JSONDecodeError, TypeError):
                return DEFAULT_CONFIG
        return DEFAULT_CONFIG

    def save_config(self):
        self.apply_gui_to_config()
        with open(CONFIG_FILE, 'w') as f:
            json.dump(self.config, f, indent=4)
        self.log("Settings saved!", "green")

    def create_widgets(self):
        main_pane = ttk.PanedWindow(self, orient="horizontal")
        main_pane.pack(fill="both", expand=True, padx=10, pady=10)
        settings_frame = ttk.Frame(main_pane)
        self.notebook = ttk.Notebook(settings_frame)
        self.notebook.pack(fill="both", expand=True)
        main_pane.add(settings_frame, weight=3)       
        run_frame = ttk.Frame(main_pane)
        self.create_run_panel(run_frame)
        main_pane.add(run_frame, weight=2)
        self.tab_modules = ttk.Frame(self.notebook)
        self.tab_spotify = ttk.Frame(self.notebook)
        self.tab_style = ttk.Frame(self.notebook)
        self.tab_log = ttk.Frame(self.notebook)
        self.notebook.add(self.tab_modules, text="Modules")
        self.notebook.add(self.tab_spotify, text="Spotify")
        self.notebook.add(self.tab_style, text="Style")
        self.notebook.add(self.tab_log, text="Log")        
        self.create_modules_tab()
        self.create_spotify_tab()
        self.create_style_tab()
        self.create_log_tab()
        
    def create_run_panel(self, parent):
        run_controls_frame = ttk.LabelFrame(parent, text="Controls")
        run_controls_frame.pack(fill="x", padx=5, pady=5, ipady=5)        
        self.start_button = ttk.Button(run_controls_frame, text="Start", command=self.start_osc, style="Accent.TButton")
        self.start_button.pack(side="left", fill="x", expand=True, padx=5, pady=5)
        ToolTip(self.start_button, "Save settings and start sending to VRChat")
        self.stop_button = ttk.Button(run_controls_frame, text="Stop", command=self.stop_osc, state="disabled")
        self.stop_button.pack(side="left", fill="x", expand=True, padx=5, pady=5)
        ToolTip(self.stop_button, "Stop sending to VRChat")        
        preview_frame = ttk.LabelFrame(parent, text="Live Preview")
        preview_frame.pack(fill="both", expand=True, padx=5, pady=5)
        self.preview_label = ttk.Label(preview_frame, text="Preview", wraplength=280, justify="left", anchor="nw")
        self.preview_label.pack(fill="both", expand=True, padx=10, pady=10)

    def create_modules_tab(self):
        frame = self.tab_modules
        frame.columnconfigure(0, weight=1)
        frame.columnconfigure(1, weight=1)

        def add_module(name, text, tooltip, row, col):
            self.vars[name] = tk.BooleanVar()
            chk = ttk.Checkbutton(frame, text=text, variable=self.vars[name], command=self.update_dependencies)
            chk.grid(row=row, column=col, sticky="w", padx=10, pady=5)
            ToolTip(chk, tooltip)
            return chk
        
        add_module("module_clock", "Clock", "Shows the current time.", 0, 0)
        # self.vars['clock_show_seconds'] = tk.BooleanVar()
        # self.clock_seconds_chk = ttk.Checkbutton(frame, text="Show Seconds", variable=self.vars['clock_show_seconds'], command=self.update_preview)
        #self.clock_seconds_chk.grid(row=1, column=0, sticky="w", padx=30)
        #ToolTip(self.clock_seconds_chk, "Toggle seconds in the time display.")
        add_module("module_heartbeat", "Heartbeat Icon", "Flashes a heart icon every 5 seconds.", 2, 0)
        add_module("module_fps", "Fake FPS Counter", "Displays a fake, fluctuating FPS count.", 3, 0)
        # add_module("module_sys_stats", "System Stats", "Shows real CPU and RAM usage.", 4, 0)
        add_module("module_animated_text", "Animated Text", "Types and deletes custom text messages.", 0, 1)
        anim_frame = ttk.Frame(frame)
        anim_frame.grid(row=1, column=1, rowspan=5, sticky="ew", padx=30)
        ttk.Label(anim_frame, text="Texts (one per line):").pack(anchor="w")
        self.vars['animated_texts'] = tk.Text(anim_frame, height=4, width=30)
        self.vars['animated_texts'].pack(anchor="w", fill="x")
        self.vars['animated_texts'].bind("<KeyRelease>", self.update_preview)
        
    def create_spotify_tab(self):
        frame = self.tab_spotify
        self.vars['module_spotify'] = tk.BooleanVar()
        spotify_chk = ttk.Checkbutton(frame, text="Enable Spotify Integration", variable=self.vars['module_spotify'], command=self.update_dependencies)
        spotify_chk.pack(anchor="w", padx=10, pady=10)
        ToolTip(spotify_chk, "Enable or disable all Spotify features.")
        self.spotify_settings_frame = ttk.LabelFrame(frame, text="Spotify Display Options")
        self.spotify_settings_frame.pack(fill="x", expand=True, padx=10, pady=5)
        sf = self.spotify_settings_frame
        self.vars['spotify_show_song_name'] = tk.BooleanVar()
        self.spotify_song_name_chk = ttk.Checkbutton(sf, text="Show Song Name", variable=self.vars['spotify_show_song_name'], command=self.update_preview)
        self.spotify_song_name_chk.pack(anchor="w", padx=5)
        ToolTip(self.spotify_song_name_chk, "Show the '🎵 Artist - Title' text.")
        self.vars['spotify_show_progress_bar'] = tk.BooleanVar()
        self.spotify_progress_bar_chk = ttk.Checkbutton(sf, text="Show Progress Bar", variable=self.vars['spotify_show_progress_bar'], command=self.update_preview)
        self.spotify_progress_bar_chk.pack(anchor="w", padx=5)
        ToolTip(self.spotify_progress_bar_chk, "Show the '[███░░░]' progress bar.")
        self.vars['spotify_show_timestamp'] = tk.BooleanVar()
        self.spotify_timestamp_chk = ttk.Checkbutton(sf, text="Show Song Timestamp", variable=self.vars['spotify_show_timestamp'], command=self.update_preview)
        self.spotify_timestamp_chk.pack(anchor="w", padx=5)
        ToolTip(self.spotify_timestamp_chk, "Show the '01:23 / 03:45' time.")        
        self.vars['spotify_show_device'] = tk.BooleanVar()
        self.spotify_device_chk = ttk.Checkbutton(sf, text="Show Playback Device Name", variable=self.vars['spotify_show_device'], command=self.update_preview)
        self.spotify_device_chk.pack(anchor="w", padx=5, pady=(5,0))
        ToolTip(self.spotify_device_chk, "Show which device Spotify is playing on.")
        creds_frame = ttk.LabelFrame(frame, text="Spotify API Credentials")
        creds_frame.pack(fill="x", expand=True, padx=10, pady=5)
        cf = creds_frame
        cf.columnconfigure(1, weight=1)   
        ttk.Label(cf, text="Client ID:").grid(row=0, column=0, sticky="w", padx=5, pady=3)
        self.vars['spotify_client_id'] = tk.StringVar()
        ttk.Entry(cf, textvariable=self.vars['spotify_client_id']).grid(row=0, column=1, sticky="ew", padx=5)
        ttk.Label(cf, text="Client Secret:").grid(row=1, column=0, sticky="w", padx=5, pady=3)
        self.vars['spotify_client_secret'] = tk.StringVar()
        ttk.Entry(cf, textvariable=self.vars['spotify_client_secret'], show="*").grid(row=1, column=1, sticky="ew", padx=5)       
        ttk.Label(cf, text="Redirect URI:").grid(row=2, column=0, sticky="w", padx=5, pady=3)
        self.vars['spotify_redirect_uri'] = tk.StringVar()
        ttk.Entry(cf, textvariable=self.vars['spotify_redirect_uri']).grid(row=2, column=1, sticky="ew", padx=5)   
        ttk.Button(cf, text="Get Credentials Help", command=lambda: webbrowser.open("https://developer.spotify.com/dashboard/")).grid(row=3, columnspan=2, pady=10)

    def create_style_tab(self):
        frame = self.tab_style
        frame.columnconfigure(1, weight=1)
        ttk.Label(frame, text="Watermark:").grid(row=0, column=0, sticky="w", padx=10, pady=5)
        self.vars['watermark_text'] = tk.StringVar()
        ttk.Entry(frame, textvariable=self.vars['watermark_text']).grid(row=0, column=1, sticky="ew", padx=10)
        self.vars['watermark_text'].trace_add("write", self.update_preview)
        ttk.Label(frame, text="Separator:").grid(row=1, column=0, sticky="w", padx=10, pady=5)
        self.vars['separator_char'] = tk.StringVar()
        ttk.Entry(frame, textvariable=self.vars['separator_char'], width=5).grid(row=1, column=1, sticky="w", padx=10)
        self.vars['separator_char'].trace_add("write", self.update_preview)
        ttk.Label(frame, text="Theme:").grid(row=2, column=0, sticky="w", padx=10, pady=5)
        self.vars['theme'] = tk.StringVar()
        theme_combo = ttk.Combobox(frame, textvariable=self.vars['theme'], values=sorted(self.master.get_themes()))
        theme_combo.grid(row=2, column=1, sticky="ew", padx=10)
        theme_combo.bind("<<ComboboxSelected>>", lambda e: messagebox.showinfo("Theme Change", "Theme will be applied on next restart."))
        ttk.Label(frame, text="GUI Font Size:").grid(row=3, column=0, sticky="w", padx=10, pady=5)
        self.vars['font_size'] = tk.IntVar()
        ttk.Spinbox(frame, from_=8, to=16, textvariable=self.vars['font_size']).grid(row=3, column=1, sticky="w", padx=10)
        
    def create_log_tab(self):
        frame = self.tab_log
        self.log_text = tk.Text(frame, height=10, state="disabled", wrap="word", background=self.style.lookup('TFrame', 'background'))
        self.log_text.pack(fill="both", expand=True, padx=5, pady=5)
        self.log_text.tag_config("INFO", foreground=self.style.lookup('TLabel', 'foreground'))
        self.log_text.tag_config("GREEN", foreground="green")
        self.log_text.tag_config("ORANGE", foreground="orange")
        self.log_text.tag_config("RED", foreground="red")

    def log(self, message, level="INFO"):
        self.log_text.config(state="normal")
        timestamp = datetime.datetime.now().strftime("[%H:%M:%S] ")
        self.log_text.insert(tk.END, timestamp + message + "\n", level.upper())
        self.log_text.see(tk.END)
        self.log_text.config(state="disabled")

    def update_dependencies(self, *args):
        spotify_master_state = "normal" if self.vars['module_spotify'].get() else "disabled"
        self.spotify_song_name_chk.config(state=spotify_master_state)
        self.spotify_progress_bar_chk.config(state=spotify_master_state)
        self.spotify_timestamp_chk.config(state=spotify_master_state)
        self.spotify_device_chk.config(state=spotify_master_state)        
        clock_state = "normal" if self.vars['module_clock'].get() else "disabled"
        #self.clock_seconds_chk.config(state=clock_state)
        self.update_preview()

    def load_settings_to_gui(self):
        for key, var in self.vars.items():
            config_val = self.config.get(key)
            if config_val is not None:
                if isinstance(var, tk.Text):
                    var.delete('1.0', tk.END)
                    var.insert('1.0', "\n".join(str(v) for v in config_val))
                else:
                    var.set(config_val)

    def apply_gui_to_config(self):
        for key, var in self.vars.items():
            if isinstance(var, tk.Text):
                self.config[key] = [line for line in var.get('1.0', tk.END).strip().split('\n') if line]
            else:
                self.config[key] = var.get()

    def update_preview(self, *args):
        temp_config = self.config.copy()
        for key, var in self.vars.items():
            if isinstance(var, tk.Text):
                temp_config[key] = [line for line in var.get('1.0', tk.END).strip().split('\n') if line]
            else:
                temp_config[key] = var.get()    
        preview_thread = VrcOscThread(temp_config, lambda *args: None)
        preview_thread.anim_state['char_idx'] = 5
        preview_text = preview_thread.build_message()
        self.preview_label.config(text=preview_text if preview_text else "Preview will appear here.")
        preview_thread.stop()
        
    def start_osc(self):
        self.save_config()
        self.start_button.config(state="disabled")
        self.stop_button.config(state="normal")    
        for i in range(self.notebook.index("end")):
            self.notebook.tab(i, state="disabled")
        self.log("Starting OSC Thread...")
        self.osc_thread = VrcOscThread(self.config, self.log)
        self.osc_thread.start()
    
    def stop_osc(self):
        if self.osc_thread and self.osc_thread.is_alive():
            self.osc_thread.stop()
            self.osc_thread.join()
        
        self.start_button.config(state="normal")
        self.stop_button.config(state="disabled")
        for i in range(self.notebook.index("end")):
            self.notebook.tab(i, state="normal")

    def on_closing(self):
        if messagebox.askokcancel("Quit", "Do you want to quit? This will stop the OSC script if it is running."):
            if self.osc_thread and self.osc_thread.is_alive():
                self.stop_osc()
            self.master.destroy()

# --- MAIN EXECUTION ---
if __name__ == "__main__":
    root = ThemedTk(theme="arc")
    app = Application(master=root)
    root.protocol("WM_DELETE_WINDOW", app.on_closing)
    app.mainloop()