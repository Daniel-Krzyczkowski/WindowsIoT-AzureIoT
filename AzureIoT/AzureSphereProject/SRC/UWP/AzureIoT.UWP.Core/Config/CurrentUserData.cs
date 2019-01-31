using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.UWP.Core.Config
{
    public static class CurrentUserData
    {
        public static string Username { get; set; }
        public static string AccessToken { get; set; }
        public static DateTimeOffset AccessTokenExpiresOn { get; set; }
    }
}
