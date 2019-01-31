using AzureIoT.UWP.Core.Config;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.UWP.Core.Services.Rest
{
    public class RestService : IRestService
    {
        private readonly IRestClient _apiClient;

        public RestService()
        {
            _apiClient = new RestClient(RestServiceConfig.WebServiceUrl);
            SetDefaultHeaders();
        }

        public async Task<TResponse> ExecuteGetRequestAsync<TResponse>(string path, Dictionary<string, string> queryParameters = null)
        {
            var request = new RestRequest(path, Method.GET);

            if (queryParameters != null)
            {
                foreach (var param in queryParameters)
                {
                    request.AddQueryParameter(param.Key, param.Value);
                }
            }

            var response = await _apiClient.ExecuteTaskAsync(request);

            var content = JsonConvert.DeserializeObject<TResponse>(response.Content);

            return content;
        }

        public async Task<TResponse> ExecutePostRequestAsync<TResponse>(string path, object body = null)
        {
            var request = new RestRequest(path, Method.POST);

            if (body != null)
            {
                request.AddHeader("Content-Type", "application/json");
                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(body);
            }

            var response = await _apiClient.ExecuteTaskAsync(request);

            var content = JsonConvert.DeserializeObject<TResponse>(response.Content);

            return content;
        }

        public void SetAuthHeader(string accessToken)
        {
            _apiClient.AddDefaultHeader(HttpRequestHeader.Authorization.ToString(), $"bearer {accessToken}");
        }

        private void SetDefaultHeaders()
        {
            _apiClient.AddDefaultHeader(HttpRequestHeader.Accept.ToString(), "application/json");
        }
    }
}
