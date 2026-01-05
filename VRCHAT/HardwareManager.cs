using System;
using System.Collections.Generic;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace VrcOscChatbox
{
    public sealed class HardwareManager : IDisposable
    {
        private readonly Computer _computer;
        private IHardware _cpu;
        private List<IHardware> _gpus;
        private IHardware _ram;
        private ISensor _cpuLoadSensor;
        private ISensor _cpuTempSensor;
        private ISensor _gpuLoadSensor;
        private ISensor _gpuTempSensor;
        private ISensor _ramUsedSensor;
        private ISensor _ramTotalSensor;

        public string CpuName { get; private set; } = "N/A";
        public string GpuName { get; private set; } = "N/A";
        public float CpuLoad { get; private set; }
        public float CpuTemp { get; private set; }
        public float GpuLoad { get; private set; }
        public float GpuTemp { get; private set; }
        public float RamUsed { get; private set; }
        public float RamTotal { get; private set; }

        public HardwareManager()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true
            };

            _computer.Open();
            AttachHardware();
            CacheSensors();
            Update();
        }

        public void Update()
        {
            _cpu?.Update();
            foreach (var gpu in _gpus)
            {
                gpu?.Update();
            }
            _ram?.Update();
            CpuLoad = ReadFloat(_cpuLoadSensor);
            CpuTemp = ReadFloat(_cpuTempSensor);
            GpuLoad = ReadFloat(_gpuLoadSensor);
            GpuTemp = ReadFloat(_gpuTempSensor);
            RamUsed = ReadFloat(_ramUsedSensor);
            if (RamTotal <= 0f) RamTotal = ReadFloat(_ramTotalSensor);
        }

        public void Close()
        {
            try 
            {
                _computer.Close(); 
            } 
            catch { }
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        private void AttachHardware()
        {
            _cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            _gpus = _computer.Hardware.Where(h => h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuIntel).ToList();
            _ram = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
            CpuName = _cpu != null ? _cpu.Name : "N/A";
            var primaryGpu = _gpus.FirstOrDefault();
            GpuName = primaryGpu != null ? primaryGpu.Name : "N/A";
        }

        private void CacheSensors()
        {
            _cpuLoadSensor = FindSensor(_cpu, SensorType.Load, "CPU Total");
            _cpuTempSensor = Prefer(FindSensor(_cpu, SensorType.Temperature, "Core Max", "CPU Package"), FindFirstSensor(_cpu, SensorType.Temperature));
            var primaryGpu = _gpus.FirstOrDefault();
            _gpuLoadSensor = primaryGpu != null ? Prefer( FindSensor(primaryGpu, SensorType.Load, "GPU Core", "GPU Usage", "D3D 3D"), FindFirstSensor(primaryGpu, SensorType.Load)) : null;
            _gpuTempSensor = primaryGpu != null ? Prefer( FindSensor(primaryGpu, SensorType.Temperature, "GPU Core", "GPU Temperature", "Hot Spot", "GPU"), FindFirstSensor(primaryGpu, SensorType.Temperature)) : null;
            _ramUsedSensor = FindSensor(_ram, SensorType.Data, "Memory Used", "Used Memory");
            _ramTotalSensor = FindSensor(_ram, SensorType.Data, "Memory Total", "Total Memory");
        }

        private static float ReadFloat(ISensor s)
        {
            if (s == null) return 0f;
            var v = s.Value;
            return v.HasValue ? v.Value : 0f;
        }

        private static ISensor Prefer(params ISensor[] sensors)
        {
            foreach (var s in sensors) if (s != null) return s;
            return null;
        }

        private static ISensor FindFirstSensor(IHardware hw, SensorType type)
        {
            if (hw == null) return null;
            hw.Update();
            foreach (var s in EnumerateAllSensors(hw)) if (s.SensorType == type) return s;
            return null;
        }

        private static ISensor FindSensor(IHardware hw, SensorType type, params string[] preferredNames)
        {
            if (hw == null) return null;
            hw.Update();
            var sensors = EnumerateAllSensors(hw).Where(s => s.SensorType == type).ToList();
            if (sensors.Count == 0) return null;
            if (preferredNames != null && preferredNames.Length > 0)
            {
                foreach (var name in preferredNames)
                {
                    var match = sensors.FirstOrDefault(s =>
                        s.Name != null &&
                        s.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (match != null) return match;
                }
            }
            return sensors.FirstOrDefault();
        }

        private static IEnumerable<ISensor> EnumerateAllSensors(IHardware hw)
        {
            foreach (var s in hw.Sensors) yield return s;
            foreach (var sub in hw.SubHardware)
            {
                sub.Update();
                foreach (var s in sub.Sensors) yield return s;
            }
        }
    }
}