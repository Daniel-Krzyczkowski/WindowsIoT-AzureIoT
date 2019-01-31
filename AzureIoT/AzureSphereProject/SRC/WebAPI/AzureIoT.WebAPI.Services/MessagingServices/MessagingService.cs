using AzureIoT.WebAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Devices;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Common.Exceptions;
using AzureIoT.WebAPI.Services.Settings;

namespace AzureIoT.WebAPI.Services.MessagingServices
{
    public class MessagingService : IMessagingService
    {
        private readonly IoTHubSettings _iotHubSettings;
        private readonly ServiceClient _iotHubServiceClient;

        public MessagingService(IoTHubSettings iotHubSettings)
        {
            _iotHubSettings = iotHubSettings;
            _iotHubServiceClient = ServiceClient.CreateFromConnectionString(_iotHubSettings.ServiceClientConnectionString);
        }

        public async Task SendCloudToDeviceMessageAsync(string message)
        {
            try
            {
                var commandMessage = new
                Message(Encoding.ASCII.GetBytes(message));
                await _iotHubServiceClient.SendAsync("d8ee9b135cabb0fb235d8d0f8953a5fe3ec3387905bea841be06d03d1235dd74972045cd0d32790662fa553a41de1772cc401fa4fbd33e47273a19e0861c93df", commandMessage);
            }
            catch (IotHubException ex)
            {
                Console.WriteLine("An error has occurred in SendCloudToDeviceMessageAsync method: " + ex.Message);
            }
        }
    }
}
