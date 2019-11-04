using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureIoT.DigitalTwins.Core.Config;
using AzureIoT.DigitalTwins.Core.Services;
using AzureIoT.DigitalTwins.Management.Actions;
using GalaSoft.MvvmLight.Command;

namespace AzureIoT.DigitalTwins.Core.ViewModel
{
    public class Level0SpaceManagementViewModel : SpaceManagementViewModel
    {
        private IRoomDetailsService _roomDetailsService;

        public Level0SpaceManagementViewModel(IDigitalTwinsManagementService digitalTwinsManagementService,
            IDialogService dialogService, IRoomDetailsService roomDetailsService) : base(digitalTwinsManagementService, dialogService)
        {
            _roomDetailsService = roomDetailsService;
        }

        protected override async Task GetSpaceDetailsAsync()
        {
            SpaceDetails = string.Empty;
            var spaceDetails = await _digitalTwinsManagementService.GetAvailableAndFreshSpaces();
            var level0spaceDetails = spaceDetails.FirstOrDefault(s => s.Name == "ALT2-L0-BIG");
            if (level0spaceDetails != null)
            {
                if (level0spaceDetails.Values != null)
                {
                    SpaceDetails = level0spaceDetails.Name + "\n" + level0spaceDetails.Values.FirstOrDefault()?.Value;
                }
                    
            }

            var confRoomInformation = await _roomDetailsService.GetConfRoomDetails(RoomNames.Alt0Big);
            if (confRoomInformation != null)
            {
                SpaceDetails = SpaceDetails + "\nNumber of people in the room: " + confRoomInformation.NumberOfPeople;
            }


            if (string.IsNullOrEmpty(SpaceDetails))
            {
                SpaceDetails = "Unfortunately there is no information available for this room.";
            }
        }
    }
}
