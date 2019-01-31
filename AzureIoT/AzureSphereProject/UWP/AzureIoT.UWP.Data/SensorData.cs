using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.UWP.Data
{
    public abstract class SensorData
    {
        public Guid Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
