using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.Devices;

namespace TTNAzureBridge.DeviceManagement
{
    public static class TTNAzureDeviceManagement
    {
        [FunctionName("OnTTNDeviceCreated")]
        public static async Task OnTTNDeviceCreated([MqttTrigger(typeof(TTNConfigProvider), "%TTNAppID%/devices/+/events/create")] IMqttMessage message, ILogger logger)
        {
            logger.LogInformation($"{DateTime.Now:g} Device creation message for topic {message.Topic}");

            var messageParts = message.Topic.Split('/');
            var deviceId = messageParts[2];

            using (var registryManager = RegistryManager.CreateFromConnectionString(AzureConfigProvider.GetIoTHubConnectionString()))
            {
                await registryManager.AddDeviceAsync(new Device(deviceId));
                logger.LogInformation($"{DateTime.Now:g} Added device {deviceId} to IoT Hub");
            }
        }

        [FunctionName("OnTTNDeviceDeleted")]
        public static async Task OnTTNDeviceDeleted([MqttTrigger(typeof(TTNConfigProvider), "%TTNAppID%/devices/+/events/delete")] IMqttMessage message, ILogger logger)
        {
            logger.LogInformation($"{DateTime.Now:g} Device deletion message for topic {message.Topic}");

            var messageParts = message.Topic.Split('/');
            var deviceId = messageParts[2];

            using (var registryManager = RegistryManager.CreateFromConnectionString(AzureConfigProvider.GetIoTHubConnectionString()))
            {
                await registryManager.RemoveDeviceAsync(deviceId);
                logger.LogInformation($"{DateTime.Now:g} Removed device {deviceId} from IoT Hub");
            }
        }
    }
}
