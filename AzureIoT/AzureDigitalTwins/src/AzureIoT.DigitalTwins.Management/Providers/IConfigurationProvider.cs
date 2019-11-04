using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.DigitalTwins.Management.Providers
{
    public interface IConfigurationProvider
    {
        string AzureDigitalTwinsApiUrl { get; }
    }
}
