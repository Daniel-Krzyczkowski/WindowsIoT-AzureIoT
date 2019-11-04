using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.DigitalTwins.Management.Models
{
    public class Device
    {
        public string ConnectionString { get; set; }
        public string HardwareId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string SpaceId { get; set; }
    }
}
