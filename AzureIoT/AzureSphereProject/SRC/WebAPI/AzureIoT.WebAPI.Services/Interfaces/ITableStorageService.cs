using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.WebAPI.Services.Interfaces
{
    public interface ITableStorageService<T> where T : ITableEntity, new()
    {
        Task<IEnumerable<T>> GetEntities(int takeCount);
    }
}
