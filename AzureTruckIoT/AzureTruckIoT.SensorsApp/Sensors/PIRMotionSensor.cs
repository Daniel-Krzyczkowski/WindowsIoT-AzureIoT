using AzureTruckIoT.SensorsApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace AzureTruckIoT.SensorsApp.Sensors
{
    class PIRMotionSensor
    {
        private GpioPin sensorDataPin;
        public EventHandler<MotionEvents> MotionEventHandler;
        private MotionEvents _motionEvents;

        public void Initialize(GpioController gpioController)
        {
            sensorDataPin = gpioController.OpenPin(16);
            sensorDataPin.SetDriveMode(GpioPinDriveMode.Input);
            sensorDataPin.ValueChanged += _sensorDataPin_ValueChanged; ;
        }

        private void _sensorDataPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                System.Diagnostics.Debug.WriteLine("MOTION DETECTED");
                _motionEvents = MotionEvents.MotionDetected;
            }

            else
            {
                System.Diagnostics.Debug.WriteLine("NO MOTION DETECTED");
                _motionEvents = MotionEvents.NoMotionDetected;
            }

            MotionEventHandler?.Invoke(this, _motionEvents);
        }
    }
}
