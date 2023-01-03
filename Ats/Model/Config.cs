using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ats.Model
{
    internal class Config
    {
        public int ShowStatTimeoutMinutes { get; set; } = 60;
        public int ThermalTargetCelcius { get; set; } = 10;
        public int ThermalTargetDeltaCelcius { get; set; } = 10;
        public int WorkerTimeoutMs { get; set; } = 500;
    }
}
