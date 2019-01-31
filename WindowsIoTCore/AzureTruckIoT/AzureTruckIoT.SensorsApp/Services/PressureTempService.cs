using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureTruckIoT.SensorsApp.Sensors;

namespace AzureTruckIoT.SensorsApp.Services
{
    class PressureTempService
    {
        private readonly BMP280PressureTempSensor _bMP280;
        private const float seaLevelPressure = 1013.25f;

        public PressureTempService()
        {
            _bMP280 = new BMP280PressureTempSensor();
        }

        public async Task InitializeAsync()
        {
            await _bMP280.InitializeAsync();
        }

        public async Task<float> GetTemperatureInCelciusDegrees()
        {
            float temperature = await _bMP280.ReadTemperature();
            return temperature;
        }

        public async Task<float> GetPressure()
        {
            float pressure = await _bMP280.ReadPressure();
            return pressure;
        }

        public async Task<float> GetAltitude()
        {
            float altitude = await _bMP280.ReadAltitude(seaLevelPressure);
            return altitude;
        }
    }
}
