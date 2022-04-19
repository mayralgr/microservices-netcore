using Mango.Services.PaymentAPI.Messages;
using Mango.Services.PaymentAPI.RabbitMQSender;
using Newtonsoft.Json;
using PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class RabbitMQPaymentConsumer : BackgroundService, IRabbitMQPaymentConsumer
    {
        private IRabbitMQPaymentMessageSender _rabbitMQPaymentMessageSender;
        private IConnection _connection;
        private IModel _channel;
        private IConfiguration _configuration;
        private readonly string _paymentQueue;
        private readonly string _orderUpdatePaymentResultTopic;
        private readonly IProcessPayment _processPayment;
        // direct
        private readonly string _DirectExchange;

        private readonly string _paymentEmailQueue;
        private readonly string _paymentOrderQueue;
        public RabbitMQPaymentConsumer(IProcessPayment processPayment, IConfiguration config, IRabbitMQPaymentMessageSender sender)
        {
            _configuration = config;
            _processPayment = processPayment;
            _paymentQueue = _configuration.GetValue<string>("PaymentTopic");
            _DirectExchange = _configuration.GetValue<string>("DirectName");
            _paymentEmailQueue = _configuration.GetValue<string>("PaymentEmailQueueNameDirect");
            _paymentOrderQueue = _configuration.GetValue<string>("PaymentOrderQueueNameDirect");
            _orderUpdatePaymentResultTopic = _configuration.GetValue<string>("ExchangeName"); // fanout
            _rabbitMQPaymentMessageSender = sender;
            var factory = new ConnectionFactory
            {
                HostName = _configuration.GetSection("RabbitMQ").GetValue<string>("hostname"),
                UserName = _configuration.GetSection("RabbitMQ").GetValue<string>("userName"),
                Password = _configuration.GetSection("RabbitMQ").GetValue<string>("password")
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _orderUpdatePaymentResultTopic, arguments: null, exclusive: false, durable: true, autoDelete: false);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                PaymentRequestMessage paymentRequestMessage = JsonConvert.DeserializeObject<PaymentRequestMessage>(body);
                
                HandleMessage(paymentRequestMessage).GetAwaiter().GetResult();
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(_paymentQueue, false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(PaymentRequestMessage paymentRequestMessage)
        {
            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage updatePaymentResultMessage = new UpdatePaymentResultMessage()
            {
                Status = result,
                OrderId = paymentRequestMessage.OrderId,
                Email = paymentRequestMessage.Email

            };
            try
            {
                if (updatePaymentResultMessage != null)
                {
                    Console.WriteLine("Success!!!");
                    //await _messageBus.PublishMessage(updatePaymentResultMessage, _orderUpdatePaymentResultTopic);
                    //await args.CompleteMessageAsync(args.Message);
                    // new rabbitmq fanout
                    _rabbitMQPaymentMessageSender.SendMessage(updatePaymentResultMessage, _DirectExchange,_paymentEmailQueue, _paymentOrderQueue);

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
