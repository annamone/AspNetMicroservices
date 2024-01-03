using EventBus.Messages.Common;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ordering.API.EventBusConsumer;
using Ordering.API.Extensions;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;

namespace Ordering.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddApplicationServices();
			builder.Services.AddInfrastructureServices(builder.Configuration);

			// MassTransit-RabbitMQ Configuration
			builder.Services.AddMassTransit(config =>
			{
				config.AddConsumer<BasketCheckoutConsumer>();
				config.UsingRabbitMq((ctx, cfg) =>
				{
					cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
					cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, c =>
					{
						c.ConfigureConsumer<BasketCheckoutConsumer>(ctx);
					});
				});
			});

			// General Configuration
			builder.Services.AddAutoMapper(typeof(Program));
			builder.Services.AddScoped<BasketCheckoutConsumer>();

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();
			app.Services.MigrateDatabase<OrderContext>((context, services) =>
			{
				var logger = services.GetService<ILogger<OrderContextSeed>>();
				OrderContextSeed.SeedAsync(context, logger).Wait();
			});

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}