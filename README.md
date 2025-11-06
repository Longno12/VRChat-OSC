# VRChat OSC Pro C#

This is a simple but powerful app for putting your PC stats, what you're listening to, and custom messages into your VRChat chatbox.

I originally built a version of this in Python and wanted to create a more stable, lightweight, and performant version in C#. The project is heavily inspired by the awesome [Magic Chatbox](https://github.com/BoiHanny/vrcosc-magicchatbox) by BoiHanny.

Here's what it looks like in-game:

![VRChat OSC Pro C# In-Game Screenshot](https://github.com/Longno12/VRChat-OSC/blob/main/%7BD527EA34-02F8-4BB7-9792-15DA5CC82198%7D.png)

---

### What It Does

*   **Real Hardware Stats:** Shows your CPU & GPU name, load, and temperature, plus RAM usage. It uses the LibreHardwareMonitor library, so it's fast and accurate.
*   **Media Display:**
    *   Shows your current song from the Spotify desktop app.
    *   Detects and shows video titles from YouTube in any browser.
*   **AFK Mode:** A simple toggle that overrides everything to show an "AFK" message that tracks how long you've been away. When you turn it off, your old settings are restored.
*   **Customization:**
    *   You can change the format of the hardware stats using placeholders like `{NAME}`, `{LOAD}`, and `{TEMP}`.
    *   Set a custom "Personal Status" message.
    *   Add your own animated, typing text.
    *   Show the current time.
*   **Simple & Forgets Itself:**
    *   Saves all your settings to a `config.json` file automatically when you close it.
    *   Minimizes to the system tray so it just runs in the background.

---

### Get Started

1.  Go to the **[Releases](https://github.com/Longno12/VRChat-OSC/releases)** page.
2.  Download the latest `.zip` file.
3.  Extract the folder anywhere you want.
4.  Run the `.exe` file.
5.  **In VRChat,** make sure OSC is enabled in your Action Menu (`Options -> OSC -> Enabled`).

That's it. Toggle the features you want in the app, click the big "Start" button, and it will start broadcasting to VRChat.

---

### Customizing the Stats Display

In the "System Stats" section, you can change how the text looks. Use these placeholders in the text boxes:

| Placeholder | What it does                                       | Used In     |
| :---------- | :------------------------------------------------- | :---------- |
| `{NAME}`    | Shows the full name of your CPU or GPU.            | CPU / GPU   |
| `{LOAD}`    | Shows the current usage percentage.                | CPU / GPU   |
| `{TEMP}`    | Shows the current temperature.                     | CPU / GPU   |
| `{USED}`    | Shows how much RAM is currently being used (in GB). | RAM         |
| `{TOTAL}`   | Shows the total amount of RAM you have (in GB).      | RAM         |

**Example:**
`GPU: {NAME} @ {LOAD}% ({TEMP})` becomes `GPU: NVIDIA GeForce RTX 5080 @ 34% (59)`

---

### Building It Yourself

If you want to mess with the code, you'll need:

*   Visual Studio 2022 (with the ".NET desktop development" workload).
*   The project requires these NuGet packages: `CoreOSC`, `LibreHardwareMonitorLib`, and `System.Text.Json`.

---

### Credits

*   This project was built for anybody, based on my original Python version.
*   Heavily inspired by **[Magic Chatbox](https://github.com/BoiHanny/vrcosc-magicchatbox)**.
*   Uses the fantastic **CoreOSC** and **LibreHardwareMonitor** libraries.
