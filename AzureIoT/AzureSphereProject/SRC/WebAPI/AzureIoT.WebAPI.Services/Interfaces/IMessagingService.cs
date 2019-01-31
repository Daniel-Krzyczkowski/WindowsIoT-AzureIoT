using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.WebAPI.Services.Interfaces
{
    public interface IMessagingService
    {
        Task SendCloudToDeviceMessageAsync(string message);
    }
}
