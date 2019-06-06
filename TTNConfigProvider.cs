using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Newtonsoft.Json.Linq;


namespace TTNAzureBridge
{
    public class TTNConfigProvider : ICreateMqttConfig
    {
        public CustomMqttConfig Create(INameResolver nameResolver, ILogger logger)
        {
            var handler = Task.Run(async () => await ResolveHandler(GetTTNAppId(), "mqtt_address")).GetAwaiter().GetResult().Split(':');

            var options = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithTcpServer(handler[0], 8883)
                    .WithCredentials(GetTTNAppId(), GetTTNAppKey())
                    .WithCleanSession()
                    .WithTls()
                    .Build())
                .Build();

            return new TTNMqttConfiguration("TTNMqttConfiguration", options);
        }

        public static async Task<string> ResolveHandler(string appId, string address)
        {
            var result = new List<string>();

            using (var httpClient = new HttpClient())
            {
                var discoveryServerUrl = $"{Environment.GetEnvironmentVariable("TTNDiscoveryServerURL")}/announcements/handler";
                var response = await httpClient.GetStringAsync(discoveryServerUrl);
                var jsonResponse = JObject.Parse(response);

                foreach (var service in jsonResponse["services"])
                {
                    foreach (var metadata in service["metadata"])
                    {
                        if (metadata["app_id"].ToString().Equals(appId))
                        {
                            result.Add(service[address].ToString());
                        }
                    }
                }
            }

            return result.FirstOrDefault();
        }

        public static string GetTTNAppId()
        {
            return Environment.GetEnvironmentVariable("TTNAppID");
        }

        public static string GetTTNAppKey()
        {
            return Environment.GetEnvironmentVariable("TTNAppKey");
        }
    }
}