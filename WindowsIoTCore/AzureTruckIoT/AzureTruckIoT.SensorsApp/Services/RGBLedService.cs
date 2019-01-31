using AzureTruckIoT.SensorsApp.Enums;
using AzureTruckIoT.SensorsApp.Sensors;
using System;
using Windows.Devices.Gpio;

namespace AzureTruckIoT.SensorsApp.Services
{
    class RGBLedService
    {
        private readonly RGBLedSensor _rGBLedSensor;
        public EventHandler<LedStatus> RGBColorChangeEventHandler;

        public RGBLedService()
        {
            _rGBLedSensor = new RGBLedSensor();
        }

        public void Initialize(GpioController gpioController)
        {
            _rGBLedSensor.Initialize(gpioController);
            _rGBLedSensor.RGBColorChangeEventHandler = HandleColorChange;
        }

        private void HandleColorChange(object sender, LedStatus args)
        {
            RGBColorChangeEventHandler?.Invoke(sender, args);
        }

        public void UpdateLEDColor()
        {
            _rGBLedSensor.UpdateLEDColor();
        }
    }
}
