using AzureIoT.DigitalTwins.Core.Config;
using AzureIoT.DigitalTwins.Management.Providers;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.Core.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private AuthenticationContext _authenticationContext;
        private IPlatformParameters _platformParameters;
        private IAccessTokenProvider _accessTokenProvider;
        public AuthenticationService(IPlatformParameters platformParameters, IAccessTokenProvider accessTokenProvider)
        {
            _authenticationContext = new AuthenticationContext(AppConfiguration.Authority);
            _platformParameters = platformParameters;
            _accessTokenProvider = accessTokenProvider;
        }

        public async Task<AuthenticationResult> Authenticate()
        {
            AuthenticationResult authResult = null;
            try
            {
                authResult = await _authenticationContext.AcquireTokenAsync(AppConfiguration.AzureDigitalTwinsResourceId,
                AppConfiguration.ClientId, new Uri(AppConfiguration.RedirectUrl), _platformParameters);
            }

            catch (AdalException ex)
            {
                if (ex.ErrorCode == "authentication_canceled")
                {
                    // The user cancelled the sign-in, no need to display a message.
                    System.Diagnostics.Debug.WriteLine("User canceled sign in operation");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("If the error continues, please contact your administrator.\n\nError Description:\n\n{0}", ex.Message), "Sorry, an error occurred while signing you in.");
                }
            }
            _accessTokenProvider.AccessToken = authResult.AccessToken;
            return authResult;
        }

        public void SignOut()
        {
            _authenticationContext.TokenCache.Clear();
        }
    }
}
