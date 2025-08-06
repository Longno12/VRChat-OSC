import tkinter as tk
from tkinter import messagebox, font
import threading
import time
import datetime
import random
import json
import os
import webbrowser
import math

try:
    from pythonosc import udp_client
    import spotipy
    from spotipy.oauth2 import SpotifyOAuth
    import psutil
except ImportError as e:
    messagebox.showerror(
        "Missing Library",
        f"A required library is missing: {e.name}.\n\nPlease install it by running:\n'pip install {e.name}'"
    )
    exit()

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
CONFIG_FILE = os.path.join(SCRIPT_DIR, "config.json")
CACHE_FILE = os.path.join(SCRIPT_DIR, ".spotipyoauthcache")
DEFAULT_CONFIG = {
    "module_spotify": True, "module_clock": True, "module_fps": True, "module_sys_stats": True,
    "module_heartbeat": True, "module_animated_text": True,
    "clock_show_seconds": True,
    "spotify_client_id": "YOUR_SPOTIFY_CLIENT_ID", "spotify_client_secret": "YOUR_SPOTIFY_CLIENT_SECRET",
    "spotify_redirect_uri": "http://localhost:8888/callback", "spotify_show_device": False,
    "spotify_show_song_name": True, "spotify_show_progress_bar": True, "spotify_show_timestamp": True,
    "watermark_text": "VRChat OSC Pro", "progress_bar_length": 20, "progress_filled_char": "█",
    "progress_empty_char": "─", "separator_char": "•",
    "animated_texts": ["discord.gg/encryptic", "Https://longno.co.uk"],
    "animation_speed": 0.15, "rewrite_pause": 2.5,
    "update_interval": 1.0
}

class VrcOscThread(threading.Thread):
    def __init__(self, config, log_callback):
        super().__init__()
        self.config = config
        self.log = log_callback
        self.is_running = True
        self.osc_client = udp_client.SimpleUDPClient("127.0.0.1", 9000)
        self.spotify_client = None
        self.anim_state = {"list_idx": 0, "char_idx": 0, "forward": True, "last_update": 0, "pause_until": 0}
        self.last_heartbeat_flash = 0

    def setup_spotify(self):
        cid = self.config.get('spotify_client_id')
        if not cid or cid == 'YOUR_SPOTIFY_CLIENT_ID': self.log("Spotify Error: Client ID not set.", "red"); return
        try:
            self.log("Authenticating Spotify...", "orange")
            auth_manager = SpotifyOAuth(
                client_id=self.config['spotify_client_id'],
                client_secret=self.config['spotify_client_secret'],
                redirect_uri=self.config['spotify_redirect_uri'],
                scope="user-read-currently-playing",
                open_browser=False,
                cache_path=CACHE_FILE
            )
            self.spotify_client = spotipy.Spotify(auth_manager=auth_manager)
            self.spotify_client.current_user()
            self.log("Spotify Authenticated Successfully!", "green")
        except Exception as e:
            self.spotify_client = None; self.log(f"Spotify Auth Error: {str(e).splitlines()[0]}", "red"); self.log("Ensure credentials are correct and auth prompt is accepted.", "orange")

    def get_spotify_info(self):
        if not self.spotify_client: return None
        try:
            track = self.spotify_client.current_user_playing_track()
            if not track or not track.get('item'): return {"name": "Nothing playing on Spotify", "is_playing": False, "progress_ms": 0, "duration_ms": 1}
            info = {"name": f"{track['item']['name']} - {', '.join([a['name'] for a in track['item']['artists']])}", "progress_ms": track.get('progress_ms', 0), "duration_ms": track['item'].get('duration_ms', 1), "is_playing": track.get('is_playing', False)}
            if self.config.get('spotify_show_device') and track.get('device'): info['device'] = track['device'].get('name', 'Unknown Device')
            return info
        except spotipy.exceptions.SpotifyException as e:
            if e.http_status == 401: self.log("Spotify token expired. Re-authenticating...", "orange"); self.setup_spotify()
            else: self.log(f"Spotify API Error: {e}", "red")
            return {"name": "Re-authenticating...", "is_playing": False, "progress_ms": 0, "duration_ms": 1}
        except Exception: self.log("An unknown Spotify error occurred.", "red"); return {"name": "Spotify Error", "is_playing": False, "progress_ms": 0, "duration_ms": 1}
            
    def run(self):
        if self.config.get('module_spotify'): self.setup_spotify()
        else: self.log("Running without Spotify module.")
        last_message = ""
        while self.is_running:
            try:
                current_message = self.build_message()
                if current_message != last_message: self.osc_client.send_message("/chatbox/input", [current_message, True]); last_message = current_message
                time.sleep(self.config.get('update_interval', 1.0))
            except Exception as e: self.log(f"OSC Loop Error: {e}", "red"); time.sleep(3)
                
    def build_message(self):
        lines, sep, line1_parts = [], f" {self.config.get('separator_char', '|')} ", []
        if self.config.get('module_clock'): line1_parts.append(f"🕒 {datetime.datetime.now().strftime('%H:%M:%S' if self.config.get('clock_show_seconds') else '%H:%M')}")
        if self.config.get('module_heartbeat'):
            now = time.time()
            if now - self.last_heartbeat_flash >= 5: self.last_heartbeat_flash = now
            if now - self.last_heartbeat_flash < 1.0: line1_parts.append("❤")
        if self.config.get('module_fps'): line1_parts.append(f"🚀 {random.randint(249, 359)} FPS")
        if self.config.get('module_sys_stats'): line1_parts.append(f"💻 CPU: {psutil.cpu_percent():.0f}% | RAM: {psutil.virtual_memory().percent:.0f}%")
        if line1_parts: lines.append(sep.join(line1_parts))
        spotify_info = self.get_spotify_info() if self.config.get('module_spotify') else None
        if spotify_info:
            spotify_line, progress_parts = [], []
            if self.config.get('spotify_show_song_name'):
                prefix = "🎵" if spotify_info['is_playing'] else "⏸️"
                song_name = spotify_info['name']
                if self.config.get('spotify_show_device') and 'device' in spotify_info: song_name += f" 🔊({spotify_info['device']})"
                spotify_line.append(f"{prefix} {song_name}")
            if self.config.get('spotify_show_progress_bar'):
                p_len, ratio = self.config.get('progress_bar_length', 14), spotify_info['progress_ms'] / spotify_info['duration_ms'] if spotify_info['duration_ms'] > 0 else 0
                filled = int(ratio * p_len)
                progress_parts.append(f"[{self.config.get('progress_filled_char', '█') * filled}{self.config.get('progress_empty_char', '─') * (p_len - filled)}]")
            if self.config.get('spotify_show_timestamp'):
                p_time, d_time = f"{int(spotify_info['progress_ms']/60000):02}:{int((spotify_info['progress_ms']/1000)%60):02}", f"{int(spotify_info['duration_ms']/60000):02}:{int((spotify_info['duration_ms']/1000)%60):02}"
                progress_parts.append(f"{p_time}/{d_time}")
            if progress_parts: spotify_line.append(" ".join(progress_parts))
            if spotify_line: lines.append("\n".join(spotify_line))
        if self.config.get('module_animated_text'):
            now = time.time()
            texts = [t for t in self.config.get('animated_texts', []) if t]
            if texts and now > self.anim_state.get('pause_until', 0):
                if (now - self.anim_state['last_update']) > self.config.get('animation_speed', 0.15):
                    self.anim_state['last_update'] = now
                    active_text = texts[self.anim_state['list_idx'] % len(texts)]
                    if self.anim_state['forward']:
                        if self.anim_state['char_idx'] < len(active_text): self.anim_state['char_idx'] += 1
                        else: self.anim_state['pause_until'] = now + self.config.get('rewrite_pause', 2.5); self.anim_state['forward'] = False
                    else:
                        if self.anim_state['char_idx'] > 0: self.anim_state['char_idx'] -= 1
                        else: self.anim_state['forward'] = True; self.anim_state['list_idx'] += 1; self.anim_state['pause_until'] = now + 1.0
            if texts:
                current_text_to_display = texts[self.anim_state['list_idx'] % len(texts)][:self.anim_state['char_idx']]
                lines.append(current_text_to_display if current_text_to_display else '\u200b')
        if self.config.get('watermark_text'): lines.append(self.config.get('watermark_text'))
        return "\n".join(lines)
        
    def stop(self):
        self.is_running = False
        try: self.osc_client.send_message("/chatbox/input", ["", True])
        except Exception as e: self.log(f"Could not clear chatbox: {e}", "orange")
        self.log("OSC thread stopped.")



class HUDFrame(tk.Canvas):
    """A custom-drawn frame with angled corners and a glowing border."""
    def __init__(self, master, theme, fonts, title="", **kwargs):
        super().__init__(master, highlightthickness=0, bg=master.cget('bg'), **kwargs)
        self.title = title
        self.theme = theme
        self.fonts = fonts
        self.bind("<Configure>", self._draw)
        self.content_frame = tk.Frame(self, bg=self.theme['bg_light'])
        self.content_frame.place(x=15, y=40, relwidth=1, relheight=1, width=-30, height=-55)

    def _draw(self, event=None):
        self.delete("all")
        width, height = self.winfo_width(), self.winfo_height()
        if width < 50 or height < 50: return
        points = [15, 0, width, 0, width, height - 15, width - 15, height, 0, height, 0, 15]
        self.create_polygon(points, fill="", outline=self.theme['glow'], width=4)
        self.create_polygon(points, fill=self.theme['bg_light'], outline=self.theme['border'], width=2)        
        self.create_text(30, 18, text=self.title, font=self.fonts['header'], fill=self.theme['accent'], anchor="w")
        self.create_line(20, 35, width - 20, 35, fill=self.theme['border'])

class HUDToggleSwitch(tk.Canvas):
    """A custom sci-fi themed toggle switch."""
    def __init__(self, master, variable, theme, fonts, command=None, **kwargs):
        super().__init__(master, width=60, height=28, **kwargs)
        self.variable = variable
        self.command = command
        self.theme = theme
        self.fonts = fonts
        self.master = master
        self.configure(bg=self.master.cget('bg'), highlightthickness=0)
        self.bind("<Button-1>", self._toggle)
        self.bind("<Enter>", lambda e: self.config(cursor="hand2"))
        self.bind("<Leave>", lambda e: self.config(cursor=""))
        self.variable.trace_add("write", self._update_display)
        self._update_display()

    def _toggle(self, event=None):
        self.variable.set(not self.variable.get())
        if self.command:
            self.command()
            
    def _update_display(self, *args):
        self.delete("all")
        is_on = self.variable.get()       
        self.create_rectangle(2, 2, 58, 26, fill=self.theme['bg_dark'], outline=self.theme['border'], width=2)
        
        if is_on:
            self.create_rectangle(4, 4, 56, 24, fill=self.theme['accent'], outline="")
            self.create_rectangle(32, 4, 56, 24, fill=self.theme['accent_fg'], outline="")
            self.create_text(18, 14, text="ON", font=self.fonts['small_bold'], fill=self.theme['accent_fg'])
        else:
            self.create_rectangle(4, 4, 28, 24, fill=self.theme['text_dark'], outline="")
            self.create_text(42, 14, text="OFF", font=self.fonts['small_bold'], fill=self.theme['text_dark'])


class Application(tk.Frame):
    def __init__(self, master=None):
        self.setup_theme_and_fonts()
        super().__init__(master, bg=self.theme['bg_dark'])     
        self.master = master
        self.config = self.load_config()
        self.master.title("Prokect Encryptic :: VRChat OSC")
        self.master.geometry("1000x800")
        self.master.minsize(900, 700)
        self.master.configure(bg=self.theme['bg_dark'])
        self.pack(fill="both", expand=True)    
        self.vars = {}
        self.osc_thread = None     
        self.create_widgets()
        self.load_settings_to_gui()
        self.update_dependencies()
        self.update_preview()       
        self.log("UI Initialized. Welcome to Encryptic OSC.", "accent")


    def setup_theme_and_fonts(self):
        self.theme = {
            'bg_dark': '#0D1117', 'bg_light': '#161B22', 'border': '#30363D',
            'text': '#C9D1D9', 'text_dark': '#8B949E', 'accent': '#58A6FF',
            'accent_fg': '#FFFFFF', 'glow': '#58A6FF', 'green': '#238636',
            'red': '#DA3633', 'orange': '#F0883E'
        }
        self.fonts = {
            'title': ("Orbitron", 24, "bold"), 'header': ("Orbitron", 12, "bold"),
            'body': ("Segoe UI", 10), 'small_bold': ("Segoe UI", 8, "bold"),
            'preview': ("Consolas", 11), 'log': ("Consolas", 10),
        }

    def load_config(self):
        if os.path.exists(CONFIG_FILE):
            try:
                with open(CONFIG_FILE, 'r') as f:
                    user_config = json.load(f)
                    config = DEFAULT_CONFIG.copy()
                    config.update(user_config)
                    return config
            except (json.JSONDecodeError, TypeError): return DEFAULT_CONFIG.copy()
        return DEFAULT_CONFIG.copy()

    def save_config(self):
        self.apply_gui_to_config()
        with open(CONFIG_FILE, 'w') as f: json.dump(self.config, f, indent=4)
        self.log("Configuration saved to disk.", "green")

    def create_widgets(self):
        self.background_canvas = tk.Canvas(self, bg=self.theme['bg_dark'], highlightthickness=0)
        self.background_canvas.place(relwidth=1, relheight=1)
        self.background_canvas.bind("<Configure>", self.draw_background_pattern)
        self.grid_rowconfigure(0, weight=3)
        self.grid_rowconfigure(1, weight=2)
        self.grid_columnconfigure(0, weight=1)
        self.grid_columnconfigure(1, weight=1)
        self.grid_columnconfigure(2, weight=1)       
        self.create_modules_panel()
        self.create_spotify_panel()
        self.create_style_panel()
        self.create_preview_panel()
        self.create_log_and_controls_panel()
        
    def draw_background_pattern(self, event=None):
        self.background_canvas.delete("all")
        width, height = event.width, event.height
        for x in range(0, width + 100, 100):
            for y in range(0, height + 86, 86):
                offset = 50 if (y // 86) % 2 == 0 else 0
                points = []
                for i in range(6):
                    angle_deg = 60 * i - 30
                    angle_rad = math.pi / 180 * angle_deg
                    px = x + offset + 40 * math.cos(angle_rad)
                    py = y + 40 * math.sin(angle_rad)
                    points.extend([px, py])
                self.background_canvas.create_polygon(points, fill="", outline=self.theme['border'], width=1)
        
    def create_setting_row(self, parent, var_name, text, tooltip_text):
        self.vars[var_name] = tk.BooleanVar()
        row = tk.Frame(parent, bg=parent.cget('bg'))
        label = tk.Label(row, text=text, font=self.fonts['body'], bg=parent.cget('bg'), fg=self.theme['text'])
        label.pack(side="left", padx=(0,10))
        switch = HUDToggleSwitch(row, self.vars[var_name], self.theme, self.fonts, command=self.update_dependencies)
        switch.pack(side="right")
        row.pack(fill="x", pady=8, padx=10)
        return switch
        
    def create_modules_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="CORE MODULES")
        panel.grid(row=0, column=0, sticky="nsew", padx=10, pady=10)
        content = panel.content_frame
        self.create_setting_row(content, "module_clock", "Clock:", "Shows the current time.")
        self.create_setting_row(content, "module_heartbeat", "Heartbeat:", "Flashes a heart icon.")
        self.create_setting_row(content, "module_fps", "Fake FPS:", "Displays a fake FPS count.")
        self.create_setting_row(content, "module_sys_stats", "System Stats:", "Shows CPU and RAM usage.")
        self.create_setting_row(content, "module_animated_text", "Animated Text:", "Types custom text messages.")

    def create_spotify_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="SPOTIFY")
        panel.grid(row=0, column=1, sticky="nsew", padx=10, pady=10)
        content = panel.content_frame
        self.spotify_master_switch = self.create_setting_row(content, "module_spotify", "Enable Spotify:", "Master toggle for all Spotify features.")
        self.spotify_controls = {}
        self.spotify_controls['song'] = self.create_setting_row(content, "spotify_show_song_name", "Show Song:", "Display artist and title.")
        self.spotify_controls['bar'] = self.create_setting_row(content, "spotify_show_progress_bar", "Show Progress Bar:", "Display song progress bar.")
        self.spotify_controls['time'] = self.create_setting_row(content, "spotify_show_timestamp", "Show Timestamp:", "Display '01:23 / 03:45'.")
        self.spotify_controls['device'] = self.create_setting_row(content, "spotify_show_device", "Show Device:", "Show which device is playing.")

    def create_style_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="STYLE & TEXT")
        panel.grid(row=0, column=2, sticky="nsew", padx=10, pady=10)
        content = panel.content_frame
        def create_entry(p, var_name, label):
            self.vars[var_name] = tk.StringVar()
            row = tk.Frame(p, bg=p.cget('bg'))
            tk.Label(row, text=label, font=self.fonts['body'], bg=p.cget('bg'), fg=self.theme['text'], width=12, anchor='w').pack(side="left")
            entry_bg = tk.Frame(row, bg=self.theme['border'], relief='flat', bd=1)
            entry = tk.Entry(entry_bg, textvariable=self.vars[var_name], bg=self.theme['bg_dark'], fg=self.theme['text'], relief='flat', insertbackground=self.theme['accent_fg'])
            entry.pack(fill='x', expand=True, padx=1, pady=1)
            entry_bg.pack(side="left", fill="x", expand=True)
            self.vars[var_name].trace_add("write", self.update_preview)
            row.pack(fill="x", pady=5, padx=10)
        create_entry(content, 'watermark_text', "Watermark:")
        create_entry(content, 'separator_char', "Separator:")
        tk.Label(content, text="Animated Text (one per line):", font=self.fonts['body'], bg=content.cget('bg'), fg=self.theme['text_dark']).pack(fill="x", pady=(10,5), padx=10)
        text_bg = tk.Frame(content, bg=self.theme['border'], relief='flat', bd=1)
        self.vars['animated_texts'] = tk.Text(text_bg, height=5, bg=self.theme['bg_dark'], fg=self.theme['text'], font=self.fonts['body'], relief='flat', insertbackground=self.theme['accent_fg'], bd=0)
        self.vars['animated_texts'].pack(padx=1, pady=1, fill="both", expand=True)
        self.vars['animated_texts'].bind("<KeyRelease>", self.update_preview)
        text_bg.pack(fill="both", expand=True, padx=10)
        
    def create_preview_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="LIVE PREVIEW")
        panel.grid(row=1, column=0, columnspan=2, sticky="nsew", padx=10, pady=10)
        self.preview_canvas = tk.Canvas(panel.content_frame, bg=panel.content_frame.cget('bg'), highlightthickness=0)
        self.preview_canvas.pack(fill="both", expand=True)

    def create_log_and_controls_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="SYSTEM LOG & CONTROLS")
        panel.grid(row=1, column=2, sticky="nsew", padx=10, pady=10)
        content = panel.content_frame
        log_bg = tk.Frame(content, bg=self.theme['border'], relief='flat', bd=1)
        self.log_text = tk.Text(log_bg, height=10, state="disabled", wrap="word", bg=self.theme['bg_dark'], fg=self.theme['text_dark'], font=self.fonts['log'], relief='flat', bd=0)
        self.log_text.pack(fill="both", expand=True, padx=1, pady=1)
        log_bg.pack(fill="both", expand=True, padx=10, pady=(0, 10))
        self.log_text.tag_config("INFO", foreground=self.theme['text_dark'])
        self.log_text.tag_config("ACCENT", foreground=self.theme['accent'])
        self.log_text.tag_config("GREEN", foreground=self.theme['green'])
        self.log_text.tag_config("ORANGE", foreground=self.theme['orange'])
        self.log_text.tag_config("RED", foreground=self.theme['red'])
        controls_frame = tk.Frame(content, bg=content.cget('bg'))
        controls_frame.pack(fill="x", side="bottom", pady=5, padx=5)
        controls_frame.grid_columnconfigure(0, weight=1)
        controls_frame.grid_columnconfigure(1, weight=1)
        controls_frame.grid_columnconfigure(2, weight=1)
        self.start_button = self.create_hud_button(controls_frame, "▶ START", self.theme['green'], self.start_osc)
        self.start_button.grid(row=0, column=0, sticky="ew", padx=5)
        self.stop_button = self.create_hud_button(controls_frame, "■ STOP", self.theme['red'], self.stop_osc, "disabled")
        self.stop_button.grid(row=0, column=1, sticky="ew", padx=5)
        self.save_button = self.create_hud_button(controls_frame, "💾 SAVE", self.theme['accent'], self.save_config)
        self.save_button.grid(row=0, column=2, sticky="ew", padx=5)
        
    def create_hud_button(self, parent, text, color, command, state='normal'):
        canvas = tk.Canvas(parent, height=40, bg=parent.cget('bg'), highlightthickness=0)
        
        def draw_button(bg_color, fg_color):
            canvas.delete("all")
            width, height = canvas.winfo_width(), canvas.winfo_height()
            if width < 10 or height < 10: return
            points = [5, 0, width, 0, width, height - 5, width - 5, height, 0, height, 0, 5]
            canvas.create_polygon(points, fill=bg_color, outline=self.theme['border'])
            canvas.create_text(width/2, height/2, text=text, font=self.fonts['header'], fill=fg_color)
            
        def on_configure(event):
            draw_button(color, self.theme['accent_fg'])

        canvas.bind("<Configure>", on_configure)
        
        if state == 'disabled':
            canvas.config(state="disabled")
            draw_button(self.theme['bg_light'], self.theme['text_dark'])
        else:
            canvas.config(cursor="hand2")
            canvas.bind("<Button-1>", lambda e: command())
            canvas.bind("<Enter>", lambda e: draw_button(self.theme['accent_fg'], color))
            canvas.bind("<Leave>", lambda e: draw_button(color, self.theme['accent_fg']))
        return canvas

    def log(self, message, level="INFO"):
        def _log():
            self.log_text.config(state="normal")
            timestamp = datetime.datetime.now().strftime("[%H:%M:%S] ")
            self.log_text.insert(tk.END, timestamp + message + "\n", level.upper())
            self.log_text.see(tk.END)
            self.log_text.config(state="disabled")
        self.master.after(0, _log)

    def update_dependencies(self, *args):
        spotify_enabled = self.vars['module_spotify'].get()
        for control in self.spotify_controls.values():
            if spotify_enabled:
                control.configure(bg=control.master.cget('bg'))
            else:
                control.configure(bg=self.theme['text_dark'])
        self.update_preview()

    def load_settings_to_gui(self):
        for key, var in self.vars.items():
            config_val = self.config.get(key)
            if config_val is not None:
                if isinstance(var, tk.Text):
                    var.delete('1.0', tk.END)
                    if isinstance(config_val, list): var.insert('1.0', "\n".join(str(v) for v in config_val))
                else: var.set(config_val)

    def apply_gui_to_config(self):
        for key, var in self.vars.items():
            if isinstance(var, tk.Text):
                self.config[key] = [line for line in var.get('1.0', tk.END).strip().split('\n') if line]
            else:
                try: self.config[key] = var.get()
                except tk.TclError: pass

    def update_preview(self, *args):
        temp_config = {}
        for key, var in self.vars.items():
             if isinstance(var, tk.Text):
                temp_config[key] = [line for line in var.get('1.0', tk.END).strip().split('\n') if line]
             else:
                try: temp_config[key] = var.get()
                except tk.TclError: pass

        full_config = {**self.config, **temp_config}
        preview_thread = VrcOscThread(full_config, lambda *a: None)
        preview_thread.anim_state['char_idx'] = 10
        full_text = preview_thread.build_message()
        del preview_thread
        canvas = self.preview_canvas
        canvas.delete("all")
        y_pos = 20
        spotify_progress_line = next((line for line in full_text.split('\n') if line.strip().startswith('[')), None)    
        for line in full_text.split('\n'):
            if line == spotify_progress_line:
                try:
                    progress_str = line.split(']')[0] + ']'
                    timestamp_str = line.split(']')[1].strip()
                    filled_char = full_config.get('progress_filled_char', '█')
                    filled_count = progress_str.count(filled_char)
                    total_count = full_config.get('progress_bar_length', 20)
                    ratio = filled_count / total_count if total_count > 0 else 0
                    bar_width = canvas.winfo_width() - 140
                    if bar_width < 20: bar_width = 20
                    canvas.create_rectangle(20, y_pos-8, 20 + bar_width, y_pos + 8, outline=self.theme['border'], width=2)
                    canvas.create_rectangle(22, y_pos-6, 22 + (bar_width-4) * ratio, y_pos + 6, fill=self.theme['accent'], outline="")
                    canvas.create_text(25 + bar_width, y_pos, text=timestamp_str, font=self.fonts['preview'], fill=self.theme['text'], anchor="w")
                    y_pos += 25
                except:
                    canvas.create_text(20, y_pos, text=line, font=self.fonts['preview'], fill=self.theme['text'], anchor="w")
                    y_pos += 20
            else:
                canvas.create_text(20, y_pos, text=line, font=self.fonts['preview'], fill=self.theme['text'], anchor="w")
                y_pos += 20

    def start_osc(self):
        self.start_button.config(state="disabled")
        self.stop_button.config(state="normal")
        self.save_button.config(state="disabled")
        self.log("OSC Transmission ENGAGED.", "green")   
        self.apply_gui_to_config()
        self.osc_thread = VrcOscThread(self.config, self.log)
        self.osc_thread.start()

    def stop_osc(self):
        if self.osc_thread and self.osc_thread.is_alive():
            self.osc_thread.stop()
        self.start_button.config(state="normal")
        self.stop_button.config(state="disabled")
        self.save_button.config(state="normal")
        self.log("OSC Transmission HALTED.", "red")

    def on_closing(self):
        if messagebox.askokcancel("SYSTEM SHUTDOWN", "Cease OSC transmission and exit the application?"):
            if self.osc_thread and self.osc_thread.is_alive():
                self.osc_thread.stop()
            self.master.destroy()

if __name__ == "__main__":
    try:
        from ctypes import windll
        windll.shcore.SetProcessDpiAwareness(1)
    except:
        pass
    root = tk.Tk()
    try:
        font.Font(family="Orbitron", size=1)
    except tk.TclError:
        print("Warning: 'Orbitron' font not found. Please install it for the best visual experience. Falling back to default fonts.")

    app = Application(master=root)
    root.protocol("WM_DELETE_WINDOW", app.on_closing)
    root.mainloop()

