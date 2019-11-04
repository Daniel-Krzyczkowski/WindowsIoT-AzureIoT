using AzureIoT.DigitalTwins.Management.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.DigitalTwins.Core.Providers
{
    public class AccessTokenProvider : IAccessTokenProvider
    {
        public string AccessToken { get; set; }
    }
}
