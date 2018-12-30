using AzureTruckIoT.SensorsApp.Model;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AzureTruckIoT.SensorsApp.Cloud
{
    class CloudDataService
    {
        private DeviceClient _deviceClient;
        public EventHandler<FromCloudMessage> FromCloudMessageEventHandler;

        public CloudDataService()
        {
            _deviceClient = DeviceClient.CreateFromConnectionString("HostName=azure-truck-iot-hub.azure-devices.net;DeviceId=SensorsIoTDevice;SharedAccessKey=0xOos2n5P0U6CfQtHjY203cD9iNnIq1OwyE0rGII5H8=", TransportType.Mqtt);
        }

        public async Task SendDataToTheCloud(ToCloudMessage toCloudMessage)
        {
            var messageString = JsonConvert.SerializeObject(toCloudMessage);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await _deviceClient.SendEventAsync(message);

            Console.ForegroundColor = ConsoleColor.Green;
            Debug.WriteLine("{0} > Message sent to the cloud: {1}", DateTime.Now, messageString);
            Console.ResetColor();
        }

        public async Task ReceiveDataFromTheCloud()
        {
            while (true)
            {
                Message receivedMessage = await _deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                var jsonMessage = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                Console.ForegroundColor = ConsoleColor.Yellow;
                Debug.WriteLine("{0} > Received message from the cloud: {1}", DateTime.Now, jsonMessage);
                Console.ResetColor();

                if (!string.IsNullOrEmpty(jsonMessage))
                {
                    try
                    {
                        var fromCloudMessage = JsonConvert.DeserializeObject<FromCloudMessage>(jsonMessage);
                        FromCloudMessageEventHandler?.Invoke(this, fromCloudMessage);
                    }
                    catch (JsonException ex)
                    {
                        Debug.WriteLine("An error hac occurred when deserializing message from the cloud: {0}", ex);
                    }
                }
                await _deviceClient.CompleteAsync(receivedMessage);
            }
        }

        public async Task SendImageToAzure(Stream imageStream)
        {
            await _deviceClient.UploadToBlobAsync($"Person.jpg", imageStream);
        }
    }
}
