using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.WebAPI.Services.Settings
{
    public class AzureAdB2CSettings
    {
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string Policy { get; set; }
    }
}
