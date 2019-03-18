using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json.Linq;


namespace TTNAzureBridge.TTNAzureMessaging
{
    public static class TTNAzureMessaging
    {
        [FunctionName("OnTTNUplinkMessage")]
        public static async Task OnTTNUplinkMessage([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request, ILogger logger)
        {
            logger.LogInformation($"{DateTime.Now:g} Device to Cloud message received");

            var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            var jsonData = JObject.Parse(requestBody);
            var appId = jsonData["app_id"].ToString();
            var deviceId = jsonData["dev_id"].ToString();

            if (appId.Equals(TTNConfigProvider.GetTTNAppId()))
            {
                using (var registryManager = RegistryManager.CreateFromConnectionString(AzureConfigProvider.GetIoTHubConnectionString()))
                {
                    var device = await registryManager.GetDeviceAsync(deviceId);

                    using (var deviceClient = DeviceClient.CreateFromConnectionString(AzureConfigProvider.GetDeviceConnectionString(deviceId, device.Authentication.SymmetricKey.PrimaryKey)))
                    {
                        var eventMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(jsonData.ToString()));

                        await deviceClient.SendEventAsync(eventMessage);
                    }

                    logger.LogInformation($"{DateTime.Now:g} Sent event from device {deviceId} to IoT Hub");
                }
            }
        }
    }
}