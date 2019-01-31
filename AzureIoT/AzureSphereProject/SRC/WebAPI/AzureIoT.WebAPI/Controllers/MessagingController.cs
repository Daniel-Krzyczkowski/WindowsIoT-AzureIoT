using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureIoT.WebAPI.Data;
using AzureIoT.WebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureIoT.WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagingController : ControllerBase
    {
        private readonly IMessagingService _messagingService;

        public MessagingController(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageData messageData)
        {
            if(ModelState.IsValid)
            {
                await _messagingService.SendCloudToDeviceMessageAsync(messageData.Content);
                return Accepted();
            }

            return BadRequest(ModelState);
        }
    }
}