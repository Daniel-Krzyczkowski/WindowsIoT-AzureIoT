using AzureIoT.DigitalTwins.Core.Authentication;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.DigitalTwins.Core.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
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

        public SpaceConfigurationViewModel SpaceConfigurationViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<SpaceConfigurationViewModel>();
            }
        }

        public Level0SpaceManagementViewModel Level0SpaceManagementViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<Level0SpaceManagementViewModel>();
            }
        }

        public Level2SpaceManagementViewModel Level2SpaceManagementViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<Level2SpaceManagementViewModel>();
            }
        }

        public SettingsViewModel SettingsViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<SettingsViewModel>();
            }
        }
    }
}
