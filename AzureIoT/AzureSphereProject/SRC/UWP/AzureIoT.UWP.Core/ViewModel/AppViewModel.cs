using AzureIoT.UWP.Core.Config;
using AzureIoT.UWP.Core.Services.Navigation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.UWP.Core.ViewModel
{
    public class AppViewModel : ViewModelBase
    {
        public INavigationService NavigationService => SimpleIoc.Default.GetInstance<INavigationService>();

        public void NavigationInvoked(AppViews viewKey)
        {
            NavigationService.NavigateTo(viewKey);
        }

        public void NavigationInvoked(AppViews viewKey, object parameter)
        {
            NavigationService.NavigateTo(viewKey, parameter);
        }
    }
}
