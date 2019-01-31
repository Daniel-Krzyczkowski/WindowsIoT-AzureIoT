using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.UWP.Core.Config
{
    public static class RestServiceConfig
    {
        public const string TemperatureEndpoint = "DeviceData/temperature";

        public const string HumidityEndpoint = "DeviceData/humidity";

        public const string MessagingEndpoint = "Messaging/send";

        public const string WebServiceUrl = "https://cloudy-of-things.azurewebsites.net/api/";
    }
}
