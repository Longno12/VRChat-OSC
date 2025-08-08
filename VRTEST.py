# ==============================================================================
# VRChat OSC Pro
# Simply cleaning up the code
# By https://github.com/pytmg
# ==============================================================================

import tkinter as tk
from tkinter import messagebox, font, filedialog, colorchooser
import threading, time, datetime, random, json, os, asyncio, math, webbrowser

IS_WINDOWS = os.name == 'nt'

# --- Updated Dependency Check ---
# This now checks for all required libraries at once for a better user experience.
missing_libs = []
lib_map = {
    'pythonosc': 'python-osc', 'spotipy': 'spotipy', 'psutil': 'psutil',
    'pypresence': 'pypresence', 'requests': 'requests', 'packaging': 'packaging'
}
if IS_WINDOWS:
    lib_map['winsdk'] = 'winsdk'

for imp, pip_name in lib_map.items():
    try:
        __import__(imp)
    except ImportError:
        missing_libs.append(pip_name)

if missing_libs:
    messagebox.showerror(
        "Missing Libraries",
        "The following required libraries are missing:\n\n" +
        "\n".join([f"• {lib}" for lib in missing_libs]) +
        "\n\nPlease install them by running the following command in your terminal:\n"
        f"pip install {' '.join(missing_libs)}"
    )
    exit()
# --- End of Updated Dependency Check ---

# Now, import the libraries since we know they exist.
from pythonosc import udp_client
import spotipy
from spotipy.oauth2 import SpotifyOAuth
import psutil
from pypresence import Presence
import requests
from packaging.version import parse as parse_version
if IS_WINDOWS:
    from winsdk.windows.media.control import GlobalSystemMediaTransportControlsSessionManager as MediaManager


CURRENT_VERSION = "2.0.0"
GITHUB_REPO = "Longno12/VRChat-OSC-Python"


def check_for_updates(log_callback):
    """
    Checks GitHub for the latest release of the application.
    Returns the new version and download URL if an update is available.
    """
    try:
        api_url = f"https://api.github.com/repos/{GITHUB_REPO}/releases/latest"
        response = requests.get(api_url, timeout=5)
        response.raise_for_status()
        latest_release = response.json()

        latest_version_str = latest_release.get('tag_name', '0.0.0').lstrip('v')
        download_url = latest_release.get('html_url')

        current_v = parse_version(CURRENT_VERSION)
        latest_v = parse_version(latest_version_str)

        if latest_v > current_v:
            log_callback(f"New version found: {latest_v} (current: {current_v})", "green")
            return latest_version_str, download_url

    except requests.exceptions.RequestException as e:
        log_callback(f"Update check failed: {e}", "orange")
    except Exception as e:
        log_callback(f"An error occurred during update check: {e}", "red")

    return None, None

# SECTION: Global Constants and Configuration

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

# SECTION: Helper Classes

class WindowsMediaManager:
    """
    Manages fetching media information from Windows for the local media module.
    Runs in a separate thread to avoid blocking the main UI.
    """
    def __init__(self, log_callback):
        self.log = log_callback
        self.current_media_info = {"title": "", "artist": ""}
        self.is_running = False
        self._thread = None

    async def _get_media_info(self):
        """Asynchronously retrieves media properties from the current Windows media session."""
        try:
            sessions = await MediaManager.request_async()
            current_session = sessions.get_current_session()
            if current_session:
                info = await current_session.try_get_media_properties_async()
                info_dict = {prop.name: info.lookup(prop) for prop in info}
                self.current_media_info['title'] = info_dict.get('title', 'Unknown Title')
                self.current_media_info['artist'] = info_dict.get('artist', 'Unknown Artist')
            else:
                 self.current_media_info = {"title": "", "artist": ""}
        except Exception:
            self.current_media_info = {"title": "", "artist": ""}
        return self.current_media_info

    async def _main_loop(self):
        """The main async loop that periodically fetches media info."""
        self.log("Local Media listener started.", "info")
        while self.is_running:
            await self._get_media_info()
            await asyncio.sleep(5)

    def _run_loop(self):
        """Initializes and runs the asyncio event loop in a separate thread."""
        asyncio.run(self._main_loop())

    def start(self):
        """Starts the media listener thread."""
        if not IS_WINDOWS:
            self.log("Local Media is only supported on Windows.", "orange")
            return
        self.is_running = True
        self._thread = threading.Thread(target=self._run_loop, daemon=True)
        self._thread.start()

    def stop(self):
        """Stops the media listener thread."""
        self.is_running = False

class VrcOscThread(threading.Thread):
    """
    The main worker thread that handles all OSC communications,
    Spotify integration, and Discord Rich Presence updates.
    """
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
        """Initializes and connects to Discord for Rich Presence."""
        if not self.config.get("discord_rpc_enabled"):
            return
        try:
            self.rpc = Presence('1390807459328823297')
            self.rpc.connect()
            self.rpc_start_time = time.time()
            self.log("Discord Rich Presence connected.", "green")
        except Exception as e:
            self.rpc = None
            self.log(f"Could not connect to Discord: {e}", "orange")

    def update_discord_presence(self, spotify_info=None):
        """Updates the Discord Rich Presence status."""
        if not self.rpc:
            return
        try:
            details = self.config.get('discord_rpc_details', 'Using OSC')
            if self.config.get('discord_rpc_show_spotify') and spotify_info and spotify_info.get('is_playing'):
                details = f"🎵 {spotify_info['name']}"[:128]

            payload = {
                'details': details,
                'state': self.config.get('discord_rpc_state'),
                'start': int(self.rpc_start_time),
                'large_image': self.config.get('discord_rpc_large_image'),
                'large_text': self.config.get('discord_rpc_large_text')
            }

            button_label = self.config.get('discord_rpc_button_label')
            button_url = self.config.get('discord_rpc_button_url')
            if button_label and button_url and button_url.startswith("http"):
                payload['buttons'] = [{"label": button_label, "url": button_url}]

            self.rpc.update(**payload)
        except Exception:
            self.rpc.close()
            self.rpc = None
            self.log("Discord presence update failed. Is Discord running?", "orange")

    def setup_spotify(self):
        """Authenticates with the Spotify API."""
        client_id = self.config.get('spotify_client_id')
        if not client_id or client_id == 'YOUR_SPOTIFY_CLIENT_ID':
            self.log("Spotify Error: Client ID not set.", "red")
            return
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
            self.spotify_client = None
            self.log(f"Spotify Auth Error: {str(e).splitlines()[0]}", "red")
            self.log("Ensure credentials are correct and auth prompt is accepted.", "orange")

    def get_spotify_info(self):
        """Fetches the currently playing track from Spotify."""
        if not self.spotify_client:
            return None
        try:
            track = self.spotify_client.current_user_playing_track()
            if not track or not track.get('item'):
                return {"name": "Nothing playing on Spotify", "is_playing": False, "progress_ms": 0, "duration_ms": 1}

            item = track['item']
            info = {
                "name": f"{item['name']} - {', '.join([a['name'] for a in item['artists']])}",
                "progress_ms": track.get('progress_ms', 0),
                "duration_ms": item.get('duration_ms', 1),
                "is_playing": track.get('is_playing', False)
            }
            if self.config.get('spotify_show_device') and track.get('device'):
                info['device'] = track['device'].get('name', 'Unknown Device')
            return info
        except spotipy.exceptions.SpotifyException as e:
            if e.http_status == 401:
                self.log("Spotify token expired. Re-authenticating...", "orange")
                self.setup_spotify()
            else:
                self.log(f"Spotify API Error: {e}", "red")
            return {"name": "Re-authenticating...", "is_playing": False, "progress_ms": 0, "duration_ms": 1}
        except Exception:
            self.log("An unknown Spotify error occurred.", "red")
            return {"name": "Spotify Error", "is_playing": False, "progress_ms": 0, "duration_ms": 1}

    # --- Message Building ---
    def _build_info_line(self):
        parts = []
        sep = f" {self.config.get('separator_char', '|')} "
        if self.config.get('module_clock'):
            time_format = '%H:%M:%S' if self.config.get('clock_show_seconds') else '%H:%M'
            parts.append(f"🕒 {datetime.datetime.now().strftime(time_format)}")
        if self.config.get('module_heartbeat'):
            now = time.time()
            if now - self.last_heartbeat_flash >= 5: self.last_heartbeat_flash = now
            if now - self.last_heartbeat_flash < 1.0: parts.append("❤")
        if self.config.get('module_fps'):
            parts.append(f"🚀 {random.randint(249, 359)} FPS")
        if self.config.get('module_sys_stats'):
            parts.append(f"💻 CPU: {psutil.cpu_percent():.0f}% | RAM: {psutil.virtual_memory().percent:.0f}%")
        return sep.join(parts) if parts else ""

    def _build_spotify_line(self, spotify_info):
        if not (spotify_info and spotify_info['is_playing']):
            return ""

        spotify_lines = []
        if self.config.get('spotify_show_song_name'):
            prefix = "🎵" if spotify_info['is_playing'] else "⏸️"
            song_name = spotify_info['name']
            if self.config.get('spotify_show_device') and 'device' in spotify_info:
                song_name += f" 🔊({spotify_info['device']})"
            spotify_lines.append(f"{prefix} {song_name}")

        progress_parts = []
        if self.config.get('spotify_show_progress_bar'):
            p_len = self.config.get('progress_bar_length', 14)
            ratio = spotify_info['progress_ms'] / spotify_info['duration_ms'] if spotify_info['duration_ms'] > 0 else 0
            filled = int(ratio * p_len)
            filled_char = self.config.get('progress_filled_char', '█')
            empty_char = self.config.get('progress_empty_char', '─')
            progress_parts.append(f"[{filled_char * filled}{empty_char * (p_len - filled)}]")

        if self.config.get('spotify_show_timestamp'):
            p_time = f"{int(spotify_info['progress_ms']/60000):02}:{int((spotify_info['progress_ms']/1000)%60):02}"
            d_time = f"{int(spotify_info['duration_ms']/60000):02}:{int((spotify_info['duration_ms']/1000)%60):02}"
            progress_parts.append(f"{p_time}/{d_time}")

        if progress_parts:
            spotify_lines.append(" ".join(progress_parts))

        return "\n".join(spotify_lines)

    def _build_local_media_line(self):
        media = self.media_manager.current_media_info
        if media and media.get('title'):
            return f"💿 {media['title']} - {media['artist']}"
        return ""

    def _build_animated_text_line(self):
        if not self.config.get('module_animated_text'):
            return ""

        now = time.time()
        texts = [t for t in self.config.get('animated_texts', []) if t]
        if not texts:
            return ""

        if now > self.anim_state.get('pause_until', 0):
            if (now - self.anim_state['last_update']) > self.config.get('animation_speed', 0.15):
                self.anim_state['last_update'] = now
                active_text = texts[self.anim_state['list_idx'] % len(texts)]

                if self.anim_state['forward']:
                    if self.anim_state['char_idx'] < len(active_text):
                        self.anim_state['char_idx'] += 1
                    else:
                        self.anim_state['pause_until'] = now + self.config.get('rewrite_pause', 2.5)
                        self.anim_state['forward'] = False
                else:
                    if self.anim_state['char_idx'] > 0:
                        self.anim_state['char_idx'] -= 1
                    else:
                        self.anim_state['forward'] = True
                        self.anim_state['list_idx'] += 1
                        self.anim_state['pause_until'] = now + 1.0

        current_text_to_display = texts[self.anim_state['list_idx'] % len(texts)][:self.anim_state['char_idx']]
        return current_text_to_display if current_text_to_display else '\u200b'

    def build_message(self, spotify_info_override=None):
        """Constructs the final message string to be sent to the VRChat chatbox."""
        lines = []

        info_line = self._build_info_line()
        if info_line:
            lines.append(info_line)

        spotify_info = spotify_info_override
        spotify_line = self._build_spotify_line(spotify_info)

        if spotify_line:
            lines.append(spotify_line)
        elif self.config.get('module_local_media'):
            local_media_line = self._build_local_media_line()
            if local_media_line:
                lines.append(local_media_line)

        anim_line = self._build_animated_text_line()
        if anim_line:
            lines.append(anim_line)

        if self.config.get('watermark_text'):
            lines.append(self.config.get('watermark_text'))

        return "\n".join(lines)

    def run(self):
        """The main loop for the OSC thread."""
        if self.config.get('module_spotify'): self.setup_spotify()
        if self.config.get('module_local_media'): self.media_manager.start()

        last_message = ""
        last_rpc_update = 0
        rpc_update_interval = 15.0

        while self.is_running:
            try:
                now = time.time()

                if self.config.get("discord_rpc_enabled") and not self.rpc:
                    self.setup_discord_presence()

                spotify_info = self.get_spotify_info() if self.config.get('module_spotify') else None
                current_message = self.build_message(spotify_info_override=spotify_info)

                if current_message != last_message:
                    self.osc_client.send_message("/chatbox/input", [current_message, True])
                    last_message = current_message

                if self.rpc and (now - last_rpc_update > rpc_update_interval):
                    self.update_discord_presence(spotify_info)
                    last_rpc_update = now

                time.sleep(self.config.get('update_interval', 1.0))
            except Exception as e:
                self.log(f"OSC Loop Error: {e}", "red")
                time.sleep(3)

    def stop(self):
        """Stops the thread and cleans up resources."""
        self.is_running = False
        self.media_manager.stop()
        if self.rpc:
            try:
                self.rpc.close()
                self.log("Discord Rich Presence connection closed.", "info")
            except Exception as e:
                self.log(f"Error closing Discord presence: {e}", "red")
        try:
            # Clear the chatbox on exit
            self.osc_client.send_message("/chatbox/input", ["", True])
        except Exception as e:
            self.log(f"Could not clear chatbox: {e}", "orange")
        self.log("OSC thread stopped.")

# SECTION: Custom Tkinter Widgets

class HUDFrame(tk.Canvas):
    """A custom-drawn frame with a futuristic, hexagonal border."""
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
        self.create_polygon(points, fill="", outline=self.theme['glow'], width=4) # Glow effect
        self.create_polygon(points, fill=self.theme['bg_light'], outline=self.theme['border'], width=2)

        self.create_text(30, 18, text=self.title, font=self.fonts['header'], fill=self.theme['accent'], anchor="w")
        self.create_line(20, 35, width - 20, 35, fill=self.theme['border'])

class HUDToggleSwitch(tk.Canvas):
    """A custom on/off toggle switch widget."""
    def __init__(self, master, variable, theme, fonts, command=None, **kwargs):
        super().__init__(master, width=60, height=28, **kwargs)
        self.variable = variable
        self.command = command
        self.theme = theme
        self.fonts = fonts

        self.configure(bg=master.cget('bg'), highlightthickness=0, cursor="hand2")
        self.bind("<Button-1>", self._toggle)
        self.variable.trace_add("write", self._update_display)
        self._update_display()

    def _toggle(self, event=None):
        self.variable.set(not self.variable.get())
        if self.command:
            self.command()

    def _update_display(self, *args):
        self.delete("all")
        is_on = self.variable.get()

        # Background
        self.create_rectangle(2, 2, 58, 26, fill=self.theme['bg_dark'], outline=self.theme['border'], width=2)

        if is_on:
            self.create_rectangle(4, 4, 56, 24, fill=self.theme['accent'], outline="")
            self.create_rectangle(32, 4, 56, 24, fill=self.theme['accent_fg'], outline="")
            self.create_text(18, 14, text="ON", font=self.fonts['small_bold'], fill=self.theme['accent_fg'])
        else:
            self.create_rectangle(4, 4, 28, 24, fill=self.theme['text_dark'], outline="")
            self.create_text(42, 14, text="OFF", font=self.fonts['small_bold'], fill=self.theme['text_dark'])

class HUDSlider(tk.Canvas):
    """A custom slider widget for float values."""
    def __init__(self, master, variable, theme, fonts, command=None, **kwargs):
        super().__init__(master, height=28, **kwargs)
        self.variable = variable # Expects a tk.DoubleVar
        self.command = command
        self.theme = theme
        self.fonts = fonts

        self.configure(bg=master.cget('bg'), highlightthickness=0)
        self.bind("<Configure>", self._update_display)
        self.bind("<B1-Motion>", self._on_drag)
        self.bind("<Button-1>", self._on_drag)
        self.variable.trace_add("write", self._update_display)

    def _on_drag(self, event):
        track_width = self.winfo_width() - 20
        value = (event.x - 10) / track_width
        value = max(0.0, min(1.0, value)) # Clamp between 0.0 and 1.0

        self.variable.set(round(value, 3))
        if self.command:
            self.command(value)

    def _update_display(self, *args):
        self.delete("all")
        width, height = self.winfo_width(), self.winfo_height()
        if width < 20: return

        value = self.variable.get()
        bar_y = height / 2
        track_width = width - 20

        self.create_line(10, bar_y, width - 10, bar_y, fill=self.theme['border'], width=4)
        if value > 0:
            self.create_line(10, bar_y, 10 + track_width * value, bar_y, fill=self.theme['accent'], width=4)

        handle_x = 10 + track_width * value
        self.create_oval(handle_x - 6, bar_y - 6, handle_x + 6, bar_y + 6, fill=self.theme['accent_fg'], outline=self.theme['border'])

# SECTION: Main Application Class

class Application(tk.Frame):
    """The main GUI application class."""
    def __init__(self, master=None):
        self.app_settings = self.load_app_settings()
        self.theme = {}
        self.config = DEFAULT_CONFIG.copy()

        super().__init__(master)
        self.master = master

        self.current_profile_path = self.app_settings.get("last_profile")

        self.setup_theme_and_fonts()
        self.master.title("Project Encryptic :: VRChat OSC")
        self.master.geometry("1200x900")
        self.master.minsize(1100, 800)
        self.master.configure(bg=self.theme['bg_dark'])
        self.pack(fill="both", expand=True)

        self.vars = {}
        self.osc_thread = None
        self.avatar_param_rows = []
        self.widget_registry = []
        self.avatar_osc_client = udp_client.SimpleUDPClient("127.0.0.1", 9000)

        self.create_widgets()

        if not self.load_profile(self.current_profile_path):
            self.log("No default profile found. Loading default settings.", "orange")

        self.load_settings_to_gui()
        self.bind_traces()
        self.update_dependencies()
        self.update_preview()
        self.log(f"UI Initialized. Welcome to Encryptic OSC v{CURRENT_VERSION}.", "accent")

        # --- Auto-update check on startup ---
        self.perform_update_check()

    # --- Auto-Updater Methods ---
    def perform_update_check(self):
        """Runs the update check in a separate thread to avoid blocking the UI."""
        self.log("Checking for updates...", "info")
        def _check():
            new_version, url = check_for_updates(self.log)
            if new_version and url:
                self.master.after(0, self.prompt_for_update, new_version, url)

        update_thread = threading.Thread(target=_check, daemon=True)
        update_thread.start()

    def prompt_for_update(self, new_version, url):
        """Asks the user if they want to download the new version."""
        if messagebox.askyesno(
            "Update Available",
            f"A new version ({new_version}) is available!\n"
            f"You are currently on version {CURRENT_VERSION}.\n\n"
            "Would you like to go to the download page?"
        ):
            webbrowser.open(url)
            self.log(f"Opening download page for version {new_version}...", "green")
            if messagebox.askokcancel(
                "Download Page Opened",
                "Your web browser has been opened to the download page.\n"
                "Please close this application before running the new version.\n\n"
                "Exit the application now?"
            ):
                self.on_closing()

    def load_app_settings(self):
        try:
            with open(APP_SETTINGS_FILE, 'r') as f:
                return json.load(f)
        except (FileNotFoundError, json.JSONDecodeError):
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
        path = filedialog.askopenfilename(
            initialdir=PROFILES_DIR, title="Load Profile",
            filetypes=(("JSON files", "*.json"), ("All files", "*.*"))
        )
        if path and self.load_profile(path):
            self.redraw_ui()

    def do_save_profile_as(self):
        path = filedialog.asksaveasfilename(
            initialdir=PROFILES_DIR, title="Save Profile As",
            filetypes=(("JSON files", "*.json"),), defaultextension=".json"
        )
        if path:
            self.current_profile_path = path
            self.apply_gui_to_config()
            with open(path, 'w') as f:
                json.dump(self.config, f, indent=4)
            self.log(f"Profile saved as {os.path.basename(path)}", "green")
            self.save_app_settings()

    # --- UI Creation and Theming ---
    def setup_theme_and_fonts(self):
        accent_color = self.config.get("theme_accent", DEFAULT_CONFIG["theme_accent"])
        self.theme = {
            'bg_dark': '#0D1117', 'bg_light': '#161B22', 'border': '#30363D',
            'text': '#C9D1D9', 'text_dark': '#8B949E', 'accent': accent_color,
            'accent_fg': '#FFFFFF', 'glow': accent_color, 'green': '#238636',
            'red': '#DA3633', 'orange': '#F0883E'
        }
        self.fonts = {
            'title': ("Orbitron", 24, "bold"), 'header': ("Orbitron", 12, "bold"),
            'body': ("Segoe UI", 10), 'small_bold': ("Segoe UI", 8, "bold"),
            'preview': ("Consolas", 11), 'log': ("Consolas", 10)
        }

    def do_change_theme(self):
        color_code = colorchooser.askcolor(title="Choose accent color", initialcolor=self.theme['accent'])
        if color_code and color_code[1]:
            self.config['theme_accent'] = color_code[1]
            self.redraw_ui()

    def redraw_ui(self):
        """Redraws the entire UI after a theme or major config change."""
        self.setup_theme_and_fonts()
        self.master.configure(bg=self.theme['bg_dark'])
        for widget in self.widget_registry:
            if hasattr(widget, '_draw'): widget._draw()
            if hasattr(widget, '_update_display'): widget._update_display()
        self.load_settings_to_gui()
        self.log("UI theme updated.", "accent")

    def create_widgets(self):
        """Initializes and places all UI panels."""
        self.background_canvas = tk.Canvas(self, bg=self.theme['bg_dark'], highlightthickness=0)
        self.background_canvas.place(relwidth=1, relheight=1)
        self.background_canvas.bind("<Configure>", self.draw_background_pattern)
        self.widget_registry.append(self.background_canvas)

        self.grid_rowconfigure(0, weight=1)
        self.grid_rowconfigure(1, weight=1)
        self.grid_columnconfigure(0, weight=1)
        self.grid_columnconfigure(1, weight=1)
        self.grid_columnconfigure(2, weight=1)

        self.create_modules_panel()
        self.create_spotify_panel()
        self.create_discord_rpc_panel()
        self.create_style_panel()
        self.create_avatar_panel()
        self.create_preview_and_log_panel()

    def draw_background_pattern(self, event=None):
        """Draws the subtle hexagonal background pattern."""
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

    # --- UI Panel Creation ---
    def create_modules_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="CORE MODULES")
        panel.grid(row=0, column=0, sticky="nsew", padx=10, pady=(10,5))
        content = panel.content_frame
        self.create_setting_row(content, "module_clock", "Clock:", "")
        self.create_setting_row(content, "module_heartbeat", "Heartbeat:", "")
        self.create_setting_row(content, "module_fps", "Fake FPS:", "")
        self.create_setting_row(content, "module_sys_stats", "System Stats:", "")
        self.create_setting_row(content, "module_animated_text", "Animated Text:", "")
        self.create_setting_row(content, "module_local_media", "Local Media:", "")
        self.widget_registry.append(panel)

    def create_spotify_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="SPOTIFY")
        panel.grid(row=0, column=1, sticky="nsew", padx=10, pady=(10,5))
        content = panel.content_frame
        self.spotify_master_switch = self.create_setting_row(content, "module_spotify", "Enable Spotify:", "")
        self.spotify_controls = {
            'song': self.create_setting_row(content, "spotify_show_song_name", "Show Song:", ""),
            'bar': self.create_setting_row(content, "spotify_show_progress_bar", "Show Progress Bar:", ""),
            'time': self.create_setting_row(content, "spotify_show_timestamp", "Show Timestamp:", ""),
            'device': self.create_setting_row(content, "spotify_show_device", "Show Device:", "")
        }
        self.widget_registry.append(panel)

    def create_discord_rpc_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="DISCORD RPC")
        panel.grid(row=0, column=2, sticky="nsew", padx=10, pady=(10,5))
        content = panel.content_frame
        self.create_setting_row(content, "discord_rpc_enabled", "Enable RPC:", "")
        self.create_setting_row(content, "discord_rpc_show_spotify", "Show Spotify Song:", "")
        self.create_entry_row(content, "discord_rpc_details", "Details Text:")
        self.create_entry_row(content, "discord_rpc_state", "State Text:")
        self.create_entry_row(content, "discord_rpc_large_image", "Image Key:")
        self.create_entry_row(content, "discord_rpc_large_text", "Image Text:")
        self.create_entry_row(content, "discord_rpc_button_label", "Button Label:")
        self.create_entry_row(content, "discord_rpc_button_url", "Button URL:")
        self.widget_registry.append(panel)

    def create_style_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="STYLE & TEXT")
        panel.grid(row=1, column=0, sticky="nsew", padx=10, pady=(5,10))
        content = panel.content_frame
        self.create_entry_row(content, 'watermark_text', "Watermark:")
        self.create_entry_row(content, 'separator_char', "Separator:")

        theme_button = self.create_hud_button(content, "🎨 CHANGE ACCENT", self.theme['accent'], self.do_change_theme)
        theme_button.pack(pady=10, padx=10, fill='x')

        tk.Label(content, text="Animated Text (one per line):", font=self.fonts['body'], bg=content.cget('bg'), fg=self.theme['text_dark']).pack(fill="x", pady=(10,5), padx=10)
        text_bg = tk.Frame(content, bg=self.theme['border'], relief='flat', bd=1)
        self.vars['animated_texts'] = tk.Text(text_bg, height=3, bg=self.theme['bg_dark'], fg=self.theme['text'], font=self.fonts['body'], relief='flat', insertbackground=self.theme['accent_fg'], bd=0)
        self.vars['animated_texts'].pack(padx=1, pady=1, fill="both", expand=True)
        self.vars['animated_texts'].bind("<KeyRelease>", self.update_preview)
        text_bg.pack(fill="both", expand=True, padx=10)
        self.widget_registry.append(panel)

    def create_avatar_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="AVATAR PARAMETERS")
        panel.grid(row=1, column=1, sticky="nsew", padx=10, pady=(5,10))
        self.avatar_panel_content = panel.content_frame
        add_button = self.create_hud_button(self.avatar_panel_content, "+ ADD PARAMETER", self.theme['green'], self.add_avatar_param_row)
        add_button.pack(side="bottom", fill="x", padx=10, pady=10)
        self.widget_registry.append(panel)

    def create_preview_and_log_panel(self):
        panel = HUDFrame(self, self.theme, self.fonts, title="LIVE PREVIEW & SYSTEM CONTROLS")
        panel.grid(row=1, column=2, sticky="nsew", padx=10, pady=(5,10))
        content = panel.content_frame
        content.grid_rowconfigure(0, weight=1); content.grid_columnconfigure(0, weight=3); content.grid_columnconfigure(1, weight=2)

        self.preview_canvas = tk.Canvas(content, bg=content.cget('bg'), highlightthickness=0)
        self.preview_canvas.grid(row=0, column=0, sticky="nsew", padx=5, pady=5)
        self.widget_registry.append(self.preview_canvas)

        log_controls_frame = tk.Frame(content, bg=content.cget('bg'))
        log_controls_frame.grid(row=0, column=1, sticky="nsew", padx=5, pady=5)
        log_controls_frame.grid_rowconfigure(0, weight=1); log_controls_frame.grid_columnconfigure(0, weight=1)

        log_bg = tk.Frame(log_controls_frame, bg=self.theme['border'], relief='flat', bd=1)
        self.log_text = tk.Text(log_bg, height=10, state="disabled", wrap="word", bg=self.theme['bg_dark'], fg=self.theme['text_dark'], font=self.fonts['log'], relief='flat', bd=0)
        self.log_text.pack(fill="both", expand=True, padx=1, pady=1)
        log_bg.grid(row=0, column=0, sticky="nsew", pady=(0, 10))

        self.log_text.tag_config("INFO", foreground=self.theme['text_dark'])
        self.log_text.tag_config("ACCENT", foreground=self.theme['accent'])
        self.log_text.tag_config("GREEN", foreground=self.theme['green'])
        self.log_text.tag_config("ORANGE", foreground=self.theme['orange'])
        self.log_text.tag_config("RED", foreground=self.theme['red'])

        controls_frame = tk.Frame(log_controls_frame, bg=content.cget('bg'))
        controls_frame.grid(row=1, column=0, sticky="ew")
        controls_frame.grid_columnconfigure(0, weight=1); controls_frame.grid_columnconfigure(1, weight=1);

        self.load_button = self.create_hud_button(controls_frame, "📂 LOAD", self.theme['accent'], self.do_load_profile)
        self.load_button.grid(row=0, column=0, sticky="ew", padx=2)
        self.save_as_button = self.create_hud_button(controls_frame, "💾 SAVE AS", self.theme['accent'], self.do_save_profile_as)
        self.save_as_button.grid(row=0, column=1, sticky="ew", padx=2)
        self.start_button = self.create_hud_button(controls_frame, "▶ START", self.theme['green'], self.start_osc)
        self.start_button.grid(row=1, column=0, sticky="ew", padx=2, pady=5)
        self.stop_button = self.create_hud_button(controls_frame, "■ STOP", self.theme['red'], self.stop_osc, "disabled")
        self.stop_button.grid(row=1, column=1, sticky="ew", padx=2, pady=5)

        self.widget_registry.append(panel)

    def create_entry_row(self, parent, var_name, label_text):
        self.vars[var_name] = tk.StringVar()
        row = tk.Frame(parent, bg=parent.cget('bg'))
        tk.Label(row, text=label_text, font=self.fonts['body'], bg=parent.cget('bg'), fg=self.theme['text'], width=12, anchor='w').pack(side="left")
        entry_bg = tk.Frame(row, bg=self.theme['border'], relief='flat', bd=1)
        entry = tk.Entry(entry_bg, textvariable=self.vars[var_name], bg=self.theme['bg_dark'], fg=self.theme['text'], relief='flat', insertbackground=self.theme['accent_fg'])
        entry.pack(fill='x', expand=True, padx=1, pady=1)
        entry_bg.pack(side="left", fill="x", expand=True)
        row.pack(fill="x", pady=5, padx=10)

    def create_setting_row(self, parent, var_name, text, tooltip_text):
        self.vars[var_name] = tk.BooleanVar()
        row = tk.Frame(parent, bg=parent.cget('bg'))
        label = tk.Label(row, text=text, font=self.fonts['body'], bg=parent.cget('bg'), fg=self.theme['text'])
        label.pack(side="left", padx=(0,10))
        switch = HUDToggleSwitch(row, self.vars[var_name], self.theme, self.fonts, command=self.update_dependencies)
        switch.pack(side="right")
        row.pack(fill="x", pady=8, padx=10)
        self.widget_registry.append(switch)
        return switch

    def create_hud_button(self, parent, text, color, command, state='normal'):
        canvas = tk.Canvas(parent, height=40, bg=parent.cget('bg'), highlightthickness=0, cursor="hand2")
        self.widget_registry.append(canvas)
        canvas.bind("<Configure>", lambda e: self.draw_button_state(canvas, text, color, canvas.cget('state')))
        canvas.bind("<Button-1>", lambda e: command() if canvas.cget('state') == 'normal' else None)
        canvas.bind("<Enter>", lambda e: self.draw_button_state(canvas, text, color, 'hover'))
        canvas.bind("<Leave>", lambda e: self.draw_button_state(canvas, text, color, canvas.cget('state')))
        self.draw_button_state(canvas, text, color, state)
        return canvas

    def draw_button_state(self, canvas, text, color, state):
        """Draws a custom button in different states (normal, hover, disabled)."""
        current_state = 'disabled' if state == 'disabled' else 'normal'
        if canvas.cget('state') != current_state:
            canvas.config(state=current_state)

        canvas.delete("all")
        width, height = canvas.winfo_width(), canvas.winfo_height()
        if width < 10 or height < 10: return

        points = [5, 0, width, 0, width, height - 5, width - 5, height, 0, height, 0, 5]
        bg_color, fg_color = color, self.theme['accent_fg']

        if state == 'disabled':
            bg_color, fg_color = self.theme['bg_light'], self.theme['text_dark']
        elif state == 'hover' and current_state == 'normal':
            bg_color, fg_color = self.theme['accent_fg'], color

        canvas.create_polygon(points, fill=bg_color, outline=self.theme['border'])
        canvas.create_text(width/2, height/2, text=text, font=self.fonts['header'], fill=fg_color)

    def add_avatar_param_row(self, param=None):
        if not param:
            param = {"name": f"Param{len(self.avatar_param_rows)+1}", "path": "", "type": "float", "value": 0.0}

        row_frame = tk.Frame(self.avatar_panel_content, bg=self.avatar_panel_content.cget('bg'))
        row_frame.pack(fill="x", padx=10, pady=2)
        row_data = {"frame": row_frame}

        row_data["name_var"] = tk.StringVar(value=param['name'])
        tk.Entry(row_frame, textvariable=row_data["name_var"], width=10, bg=self.theme['bg_dark'], fg=self.theme['text'], relief='flat').pack(side="left", padx=2)

        row_data["path_var"] = tk.StringVar(value=param['path'])
        tk.Entry(row_frame, textvariable=row_data["path_var"], width=15, bg=self.theme['bg_dark'], fg=self.theme['text'], relief='flat').pack(side="left", padx=2)

        row_data["type_var"] = tk.StringVar(value=param['type'])
        type_menu = tk.OptionMenu(row_frame, row_data["type_var"], "float", "bool", command=lambda v, d=row_data: self.update_avatar_param_control(d))
        type_menu.config(width=5)
        type_menu.pack(side="left", padx=2)

        row_data["control_frame"] = tk.Frame(row_frame, bg=row_frame.cget('bg'))
        row_data["control_frame"].pack(side="left", fill='x', expand=True, padx=2)

        del_button = tk.Button(row_frame, text="X", bg=self.theme['red'], fg='white', relief='flat', command=lambda d=row_data: self.remove_avatar_param_row(d))
        del_button.pack(side="right", padx=2)

        self.avatar_param_rows.append(row_data)
        self.update_avatar_param_control(row_data, initial_value=param['value'])

    def update_avatar_param_control(self, row_data, initial_value=None):
        for widget in row_data["control_frame"].winfo_children():
            widget.destroy()

        param_type = row_data["type_var"].get()
        if param_type == "float":
            val = float(initial_value) if initial_value is not None else 0.0
            row_data["value_var"] = tk.DoubleVar(value=val)
            slider = HUDSlider(row_data["control_frame"], row_data["value_var"], self.theme, self.fonts,
                               command=lambda v, p=row_data["path_var"]: self.send_avatar_param(p.get(), v, "float"))
            slider.pack(fill='x', expand=True)
            self.widget_registry.append(slider)
        elif param_type == "bool":
            val = bool(initial_value) if initial_value is not None else False
            row_data["value_var"] = tk.BooleanVar(value=val)
            button = HUDToggleSwitch(row_data["control_frame"], row_data["value_var"], self.theme, self.fonts,
                                     command=lambda p=row_data["path_var"], v=row_data["value_var"]: self.send_avatar_param(p.get(), v.get(), "bool"))
            button.pack()
            self.widget_registry.append(button)

    def remove_avatar_param_row(self, row_data):
        row_data["frame"].destroy()
        self.avatar_param_rows.remove(row_data)

    def send_avatar_param(self, path, value, param_type):
        if not path:
            return
        address = f"/avatar/parameters/{path}"
        final_value = float(value) if param_type == 'float' else bool(value)
        try:
            self.avatar_osc_client.send_message(address, final_value)
        except Exception as e:
            self.log(f"Failed to send avatar param: {e}", "red")

    def bind_traces(self):
        """Binds a callback to all tk variables to update dependencies when they change."""
        for var_name in self.vars:
            if isinstance(self.vars[var_name], tk.StringVar):
                self.vars[var_name].trace_add("write", lambda *a, v=var_name: self.update_dependencies())

    def update_dependencies(self, *args):
        """Called when a setting changes. Updates UI states and previews."""
        self.apply_gui_to_config()
        spotify_enabled = self.config.get('module_spotify')
        bg = self.theme['bg_light'] if spotify_enabled else self.theme['bg_dark']
        for control in self.spotify_controls.values():
            control.master.configure(bg=bg)

        if self.osc_thread and self.osc_thread.is_alive():
            self.log("Settings changed. Restart OSC to apply.", "orange")
        self.update_preview()

    def load_settings_to_gui(self):
        """Loads the current self.config into the GUI widgets."""
        for key, var in self.vars.items():
            config_val = self.config.get(key)
            if config_val is not None:
                if isinstance(var, tk.Text):
                    var.delete('1.0', tk.END)
                    if isinstance(config_val, list):
                        var.insert('1.0', "\n".join(str(v) for v in config_val))
                else:
                    var.set(config_val)

        for row in self.avatar_param_rows:
            row['frame'].destroy()
        self.avatar_param_rows.clear()
        for param in self.config.get("avatar_parameters", []):
            self.add_avatar_param_row(param)

    def apply_gui_to_config(self):
        """Saves the current state of GUI widgets back into self.config."""
        for key, var in self.vars.items():
            if isinstance(var, tk.Text):
                self.config[key] = [line for line in var.get('1.0', tk.END).strip().split('\n') if line]
            else:
                try: self.config[key] = var.get()
                except tk.TclError: pass

        self.config["avatar_parameters"] = []
        for row in self.avatar_param_rows:
            self.config["avatar_parameters"].append({
                "name": row["name_var"].get(), "path": row["path_var"].get(),
                "type": row["type_var"].get(), "value": row["value_var"].get()
            })

    def update_preview(self, *args):
        """Renders a preview of the OSC message in the UI."""
        self.apply_gui_to_config()

        preview_thread = VrcOscThread(self.config, lambda *a: None)
        preview_thread.anim_state['char_idx'] = 10
        full_text = preview_thread.build_message()
        del preview_thread

        canvas = self.preview_canvas
        canvas.delete("all")
        y_pos = 20
        for line in full_text.split('\n'):
            canvas.create_text(20, y_pos, text=line, font=self.fonts['preview'], fill=self.theme['text'], anchor="w")
            y_pos += 20

    def log(self, message, level="INFO"):
        """Logs a message to the UI log panel in a thread-safe way."""
        def _log():
            self.log_text.config(state="normal")
            timestamp = datetime.datetime.now().strftime("[%H:%M:%S] ")
            self.log_text.insert(tk.END, timestamp + message + "\n", level.upper())
            self.log_text.see(tk.END)
            self.log_text.config(state="disabled")
        self.master.after(0, _log)

    def start_osc(self):
        """Starts the OSC worker thread."""
        self.draw_button_state(self.start_button, "▶ START", self.theme['green'], 'disabled')
        self.draw_button_state(self.stop_button, "■ STOP", self.theme['red'], 'normal')
        self.draw_button_state(self.load_button, "📂 LOAD", self.theme['accent'], 'disabled')
        self.draw_button_state(self.save_as_button, "💾 SAVE AS", self.theme['accent'], 'disabled')

        self.log("OSC Transmission ENGAGED.", "green")
        self.apply_gui_to_config()

        if self.current_profile_path and os.path.exists(os.path.dirname(self.current_profile_path)):
            with open(self.current_profile_path, 'w') as f:
                json.dump(self.config, f, indent=4)

        self.osc_thread = VrcOscThread(self.config, self.log)
        self.osc_thread.start()

    def stop_osc(self):
        """Stops the OSC worker thread."""
        if self.osc_thread and self.osc_thread.is_alive():
            self.osc_thread.stop()
            self.osc_thread.join()
            self.osc_thread = None

        self.draw_button_state(self.start_button, "▶ START", self.theme['green'], 'normal')
        self.draw_button_state(self.stop_button, "■ STOP", self.theme['red'], 'disabled')
        self.draw_button_state(self.load_button, "📂 LOAD", self.theme['accent'], 'normal')
        self.draw_button_state(self.save_as_button, "💾 SAVE AS", self.theme['accent'], 'normal')
        self.log("OSC Transmission HALTED.", "red")

    def on_closing(self):
        """Handles the window close event."""
        if messagebox.askokcancel("SYSTEM SHUTDOWN", "Cease OSC transmission and exit the application?"):
            self.stop_osc()
            self.master.destroy()

if __name__ == "__main__":

    try:
        if IS_WINDOWS:
            from ctypes import windll
            windll.shcore.SetProcessDpiAwareness(1)
    except Exception as e:
        print(f"Could not set DPI awareness: {e}")

    root = tk.Tk()


    try:
        font.Font(family="Orbitron", size=1)
    except tk.TclError:
        print("Warning: 'Orbitron' font not found. Please install it for the best visual experience. Falling back to default fonts.")

    app = Application(master=root)
    root.protocol("WM_DELETE_WINDOW", app.on_closing)
    root.mainloop()

