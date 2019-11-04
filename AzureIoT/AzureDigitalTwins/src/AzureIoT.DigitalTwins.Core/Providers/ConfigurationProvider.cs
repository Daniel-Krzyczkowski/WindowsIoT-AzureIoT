using AzureIoT.DigitalTwins.Core.Config;
using AzureIoT.DigitalTwins.Management.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.DigitalTwins.Core.Providers
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public string AzureDigitalTwinsApiUrl => AppConfiguration.AzureDigitalTwinsApiEndpoint;
    }
}
