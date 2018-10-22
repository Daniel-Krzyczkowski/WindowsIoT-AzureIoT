using Microsoft.ProjectOxford.Face;
using MotionDetector.UWP.Model;
using MotionDetector.UWP.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Gpio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MotionDetector.UWP
{
    public sealed partial class MainPage : Page
    {
        private GpioController gpio;
        private GpioPin sensor;
        private AzureIoTHubService _azureIoTHubService;
        private FaceRecognitionService _faceRecognitionService;
        private MediaCapture _mediaCapture;
        private byte[] _takenImage;

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeComponent();

            InitAzureIoTHub();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            InitGPIO();
            await InitFaceRecognitionService();
        }

        private async Task InitFaceRecognitionService()
        {
            _faceRecognitionService = new FaceRecognitionService();
            _mediaCapture = new MediaCapture();
            await _mediaCapture.InitializeAsync();
            previewElement.Source = _mediaCapture;
            await _mediaCapture.StartPreviewAsync();
        }

        private async Task<string> CaptureAndAnalyzePhoto()
        {
            string analysisResult = string.Empty;

            using (var captureStream = new InMemoryRandomAccessStream())
            {
                await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), captureStream);
                await captureStream.FlushAsync();
                captureStream.Seek(0);

                var readStream = captureStream.AsStreamForRead();
                _takenImage = new byte[readStream.Length];
                await readStream.ReadAsync(_takenImage, 0, _takenImage.Length);

                var image = new BitmapImage();
                captureStream.Seek(0);
                await image.SetSourceAsync(captureStream);

                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    VideoCaptureImage.Source = image;
                });
                try
                {
                    Stream stream = await GetImageStream();
                    var faces = await _faceRecognitionService.FaceServiceClient.DetectAsync(stream, false, false, new FaceAttributeType[2] { FaceAttributeType.Accessories, FaceAttributeType.Gender });
                    if (faces.Length > 0)
                    {
                        textPlaceHolder.Text = "Detected gender: " + faces[0].FaceAttributes?.Gender + " with accessories: " + faces[0].FaceAttributes?.Accessories[0].Type;
                        analysisResult = analysisResult + textPlaceHolder.Text + "\n";

                        var faceRects = faces.Select(face => face.FaceRectangle);
                        var fRectangles = faceRects.ToArray();

                        Rectangle rectangle = new Rectangle();
                        rectangle.StrokeThickness = 2;
                        rectangle.Stroke = new SolidColorBrush(Colors.Red);

                        if (fRectangles.Length > 0)
                        {
                            foreach (var faceRect in fRectangles)
                            {
                                rectangle.SetValue(Canvas.LeftProperty, faceRect.Left);
                                rectangle.SetValue(Canvas.TopProperty, faceRect.Top);
                                rectangle.Width = faceRect.Width;
                                rectangle.Height = faceRect.Height;

                                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                                {
                                    VideoCaptureImageCanvas.Children.Add(rectangle);
                                });
                            }
                        }
                    }

                    stream = await GetImageStream();
                    var recognitionResult = await _faceRecognitionService.VerifyFaceAgainstTraindedGroup("myfamilytest", stream);
                    textPlaceHolder.Text = recognitionResult;
                    analysisResult = analysisResult + recognitionResult;
                    return analysisResult;
                }
                catch (FaceAPIException ex)
                {
                    textPlaceHolder.Text = "Unfortunately error occured: " + ex.Message;
                }

                return analysisResult;
            }
        }

        private async Task<Stream> GetImageStream()
        {
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0));

            writer.WriteBytes(_takenImage);
            await writer.StoreAsync();
            return stream.AsStream();
        }

        private void InitAzureIoTHub()
        {
            _azureIoTHubService = new AzureIoTHubService();
        }

        private void InitGPIO()
        {
            gpio = GpioController.GetDefault();
            if (gpio == null)
                return;
            GpioStatus.Text = "GPIO initialized";

            sensor = gpio.OpenPin(16);
            sensor.SetDriveMode(GpioPinDriveMode.Input);
            sensor.ValueChanged += Sensor_ValueChanged;
        }

        private async void Sensor_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                System.Diagnostics.Debug.WriteLine("MOTION DETECTED");

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                 {
                     textPlaceHolder.Text = "MOTION DETECTED";
                     var analyzisResult = await CaptureAndAnalyzePhoto();
                     await SendImageWithAnalysis(analyzisResult);
                 });
            }

            else
            {
                System.Diagnostics.Debug.WriteLine("NO MOTION DETECTED");
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {
                     textPlaceHolder.Text = "NO MOTION DETECTED";
                 });
            }
        }

        private async Task SendImageWithAnalysis(string analyzisResult)
        {
            var personPicture = await GetImageStream();
            var motionEvent = new MotionEvent
            {
                RoomNumber = 1,
                ImageAnalyzisResult = analyzisResult
            };
            await _azureIoTHubService.SendImageToAzure(personPicture);
            await _azureIoTHubService.SendDataToAzure(motionEvent);
        }
    }
}
