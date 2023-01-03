using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ats.Model
{
    public class DeviceProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public DeviceProperty() 
        {
            Name = string.Empty;
            Value = string.Empty;
        }
    }

    public class DevicePropertyList : List<DeviceProperty> 
    { 
    }
}
