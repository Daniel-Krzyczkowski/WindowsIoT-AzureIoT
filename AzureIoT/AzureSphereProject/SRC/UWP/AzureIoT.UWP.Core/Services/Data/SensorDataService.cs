using AzureIoT.UWP.Core.Config;
using AzureIoT.UWP.Core.Services.Data.Contract;
using AzureIoT.UWP.Core.Services.Rest;
using AzureIoT.UWP.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.UWP.Core.Services.Data
{
    public class SensorDataService : ISensorDataService<SensorData>
    {
        private readonly IRestService _restService;
        public SensorDataService(IRestService restService)
        {
            _restService = restService;
            _restService.SetAuthHeader(CurrentUserData.AccessToken);
        }

        public async Task<IEnumerable<SensorData>> GetData(SensorDataType dataType)
        {
            if (dataType == SensorDataType.Temperature)
            {
               return await _restService.ExecuteGetRequestAsync<IEnumerable<TemperatureData>>(RestServiceConfig.TemperatureEndpoint);
            }

            if (dataType == SensorDataType.Humidity)
            {
               return await _restService.ExecuteGetRequestAsync<IEnumerable<HumidityData>>(RestServiceConfig.HumidityEndpoint);
            }

            throw new NotSupportedException("This type of sensor data is currently not supported.");
        }
    }
}
