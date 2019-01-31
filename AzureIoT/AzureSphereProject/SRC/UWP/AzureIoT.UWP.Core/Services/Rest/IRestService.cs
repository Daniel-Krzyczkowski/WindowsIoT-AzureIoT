using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.UWP.Core.Services.Rest
{
    public interface IRestService
    {
        void SetAuthHeader(string accessToken);
        Task<TResponse> ExecutePostRequestAsync<TResponse>(string path, object body = null);
        Task<TGetModel> ExecuteGetRequestAsync<TGetModel>(string path, Dictionary<string, string> queryParameters = null);
    }
}
