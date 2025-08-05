# VRChat OSC Pro Controller

A powerful and user-friendly Python application to display a wide range of information in your VRChat chatbox using OSC. Customize your in-game presence by showing your current Spotify song, system stats, the time, and custom animated messages, all managed through a simple graphical interface

---

## Features

-   **Modular Design:** Easily enable or disable features to your liking from the GUI.
-   **Spotify Integration:**
    -   Display the currently playing song, artist, and playback status (Playing/Paused).
    -   Show a real-time progress bar and timestamps (`01:23 / 03:45`).
    -   Optionally display the name of the playback device (e.g., "Desktop Speaker").
    -   Automatically attempts to re-authenticate if your access token expires.
-   **System & Info Modules:**
    -   A live clock with optional seconds.
    -   A "heartbeat" icon (❤) that flashes periodically to show the script is running.
    -   A fun, simulated FPS counter.
    -   A display for your actual CPU and RAM usage percentage.
-   **Advanced Customization:**
    -   **Animated Text:** Display a list of custom messages with a typing and deleting animation.
    -   **Style Control:** Customize separator characters, progress bar style, and add a personal watermark.
    -   **GUI Theming:** Select from various built-in themes to alter the application's look and feel.
-   **User-Friendly GUI:**
    -   All settings are managed through a clean, tabbed interface.
    -   A **Live Preview** panel shows you exactly what your message will look like.
    -   A built-in **Log** tab makes setup and troubleshooting easy.
    -   Helpful tooltips guide you through every setting.
    -   All your settings are automatically saved to a `config.json` file.

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
    -   In the folder where you extracted the files, locate and double-click the **`install_packages.bat`** file.
    -   This will open a command prompt and automatically install all the necessary Python libraries. Wait for it to finish and say "All required packages have been installed."

---

## How to Use

1.  **Launch the Application:**
    -   After installation is complete, double-click the **`VRTEST.py`** file to run the application.

2.  **Configure Spotify (Required for music features):**
    -   In the application, go to the **Spotify** tab.
    -   Click the **"Get Credentials Help"** button. This will take you to your Spotify Developer Dashboard (you may need to log in).
    -   Click **"Create an app"**. Give it any name and description (e.g., "VRChat OSC").
    -   Once created, you will see your **Client ID** and **Client Secret**. Copy and paste each one into the matching fields in the application.
    -   In the Spotify Dashboard, go to your app's **"Settings"**.
    -   Under **"Redirect URIs"**, add `http://localhost:8888/callback` and click **"Save"**.
    -   Make sure the **"Enable Spotify Integration"** box is checked in the app.

3.  **Customize Modules and Style:**
    -   Go through the **Modules** and **Style** tabs to enable the features you want and customize their appearance.
    -   The **Live Preview** on the right will update as you make changes.

4.  **Start Sending to VRChat:**
    -   When you are satisfied with your settings, click the **Start** button.
    -   The first time you start with Spotify enabled, a browser window will open asking you to authorize the application. Log in and agree. You will be redirected to a page that says "SUCCESS" or something similar; you can then close the browser tab.
    -   The log tab will show "Spotify Authenticated Successfully!", and the script will begin sending your custom message to the VRChat chatbox.

5.  **Stop the Script:**
    -   Click the **Stop** button to clear the chatbox and stop sending messages. The GUI will become editable again.

---

## Troubleshooting

-   **"Missing Library" error on startup:**
    -   This means the `install_packages.bat` file was not run or failed. Run it again. If it still fails, you may need to reinstall Python, making sure you check the **"Add Python to PATH"** option.

-   **Nothing Appears in My Chatbox:**
    -   **Confirm OSC is enabled in VRChat.** This is the most common cause.
    -   Make sure you have clicked the **Start** button in the application.
    -   Check that your firewall or antivirus software is not blocking Python or `VRTEST.py` from accessing the network.

-   **Spotify Errors in the Log:**
    -   If you see "Client ID not set" or an authentication error, double-check that you have copied and pasted your **Client ID** and **Client Secret** correctly.
    -   Ensure that the **Redirect URI** in your Spotify Developer Dashboard settings **exactly** matches the one in the application (`http://localhost:8888/callback`).
