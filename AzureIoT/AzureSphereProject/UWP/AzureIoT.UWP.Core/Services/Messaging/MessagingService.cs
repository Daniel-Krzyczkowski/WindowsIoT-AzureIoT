using AzureIoT.UWP.Core.Config;
using AzureIoT.UWP.Core.Services.Messaging.Contract;
using AzureIoT.UWP.Core.Services.Rest;
using AzureIoT.UWP.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.UWP.Core.Services.Messaging
{
    public class MessagingService : IMessagingService
    {
        private readonly IRestService _restService;

        public MessagingService(IRestService restService)
        {
            _restService = restService;
        }

        public async Task SendMessage(MessageData messageData)
        {
            await _restService.ExecutePostRequestAsync<RestServiceResponse>(RestServiceConfig.MessagingEndpoint, messageData);
        }
    }
}
