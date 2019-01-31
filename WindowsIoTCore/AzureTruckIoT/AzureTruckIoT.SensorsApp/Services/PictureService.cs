using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace AzureTruckIoT.SensorsApp.Services
{
    class PictureService
    {
        private MediaCapture _mediaCapture;

        public PictureService(MediaCapture mediaCapture)
        {
            _mediaCapture = mediaCapture;
        }

        public async Task<byte[]> CapturePhoto()
        {
            using (var captureStream = new InMemoryRandomAccessStream())
            {
                await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), captureStream);
                await captureStream.FlushAsync();
                captureStream.Seek(0);

                var readStream = captureStream.AsStreamForRead();
                var imageInBytes = new byte[readStream.Length];
                await readStream.ReadAsync(imageInBytes, 0, imageInBytes.Length);
                return imageInBytes;
            }
        }

        public async Task<Stream> GetImageStream(byte[] imageInBytes)
        {
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0));

            writer.WriteBytes(imageInBytes);
            await writer.StoreAsync();
            return stream.AsStream();
        }
    }
}
