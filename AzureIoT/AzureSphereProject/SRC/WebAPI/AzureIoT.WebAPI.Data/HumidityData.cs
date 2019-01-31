using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.WebAPI.Data
{
    public class HumidityData : SensorData
    {
        public string Humidity { get; set; }
    }
}
