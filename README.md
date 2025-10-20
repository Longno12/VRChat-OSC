# VRChat OSC Pro

A powerful and user-friendly Python application to display a wide range of information in your VRChat chatbox using OSC. Customize your in-game presence by showing your current Spotify song, system stats, the time, and custom animated messages, all managed through a sleek, modern graphical interface.

---

## Features

- **Modern Glass-Morphism UI:**
  - Complete visual overhaul with gradient accents and glass-morphism design
  - Multiple color themes (Ocean, Emerald, Sunset, Rose, Slate) with live preview
  - Collapsible sidebar for compact mode
  - Dark/Light mode support with system theme detection

- **Modular Design:** Easily enable or disable features using visual toggle switches with status indicators
- **Profile & Preset Management:** Save and load different configurations as profiles, with built-in presets (Default, AFK, Gaming)
- **Quick Actions Panel:** One-click access to start/stop functions and common operations
- **Status Dashboard:** Real-time monitoring of all modules with active status display
- **Automatic Update Checker:** Notifies you at startup if a new version is available on GitHub
- **Live Preview & System Log:** Integrated control panel with live chatbox preview and real-time logging

- **Media Integration:**
  - **Spotify:** Display currently playing song, artist, progress bar, and timestamps
  - **YouTube:** Automatic detection of YouTube browser tabs
  - **Local Media (Windows):** Support for other media players when Spotify isn't active

- **Discord Rich Presence:**
  - Show custom status on your Discord profile
  - Display current Spotify song when enabled
  - Customizable buttons and images

- **System & Information Modules:**
  - **Clock:** Real-time clock with seconds toggle
  - **System Stats:** CPU/RAM usage monitoring
  - **FPS Counter:** Simulated FPS display
  - **Heartbeat:** Animated heartbeat icon
  - **Battery Status:** Laptop battery level and charging status
  - **Countdown Timer:** Customizable countdown to target dates

- **Advanced Features:**
  - **Animated Text:** Custom messages with typing animation and rewrite effects
  - **Information Cycling:** Rotate between different information pages automatically
  - **OSC Input Server:** Remote control via VRChat OSC commands
  - **Avatar Parameters:** Advanced parameter management (placeholder)

---

## Prerequisites

- **Python 3.8+:** This script requires a modern version of Python. You can download it from [python.org](https://www.python.org/downloads/).
  - **Important:** During installation, make sure to check the box that says **"Add Python to PATH"**.

- **VRChat:** You must have VRChat installed and running.

---

## Setup & Installation

1. **Enable OSC in VRChat:**
   - Launch VRChat
   - Open your Action Menu (the circular menu)
   - Navigate to `Options` -> `OSC`
   - Make sure OSC is **Enabled**

2. **Download the Project:**
   - Go to the main page of this GitHub repository
   - Click the green **`< > Code`** button
   - Select **"Download ZIP"**
   - Extract the ZIP file to a permanent folder on your computer (e.g., in `My Documents`)

3. **Install Dependencies:**
   - In the folder where you extracted the files, locate and double-click the **`install_requirements.bat`** file
   - This will open a command prompt and automatically install all necessary Python libraries
   - Wait for it to finish and display "All required packages have been installed successfully"

---

## How to Use

1. **Launch the Application:**
   - Double-click the main Python script (`VRChat_OSC_Pro.py`) to run the application
   - The modern interface will load with the Dashboard view

2. **Configure Spotify (For music features):**
   - Navigate to the **🎵 Media** page using the sidebar
   - Go to your [Spotify Developer Dashboard](https://developer.spotify.com/dashboard) and log in
   - Click **"Create an app"**. Give it any name and description (e.g., "VRChat OSC")
   - Once created, copy your **Client ID** and **Client Secret**. Paste them into the matching fields in the application
   - In the Spotify Dashboard, go to your app's **"Settings"**
   - Under **"Redirect URIs"**, add this exact URL: `[https://longno12.github.io/Spotify-Verify-Link-help/](https://www.longno.co.uk/spotify)`
   - Click **"Add"**, then scroll down and click **"Save"**

3. **Customize Your Setup:**
   - Use the sidebar to navigate through different pages:
     - **📊 Dashboard:** Overview and quick settings
     - **🎚️ Presets:** Manage configuration presets
     - **🎵 Media:** Spotify, YouTube, and local media settings
     - **⚙️ System:** Clock, stats, countdown, and cycling options
     - **🎨 Appearance:** Themes and text customization
     - **💬 Discord:** Rich Presence configuration
     - **👤 Avatar:** Parameter management
     - **🌐 Network:** OSC server settings

4. **Use Quick Actions:**
   - Start/Stop OSC transmission using the Quick Actions panel in the sidebar
   - Monitor module status in the Module Status tab
   - Watch real-time preview in the Live Preview tab

5. **Save Your Configuration:**
   - Use **💾 Save Profile As** to save your settings to a file
   - Create and manage presets for different scenarios (AFK, Gaming, etc.)

6. **Start Sending to VRChat:**
   - Click the **▶ Start** button in the Quick Actions panel
   - For first-time Spotify setup, your browser will open for authentication
   - The System Log will show "Spotify Authenticated Successfully!" when ready
   - Your custom messages will appear in the VRChat chatbox

7. **Stop the Application:**
   - Click the **■ Stop** button to clear the chatbox and stop transmission
   - Close the application normally to save settings

---

## OSC Commands for Remote Control

VRChat can send OSC messages to control the application remotely:

- **Address:** `/VRChatOSCPro/toggleModule`
  - **Type:** String
  - **Action:** Toggles a specific module on or off
  - **Example Value:** `"module_spotify"`

- **Address:** `/VRChatOSCPro/setPreset`
  - **Type:** String
  - **Action:** Activates a saved configuration preset
  - **Example Value:** `"AFK"`

**Note:** VRChat sends messages TO port 9000, and LISTENS for messages ON port 9001.

---

## Troubleshooting

- **"Missing Library" error on startup:**
  - Run `install_requirements.bat` again
  - Ensure Python was installed with "Add Python to PATH" option

- **Nothing Appears in My Chatbox:**
  - Confirm OSC is enabled in VRChat (most common issue)
  - Ensure you clicked the **▶ Start** button
  - Check firewall/antivirus isn't blocking Python

- **Spotify Authentication Fails:**
  - Verify Redirect URI exactly matches: `https://www.longno.co.uk/spotify`
  - Check Client ID and Secret are copied correctly with no extra spaces
  - Ensure your Spotify app is set to "Development" mode in the dashboard

- **YouTube/Local Media Not Working:**
  - These features are Windows-only
  - Ensure you're using supported browsers (Chrome, Firefox, Edge)

- **Discord Rich Presence Not Showing:**
  - Ensure Discord is running
  - Check Discord's "Activity Status" is enabled in settings

---

## Version Information

**Current Version:** 4.1.0  
**Latest Features:** Modern UI overhaul, Quick Actions panel, Status Dashboard, Theme system, Compact mode

For support and updates, visit the GitHub repository or join our Discord community.
