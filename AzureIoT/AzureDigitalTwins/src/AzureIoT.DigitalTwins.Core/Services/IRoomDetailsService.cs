using AzureIoT.DigitalTwins.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.Core.Services
{
    public interface IRoomDetailsService
    {
        Task<ConfRoomInformation> GetConfRoomDetails(string roomName);
    }
}
