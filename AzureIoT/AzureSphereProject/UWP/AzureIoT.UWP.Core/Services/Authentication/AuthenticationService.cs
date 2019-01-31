using AzureIoT.UWP.Core.Config;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.UWP.Core.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        public async Task<AuthenticationResult> Authenticate()
        {
            AuthenticationResult authResult = null;
            IEnumerable<IAccount> accounts = await AppConfiguration.PublicClientApp.GetAccountsAsync();

            try
            {
                IAccount currentUserAccount = GetAccountByPolicy(accounts, AppConfiguration.PolicySignUpSignIn);
                authResult = await AppConfiguration.PublicClientApp.AcquireTokenSilentAsync(AppConfiguration.ApiScopes, currentUserAccount, AppConfiguration.Authority, false);

            }

            catch (MsalUiRequiredException)
            {
                authResult = await InitializeAuthentication(accounts);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Users:{string.Join(",", accounts.Select(u => u.Username))}{Environment.NewLine}Error Acquiring Token:{Environment.NewLine}{ex}");
            }
            return authResult;
        }

        private IAccount GetAccountByPolicy(IEnumerable<IAccount> accounts, string policy)
        {
            foreach (var account in accounts)
            {
                string userIdentifier = account.HomeAccountId.ObjectId.Split('.')[0];
                if (userIdentifier.EndsWith(policy.ToLower())) return account;
            }

            return null;
        }

        private async Task<AuthenticationResult> InitializeAuthentication(IEnumerable<IAccount> accounts)
        {
            try
            {
                var authResult = await AppConfiguration.PublicClientApp.AcquireTokenAsync(AppConfiguration.ApiScopes, GetAccountByPolicy(accounts, AppConfiguration.PolicySignUpSignIn), UIBehavior.SelectAccount, string.Empty, null, AppConfiguration.Authority);
                return authResult;
            }
            catch (MsalClientException ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public async Task SignOut()
        {
            IEnumerable<IAccount> accounts = await AppConfiguration.PublicClientApp.GetAccountsAsync();
            accounts.ToList().ForEach(async a => await AppConfiguration.PublicClientApp.RemoveAsync(a));
            CurrentUserData.Username = string.Empty;
            CurrentUserData.AccessToken = string.Empty;
            CurrentUserData.AccessTokenExpiresOn = DateTimeOffset.MinValue;
        }
    }
}
