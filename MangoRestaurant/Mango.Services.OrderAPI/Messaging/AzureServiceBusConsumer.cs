using Azure.Messaging.ServiceBus;
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
        private readonly string _subTopicName;
        private readonly string _subSubscriptionCheckOutName;
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor _checkOutProcessor; 
        public AzureServiceBusConsumer(IConfiguration config, OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
            _configuration = config;
            _subConnectionString = _configuration.GetConnectionString("ServiceBus");
            _subTopicName = _configuration.GetValue<string>("Azuretopic");
            _subSubscriptionCheckOutName = _configuration.GetValue<string>("SubscriptionCheckOutName");
            var client = new ServiceBusClient(_subConnectionString);
            _checkOutProcessor = client.CreateProcessor(_subTopicName, _subSubscriptionCheckOutName);
            
        }

        public async Task Start()
        {
            _checkOutProcessor.ProcessMessageAsync += OnCheckOutMessageReceived;
            _checkOutProcessor.ProcessErrorAsync += ErrorEventHandler;
            await _checkOutProcessor.StartProcessingAsync();
        }
        public async Task Stop()
        {
            await _checkOutProcessor.StopProcessingAsync();
            await _checkOutProcessor.DisposeAsync();
        }
        private Task ErrorEventHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
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
                    Phone = checkoutHeaderDto.Phone
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
            }
        }
    }
}
