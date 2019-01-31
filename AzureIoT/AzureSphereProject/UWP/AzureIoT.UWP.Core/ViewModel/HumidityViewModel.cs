using AzureIoT.UWP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureIoT.UWP.Core.ViewModel
{
    public class HumidityViewModel : AppViewModel
    {
        public IEnumerable<HumidityData> HumidityData { get; set; }
        public HumidityData LatestHumidityValue => HumidityData
            .Where(h => h.Timestamp >= HumidityData
            .Max(d => d.Timestamp))
            .FirstOrDefault();
    }
}
