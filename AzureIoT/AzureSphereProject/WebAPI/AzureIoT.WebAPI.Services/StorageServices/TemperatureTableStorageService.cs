using AzureIoT.WebAPI.Resources;
using AzureIoT.WebAPI.Services.Config;
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
    public class TemperatureTableStorageService : BaseTableStorageService<TemperatureEntity>
    {
        protected override string TableName => DataTables.TemperatureTable;

        public TemperatureTableStorageService(TableStorageSettings tableStorageSettings) : base(tableStorageSettings)
        {
        }
    }
}
