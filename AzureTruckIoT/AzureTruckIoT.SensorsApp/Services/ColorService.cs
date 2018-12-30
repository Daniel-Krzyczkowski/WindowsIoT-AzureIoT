using AzureTruckIoT.SensorsApp.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureTruckIoT.SensorsApp.Services
{
    class ColorService
    {
        private readonly TCS34725ColorSensor _colorSensor;

        public ColorService()
        {
            _colorSensor = new TCS34725ColorSensor();
        }

        public async Task InitializeAsync()
        {
            await _colorSensor.InitializeAsync();
        }

        public async Task<string> GetColor()
        {
            string colorRead = await _colorSensor.getClosestColor();
            return colorRead;
        }
    }
}
