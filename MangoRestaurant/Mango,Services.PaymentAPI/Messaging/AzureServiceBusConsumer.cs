using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.PaymentAPI.Messages;
using Newtonsoft.Json;
using PaymentProcessor;
using System.Text;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string _subConnectionString;
        private readonly string _subPaymentTopicName;
        private readonly string _subSubscriptioPaymentName;
        private readonly string _orderUpdatePaymentResultTopic;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor _orderPaymentProcessor;
        private readonly IMessageBus _messageBus;
        private readonly IProcessPayment _processPayment;
        public AzureServiceBusConsumer(IConfiguration config, IMessageBus messageBus, IProcessPayment processPayment)
        {
            _processPayment = processPayment;
            _configuration = config;
            _subConnectionString = _configuration.GetConnectionString("ServiceBus");
            _subPaymentTopicName = _configuration.GetValue<string>("PaymentTopic");
            _subSubscriptioPaymentName = _configuration.GetValue<string>("SubscriptionPaymentName");
            _orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");
            var client = new ServiceBusClient(_subConnectionString);
            _orderPaymentProcessor = client.CreateProcessor(_subPaymentTopicName, _subSubscriptioPaymentName);
            _messageBus = messageBus;
        }

        public async Task Start()
        {
            _orderPaymentProcessor.ProcessMessageAsync += ProcessPayment;
            _orderPaymentProcessor.ProcessErrorAsync += ErrorEventHandler;
            await _orderPaymentProcessor.StartProcessingAsync();
        }
        public async Task Stop()
        {
            await _orderPaymentProcessor.StopProcessingAsync();
            await _orderPaymentProcessor.DisposeAsync();
        }
        private Task ErrorEventHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
        private async Task ProcessPayment(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            PaymentRequestMessage paymentRequestMessage = JsonConvert.DeserializeObject<PaymentRequestMessage>(body);
            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage updatePaymentResultMessage = new UpdatePaymentResultMessage()
            {
                Status = result,
                OrderId = paymentRequestMessage.OrderId,
                Email = paymentRequestMessage.Email

            };
            // will send it to a new topic
            try
            {
                await _messageBus.PublishMessage(updatePaymentResultMessage, _orderUpdatePaymentResultTopic);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
