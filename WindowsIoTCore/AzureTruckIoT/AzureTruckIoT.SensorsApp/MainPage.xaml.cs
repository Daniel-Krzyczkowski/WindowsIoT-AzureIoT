using AzureTruckIoT.SensorsApp.Cloud;
using AzureTruckIoT.SensorsApp.Enums;
using AzureTruckIoT.SensorsApp.Model;
using AzureTruckIoT.SensorsApp.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Media.Capture;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace AzureTruckIoT.SensorsApp
{
    public sealed partial class MainPage : Page
    {
        private GpioController _gpioController;
        private CloudDataService _cloudDataService;
        private PressureTempService _pressureTempService;
        private MotionService _motionService;
        private ColorService _colorService;
        private LCD1602Service _lCD1602Service;
        private RGBLedService _rGBLedService;
        private MediaCapture _mediaCapture;
        private PictureService _pictureService;
        private SolidColorBrush _redBrush;
        private SolidColorBrush _greenBrush;
        private SolidColorBrush _blueBrush;

        public MainPage()
        {
            this.InitializeComponent();

            _redBrush = new SolidColorBrush(Colors.Red);
            _greenBrush = new SolidColorBrush(Colors.Green);
            _blueBrush = new SolidColorBrush(Colors.Blue);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            try
            {
                var deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

                if (deviceFamily.Equals("Windows.IoT"))
                {
                    InitializeGpioController();
                    //await InitializeCamera();
                    //await InitializeLCDServiceAsync();
                    //InitializeMotionService();
                    await InitializeColorServiceAsync();
                    await InitializeTempPressureServiceAsync();
                    InitializeRGBLedServiceAsync();
                    GetDetectedParameters();
                }

                await InitializeCloudDataService();
            }
            catch (Exception ex)
            {
                await ShowDialog("Unfortunately an error has occured: " + ex.Message);
            }
        }

        private void InitializeGpioController()
        {
            _gpioController = GpioController.GetDefault();
            if (_gpioController == null)
                return;
            Debug.WriteLine("GpioController initialized");
        }

        private async Task InitializeCloudDataService()
        {
            _cloudDataService = new CloudDataService();
            _cloudDataService.FromCloudMessageEventHandler += HandleFromCloudMessageEvent;
            await _cloudDataService.ReceiveDataFromTheCloud();
        }

        private void InitializeMotionService()
        {
            _motionService = new MotionService();

            if (_gpioController != null)
            {
                _motionService.Initialize(_gpioController);
                _motionService.MotionEventHandler += HandleMotionEvent;
            }
        }

        private async Task InitializeColorServiceAsync()
        {
            _colorService = new ColorService();
            await _colorService.InitializeAsync();
        }

        private async Task InitializeTempPressureServiceAsync()
        {
            _pressureTempService = new PressureTempService();
            await _pressureTempService.InitializeAsync();
        }

        private async Task InitializeLCDServiceAsync()
        {
            _lCD1602Service = new LCD1602Service(18, 23, 24, 5, 6, 13, 26);

            if (_gpioController != null)
            {
                await _lCD1602Service.InitializeAsync(_gpioController);
                await _lCD1602Service.PrintText("Azure Truck!");
            }
        }

        private void InitializeRGBLedServiceAsync()
        {
            _rGBLedService = new RGBLedService();

            if (_gpioController != null)
            {
                _rGBLedService.Initialize(_gpioController);
                //_rGBLedService.RGBColorChangeEventHandler = HandleColorChange;
                SwipeRGBLedColors();
            }
        }

        private async Task InitializeCamera()
        {
            _mediaCapture = new MediaCapture();
            await _mediaCapture.InitializeAsync();
            facePreviewElement.Source = _mediaCapture;
            await _mediaCapture.StartPreviewAsync();
            _pictureService = new PictureService(_mediaCapture);
        }


        /// <summary>
        /// Get detected temperature, pressure, altitude and color
        /// </summary>
        private void GetDetectedParameters()
        {
            TimeSpan period = TimeSpan.FromMinutes(5);

            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                var temperature = await _pressureTempService.GetTemperatureInCelciusDegrees();
                var pressure = await _pressureTempService.GetPressure();
                var altitude = await _pressureTempService.GetAltitude();

                Debug.WriteLine("Temperature: " + temperature);
                Debug.WriteLine("Pressure: " + pressure);
                Debug.WriteLine("Altitude: " + altitude);

                var colorDetected = await _colorService.GetColor();
                Debug.WriteLine("Color: " + colorDetected);

                var toCloudData = new ToCloudMessage
                {
                    DetectedTemperature = temperature,
                    DetectedPressure = pressure,
                    DetectedAltitude = altitude,
                    DetectedColor = colorDetected
                };

                await _cloudDataService.SendDataToTheCloud(toCloudData);

                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    TemperatureGaugeControl.Value = temperature;
                    PressureGaugeControl.Value = pressure;
                    AltitudeGaugeControl.Value = altitude;
                    ColorDetectedTextBlock.Text = colorDetected;

                    var color = (Color)XamlBindingHelper.ConvertValue(typeof(Color), colorDetected);
                    var brush = new SolidColorBrush(color);
                    RgbLedEllipse.Fill = brush;
                });

            }, period);
        }

        private void SwipeRGBLedColors()
        {
            TimeSpan period = TimeSpan.FromSeconds(10);

            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    _rGBLedService.UpdateLEDColor();
                });

            }, period);
        }

        private async void HandleMotionEvent(object sender, MotionEvents args)
        {
            if (args == MotionEvents.MotionDetected)
            {
                Debug.WriteLine("Motion detected");
                await CaptureAndSendImageForAnalysis();
            }

            else
            {
                Debug.WriteLine("No motion detected");
            }
        }

        private void HandleColorChange(object sender, LedStatus ledStatus)
        {
            switch (ledStatus)
            {
                case LedStatus.Red:
                    RgbLedEllipse.Fill = _redBrush;
                    break;
                case LedStatus.Green:
                    RgbLedEllipse.Fill = _greenBrush;
                    break;
                case LedStatus.Blue:
                    RgbLedEllipse.Fill = _blueBrush;
                    break;
            }
        }

        private async void HandleFromCloudMessageEvent(object sender, FromCloudMessage args)
        {
            await _lCD1602Service.Clear();
            await _lCD1602Service.SetCursor(0, 0);
            await _lCD1602Service.PrintText("Identified as:");
            await _lCD1602Service.SetCursor(0, 1);
            await _lCD1602Service.PrintText(args.Message);
            _motionService.MotionEventHandler += HandleMotionEvent;
        }

        private async Task CaptureAndSendImageForAnalysis()
        {
            var image = await _pictureService.CapturePhoto();
            var imageStream = await _pictureService.GetImageStream(image);
            await _cloudDataService.SendImageToAzure(imageStream);
            _motionService.MotionEventHandler -= HandleMotionEvent;
        }

        private async Task ShowDialog(string text)
        {
            var messageDialog = new MessageDialog(text);

            messageDialog.Commands.Add(new UICommand("OK"));

            messageDialog.DefaultCommandIndex = 0;

            messageDialog.CancelCommandIndex = 1;

            await messageDialog.ShowAsync();
        }
    }
}
