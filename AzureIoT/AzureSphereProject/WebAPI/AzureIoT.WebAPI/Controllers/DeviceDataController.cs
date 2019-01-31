using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureIoT.WebAPI.Data;
using AzureIoT.WebAPI.Services;
using AzureIoT.WebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureIoT.WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceDataController : ControllerBase
    {
        private readonly ISensorDataService<SensorData> _dataService;

        public DeviceDataController(ISensorDataService<SensorData> dataService)
        {
            _dataService = dataService;
        }

        // GET api/temperature
        [HttpGet("temperature")]
        public async Task<IActionResult> GetTemperature()
        {
           var data = await _dataService.GetData(Services.Config.SensorDataType.Temperature);
           return Ok(data);
        }

        // GET api/humidity
        [HttpGet("humidity")]
        public async Task<IActionResult> GetHumidity()
        {
            var data = await _dataService.GetData(Services.Config.SensorDataType.Humidity);
            return Ok(data);
        }
    }
}
