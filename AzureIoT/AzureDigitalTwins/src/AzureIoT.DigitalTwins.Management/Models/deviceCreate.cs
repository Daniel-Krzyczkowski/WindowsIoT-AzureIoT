// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AzureIoT.DigitalTwins.Management.Models
{
    public class DeviceCreate
    {
        public string HardwareId { get; set; }
        public string Name { get; set; }
        public string SpaceId { get; set; }
    }
}