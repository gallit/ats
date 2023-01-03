using Ats.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ats.Helper
{
    internal class DeviceHelper
    {
        public static string GetDevicePropertyValue(Device device, string propertyName)
        {
            return device.Properties.FirstOrDefault(x => x.Name == propertyName)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Load device properties from the files in the base directory
        /// </summary>
        /// <param name="device"></param>
        public static void LoadProperties(Device device)
        {
            device.Properties.Clear();
            foreach (var f in Directory.GetFiles(device.SysBaseDirectory).OrderBy(x => x))
            {
                var fi = new FileInfo(f);
                var p = new DeviceProperty()
                {
                    Name = fi.Name,
                    Value = File.ReadAllText(f).Trim()
                };
                device.Properties.Add(p);
            }
        }

        public static Device ParseDevice(string baseDirectory)
        {
            var d = new Device();
            d.Identifier = new DirectoryInfo(baseDirectory).Name;
            d.SysBaseDirectory = baseDirectory;
            LoadProperties(d);
            d.Name = GetDevicePropertyValue(d, "name");
            d.Type = ParseDeviceType(d.Name);
            return d;
        }

        /// <summary>
        /// Parse the device type
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static DeviceType ParseDeviceType(string name)
        {
            var r = DeviceType.Unknown;
            switch(name)
            {
                case "pwmfan": r = DeviceType.FanController; break;
                case "cpu_thermal": r = DeviceType.TemperatureSensor; break;
                case "gpu_thermal": r = DeviceType.TemperatureSensor; break;
                case "tcpm_source_psy_4_0022": r = DeviceType.PowerSensor; break;
            }
            return r;
        }

        public static void SetDevicePropertyValue(Device device, string propertyName, object value)
        {
            var fp = Path.Combine(device.SysBaseDirectory, propertyName);
            if (!File.Exists(fp))
            {
                throw new ArgumentException($"File does not exists : {fp}");
            }
            File.WriteAllText(fp, $"{value}");
        }

    }
}
