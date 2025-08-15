# ==============================================================================
# VRChat OSC Pro (Modern GUI Edition)
# Original By: https://github.com/pytmg
# Complete GUI Overhaul by Gemini
# ==============================================================================

import tkinter as tk
from tkinter import ttk, messagebox, font, filedialog, colorchooser
import threading, time, datetime, random, json, os, asyncio, math, webbrowser

IS_WINDOWS = os.name == 'nt'

# --- Dependency Check (No changes needed) ---
missing_libs = []
lib_map = {
    'pythonosc': 'python-osc', 'spotipy': 'spotipy', 'psutil': 'psutil',
    'pypresence': 'pypresence', 'requests': 'requests', 'packaging': 'packaging'
}
if IS_WINDOWS:
    lib_map['winsdk'] = 'winsdk'
for imp, pip_name in lib_map.items():
    try: __import__(imp)
    except ImportError: missing_libs.append(pip_name)
if missing_libs:
    messagebox.showerror("Missing Libraries", "The following required libraries are missing:\n\n" + "\n".join([f"• {lib}" for lib in missing_libs]) + "\n\nPlease install them by running:\n" f"pip install {' '.join(missing_libs)}")
    exit()

from pythonosc import udp_client
import spotipy
from spotipy.oauth2 import SpotifyOAuth
import psutil
from pypresence import Presence
import requests
from packaging.version import parse as parse_version
if IS_WINDOWS:
    from winsdk.windows.media.control import GlobalSystemMediaTransportControlsSessionManager as MediaManager

CURRENT_VERSION = "3.0.1"
GITHUB_REPO = "Longno12/VRChat-OSC-Python"
CHANGELOG = """Version 3.0.1 - Resource Management Hotfix

Critical Fixes:
• Completely resolved all "unclosed transport" warnings during application exit
• Fixed Windows Media Manager resource leaks during shutdown
• Added proper asyncio event loop cleanup
• Implemented thread-safe termination of background processes
• Added timeout protections to prevent shutdown hangs

Under-the-Hood Improvements:
• Rewrote media monitoring system for better stability
• Added comprehensive resource cleanup handlers
• Improved error handling for media session operations
• Enhanced logging for troubleshooting media issues
• Optimized thread management during start/stop cycles
"""

def check_for_updates(log_callback):
    try:
        api_url = f"https://api.github.com/repos/{GITHUB_REPO}/releases/latest"
        response = requests.get(api_url, timeout=5)
        response.raise_for_status()
        latest = response.json()
        latest_v = parse_version(latest.get('tag_name', '0.0.0').lstrip('v'))
        if latest_v > parse_version(CURRENT_VERSION):
            log_callback(f"New version found: {latest_v}", "green")
            return latest_v, latest.get('html_url')
    except Exception as e:
        log_callback(f"Update check failed: {e}", "orange")
    return None, None

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROFILES_DIR = os.path.join(SCRIPT_DIR, "profiles")
APP_SETTINGS_FILE = os.path.join(SCRIPT_DIR, "config.json")
CACHE_FILE = os.path.join(SCRIPT_DIR, ".spotipyoauthcache")
os.makedirs(PROFILES_DIR, exist_ok=True)

DEFAULT_CONFIG = {
    "module_spotify": True, "module_clock": True, "module_fps": True, "module_sys_stats": True,
    "module_heartbeat": True, "module_animated_text": True, "module_local_media": True, "clock_show_seconds": True,
    "spotify_client_id": "YOUR_SPOTIFY_CLIENT_ID", "spotify_client_secret": "YOUR_SPOTIFY_CLIENT_SECRET",
    "spotify_redirect_uri": "https://longno12.github.io/Spotify-Verify-Link-help/", "spotify_show_device": False,
    "spotify_show_song_name": True, "spotify_show_progress_bar": True, "spotify_show_timestamp": True,
    "watermark_text": "VRChat OSC Pro", "progress_bar_length": 20, "progress_filled_char": "█", "progress_empty_char": "─", "separator_char": "•",
    "animated_texts": ["github.com/Longno12/VRChat-OSC-Python", "discord.gg/encryptic"], "animation_speed": 0.15, "rewrite_pause": 2.5, "update_interval": 1.0,
    "discord_rpc_enabled": True, "discord_rpc_show_spotify": True, "discord_rpc_details": "Controlling VRChat Chatbox", "discord_rpc_state": "Project Encryptic",
    "discord_rpc_large_image": "logo", "discord_rpc_large_text": "VRChat OSC Pro", "discord_rpc_button_label": "Get This App", "discord_rpc_button_url": "https://discord.gg/encryptic",
    "avatar_parameters": [], "theme_accent": "#007acc"
}

class WindowsMediaManager:
    def __init__(self, log_callback): 
        self.log = log_callback
        self.current_media_info = {"title": "", "artist": ""}
        self.is_running = False
        self._thread = None
        self._loop = None
        self._cleanup_complete = threading.Event()

    async def _get_media_info(self):
        try:
            sessions = await MediaManager.request_async()
            current_session = sessions.get_current_session()
            if current_session:
                info = await current_session.try_get_media_properties_async()
                self.current_media_info = {'title': info.title, 'artist': info.artist}
            else: 
                self.current_media_info = {"title": "", "artist": ""}
        except Exception as e:
            self.log(f"Media info error: {str(e)}", "orange")
            self.current_media_info = {"title": "", "artist": ""}

    async def _main_loop(self):
        self.log("Local Media listener started.", "info")
        while self.is_running:
            try:
                await self._get_media_info()
                await asyncio.sleep(5)
            except asyncio.CancelledError:
                break
            except Exception as e:
                self.log(f"Media loop error: {str(e)}", "orange")
                await asyncio.sleep(5)
        self._cleanup_complete.set()

    def _run_loop(self):
        self._loop = asyncio.new_event_loop()
        asyncio.set_event_loop(self._loop)
        try:
            self._loop.run_until_complete(self._main_loop())
        finally:
            tasks = asyncio.all_tasks(self._loop)
            for t in tasks:
                t.cancel()
            self._loop.run_until_complete(asyncio.gather(*tasks, return_exceptions=True))
            self._loop.close()
            self._loop = None

    def start(self):
        if not IS_WINDOWS: 
            return self.log("Local Media is only supported on Windows.", "orange")
        if self.is_running:
            return
        self.is_running = True
        self._cleanup_complete.clear()
        self._thread = threading.Thread(target=self._run_loop, daemon=True)
        self._thread.start()

    def stop(self):
        if not self.is_running:
            return
        self.is_running = False
        if self._loop and not self._loop.is_closed():
            self._loop.call_soon_threadsafe(self._loop.stop)
        if self._thread and self._thread.is_alive():
            self._thread.join(timeout=2.0)
            if not self._cleanup_complete.wait(timeout=1.0):
                self.log("Media manager cleanup timed out", "orange")

class VrcOscThread(threading.Thread):
    def __init__(self, config, log_callback, app_ref=None):
        super().__init__(); self.config, self.log, self.app_ref = config, log_callback, app_ref
        self.is_running, self.osc_client = True, udp_client.SimpleUDPClient("127.0.0.1", 9000)
        self.spotify_client, self.rpc, self.rpc_start_time = None, None, 0
        self.anim_state = {"list_idx": 0, "char_idx": 0, "forward": True, "last_update": 0, "pause_until": 0}
        self.last_heartbeat_flash, self.media_manager = 0, WindowsMediaManager(self.log)
    def setup_discord_presence(self):
        if not self.config.get("discord_rpc_enabled"): return
        try: self.rpc = Presence('1390807459328823297'); self.rpc.connect(); self.rpc_start_time = time.time(); self.log("Discord Rich Presence connected.", "green")
        except Exception as e: self.rpc = None; self.log(f"Could not connect to Discord: {e}", "orange")
    def update_discord_presence(self, spotify_info=None):
        if not self.rpc: return
        try:
            details = f"🎵 {spotify_info['name']}"[:128] if self.config.get('discord_rpc_show_spotify') and spotify_info and spotify_info.get('is_playing') else self.config.get('discord_rpc_details', 'Using OSC')
            payload = {'details': details, 'state': self.config.get('discord_rpc_state'), 'start': int(self.rpc_start_time), 'large_image': self.config.get('discord_rpc_large_image'), 'large_text': self.config.get('discord_rpc_large_text')}
            if (label := self.config.get('discord_rpc_button_label')) and (url := self.config.get('discord_rpc_button_url', '')) and url.startswith("http"): payload['buttons'] = [{"label": label, "url": url}]
            self.rpc.update(**payload)
        except Exception: self.rpc.close(); self.rpc = None; self.log("Discord presence update failed. Is Discord running?", "orange")
    def setup_spotify(self):
        if not (cid := self.config.get('spotify_client_id')) or cid == 'YOUR_SPOTIFY_CLIENT_ID': return self.log("Spotify Client ID not set.", "red")
        try:
            self.log("Authenticating Spotify...", "orange")
            auth_manager = SpotifyOAuth(client_id=self.config['spotify_client_id'], client_secret=self.config['spotify_client_secret'], redirect_uri=self.config['spotify_redirect_uri'], scope="user-read-currently-playing", open_browser=True, cache_path=CACHE_FILE)
            self.spotify_client = spotipy.Spotify(auth_manager=auth_manager); self.spotify_client.current_user(); self.log("Spotify Authenticated Successfully!", "green")
        except Exception as e:
            self.spotify_client = None; self.log(f"Spotify Auth Error: {str(e).splitlines()[0]}", "red")
            if ("INVALID_CLIENT" in str(e) or "Not found" in str(e)) and self.app_ref: self.app_ref.master.after(0, self.app_ref.prompt_spotify_setup)
    def get_spotify_info(self):
        if not self.spotify_client: return None
        try:
            track = self.spotify_client.current_user_playing_track()
            if not track or not track.get('item'): return {"is_playing": False}
            item = track['item']
            info = {"name": f"{item['name']} - {', '.join([a['name'] for a in item['artists']])}", "progress_ms": track.get('progress_ms', 0), "duration_ms": item.get('duration_ms', 1), "is_playing": track.get('is_playing', False)}
            if self.config.get('spotify_show_device') and track.get('device'): info['device'] = track['device'].get('name', 'Unknown')
            return info
        except spotipy.exceptions.SpotifyException as e:
            if e.http_status == 401: self.log("Spotify token expired. Re-authenticating...", "orange"); self.setup_spotify()
            else: self.log(f"Spotify API Error: {e}", "red")
        except Exception: self.log("An unknown Spotify error occurred.", "red")
        return {"is_playing": False, "name": "Spotify Error"}
    def _build_info_line(self):
        parts, sep = [], f"  {self.config.get('separator_char', '•')}  "
        if self.config.get('module_clock'): parts.append(f"🕒 {datetime.datetime.now().strftime('%H:%M:%S' if self.config.get('clock_show_seconds') else '%H:%M')}")
        if self.config.get('module_heartbeat'):
            now = time.time()
            if now - self.last_heartbeat_flash > 1: self.last_heartbeat_flash = now
            if now - self.last_heartbeat_flash < 0.5: parts.append("❤")
        if self.config.get('module_fps'): parts.append(f"🚀 {random.randint(85, 95)} FPS")
        if self.config.get('module_sys_stats'): parts.append(f"💻 CPU: {psutil.cpu_percent():.0f}% | RAM: {psutil.virtual_memory().percent:.0f}%")
        return sep.join(filter(None, parts))
    def _build_spotify_line(self, spotify_info):
        if not (spotify_info and spotify_info['is_playing']): return ""
        lines, p_parts = [], []
        if self.config.get('spotify_show_song_name'):
            name = spotify_info['name']
            if self.config.get('spotify_show_device') and 'device' in spotify_info: name += f" on {spotify_info['device']}"
            lines.append(f"🎵 {name}")
        if self.config.get('spotify_show_progress_bar'):
            p_len, ratio = self.config.get('progress_bar_length', 14), spotify_info.get('progress_ms',0) / spotify_info.get('duration_ms', 1)
            filled = int(ratio * p_len); p_parts.append(f"[{self.config.get('progress_filled_char','█') * filled}{self.config.get('progress_empty_char','─') * (p_len-filled)}]")
        if self.config.get('spotify_show_timestamp'):
            p_time, d_time = f"{int(spotify_info.get('progress_ms',0)/60000):02}:{int((spotify_info.get('progress_ms',0)/1000)%60):02}", f"{int(spotify_info.get('duration_ms',1)/60000):02}:{int((spotify_info.get('duration_ms',1)/1000)%60):02}"
            p_parts.append(f"{p_time}/{d_time}")
        if p_parts: lines.append(" ".join(p_parts))
        return "\n".join(lines)
    def _build_local_media_line(self): return f"💿 {self.media_manager.current_media_info['title']} - {self.media_manager.current_media_info['artist']}" if self.media_manager.current_media_info.get('title') else ""
    def _build_animated_text_line(self):
        if not self.config.get('module_animated_text'): return ""
        now, texts = time.time(), [t for t in self.config.get('animated_texts', []) if t]
        if not texts: return ""
        if now > self.anim_state.get('pause_until', 0):
            if (now - self.anim_state['last_update']) > self.config.get('animation_speed', 0.15):
                self.anim_state['last_update'] = now
                active_text = texts[self.anim_state['list_idx'] % len(texts)]
                if self.anim_state['forward']:
                    if self.anim_state['char_idx'] < len(active_text): self.anim_state['char_idx'] += 1
                    else: self.anim_state['pause_until'], self.anim_state['forward'] = now + self.config.get('rewrite_pause', 2.5), False
                else:
                    if self.anim_state['char_idx'] > 0: self.anim_state['char_idx'] -= 1
                    else: self.anim_state['forward'], self.anim_state['list_idx'], self.anim_state['pause_until'] = True, self.anim_state['list_idx'] + 1, now + 1.0
        return texts[self.anim_state['list_idx'] % len(texts)][:self.anim_state['char_idx']] or '\u200b'
    def build_message(self, spotify_info_override=None):
        lines = [self._build_info_line()]
        spotify_info = spotify_info_override or self.get_spotify_info() if self.config.get('module_spotify') else None
        if s_line := self._build_spotify_line(spotify_info): lines.append(s_line)
        elif self.config.get('module_local_media') and (lm_line := self._build_local_media_line()): lines.append(lm_line)
        if anim_line := self._build_animated_text_line(): lines.append(anim_line)
        if watermark := self.config.get('watermark_text'): lines.append(watermark)
        return "\n".join(filter(None, lines))
    def run(self):
        if self.config.get('module_spotify'): self.setup_spotify()
        if self.config.get('module_local_media'): self.media_manager.start()
        last_message, last_rpc_update = "", 0
        while self.is_running:
            try:
                if self.config.get("discord_rpc_enabled") and not self.rpc: self.setup_discord_presence()
                spotify_info = self.get_spotify_info() if self.config.get('module_spotify') else None
                if (current_message := self.build_message(spotify_info_override=spotify_info)) != last_message:
                    self.osc_client.send_message("/chatbox/input", [current_message, True]); last_message = current_message
                if self.rpc and (time.time() - last_rpc_update > 15.0): self.update_discord_presence(spotify_info); last_rpc_update = time.time()
                time.sleep(self.config.get('update_interval', 1.0))
            except Exception as e: self.log(f"OSC Loop Error: {e}", "red"); time.sleep(3)
    def stop(self):
        self.is_running = False; self.media_manager.stop()
        if self.rpc:
            try: self.rpc.close(); self.log("Discord RPC closed.", "info")
            except Exception: pass
        try: self.osc_client.send_message("/chatbox/input", ["", True])
        except Exception: pass
        self.log("OSC thread stopped.")

class ModernToggle(tk.Canvas):
    """A sleek, modern animated toggle switch."""
    def __init__(self, master, variable, theme, command=None, **kwargs):
        super().__init__(master, width=48, height=26, bg=master.cget('bg'), highlightthickness=0, **kwargs)
        self.variable = variable
        self.theme = theme
        self.command = command
        self.is_on = self.variable.get()
        self.knob_pos = 22 if self.is_on else 4

        self.bind("<Button-1>", self._toggle)
        self.variable.trace_add("write", self._update_from_var)
        self._draw()

    def _draw(self, knob_target=None):
        self.delete("all")
        bg_color = self.theme['accent'] if self.is_on else self.theme['bg_toggle']
        self.create_oval(1, 1, 25, 25, fill=bg_color, width=0)
        self.create_oval(23, 1, 47, 25, fill=bg_color, width=0)
        self.create_rectangle(13, 1, 35, 25, fill=bg_color, width=0)
        self.create_oval(self.knob_pos - 9, 4, self.knob_pos + 9, 22, fill=self.theme['fg'], width=0)

    def _animate(self):
        target = 22 if self.is_on else 4
        if self.knob_pos != target:
            step = 2 if self.is_on else -2
            self.knob_pos += step
            if (self.is_on and self.knob_pos > target) or \
               (not self.is_on and self.knob_pos < target):
                self.knob_pos = target
            self._draw()
            self.after(10, self._animate)

    def _toggle(self, event=None):
        self.is_on = not self.is_on
        self.variable.set(self.is_on)
        self._animate()
        if self.command: self.command()

    def _update_from_var(self, *args):
        if self.is_on != self.variable.get():
            self.is_on = self.variable.get()
            self._animate()

class App(tk.Frame):
    def __init__(self, master=None):
        self.app_settings = self.load_app_settings()
        self.config = DEFAULT_CONFIG.copy()
        super().__init__(master)
        self.master = master
        self.current_profile_path = self.app_settings.get("last_profile")

        self.setup_theme_and_fonts()
        self.master.title(f"VRChat OSC Pro v{CURRENT_VERSION}")
        self.master.geometry("1280x768")
        self.master.minsize(1100, 720)
        self.master.configure(bg=self.theme['bg'])
        self.pack(fill="both", expand=True)

        self.vars, self.pages, self.nav_buttons = {}, {}, {}
        self.osc_thread, self.avatar_param_rows = None, []
        self.avatar_osc_client = udp_client.SimpleUDPClient("127.0.0.1", 9000)
        self.current_page = 'core'

        self.create_styles()
        self.create_widgets()
        if not self.load_profile(self.current_profile_path): self.log("No default profile found. Loading defaults.", "orange")
        self.load_settings_to_gui()
        self.bind_traces()
        self.update_preview()
        self.log(f"Modern UI Initialized. Welcome to OSC Pro v{CURRENT_VERSION}.", "accent")
        self.show_page(self.current_page)
        self.master.after(100, self.run_first_time_setup)
        self.master.after(500, self.perform_update_check)

    def setup_theme_and_fonts(self):
        accent = self.config.get("theme_accent", DEFAULT_CONFIG["theme_accent"])
        self.theme = {
            'bg': '#1e1e1e', 'bg_light': '#252526', 'bg_lighter': '#333333', 'bg_toggle': '#4e4e4e',
            'border': '#404040', 'fg': '#cccccc', 'fg_dark': '#888888',
            'accent': accent, 'accent_fg': '#ffffff', 'green': '#32a852', 'red': '#e04f5f', 'orange': '#e5944f'
        }
        self.fonts = {
            'body': ('Segoe UI', 10), 'body_bold': ('Segoe UI', 10, 'bold'),
            'h1': ('Segoe UI', 18, 'bold'), 'h2': ('Segoe UI', 12, 'bold'),
            'sidebar': ('Segoe UI', 11, 'bold'), 'log': ('Consolas', 10), 'preview': ('Consolas', 10)
        }

    def create_styles(self):
        s = ttk.Style()
        s.theme_use('clam')

        s.configure('.', background=self.theme['bg'], foreground=self.theme['fg'], borderwidth=0, lightcolor=self.theme['bg'], darkcolor=self.theme['bg'])
        s.map('.', background=[('active', self.theme['bg_light']), ('disabled', self.theme['bg'])])

        s.configure("TNotebook", background=self.theme['bg'], borderwidth=0)
        s.configure("TNotebook.Tab", background=self.theme['bg_light'], foreground=self.theme['fg_dark'], padding=[10, 5], font=self.fonts['body_bold'], borderwidth=0)
        s.map("TNotebook.Tab", background=[("selected", self.theme['bg']), ("active", self.theme['bg_lighter'])], foreground=[("selected", self.theme['fg'])])

        s.configure("Treeview", background=self.theme['bg_light'], foreground=self.theme['fg'], fieldbackground=self.theme['bg_light'], font=self.fonts['body'], rowheight=28, borderwidth=1, relief='solid')
        s.configure("Treeview.Heading", background=self.theme['bg'], foreground=self.theme['fg'], font=self.fonts['body_bold'], relief='flat')
        s.map("Treeview.Heading", background=[('active', self.theme['bg_light'])])

    def create_widgets(self):
        sidebar = tk.Frame(self, bg=self.theme['bg_light'], width=220)
        sidebar.pack(side="left", fill="y")
        sidebar.pack_propagate(False)

        main_area = tk.Frame(self, bg=self.theme['bg'])
        main_area.pack(side="left", fill="both", expand=True)

        self.pages_container = tk.Frame(main_area, bg=self.theme['bg'])
        self.pages_container.pack(fill="both", expand=True, padx=30, pady=20)
        self.create_control_panel(main_area)

        tk.Label(sidebar, text="VRChat OSC Pro", font=self.fonts['h1'], bg=sidebar.cget('bg'), fg=self.theme['fg']).pack(pady=(20, 25), padx=20, anchor='w')
        nav_items = [
            ("core", "⚙️", "Core Modules"), ("spotify", "🎵", "Spotify"), ("discord", "💬", "Discord RPC"),
            ("style", "🎨", "Style & Text"), ("avatar", "👤", "Avatar Params"), ("updates", "📰", "Updates")
        ]
        for name, icon, text in nav_items:
            self.nav_buttons[name] = self.create_nav_button(sidebar, icon, text, lambda n=name: self.show_page(n))
        tk.Label(sidebar, text=f"v{CURRENT_VERSION}", font=self.fonts['body'], bg=sidebar.cget('bg'), fg=self.theme['fg_dark']).pack(side='bottom', pady=10)


        self.create_page_core()
        self.create_page_spotify()
        self.create_page_discord()
        self.create_page_style()
        self.create_page_avatar()
        self.create_page_updates()

    def create_control_panel(self, parent):
        panel = tk.Frame(parent, bg=self.theme['bg_light'], height=220)
        panel.pack(side="bottom", fill="x")
        panel.pack_propagate(False)

        notebook = ttk.Notebook(panel)
        notebook.pack(side="left", fill="both", expand=True, padx=10, pady=(0, 10))

        preview_frame = tk.Frame(notebook, bg=self.theme['bg'])
        self.preview_canvas = tk.Canvas(preview_frame, bg=self.theme['bg'], highlightthickness=0)
        self.preview_canvas.pack(fill="both", expand=True, padx=15, pady=10)
        notebook.add(preview_frame, text="Live Preview")

        log_frame = tk.Frame(notebook, bg=self.theme['bg'])
        self.log_text = tk.Text(log_frame, state="disabled", wrap="word", bg=self.theme['bg'], fg=self.theme['fg_dark'], font=self.fonts['log'], relief='flat', bd=0, selectbackground=self.theme['bg_lighter'])
        self.log_text.pack(fill="both", expand=True, padx=15, pady=10)
        log_tags = {"INFO": self.theme['fg_dark'], "ACCENT": self.theme['accent'], "GREEN": self.theme['green'], "ORANGE": self.theme['orange'], "RED": self.theme['red']}
        for tag, color in log_tags.items(): self.log_text.tag_config(tag, foreground=color)
        notebook.add(log_frame, text="System Log")

        controls_frame = tk.Frame(panel, bg=self.theme['bg_light'])
        controls_frame.pack(side="right", fill="y", padx=20, pady=10)
        self.load_button = self.create_button(controls_frame, "Load Profile", "📂", self.do_load_profile, style='secondary')
        self.load_button.pack(fill="x", pady=2)
        self.save_as_button = self.create_button(controls_frame, "Save As...", "💾", self.do_save_profile_as, style='secondary')
        self.save_as_button.pack(fill="x", pady=(2, 10))
        self.start_button = self.create_button(controls_frame, "START", "▶", self.start_osc, style='primary_green')
        self.start_button.pack(fill="x", ipady=5, pady=2)
        self.stop_button = self.create_button(controls_frame, "STOP", "■", self.stop_osc, style='primary_red', state='disabled')
        self.stop_button.pack(fill="x", ipady=5, pady=2)


    def create_page_frame(self, name):
        page = tk.Frame(self.pages_container, bg=self.theme['bg'])
        self.pages[name] = page
        return page

    def create_settings_card(self, parent, title):
        card = tk.Frame(parent, bg=self.theme['bg_light'], highlightbackground=self.theme['border'], highlightthickness=1)
        card.pack(fill='x', pady=(0, 15))
        tk.Label(card, text=title, font=self.fonts['h2'], bg=card.cget('bg'), fg=self.theme['fg']).pack(anchor='w', padx=15, pady=(10, 5))
        content = tk.Frame(card, bg=card.cget('bg'))
        content.pack(fill='x', padx=15, pady=(0, 15))
        return content

    def create_nav_button(self, parent, icon, text, command):
        btn = tk.Button(parent, text=f" {icon}  {text}", font=self.fonts['sidebar'], fg=self.theme['fg_dark'], bg=self.theme['bg_light'],
                        anchor='w', relief='flat', command=command, activebackground=self.theme['bg_lighter'],
                        activeforeground=self.theme['fg'], borderwidth=0, highlightthickness=0, takefocus=False)
        btn.pack(fill='x', padx=10, ipady=8)
        return btn

    def create_button(self, parent, text, icon, command, style='primary', state='normal'):
        colors = {
            'primary': {'bg': self.theme['accent'], 'fg': self.theme['accent_fg'], 'active': '#0062a1'},
            'primary_green': {'bg': self.theme['green'], 'fg': self.theme['accent_fg'], 'active': '#268a42'},
            'primary_red': {'bg': self.theme['red'], 'fg': self.theme['accent_fg'], 'active': '#b03f4c'},
            'secondary': {'bg': self.theme['bg_lighter'], 'fg': self.theme['fg'], 'active': self.theme['border']},
        }[style]
        btn = tk.Button(parent, text=f"{icon} {text}", font=self.fonts['body_bold'], bg=colors['bg'], fg=colors['fg'],
                        relief='flat', command=command, state=state, borderwidth=0,
                        activebackground=colors['active'], activeforeground=colors['fg'])
        return btn

    def create_setting_row(self, parent, var_name, text, is_toggle=True):
        self.vars[var_name] = tk.BooleanVar() if is_toggle else tk.StringVar()
        row = tk.Frame(parent, bg=parent.cget('bg'))
        row.pack(fill='x', pady=7)
        tk.Label(row, text=text, font=self.fonts['body'], bg=row.cget('bg'), fg=self.theme['fg']).pack(side='left')
        if is_toggle:
            ModernToggle(row, self.vars[var_name], self.theme, command=self.update_dependencies).pack(side='right')
        else:
            entry_frame = tk.Frame(row, bg=self.theme['bg'], highlightbackground=self.theme['border'], highlightthickness=1)
            entry_frame.pack(side='right', fill='x', expand=True, padx=(20,0))
            entry = tk.Entry(entry_frame, textvariable=self.vars[var_name], bg=self.theme['bg'], fg=self.theme['fg'], relief='flat', insertbackground=self.theme['fg'], width=40)
            entry.pack(padx=8, pady=4, fill='x')
        return row

    def create_text_area(self, parent, var_name):
        text_frame = tk.Frame(parent, bg=self.theme['bg'], highlightbackground=self.theme['border'], highlightthickness=1)
        text_frame.pack(fill='both', expand=True, pady=5)
        self.vars[var_name] = tk.Text(text_frame, height=5, bg=self.theme['bg'], fg=self.theme['fg'], font=self.fonts['body'], relief='flat', insertbackground=self.theme['fg'], bd=0)
        self.vars[var_name].pack(padx=8, pady=5, fill="both", expand=True)
        self.vars[var_name].bind("<KeyRelease>", self.update_preview)

    def create_page_core(self):
        page = self.create_page_frame("core")
        card = self.create_settings_card(page, "General Modules")
        self.create_setting_row(card, "module_clock", "Display Clock")
        self.create_setting_row(card, "clock_show_seconds", "   ↳ Show Seconds")
        self.create_setting_row(card, "module_heartbeat", "Display Heartbeat")
        self.create_setting_row(card, "module_fps", "Display Fake FPS Counter")
        self.create_setting_row(card, "module_sys_stats", "Display System Stats (CPU/RAM)")
        self.create_setting_row(card, "module_animated_text", "Enable Animated Text")
        self.create_setting_row(card, "module_local_media", "Enable Local Media (Windows Only)")

    def create_page_spotify(self):
        page = self.create_page_frame("spotify")
        main_card = self.create_settings_card(page, "Spotify Integration")
        self.create_setting_row(main_card, "module_spotify", "Enable Spotify Module")

        api_card = self.create_settings_card(page, "API Credentials")
        self.create_setting_row(api_card, "spotify_client_id", "Client ID:", is_toggle=False)
        self.create_setting_row(api_card, "spotify_client_secret", "Client Secret:", is_toggle=False)
        self.create_setting_row(api_card, "spotify_redirect_uri", "Redirect URI:", is_toggle=False)

        display_card = self.create_settings_card(page, "Display Options")
        self.create_setting_row(display_card, "spotify_show_song_name", "Show Song Name & Artist")
        self.create_setting_row(display_card, "spotify_show_progress_bar", "Show Progress Bar")
        self.create_setting_row(display_card, "spotify_show_timestamp", "Show Timestamps")
        self.create_setting_row(display_card, "spotify_show_device", "Show Playback Device")

    def create_page_discord(self):
        page = self.create_page_frame("discord")
        main_card = self.create_settings_card(page, "Discord Rich Presence (RPC)")
        self.create_setting_row(main_card, "discord_rpc_enabled", "Enable Discord RPC")
        self.create_setting_row(main_card, "discord_rpc_show_spotify", "Show Spotify song in status")

        content_card = self.create_settings_card(page, "Custom Content")
        self.create_setting_row(content_card, "discord_rpc_details", "Details Text:", is_toggle=False)
        self.create_setting_row(content_card, "discord_rpc_state", "State Text:", is_toggle=False)
        self.create_setting_row(content_card, "discord_rpc_large_image", "Large Image Key:", is_toggle=False)
        self.create_setting_row(content_card, "discord_rpc_large_text", "Large Image Text:", is_toggle=False)
        self.create_setting_row(content_card, "discord_rpc_button_label", "Button Label:", is_toggle=False)
        self.create_setting_row(content_card, "discord_rpc_button_url", "Button URL:", is_toggle=False)

    def create_page_style(self):
        page = self.create_page_frame("style")
        text_card = self.create_settings_card(page, "Chatbox Text")
        self.create_setting_row(text_card, "watermark_text", "Watermark:", is_toggle=False)
        self.create_setting_row(text_card, "separator_char", "Separator:", is_toggle=False)

        anim_card = self.create_settings_card(page, "Animated Text Lines")
        self.create_text_area(anim_card, 'animated_texts')

        theme_card = self.create_settings_card(page, "Application Theme")
        self.create_button(theme_card, "Change Accent Color", "🎨", self.do_change_theme, style='secondary').pack(fill='x', pady=5)

    def create_page_updates(self):
        page = self.create_page_frame("updates")
        card = self.create_settings_card(page, f"Changelog - Version {CURRENT_VERSION}")
        text = tk.Text(card, wrap="word", bg=card.cget('bg'), fg=self.theme['fg'], font=self.fonts['body'], relief='flat', bd=0, height=15)
        text.pack(fill="both", expand=True, pady=5)
        text.insert(tk.END, CHANGELOG)
        text.config(state="disabled")

    def create_page_avatar(self):
        page = self.create_page_frame("avatar")
        card = self.create_settings_card(page, "Avatar OSC Parameters")

        cols = ('name', 'path', 'type', 'control')
        self.avatar_tree = ttk.Treeview(card, columns=cols, show='headings', selectmode='none')
        for col, text, width in [('name', 'Name', 120), ('path', 'Address Path', 200), ('type', 'Type', 80), ('control', 'Control', 250)]:
            self.avatar_tree.heading(col, text=text)
            self.avatar_tree.column(col, width=width, stretch=False, anchor='w' if col != 'control' else 'center')
        self.avatar_tree.pack(fill='x', pady=5)

        btn_frame = tk.Frame(card, bg=card.cget('bg'))
        btn_frame.pack(fill='x', pady=5)
        self.create_button(btn_frame, "Add Parameter", "➕", lambda: self.add_avatar_param_row(), style='secondary').pack(side='left')

    def add_avatar_param_row(self, param=None):
        if not param: param = {"name": f"Param{len(self.avatar_param_rows)+1}", "path": "", "type": "float", "value": 0.0}

        iid = self.avatar_tree.insert('', 'end')
        row_data = {'iid': iid, 'param': param}

        name_var = tk.StringVar(value=param['name'])
        name_entry = ttk.Entry(self.avatar_tree, textvariable=name_var)
        name_var.trace_add('write', lambda *_, r=row_data, v=name_var: self._update_avatar_param_data(r, 'name', v.get()))
        self.avatar_tree.window_configure(iid, 'name', window=name_entry)

        path_var = tk.StringVar(value=param['path'])
        path_entry = ttk.Entry(self.avatar_tree, textvariable=path_var)
        path_var.trace_add('write', lambda *_, r=row_data, v=path_var: self._update_avatar_param_data(r, 'path', v.get()))
        self.avatar_tree.window_configure(iid, 'path', window=path_entry)

        type_var = tk.StringVar(value=param['type'])
        type_menu = ttk.Combobox(self.avatar_tree, textvariable=type_var, values=['float', 'bool'], state='readonly', width=7)
        type_menu.bind('<<ComboboxSelected>>', lambda e, r=row_data, v=type_var: self._update_avatar_param_control(r, v.get()))
        self.avatar_tree.window_configure(iid, 'type', window=type_menu)

        row_data['type_var'] = type_var
        self.avatar_param_rows.append(row_data)
        self._update_avatar_param_control(row_data, param['type'], initial_value=param['value'])

    def _update_avatar_param_data(self, row_data, key, value):
        row_data['param'][key] = value

    def _update_avatar_param_control(self, row_data, new_type, initial_value=None):
        self._update_avatar_param_data(row_data, 'type', new_type)
        iid = row_data['iid']

        control_frame = tk.Frame(self.avatar_tree, bg=self.theme['bg_light'])
        if new_type == "float":
            val = float(initial_value) if initial_value is not None else 0.0
            value_var = tk.DoubleVar(value=val)
            slider = ttk.Scale(control_frame, from_=0, to=1, orient='horizontal', variable=value_var,
                               command=lambda v, p=row_data['param']: self.send_avatar_param(p.get('path'), v, 'float'))
            slider.pack(side='left', expand=True, fill='x', padx=5)
            value_var.trace_add('write', lambda *_, r=row_data, v=value_var: self._update_avatar_param_data(r, 'value', v.get()))

        elif new_type == "bool":
            val = bool(initial_value) if initial_value is not None else False
            value_var = tk.BooleanVar(value=val)
            toggle = ModernToggle(control_frame, value_var, self.theme,
                                  command=lambda p=row_data['param'], v=value_var: self.send_avatar_param(p.get('path'), v.get(), 'bool'))
            toggle.pack(side='left', padx=5)
            value_var.trace_add('write', lambda *_, r=row_data, v=value_var: self._update_avatar_param_data(r, 'value', v.get()))

        del_btn = self.create_button(control_frame, "", "🗑️", lambda r=row_data: self.remove_avatar_param_row(r), style='secondary')
        del_btn.pack(side='right', padx=5)

        self.avatar_tree.window_configure(iid, 'control', window=control_frame)

    def remove_avatar_param_row(self, row_data):
        self.avatar_tree.delete(row_data['iid'])
        self.avatar_param_rows.remove(row_data)

    def send_avatar_param(self, path, value, param_type):
        if not path: return
        try: self.avatar_osc_client.send_message(f"/avatar/parameters/{path}", float(value) if param_type == 'float' else bool(value))
        except Exception as e: self.log(f"Failed to send param: {e}", "red")

    def show_page(self, page_name):
        self.current_page = page_name
        for p in self.pages.values(): p.pack_forget()
        self.pages[page_name].pack(fill='both', expand=True)
        for name, btn in self.nav_buttons.items():
            is_active = name == page_name
            btn.config(fg=self.theme['fg'] if is_active else self.theme['fg_dark'],
                       bg=self.theme['bg_lighter'] if is_active else self.theme['bg_light'])

    def run_first_time_setup(self):
        if not self.app_settings.get("ran_tutorial_v4"):
            if messagebox.askyesno("Welcome!", "It looks like this is your first time running this version.\n\nWould you like a quick tour of the new interface?"):
                messagebox.showinfo("Welcome!", "This is a completely redesigned VRChat OSC Pro!\n\n- Use the sidebar on the left to navigate.\n- Settings are grouped into cards for clarity.\n- The bottom panel contains your preview, log, and controls.\n\nEnjoy the new modern experience!")
            self.app_settings["ran_tutorial_v4"] = True; self.save_app_settings()

    def perform_update_check(self):
        self.log("Checking for updates...", "info")
        def _worker():
            """This function runs in the background thread."""
            version, url = check_for_updates(self.log)

            
            if version and url:
                self.master.after(0, self.prompt_for_update, version, url)
        threading.Thread(target=_worker, daemon=True).start()

    def prompt_for_update(self, new_version, url):
        if messagebox.askyesno("Update Available", f"A new version ({new_version}) is available!\n\nWould you like to go to the download page?"):
            webbrowser.open(url)
            if messagebox.askokcancel("Download Page Opened", "Your browser is open.\nPlease close this application before running the new version.\n\nExit now?"):
                self.on_closing()

    def load_app_settings(self):
        try:
            with open(APP_SETTINGS_FILE, 'r') as f: return json.load(f)
        except: return {"last_profile": "", "ran_tutorial_v4": False}

    def save_app_settings(self):
        self.app_settings["last_profile"] = self.current_profile_path
        with open(APP_SETTINGS_FILE, 'w') as f: json.dump(self.app_settings, f, indent=4)

    def load_profile(self, path):
        if not path or not os.path.exists(path): return False
        try:
            with open(path, 'r') as f:
                self.config = DEFAULT_CONFIG.copy(); self.config.update(json.load(f))
                self.current_profile_path = path; self.save_app_settings()
                self.log(f"Loaded profile: {os.path.basename(path)}", "green")
                self.redraw_ui()
                return True
        except Exception as e:
            self.log(f"Failed to load profile: {e}", "red")
            return False

    def do_load_profile(self):
        if path := filedialog.askopenfilename(initialdir=PROFILES_DIR, title="Load Profile", filetypes=(("JSON files", "*.json"),)):
            self.load_profile(path)

    def do_save_profile_as(self):
        if path := filedialog.asksaveasfilename(initialdir=PROFILES_DIR, title="Save Profile As", filetypes=(("JSON files", "*.json"),), defaultextension=".json"):
            self.current_profile_path = path; self.apply_gui_to_config()
            with open(path, 'w') as f: json.dump(self.config, f, indent=4)
            self.log(f"Profile saved as {os.path.basename(path)}", "green"); self.save_app_settings()

    def do_change_theme(self):
        if color := colorchooser.askcolor(title="Choose accent color", initialcolor=self.theme['accent']):
            self.config['theme_accent'] = color[1]; self.redraw_ui()

    def redraw_ui(self):
        self.setup_theme_and_fonts()
        for widget in self.winfo_children(): widget.destroy()
        self.pages.clear(); self.nav_buttons.clear()
        self.create_styles(); self.create_widgets(); self.load_settings_to_gui()
        self.show_page(self.current_page); self.update_preview(); self.log("UI theme updated.", "accent")

    def log(self, message, level="INFO"):
        self.master.after(0, lambda: (
            self.log_text.config(state="normal"),
            self.log_text.insert(tk.END, f"[{datetime.datetime.now().strftime('%H:%M:%S')}] {message}\n", level.upper()),
            self.log_text.see(tk.END),
            self.log_text.config(state="disabled")
        ))

    def bind_traces(self):
        for var_name, var in self.vars.items():
            if isinstance(var, tk.StringVar): var.trace_add("write", lambda *a, v=var_name: self.update_dependencies())

    def update_dependencies(self, *args):
        self.apply_gui_to_config()
        if self.osc_thread and self.osc_thread.is_alive(): self.log("Settings changed. Restart OSC to apply.", "orange")
        self.update_preview()

    def load_settings_to_gui(self):
        for key, var in self.vars.items():
            val = self.config.get(key)
            if val is not None:
                if isinstance(var, tk.Text): var.delete('1.0', tk.END); var.insert('1.0', "\n".join(str(v) for v in val) if isinstance(val, list) else val)
                else: var.set(val)

        for i in self.avatar_tree.get_children(): self.avatar_tree.delete(i)
        self.avatar_param_rows.clear()
        for param in self.config.get("avatar_parameters", []): self.add_avatar_param_row(param)

    def apply_gui_to_config(self):
        for key, var in self.vars.items():
            try:
                val = var.get('1.0', tk.END).strip().split('\n') if isinstance(var, tk.Text) else var.get()
                self.config[key] = [line for line in val if line] if isinstance(val, list) else val
            except tk.TclError: pass
        self.config["avatar_parameters"] = [r['param'] for r in self.avatar_param_rows]

    def update_preview(self, *args):
        self.apply_gui_to_config()
        temp_thread = VrcOscThread(self.config, lambda *a: None)
        temp_thread.anim_state['char_idx'] = 12
        text = temp_thread.build_message()
        del temp_thread

        self.preview_canvas.delete("all")
        y = 20
        
        preview_font_obj = font.Font(family=self.fonts['preview'][0], size=self.fonts['preview'][1])
        linespace = preview_font_obj.metrics('linespace')

        for line in text.split('\n'):
            self.preview_canvas.create_text(20, y, text=line, font=self.fonts['preview'], fill=self.theme['fg'], anchor="w")
            y += linespace + 5

    def start_osc(self):
        for btn in [self.start_button, self.load_button, self.save_as_button]: btn.config(state='disabled')
        self.stop_button.config(state='normal')
        self.log("OSC Transmission ENGAGED.", "green")
        self.apply_gui_to_config()
        if self.current_profile_path and os.path.exists(os.path.dirname(self.current_profile_path)):
            with open(self.current_profile_path, 'w') as f: json.dump(self.config, f, indent=4)
        self.osc_thread = VrcOscThread(self.config, self.log, self); self.osc_thread.start()

    def stop_osc(self):
        if self.osc_thread and self.osc_thread.is_alive(): self.osc_thread.stop(); self.osc_thread.join(); self.osc_thread = None
        for btn in [self.start_button, self.load_button, self.save_as_button]: btn.config(state='normal')
        self.stop_button.config(state='disabled')
        self.log("OSC Transmission HALTED.", "red")

    def on_closing(self):
     if messagebox.askokcancel("Exit Application", "Are you sure you want to stop OSC and exit?"):
        self.stop_osc()    
        if hasattr(self, 'osc_thread') and self.osc_thread:
            if hasattr(self.osc_thread, 'media_manager') and self.osc_thread.media_manager:
                try:
                    self.osc_thread.media_manager.stop()
                    if hasattr(self.osc_thread.media_manager, '_loop'):
                        loop = self.osc_thread.media_manager._loop
                        if loop and not loop.is_closed():
                            loop.call_soon_threadsafe(loop.stop)
                            time.sleep(0.1)
                except Exception as e:
                    self.log(f"Error during media manager cleanup: {e}", "orange")
        self.master.destroy()
        
        if threading.active_count() > 1:
            os._exit(0)

if __name__ == "__main__":
    if IS_WINDOWS:
        try: from ctypes import windll; windll.shcore.SetProcessDpiAwareness(1)
        except: print("Could not set DPI awareness.")
    root = tk.Tk()

    try: font.Font(family="Segoe UI", size=1)
    except: print("Segoe UI not found, using default fonts.")

    app = App(master=root)
    root.protocol("WM_DELETE_WINDOW", app.on_closing)
    root.mainloop()

