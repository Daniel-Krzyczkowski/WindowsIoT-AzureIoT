using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.UWP.Core.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> Authenticate();

        Task SignOut();
    }
}
