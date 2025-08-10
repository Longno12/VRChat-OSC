# VRChat OSC Pro Controller

A powerful and user-friendly Python application to display a wide range of information in your VRChat chatbox using OSC. Customize your in-game presence by showing your current Spotify song, system stats, the time, and custom animated messages, all managed through a sleek, modern graphical interface.

---

## Features

-   **Modular Design:** Easily enable or disable features to your liking from the main interface.
-   **Profile Management:** Save and load different configurations as profiles, allowing you to switch between setups instantly.
-   **First-Run Interactive Tutorial:** Guides new users through the interface on their first launch.
-   **Automatic Update Checker:** Notifies you at startup if a new version is available on GitHub.
-   **Live Changelog Panel:** A dedicated "Updates" section in the UI shows you what's new in the current version.
-   **Spotify Integration:**
    -   Display the currently playing song, artist, and playback status.
    -   Show a real-time progress bar and timestamps.
    -   User-friendly setup guide for API credentials.
-   **Local Media Support (Windows Only):**
    -   Displays song title and artist from other local media players if Spotify isn't active.
-   **Discord Rich Presence:**
    -   Show a custom status on your Discord profile, including your current Spotify song.
-   **System & Info Modules:**
    -   Live clock, CPU/RAM usage, animated "heartbeat" icon, and a simulated FPS counter.
-   **Advanced Customization:**
    -   **Animated Text:** Display custom messages with a typing animation.
    -   **Style Control:** Customize separators, watermarks, and the application's accent color.
-   **Modern User-Friendly GUI:**
    -   A **Live Preview** panel shows you exactly what your message will look like as you edit.
    -   A built-in **Log** panel makes setup and troubleshooting easy.
    -   Settings are saved into `.json` profiles for easy backup and sharing.

---

## Prerequisites

-   **Python 3.x:** This script requires a modern version of Python. You can download it from [python.org](https://www.python.org/downloads/).
    -   **Important:** During installation, make sure to check the box that says **"Add Python to PATH"**.
-   **VRChat:** You must have VRChat installed.

---

## Setup & Installation

1.  **Enable OSC in VRChat:**
    -   Launch VRChat.
    -   Open your Action Menu (the circular menu).
    -   Navigate to `Options` -> `OSC`.
    -   Make sure OSC is **Enabled**.

2.  **Download the Project:**
    -   Go to the main page of this GitHub repository.
    -   Click the green **`< > Code`** button.
    -   Select **"Download ZIP"**.
    -   Extract the ZIP file to a permanent folder on your computer (e.g., in `My Documents`).

3.  **Install Dependencies:**
    -   In the folder where you extracted the files, locate and double-click the **`install_requirements.bat`** file.
    -   This will open a command prompt and automatically install all the necessary Python libraries. Wait for it to finish and say "All required packages have been installed successfully."

---

## How to Use

1.  **Launch the Application:**
    -   After installation is complete, double-click the main Python file (e.g., `main.py` or `VRC_OSC_Pro.py`) to run the application.
    -   If it's your first time, a pop-up will ask if you want a tutorial. Click "Yes" for a guided tour.

2.  **Configure Spotify (For music features):**
    -   Go to your [Spotify Developer Dashboard](https://developer.spotify.com/dashboard) and log in.
    -   Click **"Create an app"**. Give it any name and description (e.g., "VRChat OSC").
    -   Once created, copy your **Client ID** and **Client Secret**. Paste them into the matching fields in the Spotify panel of our application.
    -   In the Spotify Dashboard, go to your app's **"Settings"**.
    -   Under **"Redirect URIs"**, add this exact URL: `https://longno12.github.io/Spotify-Verify-Link-help/`
    -   Click **"Add"**, then scroll down and click **"Save"**.

3.  **Customize Modules and Style:**
    -   Go through the various panels (**CORE MODULES**, **STYLE & TEXT**, etc.) to enable the features you want and customize their appearance.
    -   The **Live Preview** on the right will update as you make changes.

4.  **Save Your Profile:**
    -   Once you are happy with your settings, click the **💾 SAVE AS** button to save your configuration as a profile. You can create multiple profiles for different setups. The application will remember the last profile you loaded.

5.  **Start Sending to VRChat:**
    -   Click the **▶ START** button.
    -   The first time you start with Spotify enabled, a browser window will open asking you to authorize the application. Log in and agree.
    -   You will be redirected to the "Capture Spotify Redirect URL" website. **Copy the entire, long URL** from your browser's address bar.
    -   Go to the **console window** that opened with the script and **paste the URL** when prompted. Press Enter.
    -   The log panel will show "Spotify Authenticated Successfully!", and the script will begin sending your custom message to the VRChat chatbox.

6.  **Stop the Script:**
    -   Click the **■ STOP** button to clear the chatbox and stop sending messages.

---

## Troubleshooting

-   **"Missing Library" error on startup:**
    -   This means the `install_requirements.bat` file was not run or failed. Run it again. If it still fails, you may need to reinstall Python, making sure you check the **"Add Python to PATH"** option.

-   **Nothing Appears in My Chatbox:**
    -   **Confirm OSC is enabled in VRChat.** This is the most common cause.
    -   Make sure you have clicked the **▶ START** button in the application.
    -   Check that your firewall or antivirus software is not blocking Python from accessing the network.

-   **Spotify Errors in the Log:**
    -   If you see "Client ID not set" or an "INVALID_CLIENT" error, double-check that you have copied and pasted your **Client ID** and **Client Secret** correctly.
    -   Ensure that the **Redirect URI** in your Spotify Developer Dashboard settings **exactly** matches `https://longno12.github.io/Spotify-Verify-Link-help/`. There should be no typos.

-   **The script closes immediately or I don't see a place to paste my Spotify URL:**
    -   You must run the script from a terminal or command prompt to be able to paste the URL. If you're having trouble, create a simple `.bat` file with the line `python main.py` (or your script's filename) and `pause` on the next line. This will keep the window open.
