namespace Mango.Services.OrderAPI.Messages
{
    public class UpdatePaymentRsultMessage
    {
        public int OrderId { get; set; }
        public bool Status { get; set; }
    }
}
