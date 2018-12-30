using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureTruckIoT.SensorsApp.Model
{
    class ToCloudMessage
    {
        public double DetectedTemperature { get; set; }
        public double DetectedPressure { get; set; }
        public double DetectedAltitude { get; set; }
        public string DetectedColor { get; set; }
    }
}
