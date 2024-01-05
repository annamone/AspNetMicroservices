using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Threading.Tasks;

namespace OcelotApiGw
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
			{
				config.AddJsonFile($"ocelot.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true);
			});
			builder.Services.AddOcelot(builder.Configuration)
				.AddCacheManager(settings =>
				{
					settings.WithDictionaryHandle();
				});

			builder.Services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
				loggingBuilder.AddConsole();
				loggingBuilder.AddDebug();
			});

			var app = builder.Build();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync("Hello World!");
				});
			});

			await app.UseOcelot();

			app.Run();
		}
	}
}