using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.WebAPI.Data
{
    public class TemperatureData : SensorData
    {
        public string Temperature { get; set; }
    }
}
