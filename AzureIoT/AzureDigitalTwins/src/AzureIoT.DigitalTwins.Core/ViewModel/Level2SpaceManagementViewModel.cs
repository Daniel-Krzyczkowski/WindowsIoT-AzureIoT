using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureIoT.DigitalTwins.Core.Services;
using AzureIoT.DigitalTwins.Management.Actions;
using GalaSoft.MvvmLight.Command;

namespace AzureIoT.DigitalTwins.Core.ViewModel
{
    public class Level2SpaceManagementViewModel : SpaceManagementViewModel
    {
        public Level2SpaceManagementViewModel(IDigitalTwinsManagementService digitalTwinsManagementService,
            IDialogService dialogService) : base(digitalTwinsManagementService, dialogService)
        {
        }

        protected override async Task GetSpaceDetailsAsync()
        {
            SpaceDetails = string.Empty;
            var spaceDetails = await _digitalTwinsManagementService.GetAvailableAndFreshSpaces();
            var level2spaceDetails = spaceDetails.FirstOrDefault(s => s.Name == "ALT2-L2-MEDIUM-VIDEO");
            if(level2spaceDetails != null)
            {
                if(level2spaceDetails.Values != null)
                SpaceDetails = level2spaceDetails.Name + "\n" + level2spaceDetails.Values.FirstOrDefault()?.Value;
            }
            else
            {
                SpaceDetails = "Unfortunately there is no information available for this room.";
            }
        }
    }
}
