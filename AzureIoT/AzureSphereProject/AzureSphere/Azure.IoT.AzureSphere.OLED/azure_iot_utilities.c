#include <stdlib.h>
#include <stdio.h>
#include <stdarg.h>
#include <time.h>
#include <iothub_client_core_common.h>
#include <iothub_device_client_ll.h>
#include <iothub_client_options.h>
#include <iothubtransportmqtt.h>
#include <iothub.h>
#include <applibs/log.h>
#include <azure_sphere_provisioning.h>
#include "azure_iot_utilities.h"

// Refer to https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-device-sdk-c-intro for more
// information on Azure IoT SDK for C

//
// String containing the scope id of the Device Provisioning Service
// used to provision the app with the IoT hub hostname and the device id.
//
static const char scopeId[] = "0ne000405B7";

/// <summary>
///     Function invoked to provide the result of the Device Twin reported properties
///     delivery.
/// </summary>
static DeviceTwinDeliveryConfirmationFnType deviceTwinConfirmationCb = 0;

/// <summary>
///     Function invoked whenever a Direct Method call is received from the IoT Hub.
/// </summary>
static DirectMethodCallFnType directMethodCallCb = 0;

/// <summary>
///     Function invoked whenever a Device Twin update is received from the IoT Hub.
/// </summary>
static TwinUpdateFnType twinUpdateCb = 0;

/// <summary>
///     Function invoked whenever the connection status to the IoT Hub changes.
/// </summary>
static ConnectionStatusFnType hubConnectionStatusCb = 0;

/// <summary>
///     Function invoked whenever a message is received from the IoT Hub.
/// </summary>
static MessageReceivedFnType messageReceivedCb = 0;

/// <summary>
///     Function invoked to report the delivery confirmation of a message sent to the IoT
///     Hub.
/// </summary>
static MessageDeliveryConfirmationFnType messageDeliveryConfirmationCb = 0;

/// <summary>
///     The handle to the IoT Hub client used for communication with the hub.
/// </summary>
static IOTHUB_DEVICE_CLIENT_LL_HANDLE iothubClientHandle = NULL;

/// <summary>
///     The status of the authentication to the hub. When 'false' a
///     connection and authentication to the hub will be attempted.
/// </summary>
static bool iothubAuthenticated = false;

/// <summary>
///     Used to set the keepalive period over MQTT to 20 seconds.
/// </summary>
static int keepalivePeriodSeconds = 20;

/// <summary>
///     Set of bundle of root certificate authorities.
/// </summary>
static const char azureIoTCertificatesX[] =
    /* Baltimore */
    "-----BEGIN CERTIFICATE-----\r\n"
    "MIIDdzCCAl+gAwIBAgIEAgAAuTANBgkqhkiG9w0BAQUFADBaMQswCQYDVQQGEwJJ\r\n"
    "RTESMBAGA1UEChMJQmFsdGltb3JlMRMwEQYDVQQLEwpDeWJlclRydXN0MSIwIAYD\r\n"
    "VQQDExlCYWx0aW1vcmUgQ3liZXJUcnVzdCBSb290MB4XDTAwMDUxMjE4NDYwMFoX\r\n"
    "DTI1MDUxMjIzNTkwMFowWjELMAkGA1UEBhMCSUUxEjAQBgNVBAoTCUJhbHRpbW9y\r\n"
    "ZTETMBEGA1UECxMKQ3liZXJUcnVzdDEiMCAGA1UEAxMZQmFsdGltb3JlIEN5YmVy\r\n"
    "VHJ1c3QgUm9vdDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKMEuyKr\r\n"
    "mD1X6CZymrV51Cni4eiVgLGw41uOKymaZN+hXe2wCQVt2yguzmKiYv60iNoS6zjr\r\n"
    "IZ3AQSsBUnuId9Mcj8e6uYi1agnnc+gRQKfRzMpijS3ljwumUNKoUMMo6vWrJYeK\r\n"
    "mpYcqWe4PwzV9/lSEy/CG9VwcPCPwBLKBsua4dnKM3p31vjsufFoREJIE9LAwqSu\r\n"
    "XmD+tqYF/LTdB1kC1FkYmGP1pWPgkAx9XbIGevOF6uvUA65ehD5f/xXtabz5OTZy\r\n"
    "dc93Uk3zyZAsuT3lySNTPx8kmCFcB5kpvcY67Oduhjprl3RjM71oGDHweI12v/ye\r\n"
    "jl0qhqdNkNwnGjkCAwEAAaNFMEMwHQYDVR0OBBYEFOWdWTCCR1jMrPoIVDaGezq1\r\n"
    "BE3wMBIGA1UdEwEB/wQIMAYBAf8CAQMwDgYDVR0PAQH/BAQDAgEGMA0GCSqGSIb3\r\n"
    "DQEBBQUAA4IBAQCFDF2O5G9RaEIFoN27TyclhAO992T9Ldcw46QQF+vaKSm2eT92\r\n"
    "9hkTI7gQCvlYpNRhcL0EYWoSihfVCr3FvDB81ukMJY2GQE/szKN+OMY3EU/t3Wgx\r\n"
    "jkzSswF07r51XgdIGn9w/xZchMB5hbgF/X++ZRGjD8ACtPhSNzkE1akxehi/oCr0\r\n"
    "Epn3o0WC4zxe9Z2etciefC7IpJ5OCBRLbf1wbWsaY71k5h+3zvDyny67G7fyUIhz\r\n"
    "ksLi4xaNmjICq44Y3ekQEe5+NauQrz4wlHrQMz2nZQ/1/I6eYs9HRCwBXbsdtTLS\r\n"
    "R9I4LtD+gdwyah617jzV/OeBHRnDJELqYzmp\r\n"
    "-----END CERTIFICATE-----\r\n"
    /* MSIT */
    "-----BEGIN CERTIFICATE-----\r\n"
    "MIIFhjCCBG6gAwIBAgIEByeaqTANBgkqhkiG9w0BAQsFADBaMQswCQYDVQQGEwJJ\r\n"
    "RTESMBAGA1UEChMJQmFsdGltb3JlMRMwEQYDVQQLEwpDeWJlclRydXN0MSIwIAYD\r\n"
    "VQQDExlCYWx0aW1vcmUgQ3liZXJUcnVzdCBSb290MB4XDTEzMTIxOTIwMDczMloX\r\n"
    "DTE3MTIxOTIwMDY1NVowgYsxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5n\r\n"
    "dG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9y\r\n"
    "YXRpb24xFTATBgNVBAsTDE1pY3Jvc29mdCBJVDEeMBwGA1UEAxMVTWljcm9zb2Z0\r\n"
    "IElUIFNTTCBTSEEyMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA0eg3\r\n"
    "p3aKcEsZ8CA3CSQ3f+r7eOYFumqtTicN/HJq2WwhxGQRlXMQClwle4hslAT9x9uu\r\n"
    "e9xKCLM+FvHQrdswbdcaHlK1PfBHGQPifaa9VxM/VOo6o7F3/ELwY0lqkYAuMEnA\r\n"
    "iusrr/466wddBvfp/YQOkb0JICnobl0JzhXT5+/bUOtE7xhXqwQdvDH593sqE8/R\r\n"
    "PVGvG8W1e+ew/FO7mudj3kEztkckaV24Rqf/ravfT3p4JSchJjTKAm43UfDtWBpg\r\n"
    "lPbEk9jdMCQl1xzrGZQ1XZOyrqopg3PEdFkFUmed2mdROQU6NuryHnYrFK7sPfkU\r\n"
    "mYsHbrznDFberL6u23UykJ5jvXS/4ArK+DSWZ4TN0UI4eMeZtgzOtg/pG8v0Wb4R\r\n"
    "DsssMsj6gylkeTyLS/AydGzzk7iWa11XWmjBzAx5ihne9UkCXgiAAYkMMs3S1pbV\r\n"
    "S6Dz7L+r9H2zobl82k7X5besufIlXwHLjJaoKK7BM1r2PwiQ3Ov/OdgmyBKdHJqq\r\n"
    "qcAWjobtZ1KWAH8Nkj092XA25epCbx+uleVbXfjQOsfU3neG0PyeTuLiuKloNwnE\r\n"
    "OeOFuInzH263bR9KLxgJb95KAY8Uybem7qdjnzOkVHxCg2i4pd+/7LkaXRM72a1o\r\n"
    "/SAKVZEhZPnXEwGgCF1ZiRtEr6SsxwUQ+kFKqPsCAwEAAaOCASAwggEcMBIGA1Ud\r\n"
    "EwEB/wQIMAYBAf8CAQAwUwYDVR0gBEwwSjBIBgkrBgEEAbE+AQAwOzA5BggrBgEF\r\n"
    "BQcCARYtaHR0cDovL2N5YmVydHJ1c3Qub21uaXJvb3QuY29tL3JlcG9zaXRvcnku\r\n"
    "Y2ZtMA4GA1UdDwEB/wQEAwIBhjAdBgNVHSUEFjAUBggrBgEFBQcDAQYIKwYBBQUH\r\n"
    "AwIwHwYDVR0jBBgwFoAU5Z1ZMIJHWMys+ghUNoZ7OrUETfAwQgYDVR0fBDswOTA3\r\n"
    "oDWgM4YxaHR0cDovL2NkcDEucHVibGljLXRydXN0LmNvbS9DUkwvT21uaXJvb3Qy\r\n"
    "MDI1LmNybDAdBgNVHQ4EFgQUUa8kJpz0aCJXgCYrO0ZiFXsezKUwDQYJKoZIhvcN\r\n"
    "AQELBQADggEBAHaFxSMxH7Rz6qC8pe3fRUNqf2kgG4Cy+xzdqn+I0zFBNvf7+2ut\r\n"
    "mIx4H50RZzrNS+yovJ0VGcQ7C6eTzuj8nVvoH8tWrnZDK8cTUXdBqGZMX6fR16p1\r\n"
    "xRspTMn0baFeoYWTFsLLO6sUfUT92iUphir+YyDK0gvCNBW7r1t/iuCq7UWm6nnb\r\n"
    "2DVmVEPeNzPR5ODNV8pxsH3pFndk6FmXudUu0bSR2ndx80oPSNI0mWCVN6wfAc0Q\r\n"
    "negqpSDHUJuzbEl4K1iSZIm4lTaoNKrwQdKVWiRUl01uBcSVrcR6ozn7eQaKm6ZP\r\n"
    "2SL6RE4288kPpjnngLJev7050UblVUfbvG4=\r\n"
    "-----END CERTIFICATE-----\r\n"
    /* *.azure-devices.net */
    "-----BEGIN CERTIFICATE-----\r\n"
    "MIIGcjCCBFqgAwIBAgITWgABtrNbz7vBeV0QWwABAAG2szANBgkqhkiG9w0BAQsF\r\n"
    "ADCBizELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcT\r\n"
    "B1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEVMBMGA1UE\r\n"
    "CxMMTWljcm9zb2Z0IElUMR4wHAYDVQQDExVNaWNyb3NvZnQgSVQgU1NMIFNIQTIw\r\n"
    "HhcNMTUwODI3MDMxODA0WhcNMTcwODI2MDMxODA0WjAeMRwwGgYDVQQDDBMqLmF6\r\n"
    "dXJlLWRldmljZXMubmV0MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA\r\n"
    "nXC/qBUdlnfIm5K3HYu0o/Mb5tNNcsr0xy4Do0Puwq2W1tz0ZHvIIS9VOANhkNCb\r\n"
    "VyOncnP6dvmM/rYYKth/NQ8RUiZOYlROZ0SYC8cvxq9WOln4GXtEU8vNVqJbYrJj\r\n"
    "rPMHfxqLzTE/0ZnQffnDT3iMUE9kFLHow0YgaSRU0KZsc9KAROmzBzu+QIB1WGKX\r\n"
    "D7CN361tG1UuN68Bz7MSnbgk98Z+DjDxfusoDhiiy/Y9MLOJMt4WIy5BqL3lfLnn\r\n"
    "r+JLqmpiFuyVUDacFQDprYJ1/AFgcsKYu/ydmASARPzqJhOGaC2sZP0U5oBOoBzI\r\n"
    "bz4tfn8Bi0kJKmS53mQt+wIDAQABo4ICOTCCAjUwCwYDVR0PBAQDAgSwMB0GA1Ud\r\n"
    "JQQWMBQGCCsGAQUFBwMBBggrBgEFBQcDAjAdBgNVHQ4EFgQUKpYehBSNA53Oxivn\r\n"
    "aLCz3+eFUJ0wXQYDVR0RBFYwVIITKi5henVyZS1kZXZpY2VzLm5ldIIaKi5hbXFw\r\n"
    "d3MuYXp1cmUtZGV2aWNlcy5uZXSCISouc3UubWFuYWdlbWVudC1henVyZS1kZXZp\r\n"
    "Y2VzLm5ldDAfBgNVHSMEGDAWgBRRryQmnPRoIleAJis7RmIVex7MpTB9BgNVHR8E\r\n"
    "djB0MHKgcKBuhjZodHRwOi8vbXNjcmwubWljcm9zb2Z0LmNvbS9wa2kvbXNjb3Jw\r\n"
    "L2NybC9tc2l0d3d3Mi5jcmyGNGh0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kv\r\n"
    "bXNjb3JwL2NybC9tc2l0d3d3Mi5jcmwwcAYIKwYBBQUHAQEEZDBiMDwGCCsGAQUF\r\n"
    "BzAChjBodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL21zY29ycC9tc2l0d3d3\r\n"
    "Mi5jcnQwIgYIKwYBBQUHMAGGFmh0dHA6Ly9vY3NwLm1zb2NzcC5jb20wTgYDVR0g\r\n"
    "BEcwRTBDBgkrBgEEAYI3KgEwNjA0BggrBgEFBQcCARYoaHR0cDovL3d3dy5taWNy\r\n"
    "b3NvZnQuY29tL3BraS9tc2NvcnAvY3BzADAnBgkrBgEEAYI3FQoEGjAYMAoGCCsG\r\n"
    "AQUFBwMBMAoGCCsGAQUFBwMCMA0GCSqGSIb3DQEBCwUAA4ICAQCrjzOSW+X6v+UC\r\n"
    "u+JkYyuypXN14pPLcGFbknJWj6DAyFWXKC8ihIYdtf/szWIO7VooplSTZ05u/JYu\r\n"
    "ZYh7fAw27qih9CLhhfncXi5yzjgLMlD0mlbORvMJR/nMl7Yh1ki9GyLnpOqMmO+E\r\n"
    "yTpOiE07Uyt2uWelLHjMY8kwy2bSRXIp7/+A8qHRaIIdXNtAKIK5jo068BJpo77h\r\n"
    "4PljCb9JFdEt6sAKKuaP86Y+8oRZ7YzU4TLDCiK8P8n/gQXH0vvhOE/O0n7gWPqB\r\n"
    "n8KxsnRicop6tB6GZy32Stn8w0qktmQNXOGU+hp8OL6irULWZw/781po6d78nmwk\r\n"
    "1IFl2TB4+jgyblvJdTM0rx8vPf3F2O2kgsRNs9M5qCI7m+he43Bhue0Fj/h3oIIo\r\n"
    "Qx7X/uqc8j3VTNE9hf2A4wksSRgRydjAYoo+bduNagC5s7Eucb4mBG0MMk7HAQU9\r\n"
    "m/gyaxqth6ygDLK58wojSV0i4RiU01qZkHzqIWv5FhhMjbFwyKEc6U35Ps7kP/1O\r\n"
    "fdGm13ONaYqDl44RyFsLFFiiDYxZFDSsKM0WDxbl9ULAlVc3WR85kEBK6I+pSQj+\r\n"
    "7/Z5z2zTz9qOFWgB15SegTbjSR7uk9mEVnj9KDlGtG8W1or0EGrrEDP2CMsp0oEj\r\n"
    "VTJbZAxEaZ3cVCKva5sQUxFMjwG32g==\r\n"
    "-----END CERTIFICATE-----\r\n";

// Forward declarations.
static void sendMessageCallback(IOTHUB_CLIENT_CONFIRMATION_RESULT result, void *context);
static IOTHUBMESSAGE_DISPOSITION_RESULT receiveMessageCallback(IOTHUB_MESSAGE_HANDLE message,
                                                               void *context);
static void twinCallback(DEVICE_TWIN_UPDATE_STATE updateState, const unsigned char *payLoad,
                         size_t size, void *userContextCallback);
static int directMethodCallback(const char *methodName, const unsigned char *payload, size_t size,
                                unsigned char **response, size_t *response_size,
                                void *userContextCallback);
static void hubConnectionStatusCallback(IOTHUB_CLIENT_CONNECTION_STATUS result,
                                        IOTHUB_CLIENT_CONNECTION_STATUS_REASON reason,
                                        void *userContextCallback);

/// <summary>
///     Converts the IoT Hub connection status reason to a string.
/// </summary>
static const char *getReasonString(IOTHUB_CLIENT_CONNECTION_STATUS_REASON reason)
{
    static char *reasonString = "unknown reason";
    switch (reason) {
    case IOTHUB_CLIENT_CONNECTION_EXPIRED_SAS_TOKEN:
        reasonString = "IOTHUB_CLIENT_CONNECTION_EXPIRED_SAS_TOKEN";
        break;
    case IOTHUB_CLIENT_CONNECTION_DEVICE_DISABLED:
        reasonString = "IOTHUB_CLIENT_CONNECTION_DEVICE_DISABLED";
        break;
    case IOTHUB_CLIENT_CONNECTION_BAD_CREDENTIAL:
        reasonString = "IOTHUB_CLIENT_CONNECTION_BAD_CREDENTIAL";
        break;
    case IOTHUB_CLIENT_CONNECTION_RETRY_EXPIRED:
        reasonString = "IOTHUB_CLIENT_CONNECTION_RETRY_EXPIRED";
        break;
    case IOTHUB_CLIENT_CONNECTION_NO_NETWORK:
        reasonString = "IOTHUB_CLIENT_CONNECTION_NO_NETWORK";
        break;
    case IOTHUB_CLIENT_CONNECTION_COMMUNICATION_ERROR:
        reasonString = "IOTHUB_CLIENT_CONNECTION_COMMUNICATION_ERROR";
        break;
    case IOTHUB_CLIENT_CONNECTION_OK:
        reasonString = "IOTHUB_CLIENT_CONNECTION_OK";
        break;
    }
    return reasonString;
}

/// <summary>
///     Log a message related to the Azure IoT Hub client with
///     prefix [Azure IoT]:"
/// </summary>
/// <param name="message">The format string containing the error to output along with
/// placeholders</param>
/// <param name="...">The list of arguments to populate the format string placeholders</param>
void LogMessage(char *message, ...)
{
    va_list args;
    va_start(args, message);
    Log_Debug("[Azure IoT] ");
    Log_DebugVarArgs(message, args);
    va_end(args);
}

/// <summary>
///     Converts AZURE_SPHERE_PROV_RETURN_VALUE to a string.
/// </summary>
static char *getAzureSphereProvisioningResultString(
    AZURE_SPHERE_PROV_RETURN_VALUE provisioningResult)
{
    switch (provisioningResult.result) {
    case AZURE_SPHERE_PROV_RESULT_OK:
        return "AZURE_SPHERE_PROV_RESULT_OK";
    case AZURE_SPHERE_PROV_RESULT_INVALID_PARAM:
        return "AZURE_SPHERE_PROV_RESULT_INVALID_PARAM";
    case AZURE_SPHERE_PROV_RESULT_NETWORK_NOT_READY:
        return "AZURE_SPHERE_PROV_RESULT_NETWORK_NOT_READY";
    case AZURE_SPHERE_PROV_RESULT_DEVICEAUTH_NOT_READY:
        return "AZURE_SPHERE_PROV_RESULT_DEVICEAUTH_NOT_READY";
    case AZURE_SPHERE_PROV_RESULT_PROV_DEVICE_ERROR:
        return "AZURE_SPHERE_PROV_RESULT_PROV_DEVICE_ERROR";
    case AZURE_SPHERE_PROV_RESULT_GENERIC_ERROR:
        return "AZURE_SPHERE_PROV_RESULT_GENERIC_ERROR";
    default:
        return "UNKNOWN_RETURN_VALUE";
    }
}

/// <summary>
///     Sets up the client in order to establish the communication channel to Azure IoT Hub.
///
///     The client is created by using the scope id of the Device Provisioning System which
///     registers the device with an existing IoT hub and returns the information for
///     establishing the connection to that hub.
///     The client is setup with the following options:
///     - MQTT procotol 'keepalive' value of 20 seconds; when no PINGRESP is received after
///       20 seconds, the connection is believed to be down;
/// </summary>
/// <returns>'true' if the client has been properly set up. 'false' when a fatal error occurred
/// while setting up the client.</returns>
/// <remarks>This function is a no-op when the client has already been set up, i.e. this
/// function has already completed successfully.</remarks>
bool AzureIoT_SetupClient(void)
{
    if (iothubAuthenticated && (iothubClientHandle != NULL))
        return true;

    if (iothubClientHandle != NULL)
        IoTHubDeviceClient_LL_Destroy(iothubClientHandle);

    AZURE_SPHERE_PROV_RETURN_VALUE provResult =
        IoTHubDeviceClient_LL_CreateWithAzureSphereDeviceAuthProvisioning(scopeId, 10000,
                                                                          &iothubClientHandle);
    LogMessage("IoTHubDeviceClient_CreateWithAzureSphereDeviceAuthProvisioning returned '%s'.\n",
               getAzureSphereProvisioningResultString(provResult));

    if (provResult.result != AZURE_SPHERE_PROV_RESULT_OK) {
        return false;
    }

    // Provisioning and authentication succeeded.
    iothubAuthenticated = true;

    if (iothubClientHandle == NULL) {
        return false;
    }

    if (IoTHubDeviceClient_LL_SetOption(iothubClientHandle, "TrustedCerts",
                                        azureIoTCertificatesX) != IOTHUB_CLIENT_OK) {
        LogMessage("ERROR: failure to set option \"TrustedCerts\"\n");
        return false;
    }

    if (IoTHubDeviceClient_LL_SetOption(iothubClientHandle, OPTION_KEEP_ALIVE,
                                        &keepalivePeriodSeconds) != IOTHUB_CLIENT_OK) {
        LogMessage("ERROR: failure setting option \"%s\"\n", OPTION_KEEP_ALIVE);
        return false;
    }

    // Set callbacks for Message, MethodCall and Device Twin features.
    IoTHubDeviceClient_LL_SetMessageCallback(iothubClientHandle, receiveMessageCallback, NULL);
    IoTHubDeviceClient_LL_SetDeviceMethodCallback(iothubClientHandle, directMethodCallback, NULL);
    IoTHubDeviceClient_LL_SetDeviceTwinCallback(iothubClientHandle, twinCallback, NULL);

    // Set callbacks for connection status related events.
    if (IoTHubDeviceClient_LL_SetConnectionStatusCallback(
            iothubClientHandle, hubConnectionStatusCallback, NULL) != IOTHUB_CLIENT_OK) {
        LogMessage("ERROR: failure setting callback\n");
        return false;
    }

    return true;
}

/// <summary>
///     Destroys the Azure IoT Hub client.
/// </summary>
void AzureIoT_DestroyClient(void)
{
    if (iothubClientHandle != NULL) {
        IoTHubDeviceClient_LL_Destroy(iothubClientHandle);
        iothubClientHandle = NULL;
    }
}

/// <summary>
///     Periodically outputs a provided format string with a variable number of arguments.
/// </summary>
/// <param name="lastInvokedTime">Pointer where to store the 'last time this function has been
/// invoked' information</param>
/// <param name="periodInSeconds">The desired period of the output</param>
/// <param name="format">The format string</param>
static void PeriodicLogVarArgs(time_t *lastInvokedTime, time_t periodInSeconds, const char *format,
                               ...)
{
    struct timespec ts;
    int timeOk = timespec_get(&ts, TIME_UTC);
    if (!timeOk) {
        return;
    }

    if (ts.tv_sec > *lastInvokedTime + periodInSeconds) {
        va_list args;
        va_start(args, format);
        Log_Debug("[Azure IoT Hub client] ");
        Log_DebugVarArgs(format, args);
        va_end(args);
        *lastInvokedTime = ts.tv_sec;
    }
}

/// <summary>
///     Keeps IoT Hub Client alive by exchanging data with the Azure IoT Hub.
/// </summary>
/// <remarks>
///     This function must to be invoked periodically so that the Azure IoT Hub
///     SDK can accomplish its work (e.g. sending messages, invocation of callbacks, reconnection
///     attempts, and so forth).
/// </remarks>
void AzureIoT_DoPeriodicTasks(void)
{
    static time_t lastTimeLogged = 0;

    if (iothubAuthenticated) {
        PeriodicLogVarArgs(&lastTimeLogged, 5, "INFO: %s calls in progress...\n", __func__);

        // DoWork - send some of the buffered events to the IoT Hub, and receive some of the
        // buffered events from the IoT Hub.
        IoTHubDeviceClient_LL_DoWork(iothubClientHandle);
    }
}

/// <summary>
///     Creates and enqueues a message to be delivered the IoT Hub. The message is not actually
///     sent immediately, but it is sent on the next invocation of AzureIoT_DoPeriodicTasks().
/// </summary>
/// <param name="messagePayload">The payload of the message to send.</param>
void AzureIoT_SendMessage(const char *messagePayload)
{
    if (iothubClientHandle == NULL) {
        LogMessage("WARNING: IoT Hub client not initialized\n");
        return;
    }

    IOTHUB_MESSAGE_HANDLE messageHandle = IoTHubMessage_CreateFromString(messagePayload);

    if (messageHandle == 0) {
        LogMessage("WARNING: unable to create a new IoTHubMessage\n");
        return;
    }

    if (IoTHubDeviceClient_LL_SendEventAsync(iothubClientHandle, messageHandle, sendMessageCallback,
                                             /*&callback_param*/ 0) != IOTHUB_CLIENT_OK) {
        LogMessage("WARNING: failed to hand over the message to IoTHubClient\n");
    } else {
        LogMessage("INFO: IoTHubClient accepted the message for delivery\n");
    }

    IoTHubMessage_Destroy(messageHandle);
}

/// <summary>
///     Sets the function to be invoked whenever the Device Twin properties have been delivered
///     to the IoT Hub.
/// </summary>
/// <param name="callback">The function pointer to the callback function.</param>
void AzureIoT_SetDeviceTwinDeliveryConfirmationCallback(
    DeviceTwinDeliveryConfirmationFnType callback)
{
    deviceTwinConfirmationCb = callback;
}

/// <summary>
///     Callback invoked when the Device Twin reported properties are accepted by IoT Hub.
/// </summary>
static void reportStatusCallback(int result, void *context)
{
    LogMessage("INFO: Device Twin reported properties update result: HTTP status code %d\n",
               result);
    if (deviceTwinConfirmationCb)
        deviceTwinConfirmationCb(result);
}

/// <summary>
///     Creates and enqueues a report containing the name and value pair of a Device Twin
///     reported property. The report is not actually sent immediately, but it is sent on the
///     next invocation of AzureIoT_DoPeriodicTasks().
/// </summary>
void AzureIoT_TwinReportState(const char *propertyName, size_t propertyValue)
{
    if (iothubClientHandle == NULL) {
        LogMessage("ERROR: client not initialized\n");
        return;
    }

    char *reportedPropertiesString = NULL;
    JSON_Value *reportedPropertiesRootJson = json_value_init_object();
    if (reportedPropertiesRootJson == NULL) {
        LogMessage("ERROR: could not create the JSON_Value for Device Twin reporting.\n");
        return;
    }

    JSON_Object *reportedPropertiesJson = json_value_get_object(reportedPropertiesRootJson);
    if (reportedPropertiesJson == NULL) {
        LogMessage("ERROR: could not get the JSON_Object for Device Twin reporting.\n");
        goto cleanup;
    }

    if (JSONSuccess !=
        json_object_set_number(reportedPropertiesJson, propertyName, propertyValue)) {
        LogMessage("ERROR: could not set the property value for Device Twin reporting.\n");
        goto cleanup;
    }

    reportedPropertiesString = json_serialize_to_string(reportedPropertiesRootJson);
    if (reportedPropertiesString == NULL) {
        LogMessage(
            "ERROR: could not serialize the JSON payload to string for Device "
            "Twin reporting.\n");
        goto cleanup;
    }

    if (IoTHubDeviceClient_LL_SendReportedState(
            iothubClientHandle, (unsigned char *)reportedPropertiesString,
            strlen(reportedPropertiesString), reportStatusCallback, 0) != IOTHUB_CLIENT_OK) {
        LogMessage("ERROR: failed to set reported property '%s'.\n", propertyName);
    } else {
        LogMessage("INFO: Set reported property '%s' to value %d.\n", propertyName, propertyValue);
    }

cleanup:
    if (reportedPropertiesRootJson != NULL) {
        json_value_free(reportedPropertiesRootJson);
    }
    if (reportedPropertiesString != NULL) {
        json_free_serialized_string(reportedPropertiesString);
    }
}

/// <summary>
///     Sets a callback function invoked whenever a message is received from IoT Hub.
/// </summary>
/// <param name="callback">The callback function invoked when a message is received</param>
void AzureIoT_SetMessageReceivedCallback(MessageReceivedFnType callback)
{
    messageReceivedCb = callback;
}

/// <summary>
///     Sets the function to be invoked whenever the message to the Iot Hub has been delivered.
/// </summary>
/// <param name="callback">The function pointer to the callback function.</param>
void AzureIoT_SetMessageConfirmationCallback(MessageDeliveryConfirmationFnType callback)
{
    messageDeliveryConfirmationCb = callback;
}

/// <summary>
///     Function invoked when the message delivery confirmation is being reported.
/// </summary>
/// <param name="result">Message delivery status</param>
/// <param name="context">User specified context</param>
static void sendMessageCallback(IOTHUB_CLIENT_CONFIRMATION_RESULT result, void *context)
{
    LogMessage("INFO: Message received by IoT Hub. Result is: %d\n", result);
    if (messageDeliveryConfirmationCb)
        messageDeliveryConfirmationCb(result == IOTHUB_CLIENT_CONFIRMATION_OK);
}

/// <summary>
///     Callback function invoked when a message is received from IoT Hub.
/// </summary>
/// <param name="message">The handle of the received message</param>
/// <param name="context">The user context specified at
/// IoTHubDeviceClient_LL_SetMessageCallback() invocation time</param>
/// <returns>Return value to indicates the message procession status (i.e. accepted, rejected,
/// abandoned)</returns>
static IOTHUBMESSAGE_DISPOSITION_RESULT receiveMessageCallback(IOTHUB_MESSAGE_HANDLE message,
                                                               void *context)
{
    const unsigned char *buffer = NULL;
    size_t size = 0;
    if (IoTHubMessage_GetByteArray(message, &buffer, &size) != IOTHUB_MESSAGE_OK) {
        LogMessage("WARNING: failure performing IoTHubMessage_GetByteArray\n");
        return IOTHUBMESSAGE_REJECTED;
    }

    // 'buffer' is not zero terminated.
    unsigned char *str_msg = (unsigned char *)malloc(size + 1);
    if (str_msg == NULL) {
        LogMessage("ERROR: could not allocate buffer for incoming message\n");
        abort();
    }
    memcpy(str_msg, buffer, size);
    str_msg[size] = '\0';

    if (messageReceivedCb != 0) {
        messageReceivedCb(str_msg);
    } else {
        LogMessage("WARNING: no user callback set up for event 'message received from IoT Hub'\n");
    }

    LogMessage("INFO: Received message '%s' from IoT Hub\n", str_msg);
    free(str_msg);

    return IOTHUBMESSAGE_ACCEPTED;
}

/// <summary>
///     Sets the function to be invoked whenever a Direct Method call from the IoT Hub is
///     received.
/// </summary>
/// <param name="callback">The callback function invoked when a Direct Method call is
/// received</param>
void AzureIoT_SetDirectMethodCallback(DirectMethodCallFnType callback)
{
    directMethodCallCb = callback;
}

/// <summary>
///     Sets the function callback invoked whenever a Device Twin update from the IoT Hub is
///     received.
/// </summary>
/// <param name="callback">The callback function invoked when a Device Twin update is
/// received</param>
void AzureIoT_SetDeviceTwinUpdateCallback(TwinUpdateFnType callback)
{
    twinUpdateCb = callback;
}

/// <summary>
///     Callback when direct method is called.
/// </summary>
static int directMethodCallback(const char *methodName, const unsigned char *payload, size_t size,
                                unsigned char **response, size_t *responseSize,
                                void *userContextCallback)
{
    LogMessage("INFO: Trying to invoke method %s\n", methodName);

    int result = 404;

    if (directMethodCallCb != NULL) {
        char *responseFromCallback = NULL;
        size_t responseFromCallbackSize = 0;

        result = directMethodCallCb(methodName, payload, size, &responseFromCallback,
                                    &responseFromCallbackSize);
        *responseSize = responseFromCallbackSize;
        *response = responseFromCallback;
    } else {
        LogMessage("INFO: No method '%s' found, HttpStatus=%d\n", methodName, result);
        static const char methodNotFound[] = "\"No method found\"";
        *responseSize = strlen(methodNotFound);
        *response = (unsigned char *)malloc(*responseSize);
        if (*response != NULL) {
            strncpy((char *)(*response), methodNotFound, *responseSize);
        } else {
            LogMessage("ERROR: Cannot create response message for method call.\n");
            abort();
        }
    }

    return result;
}

/// <summary>
///     Callback invoked when a Device Twin update is received from IoT Hub.
/// </summary>
static void twinCallback(DEVICE_TWIN_UPDATE_STATE updateState, const unsigned char *payLoad,
                         size_t payLoadSize, void *userContextCallback)
{
    size_t nullTerminatedJsonSize = payLoadSize + 1;
    char *nullTerminatedJsonString = (char *)malloc(nullTerminatedJsonSize);
    if (nullTerminatedJsonString == NULL) {
        LogMessage("ERROR: Could not allocate buffer for twin update payload.\n");
        abort();
    }

    // Copy the provided buffer to a null terminated buffer.
    memcpy(nullTerminatedJsonString, payLoad, payLoadSize);
    // Add the null terminator at the end.
    nullTerminatedJsonString[nullTerminatedJsonSize - 1] = 0;

    JSON_Value *rootProperties = NULL;
    rootProperties = json_parse_string(nullTerminatedJsonString);
    if (rootProperties == NULL) {
        LogMessage("WARNING: Cannot parse the string as JSON content.\n");
        goto cleanup;
    }

    JSON_Object *rootObject = json_value_get_object(rootProperties);
    JSON_Object *desiredProperties = json_object_dotget_object(rootObject, "desired");
    if (desiredProperties == NULL) {
        desiredProperties = rootObject;
    }
    // Call the provided Twin Device callback if any.
    if (twinUpdateCb != NULL) {
        twinUpdateCb(desiredProperties);
    }

cleanup:
    // Release the allocated memory.
    json_value_free(rootProperties);
    free(nullTerminatedJsonString);
}

/// <summary>
///     Sets the function to be invoked whenever the connection status to the IoT Hub changes.
/// </summary>
/// <param name="callback">The callback function invoked when Azure IoT Hub network connection
/// status changes.</param>
void AzureIoT_SetConnectionStatusCallback(ConnectionStatusFnType callback)
{
    hubConnectionStatusCb = callback;
}

/// <summary>
///     Callback function invoked whenever the connection status to IoT Hub changes.
/// </summary>
/// <param name="result">Current authentication status</param>
/// <param name="reason">result's specific reason</param>
/// <param name="userContextCallback">User specified context</param>
static void hubConnectionStatusCallback(IOTHUB_CLIENT_CONNECTION_STATUS result,
                                        IOTHUB_CLIENT_CONNECTION_STATUS_REASON reason,
                                        void *userContextCallback)
{
    iothubAuthenticated = (result == IOTHUB_CLIENT_CONNECTION_AUTHENTICATED);
    if (hubConnectionStatusCb) {
        hubConnectionStatusCb(iothubAuthenticated);
    }
    const char *reasonString = getReasonString(reason);
    if (!iothubAuthenticated) {
        LogMessage("INFO: IoT Hub connection is down (%s), retrying connection...\n", reasonString);
    } else {
        LogMessage("INFO: connection to the IoT Hub has been established (%s).\n", reasonString);
    }
}

/// <summary>
///     Initializes the Azure IoT Hub SDK.
/// </summary>
/// <return>'true' if initialization has been successful.</param>
bool AzureIoT_Initialize(void)
{
    if (IoTHub_Init() != 0) {
        LogMessage("ERROR: failed initializing platform.\n");
        return false;
    }
    return true;
}

/// <summary>
///     Deinitializes the Azure IoT Hub SDK.
/// </summary>
void AzureIoT_Deinitialize(void)
{
    IoTHub_Deinit();
}
