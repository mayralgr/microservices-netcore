using Mango.Services.Email.DbContexts;
using Mango.Services.Email.Extensions;
using Mango.Services.Email.Messaging;
using Mango.Services.Email.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddDbContext<ApplicationDbContext>(o => o.UseSqlServer
    (builder.Configuration.GetConnectionString("EmailDBDefaultConnection")));

var optionBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionBuilder.UseSqlServer(builder.Configuration.GetConnectionString("EmailDBDefaultConnection"));

builder.Services.AddSingleton(new EmailRepository(optionBuilder.Options));
//builder.Services.AddSingleton<IMessageBus, AzureServiceBus>();
builder.Services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseAzureServiceBusConsumer();
app.Run();