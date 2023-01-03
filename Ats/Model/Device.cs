using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ats.Model
{
    public class Device
    {
        /// <summary>
        /// Hwmon class identifier
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sys base directory
        /// </summary>
        public string SysBaseDirectory { get; set; }

        /// <summary>
        /// Device properties
        /// </summary>
        public DevicePropertyList Properties { get; private set; }
        
        /// <summary>
        /// Device type
        /// </summary>
        public DeviceType Type { get; set; }        

        public Device()
        {
            Identifier = string.Empty;
            Name = string.Empty;
            SysBaseDirectory = string.Empty;
            Properties = new DevicePropertyList();
            Type = DeviceType.Unknown;
        }
    }

    public class DeviceList : List<Device>
    {

    }
}