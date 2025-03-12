using CurrencyExchangeRates.Gateway.Configurations;
using CurrencyExchangeRates.Core.Configurations.Options;
using CurrencyExchangeRates.Gateway.Contracts;
using CurrencyExchangeRates.Gateway.Services;
using CurrencyExchangeRates.Jobs;

namespace CurrencyExchangeRates.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			//Call the job method from the API
			//I ADDED PROJECT REFERENCE TO THE JOBS PROJECT
			builder.Services.AddInfrastructure();

			//bind the settings classes to appsettings.json
			builder.Services.Configure<EcbGatewayOptions>(builder.Configuration.GetSection("EcbGateway"));
			builder.Services.Configure<CurrencyRateJobOptions>(builder.Configuration.GetSection("CurrencyRateJob"));

			//Register the EcbGatewayService
			builder.Services.AddHttpClient("EcbClient", client =>
			{
				client.BaseAddress = new Uri("https://www.ecb.europa.eu/");
				client.DefaultRequestHeaders.Add("Accept", "application/xml");
			});

			builder.Services.AddScoped<IEcbGatewayService, EcbGatewayService>();



			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
