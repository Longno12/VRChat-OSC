using System;
using System.Collections.Generic;

public class AppSettings
{
    public Dictionary<string, PresetSettings> Presets 
    { 
        get; set; 
    } 
        = new Dictionary<string, PresetSettings>
    {
        { "Default", new PresetSettings() }
    };
    public string LastPreset { get; set; } = "Default";
}

public class PresetSettings
{
    public bool SpotifyEnabled { get; set; } = false;
    public bool YouTubeEnabled { get; set; } = false;
    public bool CpuInfoEnabled { get; set; } = true;
    public bool RamInfoEnabled { get; set; } = true;
    public bool GpuInfoEnabled { get; set; } = true;
    public bool AnimatedTextEnabled { get; set; } = false;
    public bool PersonalStatusEnabled { get; set; } = false;
    public bool TimeEnabled { get; set; } = true;
    public bool CountdownEnabled { get; set; } = false;
    public bool PlayspaceEnabled { get; set; } = false;
    public bool AutoAfkEnabled { get; set; } = false;
    public string CpuFormat { get; set; } = "CPU: {NAME} @ {LOAD}% ({TEMP}°C)";
    public string RamFormat { get; set; } = "RAM: {USED:F1} / {TOTAL:F1} GB";
    public string GpuFormat { get; set; } = "GPU: {NAME} @ {LOAD}% ({TEMP}°C)";
    public string PersonalStatus { get; set; } = "My Status";
    public List<string> AnimatedTexts { get; set; } = new List<string>
    {
        "VRChat OSC Pro C#",
        "by Longno"
    };
    public DateTime CountdownTarget { get; set; } = DateTime.Now.AddDays(1);
    public string CountdownFinished { get; set; } = "Countdown Finished!";
}