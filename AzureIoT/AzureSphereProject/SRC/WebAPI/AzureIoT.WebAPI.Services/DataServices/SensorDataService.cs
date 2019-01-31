using AutoMapper;
using AzureIoT.WebAPI.Data;
using AzureIoT.WebAPI.Resources;
using AzureIoT.WebAPI.Services.Config;
using AzureIoT.WebAPI.Services.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.WebAPI.Services.DataServices
{
    public class SensorDataService : ISensorDataService<SensorData>
    {
        private readonly ITableStorageService<TemperatureEntity> _temperatureStorageService;
        private readonly ITableStorageService<HumidityEntity> _humidityStorageService;

        public SensorDataService(ITableStorageService<TemperatureEntity> temperatureStorageService,
             ITableStorageService<HumidityEntity> humidityStorageService)
        {
            _temperatureStorageService = temperatureStorageService;
            _humidityStorageService = humidityStorageService;
        }

        public async Task<IEnumerable<SensorData>> GetData(SensorDataType dataType)
        {
            IList<SensorData> sensorsData = new List<SensorData>();

            if (dataType == SensorDataType.Temperature)
            {
                var temperatureEntities = await _temperatureStorageService.GetEntities(10);
                temperatureEntities.ToList().ForEach(e => sensorsData.Add(Mapper.Map<TemperatureData>(e)));
            }

            if (dataType == SensorDataType.Humidity)
            {
                var humidityEntities = await _humidityStorageService.GetEntities(10);
                humidityEntities.ToList().ForEach(e => sensorsData.Add(Mapper.Map<HumidityData>(e)));
            }
            return sensorsData;
        }
    }
}
