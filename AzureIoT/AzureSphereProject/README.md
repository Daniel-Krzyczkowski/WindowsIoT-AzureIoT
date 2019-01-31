# Micrososft Azure Sphere connected with Azure cloud services

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsAuthor.png?raw=true" alt="Windows IoT Core samples repository"/>
</p>

**TL;DR** 

Some time ago I jumped into Internet of Things topics connected with Microsoft Azure cloud. As a result I created my own series of articles about Azure IoT called **Cloudy of Things**.

You can read them all here:
[Cloudy of Things article series](https://daniel-krzyczkowski.github.io/)

In this article you can find my latest project in which I used Microsoft Azure cloud services, Microsoft Azure Sphere device and Universal Windows Platform for app development.

---

## Project structure and architecture

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsArchitecture.png?raw=true" alt="Solution architecture"/>
</p>

Above diagram presents the whole solution. There is Microsoft Azure Sphere device used with sensors and OLED screen connected. This device communicates with Azure IoT Hub and sends temperature and humidity data in the specific time intervals. Once Azure IoT Hub receives message, Azure Function App is triggered (each time some data is sent to the IoT Hub). Azure Function App is responsible for preparing received data and exporting it to the Azure Table Storage.

Web App (API) was created to provide single gateway for Universal Windows Platform application. This application was created to display temperature and humidity data in the user friendly way and to enable sending messages to the Azure Sphere device easily (to display the message on the OLED screen connected to the Azure Sphere device).

Universal Windows Platform application and Web API application are secured by Azure Active Directory B2C so users have to authenticate first to access them.

Web API is connected with the Azure Application Insights to measure its performance.

Below you can read about implementation details for each architecture component.

### Microsoft Azure Sphere

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/AzureSphere.png?raw=true" alt="Azure Sphere Board"/>
</p>

Microsoft Azure Sphere provides product manufacturers with a chance to create secured, internet-connected devices that can be controlled, updated, monitored and maintained remotely. This is not only the board. This is whole solution for creating highly secured, connected Microcontroller (MCU) devices. You can read more about Azure Sphere specification in my article [here](https://predica.pl/blog/azure-sphere-iot/.)

In this project I used Microsoft Azure Sphere Development Board from [Seeed Studio](https://www.seeedstudio.com/Azure-Sphere-MT3620-Development-Kit-EU-Version-p-3134.html)

Temperature/Humidity sensor and OLED screen were took from [Grove Starter Kit for Azure Sphere MT3620 Development Kit](https://www.seeedstudio.com/Grove-Starter-Kit-for-Azure-Sphere-MT3620-Development-Kit-p-3150.html)

In this repository you can find two projects created for Azure Sphere - written in C (as the native programming language for this type of IoT devices):

* AzureIoT.AzureSphere.TempHumidity
* Azure.IoT.AzureSphere.OLED

Both of them use Azure IoT Hub C SDK to provide bidirectional communication between Azure Sphere device and the cloud. In the first project temperature and humidity values are collected and sent to the Azure IoT Hub. In the second project (OLED) information received from the Azure IoT Hub is displayed on the connected screen.

This is the fagment of main.c file with method to send messages to the cloud using Azure IoT Hub C SDK:

```C
static void SendMessageToIotHub(char * messageToBeSent)
{
	if (connectedToIoTHub) {
		// Send a message
		AzureIoT_SendMessage(messageToBeSent);
		Log_Debug("UPDATE: Successfully sent message to the IoT Hub.\n");
	}
	else {
		Log_Debug("WARNING: Cannot send message: not connected to the IoT Hub.\n");
	}
}
```

Here is the code fragment that contains reading sensor values and sending them using above method:

```C
		GroveTempHumiSHT31_Read(sht31);
		float newTempValue = GroveTempHumiSHT31_GetTemperature(sht31);
		float newHumidityValue = GroveTempHumiSHT31_GetHumidity(sht31);

		char tempAsString[50];
		sprintf(tempAsString, "{\"Temperature\": \"%.1f\", \"Humidity\": \"%.1f\"}", newTempValue, newHumidityValue);

		SendMessageToIotHub(tempAsString);
```

As mentioned it is also possible to display messages from the IoT Hub, here is the code fragment:

```C
		static void MessageReceived(const char *payload)
		{
			Log_Debug("Message received from the Azure IoT Hub: %d\n", payload);

			// Word display
			clearDisplay();
			setNormalDisplay();

			setTextXY(0, 0);  //set Cursor to ith line, 0th column
			setGrayLevel(10); //Set Grayscale level. Any number between 0 - 15.
			putString(payload); //Print Message
		}
```

In the project I used MT3620 Grove Shield Library that enables integration with sensors.


### Microsoft Azure IoT Hub

The Azure IoT Hub provides reliable and secure communication between IoT devices. It also establishes bi-directional communication between each device and the Azure cloud. With Azure IoT Hub you can send messages:

* from a device to the cloud – e.g. temperature values provided by a sensor connected to an IoT device, sent for analysis, and
* from the cloud to a device – e.g. a message with software update payload.

In this project IoT Hub is responsible for communication with Azure Sphere device. This is the gateway for the temperature and humidity data but not only. Messages which should be displayed on the OLED screen are also sent by Azure IoT Hub to the Azure Sphere device.

Below are some helpful links how to start with Azure IoT Hub and how to configure it with Azure Sphere device:

* [Introduction to the Azure IoT Hub](https://docs.microsoft.com/da-dk/azure/iot-hub/about-iot-hub)
* [Azure Sphere with Azure IoT Hub](https://docs.microsoft.com/en-us/azure-sphere/app-development/azure-iot)


### Microsoft Azure Function App

Azure Function App was created to capture data sent to the Azure IoT Hub. In this specific case I used "Event Hub Trigger C#" template which enables capturing data once something is sent to the IoT Hub. 

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsFA1.png?raw=true" alt="Cloudy of Things Azure Function App"/>
</p>

Once data is received by the Function App it is prepared to be stored in the Azure Storage Table. Here is code fragment of that function (of course whole source code of this project is available in the repository here):

```C#
public static async Task Run(string myEventHubMessage, ILogger log)
{
    log.LogInformation($"C# Event Hub trigger function processed a message: {myEventHubMessage}");
    var deviceMessage = JsonConvert.DeserializeObject<DeviceMessage>(myEventHubMessage);
    await InsertTemperatureEntity(deviceMessage.Temperature);
    log.LogInformation($"Temperature value saved: {deviceMessage.Temperature}");
    await InsertHumidityEntity(deviceMessage.Humidity);
    log.LogInformation($"Humidity value saved: {deviceMessage.Humidity}");
}
```


### Microsoft Azure Storage Account

I decided to use cheaper option in this case to store sensors data - Azure Table Storage. Of course in the production use when there is huge amount of data, better approach would be to use Azure Stream Analytics together with Azure SQL Datbase or Azure Cosmos DB.

I created two tables, one for temperature data and one for the humidity data. Here is the sample how temperature data is stored:

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsTS1.PNG?raw=true" alt="Cloudy of Things Table Storage"/>
</p>


### Microsoft Azure Web App

I wanted to provide an easy way to cumminicate with Azure Sphere device. In this case I created ASP .NET Core Web API application hosted on Azure Web App. Access to it is secured by Azure Active Directory B2C. API provides endpoints to get temperature and humidity data from the Azure Table Storage and also endpoint responsible for sending data to the Azure IoT Hub (to display message on the OLED screen).

Below is code fragment for "MessagingController" and for the "DeviceDataController":

```C#
    public class DeviceDataController : ControllerBase
    {
        private readonly ISensorDataService<SensorData> _dataService;

        public DeviceDataController(ISensorDataService<SensorData> dataService)
        {
            _dataService = dataService;
        }

        // GET api/temperature
        [HttpGet("temperature")]
        public async Task<IActionResult> GetTemperature()
        {
           var data = await _dataService.GetData(Services.Config.SensorDataType.Temperature);
           return Ok(data);
        }

        // GET api/humidity
        [HttpGet("humidity")]
        public async Task<IActionResult> GetHumidity()
        {
            var data = await _dataService.GetData(Services.Config.SensorDataType.Humidity);
            return Ok(data);
        }
    }
```

```C#
public class MessagingController : ControllerBase
    {
        private readonly IMessagingService _messagingService;

        public MessagingController(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageData messageData)
        {
            if(ModelState.IsValid)
            {
                await _messagingService.SendCloudToDeviceMessageAsync(messageData.Content);
                return Accepted();
            }

            return BadRequest(ModelState);
        }
    }
```

**Important note**

Credentials like connection string to the Azure Iot Hub or Azure Active Directory B2C parameters are stroed in the Azure Key Vault. Here I used Managed Service Identiy which enables authenticaton between Azure Web App and Azure AD to provide access to the KeyVaul without stroing any credentials in the "appsettings.json file".

Read more here: [Tutorial: Use Azure Key Vault with an Azure web app in .NET](https://docs.microsoft.com/en-us/azure/key-vault/tutorial-net-create-vault-azure-web-app)

### Azure Application Insights

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsAI1.PNG?raw=true" alt="Cloudy of Things Application Insights"/>
</p>

Application Insights is an extensible Application Performance Management (APM) service for web developers on multiple platforms. It includes powerful analytics tools to help you diagnose issues and to understand what users actually do with your application. I connected the Azure Application Insights with Azure Web App where Web API is hosted. Now I can monitor performance and review the requests sent to and from the Web API.

[You can read more about Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

### Microsoft Azure Active Directory B2C

Azure Active Directory (Azure AD) is a multi-tenant, cloud-based directory and identity management service. It combines core directory services, application access management, and identity protection into a single solution. Now Azure Active Directory B2C (Business to Customers) is a separate service built on the same technology but not the same in functionality as Azure AD. The main difference is that Azure AD B2C  it is not to be used by single organization and its users. It allows any potential user to sign up with an email or social media provider like Facebook or Google.

Web API and UWP applications are secured by Azure AD B2C in this project.

[You can read more about Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-overview)


### Azure Key Vault

Azure Key Vault is dedicated place to store any vulnerable data like passwords and connection strings.

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsKV1.PNG?raw=true" alt="Cloudy of Things Key Vault"/>
</p>

[You can read more about Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-overview)


### Universal Windows 10 Platform App

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsUWP3.png?raw=true" alt="Cloudy of Things UWP app"/>
</p>

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsUWP2.png?raw=true" alt="Cloudy of Things UWP app"/>
</p>

Universal Windows Platform application was created to display sensors data and to enable sending messages to the Azure Sphere device. As you can see on the screens there is temperature and humidity data visualisation and also place to send text message wchich will be displayed on the OLED screen at the end. Here is the code fragment of "SensorsDataService" which is responsible for pulling temperature and humidity data from the Web API:

```C#
public async Task<IEnumerable<SensorData>> GetData(SensorDataType dataType)
        {
            if (dataType == SensorDataType.Temperature)
            {
               return await _restService.ExecuteGetRequestAsync<IEnumerable<TemperatureData>>(RestServiceConfig.TemperatureEndpoint);
            }

            if (dataType == SensorDataType.Humidity)
            {
               return await _restService.ExecuteGetRequestAsync<IEnumerable<HumidityData>>(RestServiceConfig.HumidityEndpoint);
            }

            throw new NotSupportedException("This type of sensor data is currently not supported.");
        }
```

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsUWP1.png?raw=true" alt="Cloudy of Things UWP app"/>
</p>

UWP application is secured by Azure Active Directory B2C. You have to login first and then access to the Web API is granted. "AuthenticationService" is responsible for the login process. Here is the code fragment:

```C#
        public async Task<AuthenticationResult> Authenticate()
        {
            AuthenticationResult authResult = null;
            IEnumerable<IAccount> accounts = await AppConfiguration.PublicClientApp.GetAccountsAsync();

            try
            {
                IAccount currentUserAccount = GetAccountByPolicy(accounts, AppConfiguration.PolicySignUpSignIn);
                authResult = await AppConfiguration.PublicClientApp.AcquireTokenSilentAsync(AppConfiguration.ApiScopes, currentUserAccount, AppConfiguration.Authority, false);

            }

            catch (MsalUiRequiredException)
            {
                authResult = await InitializeAuthentication(accounts);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Users:{string.Join(",", accounts.Select(u => u.Username))}{Environment.NewLine}Error Acquiring Token:{Environment.NewLine}{ex}");
            }
            return authResult;
        }
```

[I used "Microsoft.Identity.Client" library to implement authentication service](https://www.nuget.org/packages/Microsoft.Identity.Client/)

---

## Final project presentation

In this section I would like to present final effect of my project:

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsFinal1.JPG?raw=true" alt="Cloudy of Things Final Project"/>
</p>

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsUWP3.png?raw=true" alt="Cloudy of Things UWP app"/>
</p>

<p align="center">
<img src="https://github.com/Daniel-Krzyczkowski/CloudyOfThings/blob/master/Assets/CloudyOfThingsFinal2.JPG?raw=true" alt="Cloudy of Things Final Project"/>
</p>
