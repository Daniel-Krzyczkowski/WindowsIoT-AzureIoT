using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.Core.Authentication
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> Authenticate();

        void SignOut();
    }
}
