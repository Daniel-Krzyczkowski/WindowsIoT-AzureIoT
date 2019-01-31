using System;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

static CloudTableClient tableClient = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TableStorageConnectionString", EnvironmentVariableTarget.Process)).CreateCloudTableClient();

public static async Task Run(string myEventHubMessage, ILogger log)
{
    log.LogInformation($"C# Event Hub trigger function processed a message: {myEventHubMessage}");
    var deviceMessage = JsonConvert.DeserializeObject<DeviceMessage>(myEventHubMessage);
    await InsertTemperatureEntity(deviceMessage.Temperature);
    log.LogInformation($"Temperature value saved: {deviceMessage.Temperature}");
    await InsertHumidityEntity(deviceMessage.Humidity);
    log.LogInformation($"Humidity value saved: {deviceMessage.Humidity}");
}

public class TemperatureEntity : TableEntity
{
        public TemperatureEntity(string guid, string temperature)
        {
            this.PartitionKey = guid;
            this.RowKey = temperature;
        }

        public TemperatureEntity() { }
}

public class HumidityEntity : TableEntity
{
        public HumidityEntity(string guid, string humidity)
        {
            this.PartitionKey = guid;
            this.RowKey = humidity;
        }

        public HumidityEntity() { }
}

static async Task InsertTemperatureEntity(string temperature)
{
            CloudTable temperatureValuesTable = tableClient.GetTableReference("Temperature");
            await temperatureValuesTable.CreateIfNotExistsAsync();
            TemperatureEntity temperatureEntity = new TemperatureEntity(Guid.NewGuid().ToString(), temperature);

            TableOperation insertOperation = TableOperation.Insert(temperatureEntity);
            await temperatureValuesTable.ExecuteAsync(insertOperation);
}

static async Task InsertHumidityEntity(string humidity)
{
            CloudTable humidityValuesTable = tableClient.GetTableReference("Humidity");
            await humidityValuesTable.CreateIfNotExistsAsync();
            HumidityEntity humidityEntity = new HumidityEntity(Guid.NewGuid().ToString(), humidity);

            TableOperation insertOperation = TableOperation.Insert(humidityEntity);
            await humidityValuesTable.ExecuteAsync(insertOperation);
}

public class DeviceMessage
{
  public string Temperature {get; set;}
  public string Humidity {get; set;}
}