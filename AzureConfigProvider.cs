using System;

namespace TTNAzureBridge
{
    public class AzureConfigProvider
    {
        public static string GetIoTHubConnectionString()
        {
            return Environment.GetEnvironmentVariable("AzureIoTHubConnectionString");
        }

        public static string GetDeviceConnectionString(string deviceId, string deviceKey)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureIoTHubConnectionString");
            var connectionStringParts = connectionString.Split(';');
            var hostnameParts = connectionStringParts[0].Split('=');
            return $"HostName={hostnameParts[1]};DeviceId={deviceId};SharedAccessKey={deviceKey}";
        }
    }
}