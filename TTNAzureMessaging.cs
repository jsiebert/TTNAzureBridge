using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;


namespace TTNAzureBridge.TTNAzureMessaging
{
    public static class TTNAzureMessaging
    {
        [FunctionName("OnTTNUplinkMessage")]
        public static async Task OnTTNUplinkMessage([MqttTrigger(typeof(TTNConfigProvider), "+/devices/+/up")] IMqttMessage message, ILogger logger)
        {
            logger.LogInformation($"{DateTime.Now:g} Message for topic {message.Topic}");

            var messageParts = message.Topic.Split('/');
            var appId = messageParts[0];
            var deviceId = messageParts[2];
            var deviceEvent = messageParts[3];

            if (appId.Equals(TTNConfigProvider.GetTTNAppId()))
            {
                // TODO: This is a workaround for https://github.com/chkr1011/MQTTnet/issues/569. To be implemented in the correct way once this is fixed.
                switch (deviceEvent)
                {
                    case "up":
                        {
                            using (var registryManager = RegistryManager.CreateFromConnectionString(AzureConfigProvider.GetIoTHubConnectionString()))
                            {
                                var device = await registryManager.GetDeviceAsync(deviceId);
                                var deviceClient = DeviceClient.CreateFromConnectionString(AzureConfigProvider.GetDeviceConnectionString(deviceId, device.Authentication.SymmetricKey.PrimaryKey));
                                var eventMessage = new Microsoft.Azure.Devices.Client.Message(message.GetMessage());

                                await deviceClient.SendEventAsync(eventMessage);
                                logger.LogInformation($"{DateTime.Now:g} Sent event from device {deviceId} to IoT Hub");
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }
}