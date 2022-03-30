using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public class AzureServiceBus : IMessageBus
    {
        private readonly IConfiguration Configuration;
        private string connectionString;

        public AzureServiceBus(IConfiguration configuration)
        {
            Configuration = configuration;
            connectionString = configuration.GetConnectionString("ServiceBus");
        }
        public async Task PublishMessage(BaseMessage message, string topicName)
        {
            await using var client = new ServiceBusClient(connectionString);
            ServiceBusSender senderClient =client.CreateSender(topicName);
            var Jsonmessage = JsonConvert.SerializeObject(message);
            ServiceBusMessage finalMesssage = new ServiceBusMessage(Encoding.UTF8.GetBytes(Jsonmessage))
            {
                CorrelationId = Guid.NewGuid().ToString()
            };
            await senderClient.SendMessageAsync(finalMesssage);

            await senderClient.DisposeAsync();
        }
    }
}
