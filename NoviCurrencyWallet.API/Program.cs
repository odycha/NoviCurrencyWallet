using NoviCurrencyWallet.Gateway.Configurations;
using NoviCurrencyWallet.Core.Configurations.Options;
using NoviCurrencyWallet.Gateway.Contracts;
using NoviCurrencyWallet.Gateway.Services;
using NoviCurrencyWallet.Jobs;
using Microsoft.EntityFrameworkCore;
using NoviCurrencyWallet.Data;
using Microsoft.Extensions.Options;
using NoviCurrencyWallet.Core.Configurations;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Repository;

namespace NoviCurrencyWallet.API;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var connectionString = builder.Configuration.GetConnectionString("NoviCurrencyWalletDbConnectionString");

		// Bind the settings classes to appsettings.json (Fix 2: Ensure the right section is used)
		builder.Services.Configure<EcbGatewayOptions>(builder.Configuration.GetSection("EcbGateway"));
		builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("CurrencyRateJob"));


		//Set up EF and point the database
		builder.Services.AddDbContext<NoviCurrencyWalletDbContext>(options =>
		{
			options.UseSqlServer(connectionString);
		});


		// Add services to the container.
		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		builder.Services.AddAutoMapper(typeof(MapperConfig));

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

		builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
		builder.Services.AddScoped<IWalletsRepository, WalletRepository>();

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
