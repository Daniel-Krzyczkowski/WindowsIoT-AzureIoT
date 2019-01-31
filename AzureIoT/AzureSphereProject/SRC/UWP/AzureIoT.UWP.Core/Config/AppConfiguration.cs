using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.UWP.Core.Config
{
    internal static class AppConfiguration
    {
        private static string Tenant = "";
        private static string ClientId = "";
        public static string PolicySignUpSignIn = "";

        public static string[] ApiScopes = { "https://<<tenant_name>>.onmicrosoft.com/api/user_impersonation" };
        public static string ApiEndpoint = "";

        private static string BaseAuthority = "";
        public static string Authority = BaseAuthority.Replace("{tenant}", Tenant).Replace("{policy}", PolicySignUpSignIn);
        public static PublicClientApplication PublicClientApp { get; } = new PublicClientApplication(ClientId, Authority);
    }
}
