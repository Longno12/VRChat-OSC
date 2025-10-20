import tkinter as tk
from tkinter import messagebox, filedialog, colorchooser, simpledialog
import customtkinter as ctk
import threading, time, datetime, random, json, os, asyncio, webbrowser, shutil, re

IS_WINDOWS = os.name == 'nt'

# --- Dependency Check ---
missing_libs = []
lib_map = {
    'pythonosc': 'python-osc', 'spotipy': 'spotipy', 'psutil': 'psutil',
    'pypresence': 'pypresence', 'requests': 'requests', 'packaging': 'packaging',
    'customtkinter': 'customtkinter'
}
if IS_WINDOWS:
    lib_map['win32gui'] = 'pywin32'
    lib_map['winsdk'] = 'winsdk'

for imp, pip_name in lib_map.items():
    try:
        __import__(imp)
    except ImportError:
        missing_libs.append(pip_name)

if missing_libs:
    messagebox.showerror("Missing Libraries", "The following required libraries are missing:\n\n" + "\n".join([f"• {lib}" for lib in missing_libs]) + "\n\nPlease install them by running:\n" f"pip install {' '.join(missing_libs)}")
    exit()

from pythonosc import udp_client, dispatcher, osc_server
import spotipy
from spotipy.oauth2 import SpotifyOAuth
import psutil
from pypresence import Presence
import requests
from packaging.version import parse as parse_version
if IS_WINDOWS:
    import win32gui
    from winsdk.windows.media.control import GlobalSystemMediaTransportControlsSessionManager as MediaManager

CURRENT_VERSION = "4.1.0"
GITHUB_REPO = "Longno12/VRChat-OSC-Python"
CHANGELOG = """Version 4.1.0 - Modern UI & New Features

New Features:
• Complete UI Overhaul: Modern glass-morphism design with gradient accents
• Quick Actions Panel: One-click access to common functions
• Status Dashboard: Real-time monitoring of all modules
• Theme System: Multiple color themes with preview
• Compact Mode: Collapsible sidebar for more space

Improvements:
• Visual module toggles with status indicators
• Enhanced preview with live updates
• Better organization with expandable sections
• Dark/Light mode with accent color options
"""

# Modern color themes
THEMES = {
    "Ocean": {"primary": "#3B82F6", "secondary": "#06B6D4", "accent": "#8B5CF6"},
    "Emerald": {"primary": "#10B981", "secondary": "#059669", "accent": "#047857"},
    "Sunset": {"primary": "#F59E0B", "secondary": "#EF4444", "accent": "#8B5CF6"},
    "Rose": {"primary": "#EC4899", "secondary": "#F59E0B", "accent": "#8B5CF6"},
    "Slate": {"primary": "#64748B", "secondary": "#475569", "accent": "#334155"},
}

def check_for_updates(log_callback):
    try:
        api_url = f"https://api.github.com/repos/{GITHUB_REPO}/releases/latest"
        response = requests.get(api_url, timeout=5)
        response.raise_for_status()
        latest = response.json()
        latest_v = parse_version(latest.get('tag_name', '0.0.0').lstrip('v'))
        if latest_v > parse_version(CURRENT_VERSION):
            log_callback(f"New version found: {latest_v}", "success")
            return latest_v, latest.get('html_url')
    except Exception as e:
        log_callback(f"Update check failed: {e}", "warning")
    return None, None

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROFILES_DIR = os.path.join(SCRIPT_DIR, "profiles")
APP_SETTINGS_FILE = os.path.join(SCRIPT_DIR, "config.json")
CACHE_FILE = os.path.join(SCRIPT_DIR, ".spotipyoauthcache")
os.makedirs(PROFILES_DIR, exist_ok=True)

DEFAULT_CONFIG = {
    "module_spotify": True, "module_youtube": True, "module_clock": True, "module_fps": True, "module_sys_stats": True,
    "module_heartbeat": True, "module_animated_text": True, "module_local_media": True,
    "module_battery": True, "module_countdown": False, "module_info_cycling": False,
    "clock_show_seconds": True,
    "spotify_client_id": "YOUR_SPOTIFY_CLIENT_ID", "spotify_client_secret": "YOUR_SPOTIFY_CLIENT_SECRET",
    "spotify_redirect_uri": "https://www.longno.co.uk/spotify", "spotify_show_device": False,
    "spotify_show_song_name": True, "spotify_show_progress_bar": True, "spotify_show_timestamp": True,
    "watermark_text": "VRChat OSC Pro", "progress_bar_length": 20, "progress_filled_char": "▓", "progress_empty_char": "░", "separator_char": "•",
    "animated_texts": ["github.com/Longno12/VRChat-OSC-Python", "discord.gg/encryptic"], "animation_speed": 0.15, "rewrite_pause": 2.5, "update_interval": 1.0,
    "discord_rpc_enabled": True, "discord_rpc_show_spotify": True, "discord_rpc_details": "Controlling VRChat Chatbox", "discord_rpc_state": "Project Encryptic",
    "discord_rpc_large_image": "logo", "discord_rpc_large_text": "VRChat OSC Pro", "discord_rpc_button_label": "Get This App", "discord_rpc_button_url": "https://discord.gg/encryptic",
    "avatar_parameters": [],
    "countdown_target_date": "2029-12-31 23:59:59", "countdown_label": "Time Left:", "countdown_finished_text": "COUNTDOWN FINISHED!",
    "info_cycling_interval": 5, "info_cycling_pages": ["spotify", "system", "animated_text"],
    "presets": {
        "Default": {},
        "AFK": { "module_animated_text": False, "module_spotify": False, "module_youtube": False, "module_sys_stats": False, "watermark_text": "I am currently away from my keyboard." },
        "Gaming": { "module_spotify": False, "module_youtube": False, "module_animated_text": False }
    },
    "active_preset": "Default",
    "osc_server_enabled": True, "osc_server_ip": "127.0.0.1", "osc_server_port": 9001,
    "ui_theme": "System", "ui_color_theme": "Ocean", "sidebar_collapsed": False,
}

class VrcOscServerThread(threading.Thread):
    def __init__(self, app_ref, ip, port, log_callback):
        super().__init__(daemon=True)
        self.app = app_ref
        self.ip = ip
        self.port = port
        self.log = log_callback
        self.server = None

    def _handle_toggle_module(self, address, *args):
        if len(args) == 1 and isinstance(args[0], str):
            module_name = args[0]
            self.log(f"OSC IN: Received command to toggle module '{module_name}'.", "accent")
            self.app.after(0, self.app.handle_osc_toggle_module, module_name)
        else:
            self.log(f"OSC IN: Invalid arguments for {address}. Expected one string.", "warning")

    def _handle_set_preset(self, address, *args):
        if len(args) == 1 and isinstance(args[0], str):
            preset_name = args[0]
            self.log(f"OSC IN: Received command to set preset '{preset_name}'.", "accent")
            self.app.after(0, self.app.handle_osc_set_preset, preset_name)
        else:
            self.log(f"OSC IN: Invalid arguments for {address}. Expected one string.", "warning")

    def run(self):
        try:
            disp = dispatcher.Dispatcher()
            disp.map("/VRChatOSCPro/toggleModule", self._handle_toggle_module)
            disp.map("/VRChatOSCPro/setPreset", self._handle_set_preset)
            self.server = osc_server.ThreadingOSCUDPServer((self.ip, self.port), disp)
            self.log(f"OSC Server starting, listening on {self.ip}:{self.port}", "success")
            self.server.serve_forever()
        except Exception as e:
            self.log(f"Failed to start OSC Server: {e}", "error")

    def stop(self):
        if self.server:
            self.log("OSC Server shutting down.", "info")
            self.server.shutdown()
            self.server = None

class WindowsMediaManager:
    def __init__(self, log_callback):
        self.log = log_callback; self.current_media_info = {"title": "", "artist": ""}; self.is_running = False
        self._thread = None; self._loop = None
    async def _get_media_info(self):
        try:
            sessions = await MediaManager.request_async()
            current_session = sessions.get_current_session()
            if current_session:
                info = await current_session.try_get_media_properties_async()
                self.current_media_info = {'title': info.title, 'artist': info.artist}
            else: self.current_media_info = {"title": "", "artist": ""}
        except Exception: self.current_media_info = {"title": "", "artist": ""}
    async def _main_loop(self):
        self.log("Local Media listener started.", "info")
        while self.is_running:
            try: await self._get_media_info(); await asyncio.sleep(5)
            except asyncio.CancelledError: break
            except Exception as e: self.log(f"Media loop error: {str(e)}", "warning"); await asyncio.sleep(5)

    def _run_loop(self):
        self._loop = asyncio.new_event_loop(); asyncio.set_event_loop(self._loop)
        try: self._loop.run_until_complete(self._main_loop())
        finally:
            tasks = asyncio.all_tasks(self._loop)
            for t in tasks: t.cancel()
            self._loop.run_until_complete(asyncio.gather(*tasks, return_exceptions=True)); self._loop.close()

    def start(self):
        if not IS_WINDOWS: return self.log("Local Media is only supported on Windows.", "warning")
        if self.is_running: return
        self.is_running = True; self._thread = threading.Thread(target=self._run_loop, daemon=True); self._thread.start()
        
    def stop(self):
        if not self.is_running: return
        self.is_running = False
        if self._loop and not self._loop.is_closed(): self._loop.call_soon_threadsafe(self._loop.stop)
        if self._thread: self._thread.join(timeout=1.0)

class VrcOscThread(threading.Thread):
    def __init__(self, config, log_callback, app_ref=None):
        super().__init__(); self.config, self.log, self.app_ref = config, log_callback, app_ref
        self.is_running, self.osc_client = True, udp_client.SimpleUDPClient("127.0.0.1", 9000)
        self.spotify_client, self.rpc, self.rpc_start_time = None, None, 0
        self.anim_state = {"list_idx": 0, "char_idx": 0, "forward": True, "last_update": 0, "pause_until": 0}
        self.last_heartbeat_flash, self.media_manager = 0, WindowsMediaManager(self.log)
        self.cycle_page_index, self.last_cycle_time = 0, 0
        self.youtube_browsers = ["chrome.exe", "firefox.exe", "msedge.exe"]

    def setup_discord_presence(self):
        if not self.config.get("discord_rpc_enabled"): 
            return
        try:
            self.rpc = Presence('1390807459328823297')
            self.rpc.connect()
            self.rpc_start_time = int(time.time())
            self.log("Discord Rich Presence connected successfully!", "success")
            self.update_discord_presence()
        except Exception as e: 
            self.rpc = None
            error_msg = str(e)
            if "Connection to Discord failed" in error_msg:
                self.log("Discord not running or Discord connection failed", "warning")
            elif "Invalid Client ID" in error_msg:
                self.log("Invalid Discord Application ID", "error")
            else:
                self.log(f"Could not connect to Discord: {error_msg}", "error")

    def update_discord_presence(self, spotify_info=None):
        if not self.rpc:
            return
        try:
            payload = {
                'state': self.config.get('discord_rpc_state', 'Using VRChat OSC Pro'),
                'start': self.rpc_start_time,
                'large_image': self.config.get('discord_rpc_large_image', 'logo'),
                'large_text': self.config.get('discord_rpc_large_text', 'VRChat OSC Pro')
            }
            if (self.config.get('discord_rpc_show_spotify') and 
               spotify_info and spotify_info.get('is_playing')):
               song_name = spotify_info['name'][:120] + "..." if len(spotify_info['name']) > 120 else spotify_info['name']
               payload['details'] = f"🎵 {song_name}"
            else:
               payload['details'] = self.config.get('discord_rpc_details', 'Controlling VRChat Chatbox')
            button_label = self.config.get('discord_rpc_button_label')
            button_url = self.config.get('discord_rpc_button_url', '')
            if button_label and button_url.startswith("http"):
               payload['buttons'] = [{"label": button_label, "url": button_url}]
            self.rpc.update(**payload)
            self.log("Discord presence updated", "info")    
        except Exception as e:
            error_msg = str(e)
        if "Not connected to Discord" in error_msg or "Connection closed" in error_msg:
            self.log("Discord connection lost, attempting reconnect...", "warning")
            self.rpc = None
        else:
            self.log(f"Discord presence update failed: {error_msg}", "warning")

    def setup_spotify(self):
        if not (cid := self.config.get('spotify_client_id')) or cid == 'YOUR_SPOTIFY_CLIENT_ID': return self.log("Spotify Client ID not set.", "error")
        try:
            self.log("Authenticating Spotify...", "warning")
            auth_manager = SpotifyOAuth(client_id=self.config['spotify_client_id'], client_secret=self.config['spotify_client_secret'], redirect_uri=self.config['spotify_redirect_uri'], scope="user-read-currently-playing", open_browser=True, cache_path=CACHE_FILE)
            self.spotify_client = spotipy.Spotify(auth_manager=auth_manager); self.spotify_client.current_user(); self.log("Spotify Authenticated Successfully!", "success")
        except Exception as e:
            self.spotify_client = None; self.log(f"Spotify Auth Error: {str(e).splitlines()[0]}", "error")
            if ("INVALID_CLIENT" in str(e) or "Not found" in str(e)) and self.app_ref: self.app_ref.after(0, self.app_ref.prompt_spotify_setup)

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
            if e.http_status == 401: self.log("Spotify token expired. Re-authenticating...", "warning"); self.setup_spotify()
            else: self.log(f"Spotify API Error: {e}", "error")
        except Exception: self.log("An unknown Spotify error occurred.", "error")
        return {"is_playing": False, "name": "Spotify Error"}

    def get_youtube_info(self):
        if not IS_WINDOWS: return None
        try:
            def enum_windows_callback(hwnd, titles):
                if win32gui.IsWindowVisible(hwnd):
                    title = win32gui.GetWindowText(hwnd)
                    if " - YouTube" in title:
                        titles.append(title)
            
            window_titles = []
            win32gui.EnumWindows(enum_windows_callback, window_titles)

            for title in window_titles:
                clean_title = re.sub(r'^\(\d+\)\s*', '', title).replace(' - YouTube', '').strip()
                if clean_title:
                    return {"title": clean_title}
        except Exception as e:
            self.log(f"Could not get YouTube info: {e}", "warning")
        return None

    def _build_info_line(self):
        parts, sep = [], f"  {self.config.get('separator_char', '•')}  "
        if self.config.get('module_clock'): parts.append(f"🕒 {datetime.datetime.now().strftime('%H:%M:%S' if self.config.get('clock_show_seconds') else '%H:%M')}")
        if self.config.get('module_heartbeat'):
            now = time.time()
            if now - self.last_heartbeat_flash > 1: self.last_heartbeat_flash = now
            if now - self.last_heartbeat_flash < 0.5: parts.append("❤")
        if self.config.get('module_fps'): parts.append(f"🚀 {random.randint(280, 350)} FPS")
        if self.config.get('module_sys_stats'): parts.append(f"💻 CPU: {psutil.cpu_percent():.0f}% | RAM: {psutil.virtual_memory().percent:.0f}%")
        if self.config.get('module_battery'):
            try:
                battery = psutil.sensors_battery()
                if battery:
                    plugged = "🔌" if battery.power_plugged else ""
                    parts.append(f"🔋 {battery.percent:.0f}% {plugged}")
            except Exception: pass
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
            filled = int(ratio * p_len); p_parts.append(f"[{self.config.get('progress_filled_char','▓') * filled}{self.config.get('progress_empty_char','░') * (p_len-filled)}]")
        if self.config.get('spotify_show_timestamp'):
            p_time, d_time = f"{int(spotify_info.get('progress_ms',0)/60000):02}:{int((spotify_info.get('progress_ms',0)/1000)%60):02}", f"{int(spotify_info.get('duration_ms',1)/60000):02}:{int((spotify_info.get('duration_ms',1)/1000)%60):02}"
            p_parts.append(f"{p_time}/{d_time}")
        if p_parts: lines.append(" ".join(p_parts))
        return "\n".join(lines)

    def _build_youtube_line(self, youtube_info):
        if not youtube_info or not youtube_info.get('title'): return ""
        return f"📺 {youtube_info['title']}"

    def _build_local_media_line(self): 
        return f"💿 {self.media_manager.current_media_info['title']} - {self.media_manager.current_media_info['artist']}" if self.media_manager.current_media_info.get('title') else ""
    
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
    
    def _build_countdown_line(self):
        if not self.config.get('module_countdown'): return ""
        try:
            target_dt = datetime.datetime.strptime(self.config.get("countdown_target_date"), "%Y-%m-%d %H:%M:%S")
            remaining = target_dt - datetime.datetime.now()
            if remaining.total_seconds() > 0:
                d, rem = divmod(remaining.seconds, 86400); h, rem = divmod(rem, 3600); m, s = divmod(rem, 60)
                return f"{self.config.get('countdown_label', '')} {remaining.days}d {h:02}:{m:02}:{s:02}"
            else: return self.config.get("countdown_finished_text", "Countdown Finished!")
        except Exception: return "Invalid Countdown Date"

    def build_message(self, spotify_info_override=None, youtube_info_override=None):
        lines = []
        spotify_info = spotify_info_override or (self.get_spotify_info() if self.config.get('module_spotify') else None)
        youtube_info = youtube_info_override or (self.get_youtube_info() if self.config.get('module_youtube') else None)
        
        if not self.config.get("module_info_cycling"):
            lines.append(self._build_info_line())
            
            # Media Priority: Spotify > YouTube > Local Media
            if s_line := self._build_spotify_line(spotify_info): 
                lines.append(s_line)
            elif yt_line := self._build_youtube_line(youtube_info):
                lines.append(yt_line)
            elif self.config.get('module_local_media') and (lm_line := self._build_local_media_line()): 
                lines.append(lm_line)

            if anim_line := self._build_animated_text_line(): lines.append(anim_line)
            if cd_line := self._build_countdown_line(): lines.append(cd_line)
        else:
            pages = self.config.get('info_cycling_pages', [])
            if not pages: return "Info Cycling Enabled - No pages configured"
            current_page = pages[self.cycle_page_index % len(pages)]
            if current_page == "spotify": # This page now includes all media
                if s_line := self._build_spotify_line(spotify_info): lines.append(s_line)
                elif yt_line := self._build_youtube_line(youtube_info): lines.append(yt_line)
                elif lm_line := self._build_local_media_line(): lines.append(lm_line)
                else: lines.append("No media playing.")
            elif current_page == "system": lines.append(self._build_info_line())
            elif current_page == "animated_text": lines.append(self._build_animated_text_line() or "Animated Text")
            elif current_page == "countdown": lines.append(self._build_countdown_line() or "Countdown")
        
        if watermark := self.config.get('watermark_text'): lines.append(watermark)
        return "\n".join(filter(None, lines))

    def run(self):
        if self.config.get('module_spotify'): self.setup_spotify()
        if self.config.get('module_local_media'): self.media_manager.start()
        last_message, last_rpc_update = "", 0
        while self.is_running:
            try:
                if self.config.get("discord_rpc_enabled") and not self.rpc: self.setup_discord_presence()
                if self.config.get("module_info_cycling") and (time.time() - self.last_cycle_time > self.config.get("info_cycling_interval", 5)):
                    self.cycle_page_index += 1; self.last_cycle_time = time.time()
                
                spotify_info = self.get_spotify_info() if self.config.get('module_spotify') else None
                youtube_info = self.get_youtube_info() if self.config.get('module_youtube') else None

                current_message = self.build_message(spotify_info, youtube_info)
                if current_message != last_message:
                    self.osc_client.send_message("/chatbox/input", [current_message, True]); last_message = current_message
                
                if self.rpc and (time.time() - last_rpc_update > 15.0): 
                    self.update_discord_presence(spotify_info); last_rpc_update = time.time()
                
                time.sleep(self.config.get('update_interval', 1.0))
            except Exception as e: 
                self.log(f"OSC Loop Error: {e}", "error"); time.sleep(3)
    
    def stop(self):
        self.is_running = False; self.media_manager.stop()
        if self.rpc:
            try: self.rpc.close(); self.log("Discord RPC closed.", "info")
            except Exception: pass
        try: self.osc_client.send_message("/chatbox/input", ["", True])
        except Exception: pass
        self.log("OSC thread stopped.", "info")

class ModernApp(ctk.CTk):
    def __init__(self):
        super().__init__()
        self.app_settings = self.load_app_settings()
        self.config = DEFAULT_CONFIG.copy()
        
        ctk.set_appearance_mode(self.config.get("ui_theme", "System"))
        self.current_theme = THEMES[self.config.get("ui_color_theme", "Ocean")]
        
        self.title(f"VRChat OSC Pro v{CURRENT_VERSION}")
        self.geometry("1400x900")
        self.minsize(1200, 800)
        
        self.grid_columnconfigure(1, weight=1)
        self.grid_rowconfigure(0, weight=1)
        
        self.current_profile_path = self.app_settings.get("last_profile")
        self.vars, self.pages, self.nav_buttons, self.module_indicators = {}, {}, {}, {}
        self.osc_thread, self.osc_server_thread, self.avatar_param_rows = None, None, []
        self.avatar_osc_client = udp_client.SimpleUDPClient("127.0.0.1", 9000)
        self.current_page = 'dashboard'
        self.sidebar_collapsed = False

        self.create_modern_sidebar()
        self.create_main_area()
        self.create_control_panel()
        
        if not self.load_profile(self.current_profile_path):
             self.log("No default profile found. Loading defaults.", "warning")
        self.load_settings_to_gui()
        self.bind_traces()
        self.update_preview()
        
        self.log(f"Modern UI Initialized. Welcome to OSC Pro v{CURRENT_VERSION}.", "accent")
        self.show_page(self.current_page)
        self.protocol("WM_DELETE_WINDOW", self.on_closing)
        self.after(500, self.perform_update_check)

    def create_modern_sidebar(self):
        self.sidebar = ctk.CTkFrame(self, width=280, corner_radius=0, 
                                  fg_color=("gray90", "gray13"))
        self.sidebar.grid(row=0, column=0, rowspan=2, sticky="nsew")
        self.sidebar.grid_rowconfigure(8, weight=1)
        self.sidebar.grid_propagate(False)

        header_frame = ctk.CTkFrame(self.sidebar, fg_color="transparent", height=80)
        header_frame.grid(row=0, column=0, sticky="ew", padx=15, pady=(20, 10))
        header_frame.grid_propagate(False)
        
        ctk.CTkLabel(header_frame, text="VRChat OSC Pro", 
                    font=ctk.CTkFont(size=22, weight="bold"),
                    text_color=self.current_theme["primary"]).pack(anchor="w", pady=(10, 0))
        ctk.CTkLabel(header_frame, text="Control Panel", 
                    font=ctk.CTkFont(size=12),
                    text_color=("gray50", "gray60")).pack(anchor="w")

        nav_items = [
            ("dashboard", "📊 Dashboard"), 
            ("presets", "🎚️ Presets"), 
            ("media", "🎵 Media"), 
            ("system", "⚙️ System"), 
            ("appearance", "🎨 Appearance"),
            ("discord", "💬 Discord"), 
            ("avatar", "👤 Avatar"), 
            ("network", "🌐 Network")
        ]
        
        for i, (name, text) in enumerate(nav_items):
            btn = ctk.CTkButton(self.sidebar, text=text, 
                               command=lambda n=name: self.show_page(n),
                               fg_color="transparent", 
                               hover_color=("gray80", "gray20"),
                               anchor="w",
                               corner_radius=8,
                               height=40,
                               font=ctk.CTkFont(size=14))
            btn.grid(row=i+1, column=0, padx=12, pady=4, sticky="ew")
            self.nav_buttons[name] = btn

        quick_frame = ctk.CTkFrame(self.sidebar, fg_color="transparent")
        quick_frame.grid(row=len(nav_items)+1, column=0, sticky="ew", padx=12, pady=20)
        
        ctk.CTkLabel(quick_frame, text="Quick Actions", 
                    font=ctk.CTkFont(size=12, weight="bold")).pack(anchor="w")
        
        action_frame = ctk.CTkFrame(quick_frame, fg_color="transparent")
        action_frame.pack(fill="x", pady=(8, 0))
        
        self.start_btn = ctk.CTkButton(action_frame, text="▶ Start", 
                     command=self.start_osc,
                     fg_color=self.current_theme["primary"],
                     hover_color=self.current_theme["secondary"],
                     height=32, width=80)
        self.start_btn.pack(side="left", padx=(0, 5))
        
        self.stop_btn = ctk.CTkButton(action_frame, text="■ Stop", 
                     command=self.stop_osc,
                     fg_color="gray40",
                     hover_color="gray50",
                     height=32, width=80,
                     state="disabled")
        self.stop_btn.pack(side="left")

        version_frame = ctk.CTkFrame(self.sidebar, fg_color="transparent")
        version_frame.grid(row=9, column=0, sticky="ew", padx=15, pady=15)
        ctk.CTkLabel(version_frame, text=f"v{CURRENT_VERSION}", 
                    font=ctk.CTkFont(size=11),
                    text_color=("gray50", "gray60")).pack(side="left")
        
        theme_btn = ctk.CTkButton(version_frame, text="🎨", width=30, height=30,
                                 command=self.cycle_theme, 
                                 fg_color="transparent",
                                 hover_color=("gray80", "gray20"))
        theme_btn.pack(side="right")

    def create_main_area(self):
        self.main_area = ctk.CTkFrame(self, corner_radius=0, fg_color="transparent")
        self.main_area.grid(row=0, column=1, sticky="nsew", padx=0, pady=0)
        self.main_area.grid_rowconfigure(0, weight=1)
        self.main_area.grid_columnconfigure(0, weight=1)
        
        self.pages_container = ctk.CTkFrame(self.main_area, fg_color="transparent")
        self.pages_container.grid(row=0, column=0, sticky="nsew", padx=25, pady=20)
        self.pages_container.grid_rowconfigure(0, weight=1)
        self.pages_container.grid_columnconfigure(0, weight=1)

        self.create_pages()

    def create_control_panel(self):
        panel = ctk.CTkFrame(self.main_area, height=200, corner_radius=12)
        panel.grid(row=1, column=0, sticky="nsew", padx=25, pady=(0, 20))
        panel.grid_columnconfigure(0, weight=1)
        panel.grid_rowconfigure(0, weight=1)

        tab_view = ctk.CTkTabview(panel, segmented_button_selected_color=self.current_theme["primary"])
        tab_view.grid(row=0, column=0, sticky="nsew", padx=10, pady=10)
        tab_view.add("Live Preview")
        tab_view.add("System Log")
        tab_view.add("Module Status")

        preview_frame = ctk.CTkFrame(tab_view.tab("Live Preview"), fg_color="transparent")
        preview_frame.pack(fill="both", expand=True, padx=5, pady=5)
        
        self.preview_canvas = tk.Canvas(preview_frame, bg="#1E1E1E", highlightthickness=0, relief='flat')
        self.preview_canvas.pack(fill="both", expand=True)

        log_frame = ctk.CTkFrame(tab_view.tab("System Log"), fg_color="transparent")
        log_frame.pack(fill="both", expand=True, padx=5, pady=5)
        
        self.log_text = tk.Text(log_frame, state="disabled", wrap="word", bg="#1E1E1E", fg="#DCE4EE", 
                                relief='flat', bd=0, selectbackground="#4A4D50", font=("Consolas", 10))
        self.log_text.pack(fill="both", expand=True)
        
        self.log_text.tag_config("accent", foreground=self.current_theme["primary"])
        self.log_text.tag_config("success", foreground=self.current_theme["secondary"])
        self.log_text.tag_config("warning", foreground=self.current_theme["accent"])
        self.log_text.tag_config("error", foreground="#EF4444")
        self.log_text.tag_config("info", foreground="#DCE4EE")

        self.create_module_status_tab(tab_view.tab("Module Status"))

    def create_module_status_tab(self, parent):
        status_frame = ctk.CTkFrame(parent, fg_color="transparent")
        status_frame.pack(fill="both", expand=True, padx=5, pady=5)
        
        # Create a grid for module status indicators
        modules = [
            ("Spotify", "module_spotify"),
            ("YouTube", "module_youtube"), 
            ("Clock", "module_clock"),
            ("FPS", "module_fps"),
            ("System Stats", "module_sys_stats"),
            ("Heartbeat", "module_heartbeat"),
            ("Animated Text", "module_animated_text"),
            ("Local Media", "module_local_media"),
            ("Battery", "module_battery"),
            ("Countdown", "module_countdown"),
            ("Info Cycling", "module_info_cycling"),
            ("Discord RPC", "discord_rpc_enabled")
        ]
        
        for i, (display_name, config_name) in enumerate(modules):
            row = i // 3
            col = i % 3
            
            frame = ctk.CTkFrame(status_frame, corner_radius=8)
            frame.grid(row=row, column=col, padx=5, pady=5, sticky="ew")
            
            indicator = ctk.CTkLabel(frame, text="●", font=ctk.CTkFont(size=16))
            indicator.pack(side="left", padx=(10, 5), pady=5)
            
            ctk.CTkLabel(frame, text=display_name).pack(side="left", padx=5, pady=5)
            
            self.module_indicators[config_name] = indicator

    def create_pages(self):
        self.create_page_dashboard()
        self.create_page_presets()
        self.create_page_media()
        self.create_page_system()
        self.create_page_appearance()
        self.create_page_discord()
        self.create_page_avatar()
        self.create_page_network()

    def create_page_dashboard(self):
        page = ctk.CTkScrollableFrame(self.pages_container, fg_color="transparent")
        self.pages["dashboard"] = page
        
        # Welcome card
        welcome_card = ctk.CTkFrame(page, corner_radius=12)
        welcome_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(welcome_card, text="Welcome to VRChat OSC Pro", 
                    font=ctk.CTkFont(size=24, weight="bold")).pack(anchor="w", padx=20, pady=(20, 5))
        ctk.CTkLabel(welcome_card, text="Control your VRChat chatbox with real-time system information, media status, and custom messages.",
                    font=ctk.CTkFont(size=14), wraplength=800).pack(anchor="w", padx=20, pady=(0, 20))
        
        # Stats row
        stats_frame = ctk.CTkFrame(welcome_card, fg_color="transparent")
        stats_frame.pack(fill="x", padx=20, pady=(0, 20))
        
        stats_data = [
            ("Active Modules", "8/12"),
            ("OSC Status", "Ready"),
            ("Profile", "Default"),
            ("Theme", self.config.get("ui_color_theme", "Ocean"))
        ]
        
        for i, (label, value) in enumerate(stats_data):
            stat_frame = ctk.CTkFrame(stats_frame, height=80, width=180, corner_radius=8)
            stat_frame.grid(row=0, column=i, padx=5, sticky="ew")
            stat_frame.grid_propagate(False)
            
            ctk.CTkLabel(stat_frame, text=label, font=ctk.CTkFont(size=12)).pack(anchor="w", padx=10, pady=(10, 0))
            ctk.CTkLabel(stat_frame, text=value, font=ctk.CTkFont(size=18, weight="bold"),
                        text_color=self.current_theme["primary"]).pack(anchor="w", padx=10, pady=(0, 10))
            
            stats_frame.grid_columnconfigure(i, weight=1)

        quick_settings_card = ctk.CTkFrame(page, corner_radius=12)
        quick_settings_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(quick_settings_card, text="Quick Settings", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        

        toggle_frame = ctk.CTkFrame(quick_settings_card, fg_color="transparent")
        toggle_frame.pack(fill="x", padx=20, pady=(0, 20))
        
        quick_toggles = [
            ("Spotify Integration", "module_spotify"),
            ("YouTube Detection", "module_youtube"),
            ("System Stats", "module_sys_stats"),
            ("Animated Text", "module_animated_text"),
            ("Discord RPC", "discord_rpc_enabled"),
            ("Clock Display", "module_clock")
        ]
        
        for i, (label, var_name) in enumerate(quick_toggles):
            row = i // 3
            col = i % 3
            
            if var_name not in self.vars:
                self.vars[var_name] = tk.BooleanVar(value=self.config.get(var_name, False))
            
            switch = ctk.CTkSwitch(toggle_frame, text=label, variable=self.vars[var_name],
                                 command=self.update_dependencies)
            switch.grid(row=row, column=col, padx=10, pady=8, sticky="w")
            
            toggle_frame.grid_columnconfigure(col, weight=1)

    def create_page_presets(self):
        page = ctk.CTkScrollableFrame(self.pages_container, fg_color="transparent")
        self.pages["presets"] = page
        
        preset_card = ctk.CTkFrame(page, corner_radius=12)
        preset_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(preset_card, text="Configuration Presets", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        selection_frame = ctk.CTkFrame(preset_card, fg_color="transparent")
        selection_frame.pack(fill="x", padx=20, pady=10)
        
        ctk.CTkLabel(selection_frame, text="Active Preset:", 
                    font=ctk.CTkFont(size=14)).pack(side="left")
        
        self.preset_var = tk.StringVar()
        self.preset_menu = ctk.CTkComboBox(selection_frame, variable=self.preset_var, 
                                         state='readonly', command=self.on_preset_selected,
                                         width=200)
        self.preset_menu.pack(side="left", padx=10)
        
        ctk.CTkButton(selection_frame, text="💾 Save", command=self.do_save_preset,
                     width=80).pack(side="left", padx=2)
        ctk.CTkButton(selection_frame, text="🗑️ Delete", command=self.do_delete_preset,
                     width=80).pack(side="left", padx=2)
        
        button_frame = ctk.CTkFrame(preset_card, fg_color="transparent")
        button_frame.pack(fill="x", padx=20, pady=(0, 20))
        
        ctk.CTkButton(button_frame, text="📂 Load Profile", command=self.do_load_profile,
                     fg_color=self.current_theme["primary"]).pack(side="left", padx=(0, 10))
        ctk.CTkButton(button_frame, text="💾 Save Profile As", command=self.do_save_profile_as).pack(side="left")

    def create_page_media(self):
        page = ctk.CTkScrollableFrame(self.pages_container, fg_color="transparent")
        self.pages["media"] = page
        
        spotify_card = ctk.CTkFrame(page, corner_radius=12)
        spotify_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(spotify_card, text="Spotify Integration", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        self.create_setting_row(spotify_card, "module_spotify", "Enable Spotify Module")
        self.create_setting_row(spotify_card, "spotify_client_id", "Client ID:", is_toggle=False)
        self.create_setting_row(spotify_card, "spotify_client_secret", "Client Secret:", is_toggle=False)
        self.create_setting_row(spotify_card, "spotify_redirect_uri", "Redirect URI:", is_toggle=False)
        self.create_setting_row(spotify_card, "spotify_show_song_name", "Show Song Name & Artist")
        self.create_setting_row(spotify_card, "spotify_show_progress_bar", "Show Progress Bar")
        self.create_setting_row(spotify_card, "spotify_show_timestamp", "Show Timestamps")
        self.create_setting_row(spotify_card, "spotify_show_device", "Show Playback Device")

        youtube_card = ctk.CTkFrame(page, corner_radius=12)
        youtube_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(youtube_card, text="YouTube Integration", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        self.create_setting_row(youtube_card, "module_youtube", "Enable YouTube Activity (Windows Only)")

        local_media_card = ctk.CTkFrame(page, corner_radius=12)
        local_media_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(local_media_card, text="Local Media", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        self.create_setting_row(local_media_card, "module_local_media", "Enable Local Media (Windows Only)")

    def create_page_system(self):
        page = ctk.CTkScrollableFrame(self.pages_container, fg_color="transparent")
        self.pages["system"] = page
        
        system_card = ctk.CTkFrame(page, corner_radius=12)
        system_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(system_card, text="System Modules", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        self.create_setting_row(system_card, "module_clock", "Display Clock")
        self.create_setting_row(system_card, "clock_show_seconds", "   ↳ Show Seconds")
        self.create_setting_row(system_card, "module_heartbeat", "Display Heartbeat")
        self.create_setting_row(system_card, "module_fps", "Display Fake FPS Counter")
        self.create_setting_row(system_card, "module_sys_stats", "Display System Stats (CPU/RAM)")
        self.create_setting_row(system_card, "module_battery", "Display Battery Status")

        countdown_card = ctk.CTkFrame(page, corner_radius=12)
        countdown_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(countdown_card, text="Countdown Timer", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        self.create_setting_row(countdown_card, "module_countdown", "Enable Countdown Timer")
        self.create_setting_row(countdown_card, "countdown_label", "Label:", is_toggle=False)
        self.create_setting_row(countdown_card, "countdown_target_date", "Target (YYYY-MM-DD HH:MM:SS):", is_toggle=False)
        self.create_setting_row(countdown_card, "countdown_finished_text", "Finished Text:", is_toggle=False)

        cycling_card = ctk.CTkFrame(page, corner_radius=12)
        cycling_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(cycling_card, text="Information Cycling", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        self.create_setting_row(cycling_card, "module_info_cycling", "Enable Information Cycling")
        self.create_setting_row(cycling_card, "info_cycling_interval", "Cycle Interval (seconds):", is_toggle=False)
        self.create_setting_row(cycling_card, "info_cycling_pages", "Pages (spotify,system,etc):", is_toggle=False)

    def create_page_appearance(self):
        page = ctk.CTkScrollableFrame(self.pages_container, fg_color="transparent")
        self.pages["appearance"] = page
        
        theme_card = ctk.CTkFrame(page, corner_radius=12)
        theme_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(theme_card, text="Application Theme", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        theme_frame = ctk.CTkFrame(theme_card, fg_color="transparent")
        theme_frame.pack(fill="x", padx=20, pady=10)
        
        ctk.CTkLabel(theme_frame, text="Appearance Mode:").pack(side="left")
        
        self.vars['ui_theme'] = tk.StringVar(value=self.config.get("ui_theme"))
        theme_menu = ctk.CTkOptionMenu(theme_frame, variable=self.vars['ui_theme'], 
                                     values=["Light", "Dark", "System"],
                                     command=self.on_theme_changed)
        theme_menu.pack(side="left", padx=10)
        
        ctk.CTkLabel(theme_frame, text="Color Theme:").pack(side="left", padx=(20, 0))
        
        self.vars['ui_color_theme'] = tk.StringVar(value=self.config.get("ui_color_theme"))
        color_menu = ctk.CTkOptionMenu(theme_frame, variable=self.vars['ui_color_theme'],
                                     values=list(THEMES.keys()),
                                     command=self.on_color_theme_changed)
        color_menu.pack(side="left", padx=10)

        text_card = ctk.CTkFrame(page, corner_radius=12)
        text_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(text_card, text="Text Settings", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        self.create_setting_row(text_card, "watermark_text", "Watermark:", is_toggle=False)
        self.create_setting_row(text_card, "separator_char", "Separator:", is_toggle=False)
        
        anim_card = ctk.CTkFrame(page, corner_radius=12)
        anim_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(anim_card, text="Animated Text", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        self.create_setting_row(anim_card, "module_animated_text", "Enable Animated Text")
        
        ctk.CTkLabel(anim_card, text="Animation Texts (one per line):").pack(anchor="w", padx=20, pady=(10, 5))
        self.create_text_area(anim_card, 'animated_texts')

    def create_page_discord(self):
        page = ctk.CTkScrollableFrame(self.pages_container, fg_color="transparent")
        self.pages["discord"] = page
        
        main_card = ctk.CTkFrame(page, corner_radius=12)
        main_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(main_card, text="Discord Rich Presence (RPC)", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        self.create_setting_row(main_card, "discord_rpc_enabled", "Enable Discord RPC")
        self.create_setting_row(main_card, "discord_rpc_show_spotify", "Show Spotify song in status")

        content_card = ctk.CTkFrame(page, corner_radius=12)
        content_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(content_card, text="Custom Content", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        self.create_setting_row(content_card, "discord_rpc_details", "Details Text:", is_toggle=False)
        self.create_setting_row(content_card, "discord_rpc_state", "State Text:", is_toggle=False)
        self.create_setting_row(content_card, "discord_rpc_large_image", "Large Image Key:", is_toggle=False)
        self.create_setting_row(content_card, "discord_rpc_large_text", "Large Image Text:", is_toggle=False)
        self.create_setting_row(content_card, "discord_rpc_button_label", "Button Label:", is_toggle=False)
        self.create_setting_row(content_card, "discord_rpc_button_url", "Button URL:", is_toggle=False)

    def create_page_avatar(self):
        page = ctk.CTkScrollableFrame(self.pages_container, fg_color="transparent")
        self.pages["avatar"] = page
        
        avatar_card = ctk.CTkFrame(page, corner_radius=12)
        avatar_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(avatar_card, text="Avatar OSC Parameters", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        ctk.CTkButton(avatar_card, text="➕ Add Parameter", 
                     command=self.add_avatar_param_row).pack(anchor="w", padx=20, pady=(0, 10))
        
        ctk.CTkLabel(avatar_card, text="Avatar parameter management will be implemented here",
                    font=ctk.CTkFont(size=12)).pack(anchor="w", padx=20, pady=10)

    def create_page_network(self):
        page = ctk.CTkScrollableFrame(self.pages_container, fg_color="transparent")
        self.pages["network"] = page
        
        server_card = ctk.CTkFrame(page, corner_radius=12)
        server_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(server_card, text="OSC Input Server", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        self.create_setting_row(server_card, "osc_server_enabled", "Enable OSC Input Server")
        self.create_setting_row(server_card, "osc_server_ip", "Listen IP Address:", is_toggle=False)
        self.create_setting_row(server_card, "osc_server_port", "Listen Port:", is_toggle=False)

        commands_card = ctk.CTkFrame(page, corner_radius=12)
        commands_card.pack(fill="x", pady=(0, 20))
        
        ctk.CTkLabel(commands_card, text="Available OSC Commands", 
                    font=ctk.CTkFont(size=18, weight="bold")).pack(anchor="w", padx=20, pady=(20, 10))
        
        commands_text = """VRChat can send OSC messages to this application to control it remotely.

Available Commands:

Address: /VRChatOSCPro/toggleModule
Type: String
Action: Toggles a specific module on or off.
Example Value: "module_spotify"

Address: /VRChatOSCPro/setPreset  
Type: String
Action: Activates a saved configuration preset.
Example Value: "AFK"

Note: VRChat sends messages TO port 9000, and LISTENS for messages ON port 9001."""
        
        text_widget = ctk.CTkTextbox(commands_card, height=200, font=ctk.CTkFont(family="Consolas", size=12))
        text_widget.pack(fill="x", padx=20, pady=(0, 20))
        text_widget.insert("1.0", commands_text)
        text_widget.configure(state="disabled")

    def create_setting_row(self, parent, var_name, text, is_toggle=True):
        if var_name not in self.vars:
            self.vars[var_name] = tk.BooleanVar() if is_toggle else tk.StringVar()
        
        row = ctk.CTkFrame(parent, fg_color="transparent")
        row.pack(fill='x', padx=20, pady=8)
        row.grid_columnconfigure(1, weight=1)
        
        ctk.CTkLabel(row, text=text, font=ctk.CTkFont(size=14)).grid(row=0, column=0, sticky="w")
        if is_toggle:
            ctk.CTkSwitch(row, text="", variable=self.vars[var_name], 
                         command=self.update_dependencies).grid(row=0, column=2, sticky="e")
        else:
            entry = ctk.CTkEntry(row, textvariable=self.vars[var_name])
            entry.grid(row=0, column=1, columnspan=2, padx=(20,0), sticky="ew")

    def create_text_area(self, parent, var_name):
        if var_name not in self.vars:
            self.vars[var_name] = tk.Text(parent, height=5, bg="#343638", fg="#DCE4EE", 
                                        font=("Segoe UI", 12), relief='flat', 
                                        insertbackground="#DCE4EE", bd=0, 
                                        highlightthickness=1, highlightbackground="#565B5E")
        
        self.vars[var_name].pack(padx=20, pady=10, fill="both", expand=True)
        self.vars[var_name].bind("<KeyRelease>", self.update_preview)

    def show_page(self, page_name):
        self.current_page = page_name
        for p in self.pages.values(): 
            p.pack_forget()
        self.pages[page_name].pack(fill='both', expand=True)

        # Update nav buttons - FIXED VERSION
        for name, btn in self.nav_buttons.items():
            is_active = name == page_name
            if is_active:
                btn.configure(fg_color=self.current_theme["primary"],
                            text_color="white")
            else:
                btn.configure(fg_color="transparent",
                            text_color=ctk.ThemeManager.theme["CTkButton"]["text_color"])

    def cycle_theme(self):
        themes = list(THEMES.keys())
        current_index = themes.index(self.config.get("ui_color_theme", "Ocean"))
        next_theme = themes[(current_index + 1) % len(themes)]
        self.vars['ui_color_theme'].set(next_theme)
        self.on_color_theme_changed(next_theme)

    def on_theme_changed(self, new_theme):
        ctk.set_appearance_mode(new_theme)
        self.config['ui_theme'] = new_theme
        self.update_preview()

    def on_color_theme_changed(self, new_theme):
        self.current_theme = THEMES[new_theme]
        self.config['ui_color_theme'] = new_theme
        
        self.start_btn.configure(fg_color=self.current_theme["primary"],
                               hover_color=self.current_theme["secondary"])
        
        for name, btn in self.nav_buttons.items():
            if name == self.current_page:
                btn.configure(fg_color=self.current_theme["primary"])
        
        self.update_preview()

    def perform_update_check(self):
        self.log("Checking for updates...", "info")
        threading.Thread(target=self._update_worker, daemon=True).start()
    
    def _update_worker(self):
        version, url = check_for_updates(self.log)
        if version and url: 
            self.after(0, self.prompt_for_update, version, url)

    def prompt_for_update(self, new_version, url):
        if messagebox.askyesno("Update Available", f"A new version ({new_version}) is available!\n\nWould you like to go to the download page?"):
            webbrowser.open(url)

    def load_app_settings(self):
        try:
            with open(APP_SETTINGS_FILE, 'r') as f: 
                return json.load(f)
        except: 
            return {"last_profile": ""}

    def save_app_settings(self):
        self.app_settings["last_profile"] = self.current_profile_path
        with open(APP_SETTINGS_FILE, 'w') as f: 
            json.dump(self.app_settings, f, indent=4)
    
    def load_profile(self, path):
        if not path or not os.path.exists(path): 
            return False
        try:
            with open(path, 'r') as f:
                loaded_config = json.load(f)
                self.config = DEFAULT_CONFIG.copy()
                self.config.update(loaded_config)
                self.current_profile_path = path
                self.save_app_settings()
                self.log(f"Loaded profile: {os.path.basename(path)}", "success")
                self.load_settings_to_gui()
                return True
        except Exception as e:
            self.log(f"Failed to load profile: {e}", "error")
            return False

    def do_load_profile(self):
        path = filedialog.askopenfilename(initialdir=PROFILES_DIR, title="Load Profile", 
                                         filetypes=(("JSON files", "*.json"),))
        if path: 
            self.load_profile(path)

    def do_save_profile_as(self):
        path = filedialog.asksaveasfilename(initialdir=PROFILES_DIR, title="Save Profile As", 
                                           filetypes=(("JSON files", "*.json"),), 
                                           defaultextension=".json")
        if path:
            self.current_profile_path = path
            self.apply_gui_to_config(save_to_preset=False)
            with open(path, 'w') as f: 
                json.dump(self.config, f, indent=4)
            self.log(f"Profile saved as {os.path.basename(path)}", "success")
            self.save_app_settings()

    def update_preset_menu(self):
        presets = list(self.config.get('presets', {'Default':{}}).keys())
        self.preset_menu.configure(values=presets)
        active_preset = self.config.get('active_preset', 'Default')
        if active_preset in presets: 
            self.preset_var.set(active_preset)
        elif presets: 
            self.preset_var.set(presets[0])

    def on_preset_selected(self, preset_name):
        self.config['active_preset'] = preset_name
        preset_settings = self.config.get('presets', {}).get(preset_name, {})
        for key, value in preset_settings.items(): 
            self.config[key] = value
        self.load_settings_to_gui()
        self.update_preview()
        self.log(f"Loaded preset: {preset_name}", "accent")

    def do_save_preset(self):
        preset_name = self.preset_var.get()
        if not preset_name or preset_name == "Default":
            dialog = ctk.CTkInputDialog(text="Enter a name for the new preset:", title="Save Preset")
            preset_name = dialog.get_input()
            if not preset_name: 
                return

        if 'presets' not in self.config: 
            self.config['presets'] = {}
        self.apply_gui_to_config(save_to_preset=False)
        current_settings = {k: v for k, v in self.config.items() if k not in ['presets', 'active_preset', 'avatar_parameters', 'spotify_client_id', 'spotify_client_secret']}
        self.config['presets'][preset_name] = current_settings
        self.config['active_preset'] = preset_name
        self.update_preset_menu()
        self.log(f"Saved settings to preset '{preset_name}'", "success")

    def do_delete_preset(self):
        preset_name = self.preset_var.get()
        if not preset_name or preset_name == "Default":
            messagebox.showerror("Error", "The Default preset cannot be deleted.")
            return
        if messagebox.askyesno("Delete Preset", f"Are you sure you want to delete the '{preset_name}' preset?"):
            self.config['presets'].pop(preset_name, None)
            self.config['active_preset'] = 'Default'
            self.on_preset_selected('Default')
            self.update_preset_menu()
            self.log(f"Deleted preset '{preset_name}'", "warning")

    def log(self, message, level="info"):
        self.after(0, lambda: self._log_update(message, level))

    def _log_update(self, message, level):
        self.log_text.config(state="normal")
        self.log_text.insert(tk.END, f"[{datetime.datetime.now().strftime('%H:%M:%S')}] {message}\n", level)
        self.log_text.see(tk.END)
        self.log_text.config(state="disabled")
    
    def bind_traces(self):
        for var_name, var in self.vars.items():
            if isinstance(var, tk.StringVar):
                var.trace_add("write", lambda *a, v=var_name: self.update_dependencies())

    def update_dependencies(self, *args):
        self.apply_gui_to_config()
        self.update_module_indicators()
        if self.osc_thread and self.osc_thread.is_alive():
            self.log("Settings changed. Restart OSC to apply.", "warning")
        self.update_preview()

    def update_module_indicators(self):
        for config_name, indicator in self.module_indicators.items():
            is_enabled = self.config.get(config_name, False)
            color = self.current_theme["secondary"] if is_enabled else "#EF4444"
            indicator.configure(text_color=color)

    def load_settings_to_gui(self):
        for key, var in self.vars.items():
            val = self.config.get(key)
            if val is not None:
                if isinstance(var, tk.Text): 
                    var.delete('1.0', tk.END)
                    var.insert('1.0', "\n".join(str(v) for v in val) if isinstance(val, list) else str(val))
                elif key == "info_cycling_pages": 
                    var.set(",".join(val))
                else: 
                    var.set(val)
        
        ctk.set_appearance_mode(self.config.get("ui_theme", "System"))
        self.current_theme = THEMES[self.config.get("ui_color_theme", "Ocean")]
        
        self.update_preset_menu()
        self.update_module_indicators()
    
    def apply_gui_to_config(self, save_to_preset=True):
        for key, var in self.vars.items():
            try:
                if isinstance(var, tk.Text):
                    val = var.get('1.0', tk.END).strip().split('\n')
                else:
                    val = var.get()
                
                if key == "info_cycling_pages" and isinstance(val, str): 
                    self.config[key] = [p.strip() for p in val.split(',')]
                elif isinstance(val, list):
                    self.config[key] = [line for line in val if line]
                else:
                    self.config[key] = val
            except (tk.TclError, AttributeError): 
                pass
        
        self.config['ui_theme'] = self.vars['ui_theme'].get()
        self.config['ui_color_theme'] = self.vars['ui_color_theme'].get()

        if save_to_preset:
            preset_name = self.preset_var.get()
            if preset_name and preset_name in self.config.get('presets', {}):
                current_settings = {k: v for k, v in self.config.items() if k not in ['presets', 'active_preset', 'avatar_parameters', 'spotify_client_id', 'spotify_client_secret']}
                self.config['presets'][preset_name] = current_settings
    
    def update_preview(self, *args):
        self.apply_gui_to_config(save_to_preset=False)
        temp_thread = VrcOscThread(self.config, lambda *a: None)
        temp_thread.anim_state['char_idx'] = 12
        text = temp_thread.build_message()
        del temp_thread

        self.preview_canvas.delete("all")
        bg_color = "#1E1E1E" if ctk.get_appearance_mode().lower() == "dark" else "#FFFFFF"
        fg_color = "#DCE4EE" if ctk.get_appearance_mode().lower() == "dark" else "#1A1A1A"
        self.preview_canvas.config(bg=bg_color)
        
        y = 20
        for line in text.split('\n'):
            self.preview_canvas.create_text(20, y, text=line, font=("Consolas", 11), 
                                          fill=fg_color, anchor="w")
            y += 20

    def handle_osc_toggle_module(self, module_name):
        if module_name in self.vars and isinstance(self.vars[module_name], tk.BooleanVar):
            current_val = self.vars[module_name].get()
            self.vars[module_name].set(not current_val)
            self.log(f"OSC Action: Toggled '{module_name}' to {not current_val}.", "info")
            self.update_dependencies()
        else:
            self.log(f"OSC Action: Module '{module_name}' not found or not a toggle.", "warning")

    def handle_osc_set_preset(self, preset_name):
        if preset_name in self.config.get('presets', {}):
            self.preset_var.set(preset_name)
            self.on_preset_selected(preset_name)
        else:
            self.log(f"OSC Action: Preset '{preset_name}' not found.", "warning")

    def add_avatar_param_row(self, param=None):
        self.log("Avatar Parameter UI is a placeholder in this version.", "warning")

    def start_osc(self):
        self.start_btn.configure(state='disabled')
        self.stop_btn.configure(state='normal')
        self.log("OSC Transmission ENGAGED.", "success")
        self.apply_gui_to_config(save_to_preset=True)
        
        if self.current_profile_path and os.path.exists(os.path.dirname(self.current_profile_path)):
            with open(self.current_profile_path, 'w') as f: 
                json.dump(self.config, f, indent=4)
        
        self.osc_thread = VrcOscThread(self.config, self.log, self)
        self.osc_thread.start()
        
        if self.config.get("osc_server_enabled"):
            ip = self.config.get("osc_server_ip", "127.0.0.1")
            port = int(self.config.get("osc_server_port", 9001))
            self.osc_server_thread = VrcOscServerThread(self, ip, port, self.log)
            self.osc_server_thread.start()

    def stop_osc(self):
        if self.osc_thread and self.osc_thread.is_alive():
            self.osc_thread.stop()
            self.osc_thread.join()
            self.osc_thread = None
        if self.osc_server_thread and self.osc_server_thread.is_alive():
            self.osc_server_thread.stop()
            self.osc_server_thread.join(timeout=1)
            self.osc_server_thread = None
        
        self.start_btn.configure(state='normal')
        self.stop_btn.configure(state='disabled')
        self.log("OSC Transmission HALTED.", "error")

    def on_closing(self):
        if messagebox.askokcancel("Exit Application", "Are you sure you want to stop OSC and exit?"):
            self.stop_osc()
            self.destroy()
            if threading.active_count() > 1: 
                os._exit(0)

if __name__ == "__main__":
    app = ModernApp()
    app.mainloop()
