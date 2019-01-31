using AzureIoT.WebAPI.Resources;
using AzureIoT.WebAPI.Services.Config;
using AzureIoT.WebAPI.Services.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.WebAPI.Services.StorageServices
{
    public class HumidityTableStorageService : BaseTableStorageService<HumidityEntity>
    {
        protected override string TableName => DataTables.HumidityTable;

        public HumidityTableStorageService(TableStorageSettings tableStorageSettings):base(tableStorageSettings)
        {
        }
    }
}
