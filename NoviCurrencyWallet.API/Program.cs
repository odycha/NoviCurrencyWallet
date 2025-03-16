using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

		// Bind EcbGatewayOptions Correctly
		builder.Services.AddOptions<EcbGatewayOptions>()
			.Bind(builder.Configuration.GetSection("EcbGateway"))
			.ValidateDataAnnotations() // Optional: Ensures config values are valid
			.ValidateOnStart(); // Ensures settings are validated at app startup

		// Bind the settings classes to appsettings.json
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

		// Register the Job Infrastructure
		builder.Services.AddInfrastructure();

		//Register EcbGatewayService with HttpClient using IOptions<EcbGatewayOptions>
		builder.Services.AddHttpClient("EcbClient", (provider, client) =>
		{
			var options = provider.GetRequiredService<IOptionsMonitor<EcbGatewayOptions>>().CurrentValue;
			client.BaseAddress = new Uri(options.BaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/xml");
		});


		// Register Services
		builder.Services.AddScoped<IEcbGatewayService, EcbGatewayService>();
		builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
		builder.Services.AddScoped<IWalletsRepository, WalletRepository>();

		//Caching
		builder.Services.AddMemoryCache();

		//Rate limiting
		var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
		int permitLimit = rateLimitConfig.GetValue<int>("PermitLimit");
		int windowSeconds = rateLimitConfig.GetValue<int>("WindowSeconds");

		builder.Services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
			options.AddPolicy("fixed", httpContext =>
				RateLimitPartition.GetFixedWindowLimiter(
					partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
					factory: _ => new FixedWindowRateLimiterOptions
					{
						PermitLimit = permitLimit,
						Window = TimeSpan.FromSeconds(windowSeconds)
					}));
		});

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

		
		var app = builder.Build();

		app.UseSerilogRequestLogging();

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
