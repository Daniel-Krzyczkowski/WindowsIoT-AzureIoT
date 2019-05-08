---
title: "Azure Sphere Device Connected To Azure IoT Central"
---

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere1.PNG?raw=true" alt="Azure Sphere Device Connected To Azure IoT Central"/>
</p>

**TL;DR** 

Some time ago I jumped into Internet of Things topics connected with Microsoft Azure cloud. As a result I created my own series of articles about Azure IoT called **Cloudy of Things**.

You can read them all on this blog.

In this article you can find my latest project in which I connected Azure Sphere IoT device to the Azure IoT Central service to display data from sensors and take up some actions.

[Source code for this project is available here](https://github.com/Daniel-Krzyczkowski/WindowsIoT-AzureIoT/tree/master/AzureIoT/AzureSphereProject/SRC)

---

## Project structure and architecture

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere2.png?raw=true" alt="Solution architecture"/>
</p>

Above diagram presents the whole solution. As you can see it is simple - Azure Sphere IoT device is directly connected to the Azure IoT Central. This is huge advantage because we do not have to create complex architecture.

### Microsoft Azure Sphere

In this project I used Microsoft Azure Sphere Development Board from [Seeed Studio](https://www.seeedstudio.com/Azure-Sphere-MT3620-Development-Kit-EU-Version-p-3134.html)

Temperature/Humidity sensor and OLED screen were took from [Grove Starter Kit for Azure Sphere MT3620 Development Kit](https://www.seeedstudio.com/Grove-Starter-Kit-for-Azure-Sphere-MT3620-Development-Kit-p-3150.html)

### Azure IoT Central connection with Azure Sphere device

There are few prerequisites you have to complete to be able to display data from sensors connected to the Azure Sphere in the Azure IoT Central portal:

1. Create Azure IoT Central application
2. Download the tenant authentication CA certificate
3. Upload the tenant CA certificate to Azure IoT Central and generate a verification code
4. Verify the tenant CA certificate
5. Use the validation certificate to verify the tenant identity

Once steps above are completed you can configure sample application from my repository to work with your Azure Sphere device.
All above steps are well documented by Microsoft under [this](https://docs.microsoft.com/en-us/azure-sphere/app-development/setup-iot-central#step-4-verify-the-tenant-ca-certificate) link.

Once Azure IoT Central application is ready and you versified Azure Sphere tenant with certificate we can proceed with code sample setup.

To configure the sample application, you'll need to supply the following information in the app_manifest.json file for AzureIoT:

The Tenant ID for your Azure Sphere device
The Scope ID for your Azure IoT Central application
The IoT hub URL for your Azure IoT Central application
The Azure DPS global endpoint address
Follow these steps to gather the information and configure the application:

1. Open AzureIoT.sln in Visual Studio.

2. In Solution Explorer, find the app_manifest.json file and open it.

3. In an Azure Sphere Developer Command Prompt, issue the following command to get the tenant ID. Copy the returned value and paste it into the DeviceAuthentication field of the app_manifest.json file:

4. azsphere tenant show-selected

5. In an Azure Sphere Developer Command Prompt, run the ShowIoTCentralConfig.exe program from the sample repository (the ShowIoTCentralConfig program is located in the AzureIoT\Tools folder). Just type: ShowIoTCentralConfig

When prompted, log in with the credentials you use for Azure IoT Central.

Copy the information from the output into the app_manifest.json file in Visual Studio.
In the manifest file you should see that there is new address added to the "AllowedConnections" section that targets your Azure IoT Central application.

### Prepare Azure IoT Central dashboard to display data from humidity and temperature sensors

Once device is connected with the Azure IoT Central we can prepare dashboards to display temperature and humidity values. Below steps present how to do it:

1. Open "Device templates" tab:

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere3.PNG?raw=true" alt="Image not found"/>
</p>

2. Click "New Measurement":

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere4.PNG?raw=true" alt="Image not found"/>
</p>

3. Select "Telemetry":

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere5.PNG?raw=true" alt="Image not found"/>
</p>

4. Provide details about temperature telemetry:

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere6.PNG?raw=true" alt="Image not found"/>
</p>

Now repeat the same above steps for the humidity:

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere7.PNG?raw=true" alt="Image not found"/>
</p>

##### Please note that in the text box called "Field name" you should provide the exact name of the field included in the data send from the device. Below you can see sample application code fragment where I set "Temperature" and "Humidity" values in JSON payload sent to IoT Central:

```C#
if (tempDataSize > 0)
	SendTelemetry("Temperature", tempBuffer);
```

```C#
if (humidityDataSize > 0)
	SendTelemetry("Humidity", humidityBuffer);
```

### Connect device with sensors and run sample application

Once you got your Azure Sphere device prepared, run the sample application I provided. After few minutes you should see data coming to the Azure IoT Central.

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere10.png?raw=true" alt="Image not found"/>
</p>

Now open "Device Explorer" tab and select template for the Azure Sphere device:

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere8.PNG?raw=true" alt="Image not found"/>
</p>

In the "Measuremnts" tab you should be able to see the data coming from the device:

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere9.PNG?raw=true" alt="Image not found"/>
</p>

### Create main dashboard with data

Now once device is connected with Azure IoT Central there is an option to create custom dashboards. Let's see how to do it.

1. In the main dashboard click "+New":

2. Now you can type the name of the new dashboard and select some elements like image and charts:

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere11.PNG?raw=true" alt="Image not found"/>
</p>

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere12.PNG?raw=true" alt="Image not found"/>
</p>

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere13.PNG?raw=true" alt="Image not found"/>
</p>

## Final project presentation

This is my final project:

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere14.PNG?raw=true" alt="Image not found"/>
</p>

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere15.png?raw=true" alt="Image not found"/>
</p>

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere16.png?raw=true" alt="Image not found"/>
</p>

## This is not the end - you can do more!

This is only beginning - now once device is connected and we have some measurements collected it is possible to take some actions! Below for instance I prepared rule for the temperature measurement - if temperature increase I will be notified by e-mail message:

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere18.PNG?raw=true" alt="Image not found"/>
</p>

<p align="center">
<img src="/images/cloudyofthings/article6/assets/IoTCentralWithAzureSphere17.PNG?raw=true" alt="Image not found"/>
</p>

I encourage you to read more about rules and action [here.](https://docs.microsoft.com/en-us/azure/iot-central/tutorial-configure-rules)
