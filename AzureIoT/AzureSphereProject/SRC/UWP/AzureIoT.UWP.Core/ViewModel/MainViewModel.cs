using AzureIoT.UWP.Core.Config;
using AzureIoT.UWP.Core.Services.Data.Contract;
using AzureIoT.UWP.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.UWP.Core.ViewModel
{
    public class MainViewModel : AppViewModel
    {
        private readonly ISensorDataService<SensorData> _sensorDataService;

        public IEnumerable<TemperatureData> TemperatureData { get; set; }

        public IEnumerable<HumidityData> HumidityData { get; set; }

        public MainViewModel(ISensorDataService<SensorData> sensorDataService)
        {
            _sensorDataService = sensorDataService;
        }

        public async Task DownloadSensorsData()
        {
            TemperatureData = (IEnumerable<TemperatureData>)await _sensorDataService.GetData(SensorDataType.Temperature);
            HumidityData = (IEnumerable<HumidityData>)await _sensorDataService.GetData(SensorDataType.Humidity);
        }
    }
}
