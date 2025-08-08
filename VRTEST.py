# Simply cleaning up the code
# By https://github.com/pytmg
#  Splitting Sections


import tkinter as tk
from tkinter import messagebox, font, filedialog, colorchooser
import threading, time, datetime, random, json, os, asyncio, math

IS_WINDOWS = os.name == 'nt'
try:
    from pythonosc import udp_client
    import spotipy
    from spotipy.oauth2 import SpotifyOAuth
    import psutil
    from pypresence import Presence
    if IS_WINDOWS:
        from winsdk.windows.media.control import GlobalSystemMediaTransportControlsSessionManager as MediaManager
        from winsdk.windows.storage.streams import DataReader, Buffer, InputStreamOptions
except ImportError as e:
    messagebox.showerror("Missing Library", f"A required library is missing: {e.name}.\n\nPlease install it by running:\n'pip install {e.name}'")
    exit()

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROFILES_DIR = os.path.join(SCRIPT_DIR, "profiles")
APP_SETTINGS_FILE = os.path.join(SCRIPT_DIR, "config.json")
CACHE_FILE = os.path.join(SCRIPT_DIR, ".spotipyoauthcache")
os.makedirs(PROFILES_DIR, exist_ok=True)

DEFAULT_CONFIG = {
    "module_spotify": True, "module_clock": True, "module_fps": True, "module_sys_stats": True,
    "module_heartbeat": True, "module_animated_text": True, "module_local_media": True,
    "clock_show_seconds": True,
    "spotify_client_id": "YOUR_SPOTIFY_CLIENT_ID", "spotify_client_secret": "YOUR_SPOTIFY_CLIENT_SECRET",
    "spotify_redirect_uri": "http://localhost:8888/callback", "spotify_show_device": False,
    "spotify_show_song_name": True, "spotify_show_progress_bar": True, "spotify_show_timestamp": True,
    "watermark_text": "VRChat OSC Pro", "progress_bar_length": 20, "progress_filled_char": "█",
    "progress_empty_char": "─", "separator_char": "•",
    "animated_texts": ["discord.gg/encryptic", "https://longno.co.uk"],
    "animation_speed": 0.15, "rewrite_pause": 2.5, "update_interval": 1.0,
    "discord_rpc_enabled": True, "discord_rpc_show_spotify": True,
    "discord_rpc_details": "Controlling VRChat Chatbox", "discord_rpc_state": "Project Encryptic",
    "discord_rpc_large_image": "logo", "discord_rpc_large_text": "VRChat OSC Pro",
    "discord_rpc_button_label": "Get This App", "discord_rpc_button_url": "https://discord.gg/encryptic",
    "avatar_parameters": [],
    "theme_accent": "#58A6FF"
}


class WindowsMediaManager:
    def __init__(self, log_callback):
        self.log = log_callback
        self.current_media_info = {"title": "", "artist": ""}
        self.is_running = False
        self._thread = None

    async def _get_media_info(self):
        try:
            sessions = await MediaManager.request_async()
            current_session = sessions.get_current_session()
            if current_session:
                info = await current_session.try_get_media_properties_async()
                info_dict = {song_property.name: info.lookup(song_property) for song_property in info}
                self.current_media_info['title'] = info_dict.get('title', 'Unknown Title')
                self.current_media_info['artist'] = info_dict.get('artist', 'Unknown Artist')
                return self.current_media_info
        except Exception:
            self.current_media_info['title'] = ""
            self.current_media_info['artist'] = ""
        return self.current_media_info

    async def _main_loop(self):
        self.log("Local Media listener started.", "info")
        while self.is_running:
            await self._get_media_info()
            await asyncio.sleep(5)

    def _run_loop(self):
        asyncio.run(self._main_loop())

    def start(self):
        if not IS_WINDOWS:
            self.log("Local Media is only supported on Windows.", "orange")
            return
        self.is_running = True
        self._thread = threading.Thread(target=self._run_loop)
        self._thread.daemon = True
        self._thread.start()

    def stop(self):
        self.is_running = False

class VrcOscThread(threading.Thread):
    def __init__(self, config, log_callback):
        super().__init__()
        self.config = config
        self.log = log_callback
        self.is_running = True
        self.osc_client = udp_client.SimpleUDPClient("127.0.0.1", 9000)
        self.spotify_client = None
        self.rpc = None
        self.rpc_start_time = 0
        self.anim_state = {"list_idx": 0, "char_idx": 0, "forward": True, "last_update": 0, "pause_until": 0}
        self.last_heartbeat_flash = 0
        self.media_manager = WindowsMediaManager(self.log)

    def setup_discord_presence(self):
        if not self.config.get("discord_rpc_enabled"): return
        try:
            self.rpc = Presence('1390807459328823297'); self.rpc.connect(); self.rpc_start_time = time.time()
            self.log("Discord Rich Presence connected.", "green")
        except Exception as e: self.rpc = None; self.log(f"Could not connect to Discord: {e}", "orange")

    def update_discord_presence(self, spotify_info=None):
        if not self.rpc: return
        try:
            details = self.config.get('discord_rpc_details', 'Using OSC')
            if self.config.get('discord_rpc_show_spotify') and spotify_info and spotify_info.get('is_playing'):
                details = f"🎵 {spotify_info['name']}"[:128]
            payload = {'details': details, 'state': self.config.get('discord_rpc_state'), 'start': int(self.rpc_start_time), 'large_image': self.config.get('discord_rpc_large_image'), 'large_text': self.config.get('discord_rpc_large_text')}
            button_label, button_url = self.config.get('discord_rpc_button_label'), self.config.get('discord_rpc_button_url')
            if button_label and button_url and (button_url.startswith("http://") or button_url.startswith("https://")):
                payload['buttons'] = [{"label": button_label, "url": button_url}]
            self.rpc.update(**payload)
        except Exception: self.rpc.close(); self.rpc = None; self.log("Discord presence update failed. Is Discord running?", "orange")

    def setup_spotify(self):
        cid = self.config.get('spotify_client_id')
        if not cid or cid == 'YOUR_SPOTIFY_CLIENT_ID': self.log("Spotify Error: Client ID not set.", "red"); return
        try:
            self.log("Authenticating Spotify...", "orange")
            auth_manager = SpotifyOAuth(client_id=self.config['spotify_client_id'], client_secret=self.config['spotify_client_secret'], redirect_uri=self.config['spotify_redirect_uri'], scope="user-read-currently-playing", open_browser=False, cache_path=CACHE_FILE)
            self.spotify_client = spotipy.Spotify(auth_manager=auth_manager); self.spotify_client.current_user(); self.log("Spotify Authenticated Successfully!", "green")
        except Exception as e: self.spotify_client = None; self.log(f"Spotify Auth Error: {str(e).splitlines()[0]}", "red"); self.log("Ensure credentials are correct and auth prompt is accepted.", "orange")

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
        if self.config.get('module_local_media'): self.media_manager.start()
        last_message, last_rpc_update, rpc_update_interval = "", 0, 15.0
        while self.is_running:
            try:
                now = time.time()
                if self.config.get("discord_rpc_enabled") and not self.rpc: self.setup_discord_presence()
                spotify_info = self.get_spotify_info() if self.config.get('module_spotify') else None
                current_message = self.build_message(spotify_info_override=spotify_info)
                if current_message != last_message: self.osc_client.send_message("/chatbox/input", [current_message, True]); last_message = current_message
                if self.rpc and (now - last_rpc_update > rpc_update_interval): self.update_discord_presence(spotify_info); last_rpc_update = now
                time.sleep(self.config.get('update_interval', 1.0))
            except Exception as e: self.log(f"OSC Loop Error: {e}", "red"); time.sleep(3)

    def build_message(self, spotify_info_override=None):
        lines, sep, line1_parts = [], f" {self.config.get('separator_char', '|')} ", []
        if self.config.get('module_clock'): line1_parts.append(f"🕒 {datetime.datetime.now().strftime('%H:%M:%S' if self.config.get('clock_show_seconds') else '%H:%M')}")
        if self.config.get('module_heartbeat'):
            now = time.time()
            if now - self.last_heartbeat_flash >= 5: self.last_heartbeat_flash = now
            if now - self.last_heartbeat_flash < 1.0: line1_parts.append("❤")
        if self.config.get('module_fps'): line1_parts.append(f"🚀 {random.randint(249, 359)} FPS")
        if self.config.get('module_sys_stats'): line1_parts.append(f"💻 CPU: {psutil.cpu_percent():.0f}% | RAM: {psutil.virtual_memory().percent:.0f}%")
        if line1_parts: lines.append(sep.join(line1_parts))
        spotify_info = spotify_info_override

        if self.config.get('module_local_media') and not (spotify_info and spotify_info['is_playing']):
            media = self.media_manager.current_media_info
            if media and media['title']:
                lines.append(f"💿 {media['title']} - {media['artist']}")
        if spotify_info and spotify_info['is_playing']:

            spotify_line, progress_parts = [], []
            if self.config.get('spotify_show_song_name'):
                prefix = "🎵" if spotify_info['is_playing'] else "⏸️"; song_name = spotify_info['name']
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
            now = time.time(); texts = [t for t in self.config.get('animated_texts', []) if t]
            if texts and now > self.anim_state.get('pause_until', 0):
                if (now - self.anim_state['last_update']) > self.config.get('animation_speed', 0.15):
                    self.anim_state['last_update'] = now; active_text = texts[self.anim_state['list_idx'] % len(texts)]
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
        self.is_running = False; self.media_manager.stop()
        if self.rpc:
            try: self.rpc.close(); self.log("Discord Rich Presence connection closed.", "info")
            except Exception as e: self.log(f"Error closing Discord presence: {e}", "red")
        try: self.osc_client.send_message("/chatbox/input", ["", True])
        except Exception as e: self.log(f"Could not clear chatbox: {e}", "orange")
        self.log("OSC thread stopped.")

class HUDFrame(tk.Canvas):
    def __init__(self, master, theme, fonts, title="", **kwargs):
        super().__init__(master, highlightthickness=0, bg=master.cget('bg'), **kwargs)
        self.title, self.theme, self.fonts = title, theme, fonts
        self.bind("<Configure>", self._draw)
        self.content_frame = tk.Frame(self, bg=self.theme['bg_light'])
        self.content_frame.place(x=15, y=40, relwidth=1, relheight=1, width=-30, height=-55)
    def _draw(self, event=None):
        self.delete("all"); width, height = self.winfo_width(), self.winfo_height()
        if width < 50 or height < 50: return
        points = [15, 0, width, 0, width, height - 15, width - 15, height, 0, height, 0, 15]

        self.create_polygon(points, fill="", outline=self.theme['glow'], width=4)
        self.create_polygon(points, fill=self.theme['bg_light'], outline=self.theme['border'], width=2)
        self.create_text(30, 18, text=self.title, font=self.fonts['header'], fill=self.theme['accent'], anchor="w")
        self.create_line(20, 35, width - 20, 35, fill=self.theme['border'])

class HUDToggleSwitch(tk.Canvas):
    def __init__(self, master, variable, theme, fonts, command=None, **kwargs):
        super().__init__(master, width=60, height=28, **kwargs)
        self.variable, self.command, self.theme, self.fonts, self.master = variable, command, theme, fonts, master
        self.configure(bg=self.master.cget('bg'), highlightthickness=0)
        self.bind("<Button-1>", self._toggle); self.bind("<Enter>", lambda e: self.config(cursor="hand2")); self.bind("<Leave>", lambda e: self.config(cursor=""))
        self.variable.trace_add("write", self._update_display); self._update_display()
    def _toggle(self, event=None): self.variable.set(not self.variable.get()); self.command() if self.command else None
    def _update_display(self, *args):
        self.delete("all"); is_on = self.variable.get()
        self.create_rectangle(2, 2, 58, 26, fill=self.theme['bg_dark'], outline=self.theme['border'], width=2)
        if is_on:
            self.create_rectangle(4, 4, 56, 24, fill=self.theme['accent'], outline=""); self.create_rectangle(32, 4, 56, 24, fill=self.theme['accent_fg'], outline="")
            self.create_text(18, 14, text="ON", font=self.fonts['small_bold'], fill=self.theme['accent_fg'])
        else:
            self.create_rectangle(4, 4, 28, 24, fill=self.theme['text_dark'], outline=""); self.create_text(42, 14, text="OFF", font=self.fonts['small_bold'], fill=self.theme['text_dark'])


class HUDSlider(tk.Canvas):
    def __init__(self, master, variable, theme, fonts, command=None, **kwargs):
        super().__init__(master, height=28, **kwargs)
        self.variable, self.command, self.theme, self.fonts = variable, command, theme, fonts
        self.configure(bg=master.cget('bg'), highlightthickness=0)
        self.bind("<Configure>", self._update_display)
        self.bind("<B1-Motion>", self._on_drag)
        self.bind("<Button-1>", self._on_drag)
        self.variable.trace_add("write", self._update_display)
    def _on_drag(self, event):
        width = self.winfo_width() - 20
        value = (event.x - 10) / width
        value = max(0.0, min(1.0, value))
        self.variable.set(round(value, 3))
        if self.command: self.command(value)
    def _update_display(self, *args):
        self.delete("all")
        width, height = self.winfo_width(), self.winfo_height()
        if width < 20: return
        value = self.variable.get()
        bar_y = height / 2

        self.create_line(10, bar_y, width - 10, bar_y, fill=self.theme['border'], width=4)

        if value > 0: self.create_line(10, bar_y, 10 + (width - 20) * value, bar_y, fill=self.theme['accent'], width=4)

        handle_x = 10 + (width - 20) * value
        self.create_oval(handle_x - 6, bar_y - 6, handle_x + 6, bar_y + 6, fill=self.theme['accent_fg'], outline=self.theme['border'])

class Application(tk.Frame):
    def __init__(self, master=None):
        self.app_settings = self.load_app_settings()
        self.theme = {}
        super().__init__(master)
        self.master = master
        self.config = {}
        self.current_profile_path = self.app_settings.get("last_profile")
        self.setup_theme_and_fonts()
        self.master.title("Project Encryptic :: VRChat OSC"); self.master.geometry("1200x900"); self.master.minsize(1100, 800)
        self.master.configure(bg=self.theme['bg_dark'])
        self.pack(fill="both", expand=True)
        self.vars, self.osc_thread = {}, None
        self.avatar_param_rows = []
        self.widget_registry = []
        self.avatar_osc_client = udp_client.SimpleUDPClient("127.0.0.1", 9000)
        self.create_widgets()
        if not self.load_profile(self.current_profile_path):

            self.config = DEFAULT_CONFIG.copy()
            self.log("No default profile found. Loading default settings.", "orange")
        self.load_settings_to_gui()
        self.bind_traces()
        self.update_dependencies()
        self.update_preview()
        self.log("UI Initialized. Welcome to Encryptic OSC.", "accent")

    def load_app_settings(self):
        try:
            with open(APP_SETTINGS_FILE, 'r') as f: return json.load(f)
        except (FileNotFoundError, json.JSONDecodeError): return {"last_profile": ""}
    def save_app_settings(self):
        self.app_settings["last_profile"] = self.current_profile_path
        with open(APP_SETTINGS_FILE, 'w') as f: json.dump(self.app_settings, f, indent=4)
    def setup_theme_and_fonts(self):
        accent_color = self.config.get("theme_accent", DEFAULT_CONFIG["theme_accent"])
        self.theme = {'bg_dark': '#0D1117', 'bg_light': '#161B22', 'border': '#30363D', 'text': '#C9D1D9', 'text_dark': '#8B949E', 'accent': accent_color, 'accent_fg': '#FFFFFF', 'glow': accent_color, 'green': '#238636', 'red': '#DA3633', 'orange': '#F0883E'}
        self.fonts = {'title': ("Orbitron", 24, "bold"), 'header': ("Orbitron", 12, "bold"), 'body': ("Segoe UI", 10), 'small_bold': ("Segoe UI", 8, "bold"), 'preview': ("Consolas", 11), 'log': ("Consolas", 10)}
    def load_profile(self, path):
        if not path or not os.path.exists(path): return False
        try:
            with open(path, 'r') as f:
                user_config = json.load(f)
                self.config = DEFAULT_CONFIG.copy()
                self.config.update(user_config)
                self.current_profile_path = path
                self.save_app_settings()
                self.log(f"Loaded profile: {os.path.basename(path)}", "green")
                return True
        except (json.JSONDecodeError, TypeError):
            self.log(f"Failed to load profile: {os.path.basename(path)}. It may be corrupted.", "red")
            return False
    def do_load_profile(self):
        path = filedialog.askopenfilename(initialdir=PROFILES_DIR, title="Load Profile", filetypes=(("JSON files", "*.json"), ("All files", "*.*")))
        if path and self.load_profile(path):
            self.redraw_ui()
    def do_save_profile_as(self):
        path = filedialog.asksaveasfilename(initialdir=PROFILES_DIR, title="Save Profile As", filetypes=(("JSON files", "*.json"),), defaultextension=".json")
        if path:
            self.current_profile_path = path
            self.apply_gui_to_config()
            with open(path, 'w') as f: json.dump(self.config, f, indent=4)
            self.log(f"Profile saved as {os.path.basename(path)}", "green")
            self.save_app_settings()
    def do_change_theme(self):
        color_code = colorchooser.askcolor(title="Choose accent color", initialcolor=self.theme['accent'])
        if color_code and color_code[1]:
            self.config['theme_accent'] = color_code[1]
            self.redraw_ui()
    def redraw_ui(self):
        self.setup_theme_and_fonts()
        self.master.configure(bg=self.theme['bg_dark'])
        for widget in self.widget_registry:
            if hasattr(widget, '_draw'): widget._draw()
            if hasattr(widget, '_update_display'): widget._update_display()
        self.load_settings_to_gui()
        self.log("UI theme updated.", "accent")

    def create_widgets(self):
        self.background_canvas = tk.Canvas(self, bg=self.theme['bg_dark'], highlightthickness=0); self.background_canvas.place(relwidth=1, relheight=1)
        self.background_canvas.bind("<Configure>", self.draw_background_pattern); self.widget_registry.append(self.background_canvas)
        self.grid_rowconfigure(0, weight=1); self.grid_rowconfigure(1, weight=1)
        self.grid_columnconfigure(0, weight=1); self.grid_columnconfigure(1, weight=1); self.grid_columnconfigure(2, weight=1)
        self.create_modules_panel(); self.create_spotify_panel(); self.create_discord_rpc_panel()
        self.create_style_panel(); self.create_avatar_panel(); self.create_preview_and_log_panel()

    def draw_background_pattern(self, event=None):
        self.background_canvas.delete("all"); width, height = event.width, event.height
        for x in range(0, width + 100, 100):
            for y in range(0, height + 86, 86):
                offset = 50 if (y // 86) % 2 == 0 else 0; points = []
                for i in range(6):
                    angle_deg, angle_rad = 60 * i - 30, math.pi / 180 * angle_deg
                    px, py = x + offset + 40 * math.cos(angle_rad), y + 40 * math.sin(angle_rad)
                    points.extend([px, py])
                self.background_canvas.create_polygon(points, fill="", outline=self.theme['border'], width=1)

    def create_entry_row(self, parent, var_name, label_text):
        self.vars[var_name] = tk.StringVar()
        row = tk.Frame(parent, bg=parent.cget('bg'))
        tk.Label(row, text=label_text, font=self.fonts['body'], bg=parent.cget('bg'), fg=self.theme['text'], width=12, anchor='w').pack(side="left")
        entry_bg = tk.Frame(row, bg=self.theme['border'], relief='flat', bd=1)
        entry = tk.Entry(entry_bg, textvariable=self.vars[var_name], bg=self.theme['bg_dark'], fg=self.theme['text'], relief='flat', insertbackground=self.theme['accent_fg'])
        entry.pack(fill='x', expand=True, padx=1, pady=1); entry_bg.pack(side="left", fill="x", expand=True)
        row.pack(fill="x", pady=5, padx=10)
    
    def bind_traces(self):
        for var_name in self.vars:
            if isinstance(self.vars[var_name], tk.StringVar):
                self.vars[var_name].trace_add("write", lambda *a, v=var_name: self.update_dependencies())

    def create_setting_row(self, parent, var_name, text, tooltip_text):
        self.vars[var_name] = tk.BooleanVar()
        row = tk.Frame(parent, bg=parent.cget('bg')); label = tk.Label(row, text=text, font=self.fonts['body'], bg=parent.cget('bg'), fg=self.theme['text']); label.pack(side="left", padx=(0,10))
        switch = HUDToggleSwitch(row, self.vars[var_name], self.theme, self.fonts, command=self.update_dependencies); switch.pack(side="right"); row.pack(fill="x", pady=8, padx=10)
        self.widget_registry.append(switch)
        return switch

    def create_modules_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="CORE MODULES"); panel.grid(row=0, column=0, sticky="nsew", padx=10, pady=(10,5)); content = panel.content_frame
        self.create_setting_row(content, "module_clock", "Clock:", ""); self.create_setting_row(content, "module_heartbeat", "Heartbeat:", "")
        self.create_setting_row(content, "module_fps", "Fake FPS:", ""); self.create_setting_row(content, "module_sys_stats", "System Stats:", "")
        self.create_setting_row(content, "module_animated_text", "Animated Text:", "")
        self.create_setting_row(content, "module_local_media", "Local Media:", "")
        self.widget_registry.append(panel)

    def create_spotify_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="SPOTIFY"); panel.grid(row=0, column=1, sticky="nsew", padx=10, pady=(10,5)); content = panel.content_frame
        self.spotify_master_switch = self.create_setting_row(content, "module_spotify", "Enable Spotify:", ""); self.spotify_controls = {}
        self.spotify_controls['song'] = self.create_setting_row(content, "spotify_show_song_name", "Show Song:", "")
        self.spotify_controls['bar'] = self.create_setting_row(content, "spotify_show_progress_bar", "Show Progress Bar:", "")
        self.spotify_controls['time'] = self.create_setting_row(content, "spotify_show_timestamp", "Show Timestamp:", "")
        self.spotify_controls['device'] = self.create_setting_row(content, "spotify_show_device", "Show Device:", "")
        self.widget_registry.append(panel)

    def create_discord_rpc_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="DISCORD RPC"); panel.grid(row=0, column=2, sticky="nsew", padx=10, pady=(10,5)); content = panel.content_frame
        self.create_setting_row(content, "discord_rpc_enabled", "Enable RPC:", ""); self.create_setting_row(content, "discord_rpc_show_spotify", "Show Spotify Song:", "")
        self.create_entry_row(content, "discord_rpc_details", "Details Text:"); self.create_entry_row(content, "discord_rpc_state", "State Text:")
        self.create_entry_row(content, "discord_rpc_large_image", "Image Key:"); self.create_entry_row(content, "discord_rpc_large_text", "Image Text:")
        self.create_entry_row(content, "discord_rpc_button_label", "Button Label:"); self.create_entry_row(content, "discord_rpc_button_url", "Button URL:")
        self.widget_registry.append(panel)

    def create_style_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="STYLE & TEXT"); panel.grid(row=1, column=0, sticky="nsew", padx=10, pady=(5,10)); content = panel.content_frame
        self.create_entry_row(content, 'watermark_text', "Watermark:"); self.create_entry_row(content, 'separator_char', "Separator:")

        theme_button = self.create_hud_button(content, "🎨 CHANGE ACCENT", self.theme['accent'], self.do_change_theme); theme_button.pack(pady=10, padx=10, fill='x')
        tk.Label(content, text="Animated Text (one per line):", font=self.fonts['body'], bg=content.cget('bg'), fg=self.theme['text_dark']).pack(fill="x", pady=(10,5), padx=10)
        text_bg = tk.Frame(content, bg=self.theme['border'], relief='flat', bd=1)
        self.vars['animated_texts'] = tk.Text(text_bg, height=3, bg=self.theme['bg_dark'], fg=self.theme['text'], font=self.fonts['body'], relief='flat', insertbackground=self.theme['accent_fg'], bd=0)
        self.vars['animated_texts'].pack(padx=1, pady=1, fill="both", expand=True); self.vars['animated_texts'].bind("<KeyRelease>", self.update_preview); text_bg.pack(fill="both", expand=True, padx=10)
        self.widget_registry.append(panel)


    def create_avatar_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="AVATAR PARAMETERS"); panel.grid(row=1, column=1, sticky="nsew", padx=10, pady=(5,10)); self.avatar_panel_content = panel.content_frame
        add_button = self.create_hud_button(self.avatar_panel_content, "+ ADD PARAMETER", self.theme['green'], self.add_avatar_param_row); add_button.pack(side="bottom", fill="x", padx=10, pady=10)
        self.widget_registry.append(panel)

    def add_avatar_param_row(self, param=None):
        if not param: param = {"name": f"Param{len(self.avatar_param_rows)+1}", "path": "", "type": "float", "value": 0.0}
        row_frame = tk.Frame(self.avatar_panel_content, bg=self.avatar_panel_content.cget('bg')); row_frame.pack(fill="x", padx=10, pady=2)
        row_data = {"frame": row_frame}
        # Name Entry
        row_data["name_var"] = tk.StringVar(value=param['name']); tk.Entry(row_frame, textvariable=row_data["name_var"], width=10, bg=self.theme['bg_dark'], fg=self.theme['text'], relief='flat').pack(side="left", padx=2)
        # Path Entry
        row_data["path_var"] = tk.StringVar(value=param['path']); tk.Entry(row_frame, textvariable=row_data["path_var"], width=15, bg=self.theme['bg_dark'], fg=self.theme['text'], relief='flat').pack(side="left", padx=2)
        # Type Dropdown
        row_data["type_var"] = tk.StringVar(value=param['type']); type_menu = tk.OptionMenu(row_frame, row_data["type_var"], "float", "bool", command=lambda v, d=row_data: self.update_avatar_param_control(d)); type_menu.config(width=5); type_menu.pack(side="left", padx=2)
        # Control Frame (for slider or button)
        row_data["control_frame"] = tk.Frame(row_frame, bg=row_frame.cget('bg')); row_data["control_frame"].pack(side="left", fill='x', expand=True, padx=2)
        # Delete Button
        del_button = tk.Button(row_frame, text="X", bg=self.theme['red'], fg='white', relief='flat', command=lambda d=row_data: self.remove_avatar_param_row(d)); del_button.pack(side="right", padx=2)
        self.avatar_param_rows.append(row_data)
        self.update_avatar_param_control(row_data, initial_value=param['value'])

    def update_avatar_param_control(self, row_data, initial_value=None):
        for widget in row_data["control_frame"].winfo_children(): widget.destroy()
        param_type = row_data["type_var"].get()
        if param_type == "float":
            val = float(initial_value) if initial_value is not None else 0.0
            row_data["value_var"] = tk.DoubleVar(value=val)
            slider = HUDSlider(row_data["control_frame"], row_data["value_var"], self.theme, self.fonts, command=lambda v, p=row_data["path_var"]: self.send_avatar_param(p.get(), v, "float"))
            slider.pack(fill='x', expand=True)
            self.widget_registry.append(slider)
        elif param_type == "bool":
            val = bool(initial_value) if initial_value is not None else False
            row_data["value_var"] = tk.BooleanVar(value=val)
            button = HUDToggleSwitch(row_data["control_frame"], row_data["value_var"], self.theme, self.fonts, command=lambda p=row_data["path_var"], v=row_data["value_var"]: self.send_avatar_param(p.get(), v.get(), "bool"))
            button.pack()
            self.widget_registry.append(button)

    def remove_avatar_param_row(self, row_data):
        row_data["frame"].destroy()
        self.avatar_param_rows.remove(row_data)

    def send_avatar_param(self, path, value, param_type):
        if not path: return
        address = f"/avatar/parameters/{path}"

        if param_type == 'float': final_value = float(value)
        elif param_type == 'bool': final_value = bool(value)
        else: final_value = value
        try:
            self.avatar_osc_client.send_message(address, final_value)
        except Exception as e:
            self.log(f"Failed to send avatar param: {e}", "red")

    def create_preview_and_log_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="LIVE PREVIEW & SYSTEM CONTROLS"); panel.grid(row=1, column=2, sticky="nsew", padx=10, pady=(5,10)); content = panel.content_frame
        content.grid_rowconfigure(0, weight=1); content.grid_columnconfigure(0, weight=3); content.grid_columnconfigure(1, weight=2)
        self.preview_canvas = tk.Canvas(content, bg=content.cget('bg'), highlightthickness=0); self.preview_canvas.grid(row=0, column=0, sticky="nsew", padx=5, pady=5); self.widget_registry.append(self.preview_canvas)
        log_controls_frame = tk.Frame(content, bg=content.cget('bg')); log_controls_frame.grid(row=0, column=1, sticky="nsew", padx=5, pady=5)
        log_controls_frame.grid_rowconfigure(0, weight=1); log_controls_frame.grid_rowconfigure(1, weight=0); log_controls_frame.grid_columnconfigure(0, weight=1)
        log_bg = tk.Frame(log_controls_frame, bg=self.theme['border'], relief='flat', bd=1)
        self.log_text = tk.Text(log_bg, height=10, state="disabled", wrap="word", bg=self.theme['bg_dark'], fg=self.theme['text_dark'], font=self.fonts['log'], relief='flat', bd=0)
        self.log_text.pack(fill="both", expand=True, padx=1, pady=1); log_bg.grid(row=0, column=0, sticky="nsew", pady=(0, 10))
        self.log_text.tag_config("INFO", foreground=self.theme['text_dark']); self.log_text.tag_config("ACCENT", foreground=self.theme['accent']); self.log_text.tag_config("GREEN", foreground=self.theme['green']); self.log_text.tag_config("ORANGE", foreground=self.theme['orange']); self.log_text.tag_config("RED", foreground=self.theme['red'])
        controls_frame = tk.Frame(log_controls_frame, bg=content.cget('bg')); controls_frame.grid(row=1, column=0, sticky="ew")
        controls_frame.grid_columnconfigure(0, weight=1); controls_frame.grid_columnconfigure(1, weight=1); controls_frame.grid_columnconfigure(2, weight=1)

        self.load_button = self.create_hud_button(controls_frame, "📂 LOAD", self.theme['accent'], self.do_load_profile); self.load_button.grid(row=0, column=0, sticky="ew", padx=2)
        self.save_as_button = self.create_hud_button(controls_frame, "💾 SAVE AS", self.theme['accent'], self.do_save_profile_as); self.save_as_button.grid(row=0, column=1, sticky="ew", padx=2)
        self.start_button = self.create_hud_button(controls_frame, "▶ START", self.theme['green'], self.start_osc); self.start_button.grid(row=1, column=0, sticky="ew", padx=2, pady=5)
        self.stop_button = self.create_hud_button(controls_frame, "■ STOP", self.theme['red'], self.stop_osc, "disabled"); self.stop_button.grid(row=1, column=1, sticky="ew", padx=2, pady=5)
        self.widget_registry.append(panel)

    def create_hud_button(self, parent, text, color, command, state='normal'):
        canvas = tk.Canvas(parent, height=40, bg=parent.cget('bg'), highlightthickness=0); self.widget_registry.append(canvas)
        canvas.bind("<Configure>", lambda e: self.draw_button_state(canvas, text, color, canvas.cget('state')))
        canvas.bind("<Button-1>", lambda e: command() if canvas.cget('state') == 'normal' else None)
        canvas.bind("<Enter>", lambda e: self.draw_button_state(canvas, text, color, 'hover'))
        canvas.bind("<Leave>", lambda e: self.draw_button_state(canvas, text, color, canvas.cget('state')))
        self.draw_button_state(canvas, text, color, state)
        return canvas

    def draw_button_state(self, canvas, text, color, state):
        current_state = 'disabled' if state == 'disabled' else 'normal'
        if canvas.cget('state') != current_state: canvas.config(state=current_state)
        canvas.delete("all"); width, height = canvas.winfo_width(), canvas.winfo_height()
        if width < 10 or height < 10: return
        points = [5, 0, width, 0, width, height - 5, width - 5, height, 0, height, 0, 5]
        bg_color, fg_color = color, self.theme['accent_fg']
        if state == 'disabled': bg_color, fg_color = self.theme['bg_light'], self.theme['text_dark']
        elif state == 'hover' and current_state == 'normal': bg_color, fg_color = self.theme['accent_fg'], color
        canvas.create_polygon(points, fill=bg_color, outline=self.theme['border'])
        canvas.create_text(width/2, height/2, text=text, font=self.fonts['header'], fill=fg_color)

    def log(self, message, level="INFO"):
        def _log():
            self.log_text.config(state="normal"); timestamp = datetime.datetime.now().strftime("[%H:%M:%S] ")
            self.log_text.insert(tk.END, timestamp + message + "\n", level.upper()); self.log_text.see(tk.END); self.log_text.config(state="disabled")
        self.master.after(0, _log)

    def update_dependencies(self, *args):
        self.apply_gui_to_config()
        spotify_enabled = self.config.get('module_spotify')
        for control in self.spotify_controls.values(): control.master.configure(bg=self.theme['bg_light'] if spotify_enabled else self.theme['bg_dark'])
        if self.osc_thread and self.osc_thread.is_alive(): self.log("Settings changed. Restart OSC to apply.", "orange")
        self.update_preview()

    def load_settings_to_gui(self):
        for key, var in self.vars.items():
            config_val = self.config.get(key)
            if config_val is not None:
                if isinstance(var, tk.Text): var.delete('1.0', tk.END); var.insert('1.0', "\n".join(str(v) for v in config_val)) if isinstance(config_val, list) else None
                else: var.set(config_val)

        for row in self.avatar_param_rows: row['frame'].destroy()
        self.avatar_param_rows.clear()
        for param in self.config.get("avatar_parameters", []): self.add_avatar_param_row(param)

    def apply_gui_to_config(self):
        for key, var in self.vars.items():
            if isinstance(var, tk.Text): self.config[key] = [line for line in var.get('1.0', tk.END).strip().split('\n') if line]
            else:
                try: self.config[key] = var.get()
                except tk.TclError: pass

        self.config["avatar_parameters"] = []
        for row in self.avatar_param_rows:
            self.config["avatar_parameters"].append({"name": row["name_var"].get(), "path": row["path_var"].get(), "type": row["type_var"].get(), "value": row["value_var"].get()})

    def update_preview(self, *args):
        temp_config = self.config.copy(); self.apply_gui_to_config(); temp_config.update(self.config)
        preview_thread = VrcOscThread(temp_config, lambda *a: None); preview_thread.anim_state['char_idx'] = 10; full_text = preview_thread.build_message(); del preview_thread; canvas = self.preview_canvas; canvas.delete("all"); y_pos = 20
        spotify_progress_line = next((line for line in full_text.split('\n') if line.strip().startswith('[')), None)
        for line in full_text.split('\n'):
            if line == spotify_progress_line:
                try:
                    progress_str, timestamp_str = line.split(']')[0] + ']', line.split(']')[1].strip(); filled_char, filled_count = temp_config.get('progress_filled_char', '█'), progress_str.count(filled_char); total_count = temp_config.get('progress_bar_length', 20); ratio = filled_count / total_count if total_count > 0 else 0; bar_width = max(canvas.winfo_width() - 140, 20)
                    canvas.create_rectangle(20, y_pos-8, 20 + bar_width, y_pos + 8, outline=self.theme['border'], width=2); canvas.create_rectangle(22, y_pos-6, 22 + (bar_width-4) * ratio, y_pos + 6, fill=self.theme['accent'], outline=""); canvas.create_text(25 + bar_width, y_pos, text=timestamp_str, font=self.fonts['preview'], fill=self.theme['text'], anchor="w"); y_pos += 25
                except: canvas.create_text(20, y_pos, text=line, font=self.fonts['preview'], fill=self.theme['text'], anchor="w"); y_pos += 20
            else: canvas.create_text(20, y_pos, text=line, font=self.fonts['preview'], fill=self.theme['text'], anchor="w"); y_pos += 20

    def start_osc(self):
        self.draw_button_state(self.start_button, "▶ START", self.theme['green'], 'disabled'); self.draw_button_state(self.stop_button, "■ STOP", self.theme['red'], 'normal')
        self.draw_button_state(self.load_button, "📂 LOAD", self.theme['accent'], 'disabled'); self.draw_button_state(self.save_as_button, "💾 SAVE AS", self.theme['accent'], 'disabled')
        self.log("OSC Transmission ENGAGED.", "green"); self.apply_gui_to_config();
        
        if self.current_profile_path:
            with open(self.current_profile_path, 'w') as f: json.dump(self.config, f, indent=4)
        self.osc_thread = VrcOscThread(self.config, self.log); self.osc_thread.start()

    def stop_osc(self):
        if self.osc_thread and self.osc_thread.is_alive(): self.osc_thread.stop(); self.osc_thread.join(); self.osc_thread = None
        self.draw_button_state(self.start_button, "▶ START", self.theme['green'], 'normal'); self.draw_button_state(self.stop_button, "■ STOP", self.theme['red'], 'disabled')
        self.draw_button_state(self.load_button, "📂 LOAD", self.theme['accent'], 'normal'); self.draw_button_state(self.save_as_button, "💾 SAVE AS", self.theme['accent'], 'normal')
        self.log("OSC Transmission HALTED.", "red")

    def on_closing(self):
        if messagebox.askokcancel("SYSTEM SHUTDOWN", "Cease OSC transmission and exit the application?"):
            self.stop_osc(); self.master.destroy()

if __name__ == "__main__":
    try:
        if IS_WINDOWS: from ctypes import windll; windll.shcore.SetProcessDpiAwareness(1)
    except: pass
    root = tk.Tk()
    try: font.Font(family="Orbitron", size=1)
    except tk.TclError: print("Warning: 'Orbitron' font not found. Please install it for the best visual experience. Falling back to default fonts.")
    app = Application(master=root)
    root.protocol("WM_DELETE_WINDOW", app.on_closing)
    root.mainloop()


