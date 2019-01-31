using AzureIoT.WebAPI.Services.Extensions;
using AzureIoT.WebAPI.Services.Interfaces;
using AzureIoT.WebAPI.Services.Settings;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.WebAPI.Services.StorageServices
{
    public abstract class BaseTableStorageService<T> : ITableStorageService<T> where T : ITableEntity, new()
    {
        private readonly TableStorageSettings _tableStorageSettings;
        private readonly CloudTableClient _tableClient;

        protected abstract string TableName
        {
            get;
        }

        public BaseTableStorageService(TableStorageSettings tableStorageSettings)
        {
            _tableStorageSettings = tableStorageSettings;
            var cloudStorageAccount = CloudStorageAccount.Parse(_tableStorageSettings.ConnectionString);
            _tableClient = cloudStorageAccount.CreateCloudTableClient();
        }

        public async Task<IEnumerable<T>> GetEntities(int takeCount)
        {
            CloudTable table = _tableClient.GetTableReference(TableName);

            TableQuery<T> query = new TableQuery<T>().Take(takeCount);
            IList<T> tableQueryResult = await table.ExecuteQueryAsync(query);

            return tableQueryResult;
        }
    }
}
