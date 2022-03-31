using Mango.Services.OrderAPI.Models;

namespace Mango.Services.OrderAPI.Repository
{
    public interface IOrderRepository
    {
        Task<bool> AddOrder(OrderHeader orderHeader);

        Task UpdateOrderPaymentStatus(int orderHeaderId, bool paid);

        // Get all of the orders for a user
        Task GetAllOrderdByUserId(int userId);
        // Get the details byUserId
        Task GetDetailsOrderdByUserId(int userId);   
    }
}
