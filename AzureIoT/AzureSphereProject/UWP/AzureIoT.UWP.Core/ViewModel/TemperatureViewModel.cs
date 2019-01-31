using AzureIoT.UWP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureIoT.UWP.Core.ViewModel
{
    public class TemperatureViewModel : AppViewModel
    {
        public IEnumerable<TemperatureData> TemperatureData { get; set; }
        public TemperatureData LatestTemperatureValue => TemperatureData
            .Where(t => t.Timestamp >= TemperatureData
            .Max(d => d.Timestamp))
            .FirstOrDefault();
    }
}
