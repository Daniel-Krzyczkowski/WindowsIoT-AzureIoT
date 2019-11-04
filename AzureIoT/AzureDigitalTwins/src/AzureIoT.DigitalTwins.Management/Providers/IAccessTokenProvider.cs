using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.DigitalTwins.Management.Providers
{
    public interface IAccessTokenProvider
    {
        string AccessToken { get; set; }
    }
}
