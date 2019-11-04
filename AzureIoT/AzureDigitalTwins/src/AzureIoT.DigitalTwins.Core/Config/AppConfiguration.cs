using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AzureIoT.DigitalTwins.Core.Config
{
    internal static class AppConfiguration
    {
        public const string Tenant = "<<Azure AD B2C Tenant>>";
        public const string ClientId = "<<Client ID>>";

        public const string AzureDigitalTwinsApiEndpoint = "<<Azure Digital Twins Management API endpoint>>";
        public const string AzureAdInstance = "<<https://login.microsoftonline.com/{0}>>";

        public static string Authority = String.Format(CultureInfo.InvariantCulture, AzureAdInstance, Tenant);

        public const string AzureDigitalTwinsResourceId = "<<Azure Digital Twins Resource ID>>";

        public const string RedirectUrl = "ms-app://<<Unique App Identifier>>";
        public const string RoomDetailsApiEndpoint = "<<Room Details Azure Function URL>>";
        public const string RoomDetailsApiKey = "<<Room Details Azure Function Key>>";
    }
}
