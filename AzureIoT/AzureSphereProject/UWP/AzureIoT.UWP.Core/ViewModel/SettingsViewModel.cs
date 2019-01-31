using AzureIoT.UWP.Core.Config;
using AzureIoT.UWP.Core.Services.Authentication;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.UWP.Core.ViewModel
{
    public class SettingsViewModel : AppViewModel
    {
        private readonly IAuthenticationService _authenticationService;

        public bool IsLoading { get; set; }
        public bool IsLogoutEnabled { get; set; } = true;

        private RelayCommand _signOutAsyncCommand;
        public RelayCommand SignOutAsyncCommand
        {
            get
            {
                if (_signOutAsyncCommand == null)
                {
                    _signOutAsyncCommand = new RelayCommand(async () =>
                    {
                        IsLoading = !IsLoading;
                        RaisePropertyChanged(nameof(IsLoading));

                        IsLogoutEnabled = !IsLogoutEnabled;
                        RaisePropertyChanged(nameof(IsLogoutEnabled));

                        await Logout();

                        IsLoading = !IsLoading;
                        RaisePropertyChanged(nameof(IsLoading));

                        IsLogoutEnabled = !IsLogoutEnabled;
                        RaisePropertyChanged(nameof(IsLogoutEnabled));
                    });
                }

                return _signOutAsyncCommand;
            }
        }

        public SettingsViewModel(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        private async Task Logout()
        {
            await _authenticationService.SignOut();
            NavigationService.SwitchToRootViewAsMainNavigationView();
            NavigationService.NavigateTo(AppViews.LoginView);
            NavigationService.ClearViewStack();
        }
    }
}
