using Mango.MessageBus;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.PaymentAPI.RabbitMQSender
{
    public class RabbitMQPaymentMessageSender : IRabbitMQPaymentMessageSender
    {
        private readonly IConfiguration _configuration;
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;

        public RabbitMQPaymentMessageSender(IConfiguration config)
        {
            _configuration = config;
            _hostname = _configuration.GetSection("RabbitMQ").GetValue<string>("hostname");
            _username = _configuration.GetSection("RabbitMQ").GetValue<string>("userName");
            _password = _configuration.GetSection("RabbitMQ").GetValue<string>("password");
            
        }
        public void SendMessage(BaseMessage baseMessage, string direct, string q1, string q2)
        {
            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();
                channel.ExchangeDeclare(exchange: direct, ExchangeType.Direct, true, false);
                channel.QueueDeclare(q1,true,false,false);
                channel.QueueDeclare(q2, true, false, false);

                channel.QueueBind(q1, direct, q1);
                channel.QueueBind(q2, direct, q2);

                var json = JsonConvert.SerializeObject(baseMessage);
                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: direct, q1, body: body, basicProperties: null);
                channel.BasicPublish(exchange: direct, q2, body: body, basicProperties: null);
            }
            else
            {
                Console.WriteLine("Unable to create connection with RabbitMQ");
            }

        }

        private void CreateConnection()
        {
            try
            {   
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };
                _connection = factory.CreateConnection();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private bool ConnectionExists()
        {
            if(_connection != null)
            {
                return true;
            }
            CreateConnection();
            return _connection != null;
        }
    }
}
