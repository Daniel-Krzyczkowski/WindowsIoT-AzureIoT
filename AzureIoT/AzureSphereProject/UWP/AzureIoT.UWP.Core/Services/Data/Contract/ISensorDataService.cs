using AzureIoT.UWP.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.UWP.Core.Services.Data.Contract
{
    public interface ISensorDataService<T> where T : SensorData
    {
        Task<IEnumerable<SensorData>> GetData(SensorDataType dataType);
    }
}
