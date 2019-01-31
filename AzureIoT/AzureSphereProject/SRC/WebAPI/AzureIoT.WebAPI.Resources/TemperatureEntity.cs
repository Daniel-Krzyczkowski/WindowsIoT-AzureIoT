using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.WebAPI.Resources
{
    public class TemperatureEntity : TableEntity
    {
        public TemperatureEntity(string guid, string temperature)
        {
            this.PartitionKey = guid;
            this.RowKey = temperature;
        }

        public TemperatureEntity() { }
    }

}
