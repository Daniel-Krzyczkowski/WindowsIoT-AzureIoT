using AzureIoT.UWP.Core.Services.Authentication;
using AzureIoT.UWP.Core.Services.Data;
using AzureIoT.UWP.Core.Services.Data.Contract;
using AzureIoT.UWP.Core.Services.Messaging;
using AzureIoT.UWP.Core.Services.Messaging.Contract;
using AzureIoT.UWP.Core.Services.Rest;
using AzureIoT.UWP.Data;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.UWP.Core.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            RegisterDependencies();
        }

        public LoginViewModel LoginViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<LoginViewModel>();
            }
        }

        public MainViewModel MainViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<MainViewModel>();
            }
        }

        public TemperatureViewModel TemperatureViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<TemperatureViewModel>();
            }
        }

        public HumidityViewModel HumidityViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<HumidityViewModel>();
            }
        }

        public MessagingViewModel MessagingViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<MessagingViewModel>();
            }
        }

        public SettingsViewModel SettingsViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<SettingsViewModel>();
            }
        }

        private void RegisterDependencies()
        {
            SimpleIoc.Default.Register<ISensorDataService<SensorData>, SensorDataService>();
            SimpleIoc.Default.Register<IRestService, RestService>();
            SimpleIoc.Default.Register<IMessagingService, MessagingService>();
            SimpleIoc.Default.Register<IAuthenticationService, AuthenticationService>();

            SimpleIoc.Default.Register<LoginViewModel>(true);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<TemperatureViewModel>();
            SimpleIoc.Default.Register<HumidityViewModel>();
            SimpleIoc.Default.Register<MessagingViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }
    }
}
