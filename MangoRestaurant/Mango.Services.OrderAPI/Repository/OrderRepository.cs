using Mango.Services.OrderAPI.DbContexts;
using Mango.Services.OrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.OrderAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        // Accoring to the video the dbcontext needs to be singleton per the requirements of the service bus
        // will investigate the case
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public OrderRepository(DbContextOptions<ApplicationDbContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;

        }
        public async Task<bool> AddOrder(OrderHeader orderHeader)
        {
            try
            {
                await using var _db = new ApplicationDbContext(_dbContextOptions);
                _db.OrderHeaders.Add(orderHeader);
                await _db.SaveChangesAsync();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public Task GetAllOrderdByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public Task GetDetailsOrderdByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateOrderPaymentStatus(int orderHeaderId, bool paid)
        {
            try
            {
                await using var _db = new ApplicationDbContext(_dbContextOptions);
                var orderHeader = await _db.OrderHeaders.FirstOrDefaultAsync(x => x.OrderHeaderId == orderHeaderId);
                if (orderHeader != null)
                {
                    orderHeader.PaymentStatus = paid;
                    await _db.SaveChangesAsync();
                }
            }catch(Exception ex)
            {
                return;
            }
        }
    }
}
