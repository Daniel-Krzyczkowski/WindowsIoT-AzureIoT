using AzureIoT.WebAPI.Data;
using AzureIoT.WebAPI.Services.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.WebAPI.Services.Interfaces
{
    public interface ISensorDataService<T> where T : SensorData
    {
        Task<IEnumerable<T>> GetData(SensorDataType dataType);
    }
}
