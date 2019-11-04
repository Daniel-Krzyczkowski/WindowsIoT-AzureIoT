using AzureIoT.DigitalTwins.Core.Authentication;
using AzureIoT.DigitalTwins.Core.Config;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.Core.ViewModel
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
                var name = authenticationResult.UserInfo.GivenName;
                var accessToken = authenticationResult.AccessToken;
                var accessTokenExpiresOn = authenticationResult.ExpiresOn.ToLocalTime();

                System.Diagnostics.Debug.WriteLine($"Name: {name}");
                System.Diagnostics.Debug.WriteLine($"Token Expires: {accessTokenExpiresOn}");
                System.Diagnostics.Debug.WriteLine($"Access Token: {accessToken}");

                NavigationService.NavigateTo(AppViews.MainView, null);
            }
        }

    }
}
