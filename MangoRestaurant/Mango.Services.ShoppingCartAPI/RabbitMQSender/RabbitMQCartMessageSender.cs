using Mango.MessageBus;

namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public class RabbitMQCartMessageSender : IRabbitMQCartMessageSender
    {
        private readonly IConfiguration _configuration;
        private readonly string _hostname;

        private readonly string _username;

        private readonly string _password;
        public RabbitMQCartMessageSender(IConfiguration config)
        {
            _configuration = config;
            _hostname = _configuration.GetSection("RabbitMQ").GetValue<string>("hostname");
            _username = _configuration.GetSection("RabbitMQ").GetValue<string>("userName");
            _password = _configuration.GetSection("RabbitMQ").GetValue<string>("password");
        }
        public void SendMessage(BaseMessage baseMessage, string queueName)
        {

        }
    }
}
