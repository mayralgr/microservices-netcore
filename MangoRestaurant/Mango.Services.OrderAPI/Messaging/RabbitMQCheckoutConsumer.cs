using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.RabbitMQSender;
using Mango.Services.OrderAPI.Repository;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.OrderAPI.Messaging
{
    public class RabbitMQCheckoutConsumer : BackgroundService, IRabbitMQCheckoutConsumer
    {
        private readonly OrderRepository _orderRepository;
        private IRabbitMQOrderMessageSender _rabbitMQOrderMessageSender;
        private IConnection _connection;
        private IModel _channel;
        private IConfiguration _configuration;
        private string _queueName;
        private string _paymentQueue;
        public RabbitMQCheckoutConsumer(OrderRepository orderRepository, IConfiguration config, IRabbitMQOrderMessageSender rabbitMQOrderMessageSender)
        {
            _configuration = config;
            _rabbitMQOrderMessageSender = rabbitMQOrderMessageSender;
            _orderRepository = orderRepository;
            _queueName = _configuration.GetValue<string>("CheckoutQueue");
            _paymentQueue = _configuration.GetValue<string>("PaymentTopic");
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName, arguments: null, exclusive: false, durable: true, autoDelete: false);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                CheckoutHeaderDto checkoutHeaderDto = JsonConvert.DeserializeObject<CheckoutHeaderDto>(content);
                HandleMessage(checkoutHeaderDto).GetAwaiter().GetResult();
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(_queueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(CheckoutHeaderDto checkoutHeaderDto)
        {
            if (checkoutHeaderDto != null)
            {
                OrderHeader orderHeader = new OrderHeader // can be included in automapper
                {
                    CouponCode = checkoutHeaderDto.CouponCode,
                    DiscountTotal = checkoutHeaderDto.DiscountTotal,
                    OrderTotal = checkoutHeaderDto.OrderTotal,
                    UserId = checkoutHeaderDto.UserId,
                    CardNumber = checkoutHeaderDto.CardNumber,
                    ExpiryMonthYear = checkoutHeaderDto.ExpiryMonthYear,
                    FirstName = checkoutHeaderDto.FirstName,
                    PickupDate = checkoutHeaderDto.PickupDate,
                    OrderDetails = new List<OrderDetails>(),
                    OrderTime = DateTime.Now,
                    CVV = checkoutHeaderDto.CVV,
                    Email = checkoutHeaderDto.Email,
                    LastName = checkoutHeaderDto.LastName,
                    Phone = checkoutHeaderDto.Phone,
                };
                foreach (var detail in checkoutHeaderDto.CartDetails)
                {
                    OrderDetails orderDetails = new()
                    {
                        Count = detail.Count,
                        ProductId = detail.ProductId,
                        Price = detail.Product.Price,
                        ProductName = detail.Product.Name
                    };
                    orderHeader.CartTotalItems += detail.Count;
                    orderHeader.OrderDetails.Add(orderDetails);
                }

                await _orderRepository.AddOrder(orderHeader);
                PaymentRequestMessage paymentRequestMessage = new PaymentRequestMessage()
                {
                    Name = orderHeader.FirstName + " " + orderHeader.LastName,
                    CardNumber = orderHeader.CardNumber,
                    CVV = orderHeader.CVV,
                    ExpiryMonthYear = orderHeader.ExpiryMonthYear,
                    OrderId = orderHeader.OrderHeaderId,
                    OrderTotal = orderHeader.OrderTotal,
                    Email = orderHeader.Email
                };
                try
                {
                    //await _messageBus.PublishMessage(paymentRequestMessage, _subPaymentTopicName);
                    //await args.CompleteMessageAsync(args.Message);
                    // new rabbitmq queue
                    _rabbitMQOrderMessageSender.SendMessage(paymentRequestMessage, _paymentQueue);

                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }
}
