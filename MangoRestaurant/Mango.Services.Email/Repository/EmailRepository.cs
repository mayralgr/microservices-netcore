using Mango.Services.Email.DbContexts;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.Email.Repository
{
    public class EmailRepository : IEmailRepository
    {
        // Accoring to the video the dbcontext needs to be singleton per the requirements of the service bus
        // will investigate the case
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public EmailRepository(DbContextOptions<ApplicationDbContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;

        }

        public async Task SendAndLogEmail(UpdatePaymentResultMessage message)
        {
            // implement an email sender or call some library for it
            EmailLog emailLog = new EmailLog()
            {
                Email = message.Email,
                EmailSent = DateTime.Now,
                Log = $"Order - {message.OrderId} has been created sucessfully"
            };

            await using var _db = new ApplicationDbContext(_dbContextOptions);
            _db.EmailLogs.Add(emailLog);
            await _db.SaveChangesAsync();
        }
    }
}
