using AzureTruckIoT.SensorsApp.Sensors;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace AzureTruckIoT.SensorsApp.Services
{
    class LCD1602Service
    {
        private readonly LCD1602Sensor _lCD1602Sensor;

        public LCD1602Service(int RS, int RW, int E, int D0, int D1, int D2, int D3, int D4, int D5, int D6, int D7)
        {
            _lCD1602Sensor = new LCD1602Sensor(RS, RW, E, D0, D1, D2, D3, D4, D5, D6, D7);
        }

        public LCD1602Service(int RS, int RW, int E, int D4, int D5, int D6, int D7)
        {
            _lCD1602Sensor = new LCD1602Sensor(RS, RW, E, D4, D5, D6, D7);
        }

        public async Task InitializeAsync(GpioController gpioController)
        {
            await _lCD1602Sensor.InitializeAsync(gpioController);
        }

        public async Task PrintText(string text)
        {
            await _lCD1602Sensor.Print(text);
        }

        public async Task SetCursor(int column, int row)
        {
            await _lCD1602Sensor.SetCursor(column, row);
        }

        public async Task Clear()
        {
            await _lCD1602Sensor.Clear();
        }
    }
}
