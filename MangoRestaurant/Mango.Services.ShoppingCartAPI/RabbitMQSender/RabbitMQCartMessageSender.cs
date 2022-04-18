using Mango.MessageBus;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public class RabbitMQCartMessageSender : IRabbitMQCartMessageSender
    {
        private readonly IConfiguration _configuration;
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;

        public RabbitMQCartMessageSender(IConfiguration config)
        {
            _configuration = config;
            _hostname = _configuration.GetSection("RabbitMQ").GetValue<string>("hostname");
            _username = _configuration.GetSection("RabbitMQ").GetValue<string>("userName");
            _password = _configuration.GetSection("RabbitMQ").GetValue<string>("password");
        }
        public void SendMessage(BaseMessage baseMessage, string queueName)
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            _connection = factory.CreateConnection();
            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: queueName, arguments: null);
            var json = JsonConvert.SerializeObject(baseMessage);
            var body = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish(exchange:"",routingKey: queueName, body: body, basicProperties: null);

        }
    }
}
