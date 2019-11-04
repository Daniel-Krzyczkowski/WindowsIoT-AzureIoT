using AzureIoT.DigitalTwins.Core.Config;
using AzureIoT.DigitalTwins.Core.Navigation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.DigitalTwins.Core.ViewModel
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
