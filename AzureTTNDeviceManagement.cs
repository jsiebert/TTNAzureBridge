using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace TTNAzureBridge.AzureTTNDeviceManagement
{
    public static class AzureTTNDeviceManagement
    {
        [FunctionName("OnAzureDeviceEvent")]
        public static async Task OnAzureDeviceEvent([EventGridTrigger] EventGridEvent message, ILogger logger)
        {
            var eventData = JObject.Parse(message.Data.ToString());
            var deviceId = eventData["deviceId"];

            logger.LogInformation($"{DateTime.Now:g} Device Event for device {deviceId}");

            switch (message.EventType)
            {
                case "Microsoft.Devices.DeviceCreated":
                    break;

                case "Microsoft.Devices.DeviceDeleted":
                    {
                        using (var httpClient = new HttpClient(new TTNAuthHandler(new HttpClientHandler())))
                        {
                            var handler = TTNConfigProvider.ResolveHandler(TTNConfigProvider.GetTTNAppId(), "api_address");

                            await httpClient.DeleteAsync($"{handler}//applications/{TTNConfigProvider.GetTTNAppId()}/devices/{deviceId}");

                            logger.LogInformation($"{DateTime.Now:g} Removed device {deviceId} from TTN");
                        }
                    }
                    break;

                default:
                    break;
            }
        }
    }
}