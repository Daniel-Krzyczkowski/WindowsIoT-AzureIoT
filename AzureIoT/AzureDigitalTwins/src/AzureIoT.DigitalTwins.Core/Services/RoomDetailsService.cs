using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AzureIoT.DigitalTwins.Core.Config;
using AzureIoT.DigitalTwins.Core.Model;
using Newtonsoft.Json;

namespace AzureIoT.DigitalTwins.Core.Services
{
    public class RoomDetailsService : IRoomDetailsService
    {
        private HttpClient _httpClient;

        public RoomDetailsService()
        {
            _httpClient = new HttpClient() { BaseAddress = new Uri(AppConfiguration.RoomDetailsApiEndpoint) };
        }

        public async Task<ConfRoomInformation> GetConfRoomDetails(string roomName)
        {
            SetupHttpClient();
            HttpResponseMessage response = await _httpClient.GetAsync(roomName);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var confRoomInformation = JsonConvert.DeserializeObject<ConfRoomInformation>(jsonResponse);
                return confRoomInformation;
            }

            return null;
        }

        private void SetupHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Remove("x-functions-key");
            _httpClient.DefaultRequestHeaders.Add("x-functions-key", AppConfiguration.RoomDetailsApiKey);
        }
    }
}
