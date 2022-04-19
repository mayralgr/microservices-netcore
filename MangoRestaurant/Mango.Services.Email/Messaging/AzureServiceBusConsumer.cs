using Azure.Messaging.ServiceBus;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Models;
using Mango.Services.Email.Repository;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.Email.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string _subConnectionString;
        private readonly string _subcriptionEmail;
        private readonly string _orderUpdatePaymentResultTopic;
        private readonly EmailRepository _emailRepository;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor orderUpdatePaymentsStatusProcessor;
        public AzureServiceBusConsumer(IConfiguration config, EmailRepository emailRepository)
        {
            _emailRepository = emailRepository;
            _configuration = config;
            _subConnectionString = _configuration.GetConnectionString("ServiceBus");
            _subcriptionEmail = _configuration.GetValue<string>("EmailSubscriptionName");
            _orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");
            var client = new ServiceBusClient(_subConnectionString);
            orderUpdatePaymentsStatusProcessor = client.CreateProcessor(_orderUpdatePaymentResultTopic, _subcriptionEmail);

        }

        public async Task Start()
        {
            orderUpdatePaymentsStatusProcessor.ProcessMessageAsync += OnOrderPaymentUpdateReceived;
            orderUpdatePaymentsStatusProcessor.ProcessErrorAsync += ErrorEventHandler;
            await orderUpdatePaymentsStatusProcessor.StartProcessingAsync();
        }
        public async Task Stop()
        {
            await orderUpdatePaymentsStatusProcessor.StopProcessingAsync();
            await orderUpdatePaymentsStatusProcessor.DisposeAsync();
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
            try
            {
                if (paymentResultMessage != null)
                {
                    await _emailRepository.SendAndLogEmail(paymentResultMessage);
                    await args.CompleteMessageAsync(args.Message);

                }
            }
            catch(Exception ex)
            {
                throw;
            }


        }

    }
}
