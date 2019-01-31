using AzureIoT.UWP.Core.Config;
using AzureIoT.UWP.Core.Services.Authentication;
using AzureIoT.UWP.Core.Services.Navigation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.UWP.Core.ViewModel
{
    public class LoginViewModel : AppViewModel
    {
        private readonly IAuthenticationService _authenticationService;

        public bool IsLoading { get; set; }
        public bool IsLoginEnabled { get; set; } = true;

        private RelayCommand _signInAsyncCommand;
        public RelayCommand SignInAsyncCommand
        {
            get
            {
                if (_signInAsyncCommand == null)
                {
                    _signInAsyncCommand = new RelayCommand(async () =>
                    {
                        IsLoading = !IsLoading;
                        RaisePropertyChanged(nameof(IsLoading));

                        IsLoginEnabled = !IsLoginEnabled;
                        RaisePropertyChanged(nameof(IsLoginEnabled));

                        await SignInAsync();

                        IsLoading = !IsLoading;
                        RaisePropertyChanged(nameof(IsLoading));

                        IsLoginEnabled = !IsLoginEnabled;
                        RaisePropertyChanged(nameof(IsLoginEnabled));
                    });
                }

                return _signInAsyncCommand;
            }
        }

        public LoginViewModel(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        private async Task SignInAsync()
        {
            var authenticationResult = await _authenticationService.Authenticate();
            if (authenticationResult != null)
            {
                CurrentUserData.Username = authenticationResult.Account.Username;
                CurrentUserData.AccessToken = authenticationResult.AccessToken;
                CurrentUserData.AccessTokenExpiresOn = authenticationResult.ExpiresOn.ToLocalTime();

                System.Diagnostics.Debug.WriteLine($"Name: {authenticationResult.Account.Username}");
                System.Diagnostics.Debug.WriteLine($"Token Expires: {authenticationResult.ExpiresOn.ToLocalTime()}");
                System.Diagnostics.Debug.WriteLine($"Access Token: {authenticationResult.AccessToken}");
                NavigationService.NavigateTo(AppViews.MainView);
            }
        }
    }
}
