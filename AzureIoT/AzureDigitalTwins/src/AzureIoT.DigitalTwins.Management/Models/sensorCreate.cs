// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AzureIoT.DigitalTwins.Management.Models
{
    public class SensorCreate
    {
        public string DataType { get; set; }
        public string DeviceId { get; set; }
        public string HardwareId { get; set; }
    }
}