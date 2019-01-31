using AzureIoT.UWP.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.UWP.Core.Services.Messaging.Contract
{
    public interface IMessagingService
    {
        Task SendMessage(MessageData messageData);
    }
}
