using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.WebAPI.Data
{
    public abstract class SensorData
    {
        public Guid Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
