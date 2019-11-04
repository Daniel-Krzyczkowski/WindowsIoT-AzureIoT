using AzureIoT.DigitalTwins.Core.Services;
using AzureIoT.DigitalTwins.Management.Actions;
using AzureIoT.DigitalTwins.Management.Providers;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.Core.ViewModel
{
    public class SpaceConfigurationViewModel : AppViewModel
    {
        private readonly IDigitalTwinsManagementService _digitalTwinsManagementService;
        private readonly IDialogService _dialogService;

        public bool IsLoading { get; set; }
        public bool IsConfigurationEnabled { get; set; } = false;

        private string _configurationResult;

        public string ConfigurationResult
        {
            get
            {
                return _configurationResult;
            }

            set
            {
                _configurationResult = value;
                RaisePropertyChanged(nameof(ConfigurationResult));
            }
        }

        private RelayCommand _uploadSpaceDefinitionAsyncCommand;
        public RelayCommand UploadSpaceDefinitionAsyncCommand
        {
            get
            {
                if (_uploadSpaceDefinitionAsyncCommand == null)
                {
                    _uploadSpaceDefinitionAsyncCommand = new RelayCommand(async () =>
                    {
                        await UploadSpaceDefinitionAsync();
                    });
                }

                return _uploadSpaceDefinitionAsyncCommand;
            }
        }

        public SpaceConfigurationViewModel(IDigitalTwinsManagementService digitalTwinsManagementService,
            IDialogService dialogService)
        {
            _digitalTwinsManagementService = digitalTwinsManagementService;
            _dialogService = dialogService;
        }

        private async Task UploadSpaceDefinitionAsync()
        {
            await _dialogService.ShowDialogAsync("IMPORTANT", "First you have to uplad YAML space definition, then you will be asked to provide JS file with user defined actions for each matcher.");

            IsLoading = !IsLoading;
            RaisePropertyChanged(nameof(IsLoading));

            var spaceProvisioningResult = await _digitalTwinsManagementService.ProvisionSpace();

            if (spaceProvisioningResult != null)
            {
                ConfigurationResult = JsonConvert.SerializeObject(spaceProvisioningResult);
                IsConfigurationEnabled = !IsConfigurationEnabled;
                RaisePropertyChanged(nameof(IsConfigurationEnabled));
            }

            IsLoading = !IsLoading;
            RaisePropertyChanged(nameof(IsLoading));
        }
    }
}
