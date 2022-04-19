using Mango.Services.Email.Messages;
using Mango.Services.Email.Repository;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.Email.Messaging
{
    public class RabbitMQPaymentConsumer : BackgroundService, IRabbitMQPaymentConsumer
    {
        private IConnection _connection;
        private IModel _channel;
        private IConfiguration _configuration;
        private readonly string _ExchangeName;
        private string _queueName;
        private readonly EmailRepository _emailRepository;
        public RabbitMQPaymentConsumer(IConfiguration config, EmailRepository emailRepository)
        {
            _configuration = config;
            _emailRepository = emailRepository;
            _ExchangeName = _configuration.GetValue<string>("DirectName");
            _queueName = _configuration.GetValue<string>("PaymentEmailQueueNameDirect");
            var factory = new ConnectionFactory
            {
                HostName = _configuration.GetSection("RabbitMQ").GetValue<string>("hostname"),
                UserName = _configuration.GetSection("RabbitMQ").GetValue<string>("userName"),
                Password = _configuration.GetSection("RabbitMQ").GetValue<string>("password")
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _ExchangeName, ExchangeType.Direct, arguments: null, durable: true, autoDelete: false);
            _channel.QueueDeclare(queue: _queueName, arguments: null, exclusive: false, durable: true, autoDelete: false);
            _channel.QueueBind(_queueName, _ExchangeName, _queueName);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                UpdatePaymentResultMessage paymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);
                HandleMessage(paymentResultMessage).GetAwaiter().GetResult();
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(_queueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(UpdatePaymentResultMessage paymentResultMessage)
        {
            try
            {
                if (paymentResultMessage != null)
                {
                    await _emailRepository.SendAndLogEmail(paymentResultMessage);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
