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
        [FunctionName("OnTTNDeviceEvent")]
        public static async Task OnTTNDeviceEvent([MqttTrigger(typeof(TTNConfigProvider), "%TTNAppID%/devices/+/events/#")] IMqttMessage message, ILogger logger)
        {
            logger.LogInformation($"{DateTime.Now:g} Message for topic {message.Topic}");

            var messageParts = message.Topic.Split('/');
            var deviceId = messageParts[2];
            var deviceEvent = messageParts[4];

            using (var registryManager = RegistryManager.CreateFromConnectionString(AzureConfigProvider.GetIoTHubConnectionString()))
            {
                // TODO: This is a workaround for https://github.com/chkr1011/MQTTnet/issues/569. To be implemented in the correct way once this is fixed.
                switch (deviceEvent)
                {
                    case "create":
                        {
                            await registryManager.AddDeviceAsync(new Device(deviceId));
                            logger.LogInformation($"{DateTime.Now:g} Added device {deviceId} to IoT Hub");
                        }
                        break;

                    case "delete":
                        {
                            await registryManager.RemoveDeviceAsync(deviceId);
                            logger.LogInformation($"{DateTime.Now:g} Removed device {deviceId} from IoT Hub");
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
