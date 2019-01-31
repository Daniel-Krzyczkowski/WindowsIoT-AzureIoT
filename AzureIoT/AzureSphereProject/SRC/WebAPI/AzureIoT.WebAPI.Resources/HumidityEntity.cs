using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.WebAPI.Resources
{
    public class HumidityEntity : TableEntity
    {
        public HumidityEntity(string guid, string humidity)
        {
            this.PartitionKey = guid;
            this.RowKey = humidity;
        }

        public HumidityEntity() { }
    }
}
