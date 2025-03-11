
using CurrencyExchangeRates.Gateway.Contracts;
using CurrencyExchangeRates.Gateway.Services;

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
