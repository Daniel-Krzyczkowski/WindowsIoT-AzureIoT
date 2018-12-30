using AzureTruckIoT.SensorsApp.Enums;
using AzureTruckIoT.SensorsApp.Sensors;
using System;
using Windows.Devices.Gpio;

namespace AzureTruckIoT.SensorsApp.Services
{
    class MotionService
    {
        private readonly PIRMotionSensor _motionSensor;
        public EventHandler<MotionEvents> MotionEventHandler;

        public MotionService()
        {
            _motionSensor = new PIRMotionSensor();
        }

        public void Initialize(GpioController gpioController)
        {
            _motionSensor.Initialize(gpioController);
            _motionSensor.MotionEventHandler += HandleMotionEvent;
        }

        private void HandleMotionEvent(object sender, MotionEvents args)
        {
            MotionEventHandler?.Invoke(sender, args);
        }
    }
}
