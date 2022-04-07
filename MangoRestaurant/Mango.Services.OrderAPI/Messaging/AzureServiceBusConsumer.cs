using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repository;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.OrderAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string _subConnectionString;
        // private readonly string _subTopicName;
        private readonly string _subPaymentTopicName;
        private readonly string _subSubscriptionCheckOutName;
        private readonly string _CheckoutQueueName;
        private readonly string _orderUpdatePaymentResultTopic;
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor _checkOutProcessor;
        private ServiceBusProcessor _paymenntResultProcessor;
        private readonly IMessageBus _messageBus;
        public AzureServiceBusConsumer(IConfiguration config, OrderRepository orderRepository, IMessageBus messageBus)
        {
            _orderRepository = orderRepository;
            _configuration = config;
            _subConnectionString = _configuration.GetConnectionString("ServiceBus");
            // _subTopicName = _configuration.GetValue<string>("CheckoutTopic");
            _CheckoutQueueName = _configuration.GetValue<string>("CheckoutQueue");
            _subPaymentTopicName = _configuration.GetValue<string>("PaymentTopic");
            _subSubscriptionCheckOutName = _configuration.GetValue<string>("SubscriptionCheckOutName");
            _orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");
            var client = new ServiceBusClient(_subConnectionString);
            _messageBus = messageBus;
            _checkOutProcessor = client.CreateProcessor(_CheckoutQueueName);
            _paymenntResultProcessor = client.CreateProcessor(_orderUpdatePaymentResultTopic, _subSubscriptionCheckOutName);

        }

        public async Task Start()
        {
            _checkOutProcessor.ProcessMessageAsync += OnCheckOutMessageReceived;
            _checkOutProcessor.ProcessErrorAsync += ErrorEventHandler;
            await _checkOutProcessor.StartProcessingAsync();

            _paymenntResultProcessor.ProcessMessageAsync += OnOrderPaymentUpdateReceived;
            _paymenntResultProcessor.ProcessErrorAsync += ErrorEventHandler;
            await _paymenntResultProcessor.StartProcessingAsync();
        }
        public async Task Stop()
        {
            await _checkOutProcessor.StopProcessingAsync();
            await _checkOutProcessor.DisposeAsync();
            await _paymenntResultProcessor.StopProcessingAsync();
            await _paymenntResultProcessor.DisposeAsync();
        }
        private Task ErrorEventHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnOrderPaymentUpdateReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            UpdatePaymentResultMessage paymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);
            if (paymentResultMessage != null)
            {
                await _orderRepository.UpdateOrderPaymentStatus(paymentResultMessage.OrderId, paymentResultMessage.Status);
                await args.CompleteMessageAsync(args.Message);

            }


        }

        private async Task OnCheckOutMessageReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            CheckoutHeaderDto checkoutHeaderDto = JsonConvert.DeserializeObject<CheckoutHeaderDto>(body);
            if(checkoutHeaderDto != null)
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
                PaymentRequestMessage paymentRequestMessage = new PaymentRequestMessage() { 
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
                    await _messageBus.PublishMessage(paymentRequestMessage, _subPaymentTopicName);
                    await args.CompleteMessageAsync(args.Message);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }
}
