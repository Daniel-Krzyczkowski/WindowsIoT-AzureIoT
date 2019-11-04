using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.DigitalTwins.Core.Model
{
    public class ConfRoomInformation
    {
        [JsonProperty("NumberOfPeople")]
        public int NumberOfPeople { get; set; }
    }
}
