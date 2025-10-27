using LibreHardwareMonitor.Hardware;
using System.Linq;

public class HardwareManager
{
    private Computer _computer;
    private IHardware _cpu;
    private IHardware _gpu;
    private IHardware _ram;

    public string CpuName { get; private set; }
    public string GpuName { get; private set; }
    public float CpuLoad { get; private set; }
    public float CpuTemp { get; private set; }
    public float GpuLoad { get; private set; }
    public float GpuTemp { get; private set; }
    public float RamUsed { get; private set; }
    public float RamTotal { get; private set; }

    public HardwareManager()
    {
        _computer = new Computer { IsCpuEnabled = true, IsGpuEnabled = true, IsMemoryEnabled = true };
        _computer.Open();
        _cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
        _gpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuIntel);
        _ram = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
        CpuName = _cpu?.Name ?? "N/A";
        GpuName = _gpu?.Name ?? "N/A";
        Update();
        var totalRamSensor = _ram?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name == "Memory Total");
        if (totalRamSensor?.Value != null) RamTotal = totalRamSensor.Value.Value;
    }

    public void Update()
    {
        _cpu?.Update();
        _gpu?.Update();
        _ram?.Update();
        CpuLoad = _cpu?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "CPU Total")?.Value ?? 0;
        CpuTemp = _cpu?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature)?.Value ?? 0;
        GpuLoad = _gpu?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "GPU Core")?.Value ?? 0;
        GpuTemp = _gpu?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature)?.Value ?? 0;
        RamUsed = _ram?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name == "Memory Used")?.Value ?? 0;
    }

    public void Close() => _computer.Close();
}