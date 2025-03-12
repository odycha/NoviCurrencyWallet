using NoviCurrencyWallet.Gateway.Configurations;
using NoviCurrencyWallet.Core.Configurations.Options;
using NoviCurrencyWallet.Gateway.Contracts;
using NoviCurrencyWallet.Gateway.Services;
using NoviCurrencyWallet.Jobs;
using Microsoft.EntityFrameworkCore;
using NoviCurrencyWallet.Data;
using Microsoft.Extensions.Options;

namespace NoviCurrencyWallet.API;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Bind configuration using Options Pattern
		builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("ConnectionStrings"));

		// Bind the settings classes to appsettings.json (Fix 2: Ensure the right section is used)
		builder.Services.Configure<EcbGatewayOptions>(builder.Configuration.GetSection("EcbGateway"));
		builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("CurrencyRateJob"));


		//Set up EF and point the database
		builder.Services.AddDbContext<NoviCurrencyWalletDbContext>(options =>
		{
			var serviceProvider = builder.Services.BuildServiceProvider();
			var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
			options.UseSqlServer(dbOptions.ConnectionString);
		});



		// Add services to the container.
		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		// Call the job method from the API
		// Ensure project reference to Jobs project exists
		builder.Services.AddInfrastructure();


		//Register the EcbGatewayService
		builder.Services.AddHttpClient("EcbClient", client =>
		{
			client.BaseAddress = new Uri("https://www.ecb.europa.eu/");
			client.DefaultRequestHeaders.Add("Accept", "application/xml");
		});
		builder.Services.AddScoped<IEcbGatewayService, EcbGatewayService>();


		//Cors configuration(1/2)
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("AllowAll",
				b => b.AllowAnyHeader()
				.AllowAnyOrigin()
				.AllowAnyMethod());
		});


		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}



		//Cors configuration(2/2)
		app.UseCors("AllowAll");

		app.UseHttpsRedirection();
		app.UseAuthorization();
		app.MapControllers();
		app.Run();
	}
}
