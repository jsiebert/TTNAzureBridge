using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Newtonsoft.Json;
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

        public static string GetTTNBearerToken()
        {
            return Environment.GetEnvironmentVariable("TTNBearerToken");
        }

        public static void RenewTTNBearerToken()
        {
            using (var httpClient = new HttpClient())
            {
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);

                using (var writer = new JsonTextWriter(sw))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("username"); writer.WriteValue(Environment.GetEnvironmentVariable("TTNAppID"));
                    writer.WritePropertyName("password"); writer.WriteValue(Environment.GetEnvironmentVariable("TTNAppKey"));
                    writer.WritePropertyName("grant_type"); writer.WriteValue("password");
                    writer.WriteEndObject();
                }

                var body = new StringContent(sb.ToString());
                var basicAuthCredentials = Encoding.UTF8.GetBytes($"{Environment.GetEnvironmentVariable("TTNClientID")}:{Environment.GetEnvironmentVariable("TTNClientSecret")}");
                var basicAuthString = Convert.ToBase64String(basicAuthCredentials);

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthString);
                var response = Task.Run(async () => await httpClient.PostAsync("https://account.thethingsnetwork.org/api/v2/applications/token", body)).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                   // TODO: Parse response and update App Setting
                }
            }
        }
    }
}