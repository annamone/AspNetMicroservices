using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;

namespace Discount.API.Extensions
{
	public static class HostExtensions
	{
		public static IServiceProvider MigrateDatabase<TContext>(this IServiceProvider sp, int? retry = 0)
		{
			int retryForAvailability = retry.Value;

			var configuration = sp.GetRequiredService<IConfiguration>();
			var logger = sp.GetRequiredService<ILogger<TContext>>();

			try
			{
				logger.LogInformation("Migrating postgresql database");

				using var connection = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
				connection.Open();

				using var command = new NpgsqlCommand
				{
					Connection = connection
				};

				command.CommandText = "DROP TABLE IF EXISTS Coupon";
				command.ExecuteNonQuery();

				command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY,
														    ProductName VARCHAR(24) NOT NULL,
                                                            Description TEXT,
															Amount INT)";
				command.ExecuteNonQuery();

				command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150);";
				command.ExecuteNonQuery();

				command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100);";
				command.ExecuteNonQuery();

				logger.LogInformation("Migrated postresql database.");
			}
			catch (NpgsqlException ex)
			{
				logger.LogError(ex, "An error occurred while migrating the postresql database");

				if (retryForAvailability < 50)
				{
					retryForAvailability++;
					System.Threading.Thread.Sleep(2000);
					MigrateDatabase<TContext>(sp, retryForAvailability);
				}
			}

			return sp;
		}
	}
}
