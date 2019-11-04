using AzureIoT.DigitalTwins.Core.Services;
using AzureIoT.DigitalTwins.Management.Actions;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.Core.ViewModel
{
    public abstract class SpaceManagementViewModel : AppViewModel
    {
        protected readonly IDigitalTwinsManagementService _digitalTwinsManagementService;
        protected readonly IDialogService _dialogService;

        public bool IsLoading { get; set; }
        public bool IsGetDetailsEnabled { get; set; } = true;

        private RelayCommand _getSpaceDetailsAsyncCommand;
        public RelayCommand GetSpaceDetailsAsyncCommand
        {
            get
            {
                if (_getSpaceDetailsAsyncCommand == null)
                {
                    _getSpaceDetailsAsyncCommand = new RelayCommand(async () =>
                    {
                        IsGetDetailsEnabled = !IsGetDetailsEnabled;
                        RaisePropertyChanged(nameof(IsGetDetailsEnabled));

                        IsLoading = !IsLoading;
                        RaisePropertyChanged(nameof(IsLoading));

                        await GetSpaceDetailsAsync();

                        IsGetDetailsEnabled = !IsGetDetailsEnabled;
                        RaisePropertyChanged(nameof(IsGetDetailsEnabled));

                        IsLoading = !IsLoading;
                        RaisePropertyChanged(nameof(IsLoading));
                    });
                }

                return _getSpaceDetailsAsyncCommand;
            }
        }

        private string _spaceDetails;

        public string SpaceDetails
        {
            get
            {
                return _spaceDetails;
            }

            set
            {
                _spaceDetails = value;
                RaisePropertyChanged(nameof(SpaceDetails));
            }
        }

        public SpaceManagementViewModel(IDigitalTwinsManagementService digitalTwinsManagementService,
            IDialogService dialogService)
        {
            _digitalTwinsManagementService = digitalTwinsManagementService;
            _dialogService = dialogService;
        }

        protected abstract Task GetSpaceDetailsAsync();
    }
}
