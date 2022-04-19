using Mango.MessageBus;

namespace Mango.Services.PaymentAPI.RabbitMQSender
{
    public interface IRabbitMQPaymentMessageSender
    {
        void SendMessage(BaseMessage baseMessage, String direct, String q1, String q2);
    }
}
