using System;

namespace TTNAzureBridge
{
    public class AzureConfigProvider
    {
        public static string GetIoTHubConnectionString()
        {
            return $"HostName={Environment.GetEnvironmentVariable("AzureIoTHubHostname")};SharedAccessKeyName={Environment.GetEnvironmentVariable("AzureIoTHubAccessKeyOwner")};SharedAccessKey={Environment.GetEnvironmentVariable("AzureIotHubAccessKey")}";
        }

        public static string GetDeviceConnectionString(string deviceId, string deviceKey)
        {
            return $"HostName={Environment.GetEnvironmentVariable("AzureIoTHubHostname")};DeviceId={deviceId};SharedAccessKey={deviceKey}";
        }
    }
}