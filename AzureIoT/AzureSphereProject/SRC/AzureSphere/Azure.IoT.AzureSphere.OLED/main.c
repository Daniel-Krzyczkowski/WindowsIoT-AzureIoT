#include <signal.h>
#include <stdbool.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <unistd.h>

// applibs_versions.h defines the API struct versions to use for applibs APIs.
#include "applibs_versions.h"
#include <applibs/log.h>
#include "epoll_timerfd_utilities.h"

#include "mt3620_rdb.h"
#include <applibs/wificonfig.h>

#include "Grove.h"
#include "Sensors/GroveOledDisplay96x96.h"
#include "azure_iot_utilities.h"

// This C application for the MT3620 Reference Development Board (Azure Sphere)
// outputs a string every second to Visual Studio's Device Output window
//
// It uses the API for the following Azure Sphere application libraries:
// - log (messages shown in Visual Studio's Device Output window during debugging)

static volatile sig_atomic_t terminationRequested = false;

// Connectivity state
static bool connectedToIoTHub = false;

// Termination state
static volatile sig_atomic_t terminationRequired = false;

static int epollFd = -1;
static int azureIotDoWorkTimerFd = -1;

/// <summary>
///     Signal handler for termination requests. This handler must be async-signal-safe.
/// </summary>
static void TerminationHandler(int signalNumber)
{
	// Don't use Log_Debug here, as it is not guaranteed to be async signal safe
	terminationRequested = true;
}

/// <summary>
///     IoT Hub connection status callback function.
/// </summary>
/// <param name="connected">'true' when the connection to the IoT Hub is established.</param>
static void IoTHubConnectionStatusChanged(bool connected)
{
	connectedToIoTHub = connected;
}

/// <summary>
///     Show details of the currently connected WiFi network.
/// </summary>
static void DebugPrintCurrentlyConnectedWiFiNetwork(void)
{
	WifiConfig_ConnectedNetwork network;
	int result = WifiConfig_GetCurrentNetwork(&network);
	if (result < 0) {
		Log_Debug("INFO: Not currently connected to a WiFi network.\n");
	}
	else {
		Log_Debug("INFO: Currently connected WiFi network: \n");
		Log_Debug("INFO: SSID \"%.*s\", BSSID %02x:%02x:%02x:%02x:%02x:%02x, Frequency %dMHz.\n",
			network.ssidLength, network.ssid, network.bssid[0], network.bssid[1],
			network.bssid[2], network.bssid[3], network.bssid[4], network.bssid[5],
			network.frequencyMHz);
	}
}

/// <summary>
///     Hand over control periodically to the Azure IoT SDK's DoWork.
/// </summary>
static void AzureIotDoWorkHandler(event_data_t *eventData)
{
	if (ConsumeTimerFdEvent(azureIotDoWorkTimerFd) != 0) {
		terminationRequired = true;
		return;
	}

	// Set up the connection to the IoT Hub client.
	// Notes it is safe to call this function even if the client has already been set up, as in
	//   this case it would have no effect
	if (AzureIoT_SetupClient()) {
		// AzureIoT_DoPeriodicTasks() needs to be called frequently in order to keep active
		// the flow of data with the Azure IoT Hub
		AzureIoT_DoPeriodicTasks();
	}
}

/// <summary>
///     MessageReceived callback function, called when a message is received from the Azure IoT Hub.
/// </summary>
/// <param name="payload">The payload of the received message.</param>
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

// event handler data structures. Only the event handler field needs to be populated.
static event_data_t azureIotEventData = { .eventHandler = &AzureIotDoWorkHandler };


/// <summary>
///     Initialize peripherals, termination handler, and Azure IoT
/// </summary>
/// <returns>0 on success, or -1 on failure</returns>
static int InitPeripheralsAndHandlers(void)
{
	// Initialize the Azure IoT SDK
	if (!AzureIoT_Initialize()) {
		Log_Debug("ERROR: Cannot initialize Azure IoT Hub SDK.\n");
		return -1;
	}

	// Set the Azure IoT hub related callbacks
	AzureIoT_SetConnectionStatusCallback(&IoTHubConnectionStatusChanged);
	AzureIoT_SetMessageReceivedCallback(&MessageReceived);

	// Display the currently connected WiFi connection.
	DebugPrintCurrentlyConnectedWiFiNetwork();

	epollFd = CreateEpollFd();
	if (epollFd < 0) {
		return -1;
	}

	// Set up a timer for Azure IoT SDK DoWork execution.
	static struct timespec azureIotDoWorkPeriod = { 1, 0 };
	azureIotDoWorkTimerFd =
		CreateTimerFdAndAddToEpoll(epollFd, &azureIotDoWorkPeriod, &azureIotEventData, EPOLLIN);
	if (azureIotDoWorkTimerFd < 0) {
		return -1;
	}

	return 0;
}

static void ClosePeripheralsAndHandlers(void)
{
	Log_Debug("INFO: Closing GPIOs and Azure IoT client.\n");

	// Close all file descriptors
	CloseFdAndPrintError(azureIotDoWorkTimerFd, "IotDoWorkTimer");
	CloseFdAndPrintError(epollFd, "Epoll");

	// Destroy the IoT Hub client
	AzureIoT_DestroyClient();
	AzureIoT_Deinitialize();
}


void i2cScanner(int i2cFd)
{
	uint8_t i2cState;
	const struct timespec sleepTime = { 0, 10000000 };

	for (uint8_t addr = 1; addr < 127; addr++)
	{
		GroveI2C_WriteBytes(i2cFd, (uint8_t)(addr << 1), NULL, 0);
		SC18IM700_ReadReg(i2cFd, 0x0A, &i2cState);
		if (i2cState == I2C_OK)
		{
			Log_Debug("I2C_OK, address detect: 0x%02X\r\n", addr);
		}

		nanosleep(&sleepTime, NULL);
	}
}

/// <summary>
///     Main entry point for this sample.
/// </summary>
int main(int argc, char *argv[])
{
	Log_Debug("Application starting\n");

	// Register a SIGTERM handler for termination requests
	struct sigaction action;
	memset(&action, 0, sizeof(struct sigaction));
	action.sa_handler = TerminationHandler;
	sigaction(SIGTERM, &action, NULL);

	int i2cFd;
	GroveShield_Initialize(&i2cFd, 230400);

	/** Initialize OLED */
	GroveOledDisplay_Init(i2cFd, SH1107G);

	int initResult = InitPeripheralsAndHandlers();
	if (initResult != 0) {
		terminationRequired = true;
	}

	Log_Debug("Azure Sphere application is starting...\n");

	while (!terminationRequested)
	{
		if (WaitForEventAndCallHandler(epollFd) != 0)
		{
			terminationRequired = true;
		}
	}

	ClosePeripheralsAndHandlers();
	Log_Debug("Application exiting\n");
	return 0;
}