using Microsoft.EntityFrameworkCore;
using NoviCurrencyWallet.Core.Configurations;
using NoviCurrencyWallet.Core.Configurations.Options;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Middleware;
using NoviCurrencyWallet.Core.Repository;
using NoviCurrencyWallet.Data;
using NoviCurrencyWallet.Gateway.Configurations;
using NoviCurrencyWallet.Gateway.Contracts;
using NoviCurrencyWallet.Gateway.Services;
using NoviCurrencyWallet.Jobs;
using Serilog;
using System.Threading.RateLimiting;

namespace NoviCurrencyWallet.API;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var connectionString = builder.Configuration.GetConnectionString("NoviCurrencyWalletDbConnectionString");

		// Bind the settings classes to appsettings.json
		builder.Services.Configure<EcbGatewayOptions>(builder.Configuration.GetSection("EcbGateway"));
		builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));

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

		//Serilog and Seq Configuration
		builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

		//Caching
		builder.Services.AddMemoryCache();

		builder.Services.AddScoped<IEcbGatewayService, EcbGatewayService>();
		//builder.Services.Decorate<IEcbGatewayService, CachedEcbGatewayService>(); // Scrutor required

		builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
		builder.Services.AddScoped<IWalletsRepository, WalletRepository>();

		//Rate limiting
		builder.Services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			options.AddPolicy("fixed", httpContext =>
				RateLimitPartition.GetFixedWindowLimiter(
					partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
					factory: _ => new FixedWindowRateLimiterOptions
					{
						PermitLimit = 10,
						Window = TimeSpan.FromSeconds(10)
					}));
		});

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

		//Cors configuration(2/2)
		app.UseCors("AllowAll");

		app.UseHttpsRedirection();

		app.UseRateLimiter();

		app.UseAuthorization();

		app.MapControllers();

		app.Run();
	}
}
