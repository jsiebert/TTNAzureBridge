using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using MQTTnet.Extensions.ManagedClient;

namespace TTNAzureBridge
{
    public class TTNMqttConfiguration : CustomMqttConfig
    {
        public override IManagedMqttClientOptions Options { get; }

        public override string Name { get; }

        public TTNMqttConfiguration(string name, IManagedMqttClientOptions options)
        {
            Options = options;
            Name = name;
        }
    }
}