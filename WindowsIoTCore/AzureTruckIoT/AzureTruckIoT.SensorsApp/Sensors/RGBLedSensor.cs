using AzureTruckIoT.SensorsApp.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Xaml.Media;

namespace AzureTruckIoT.SensorsApp.Sensors
{
    class RGBLedSensor
    {
        private const int RPI2_RED_LED_PIN = 5;
        private const int RPI2_GREEN_LED_PIN = 13;
        private const int RPI2_BLUE_LED_PIN = 6;
        private GpioPin redpin;
        private GpioPin greenpin;
        private GpioPin bluepin;
        private LedStatus ledStatus;
        private SolidColorBrush redBrush;
        private SolidColorBrush greenBrush;
        private SolidColorBrush blueBrush;
        public EventHandler<LedStatus> RGBColorChangeEventHandler;

        public RGBLedSensor()
        {
            redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
            greenBrush = new SolidColorBrush(Windows.UI.Colors.Green);
            blueBrush = new SolidColorBrush(Windows.UI.Colors.Blue);
        }

        public void Initialize(GpioController gpioController)
        {
            var deviceModel = GetDeviceModel();
            if (deviceModel == DeviceModel.RaspberryPi)
            {
                redpin = gpioController.OpenPin(RPI2_RED_LED_PIN);
                greenpin = gpioController.OpenPin(RPI2_GREEN_LED_PIN);
                bluepin = gpioController.OpenPin(RPI2_BLUE_LED_PIN);

                redpin.Write(GpioPinValue.High);
                redpin.SetDriveMode(GpioPinDriveMode.Output);
                greenpin.Write(GpioPinValue.High);
                greenpin.SetDriveMode(GpioPinDriveMode.Output);
                bluepin.Write(GpioPinValue.High);
                bluepin.SetDriveMode(GpioPinDriveMode.Output);

                Debug.WriteLine(string.Format(
                "RGB LED data: Red Pin = {0}, Green Pin = {1}, Blue Pin = {2}",
                redpin.PinNumber,
                greenpin.PinNumber,
                bluepin.PinNumber));
            }

            else
            {
                Debug.WriteLine("RGB LED - Please use different configuration for device other than Raspberry Pi");
            }
        }

        public void UpdateLEDColor()
        {
            Debug.Assert(redpin != null && bluepin != null && greenpin != null);

            switch (ledStatus)
            {
                case LedStatus.Red:
                    //turn on red
                    redpin.Write(GpioPinValue.High);
                    bluepin.Write(GpioPinValue.Low);
                    greenpin.Write(GpioPinValue.Low);

                    RGBColorChangeEventHandler?.Invoke(this, LedStatus.Red);
                    ledStatus = LedStatus.Green;
                    break;
                case LedStatus.Green:

                    //turn on green
                    redpin.Write(GpioPinValue.Low);
                    greenpin.Write(GpioPinValue.High);
                    bluepin.Write(GpioPinValue.Low);

                    RGBColorChangeEventHandler?.Invoke(this, LedStatus.Green);
                    ledStatus = LedStatus.Blue;
                    break;
                case LedStatus.Blue:
                    //turn on blue
                    redpin.Write(GpioPinValue.Low);
                    greenpin.Write(GpioPinValue.Low);
                    bluepin.Write(GpioPinValue.High);

                    RGBColorChangeEventHandler?.Invoke(this, LedStatus.Blue);
                    ledStatus = LedStatus.Red;
                    break;
            }
        }

        public void TurnOffLED()
        {
            redpin.Write(GpioPinValue.Low);
            greenpin.Write(GpioPinValue.Low);
            bluepin.Write(GpioPinValue.Low);
        }

        private DeviceModel GetDeviceModel()
        {
            var deviceInfo = new EasClientDeviceInformation();
            if (deviceInfo.SystemProductName.IndexOf("Raspberry", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return DeviceModel.RaspberryPi;
            }
            else if (deviceInfo.SystemProductName.IndexOf("MinnowBoard", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return DeviceModel.MinnowBoardMax;
            }
            else if (deviceInfo.SystemProductName == "SBC")
            {
                return DeviceModel.DragonBoard410;
            }
            else
            {
                return DeviceModel.Unknown;
            }
        }
    }
}
