using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NoviCurrencyWallet.Core.Configurations.Options;
using NoviCurrencyWallet.Gateway.Contracts;
using Quartz;
using System.Data;

namespace NoviCurrencyWallet.Jobs;


[DisallowConcurrentExecution]
public class RetrieveRatesBackgroundJob : IJob
{
	private readonly IEcbGatewayService _gatewayService;
	private readonly ConnectionStrings _options;
	private readonly IMemoryCache _cache;
	private readonly ILogger<RetrieveRatesBackgroundJob> _logger;

	public RetrieveRatesBackgroundJob(IEcbGatewayService gatewayService,
		IOptions<ConnectionStrings> options,
		IMemoryCache cache,
		ILogger<RetrieveRatesBackgroundJob> logger)
	{
		_gatewayService = gatewayService;
		_options = options.Value;
		_cache = cache;
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		_logger.LogInformation("🔍RetrieveRatesBackgroundJob executed.");

		var rates = await _gatewayService.GetExchangeRatesAsync();

		_logger.LogInformation($"🔍Fetched {rates?.Rates?.Count ?? 0} exchange rates.");

		// If rates are empty, log an error
		if (rates == null || rates.Rates == null || rates.Rates.Count == 0)
		{
			_logger.LogInformation("🔍No exchange rates received from ECB API!");

			return;  // Stop execution to avoid running an empty SQL query
		}

		var connectionString = _options.NoviCurrencyWalletDbConnectionString;

		using var connection = new SqlConnection(connectionString);           // Establishes a new SQL database connection using the retrieved connection string.
		await connection.OpenAsync();                                           // Opens the database connection asynchronously.

		var commandText = @"
        MERGE INTO CurrencyRates AS target
        USING (VALUES {0}) AS source (Currency, Rate, Date)
        ON target.Currency = source.Currency AND target.Date = source.Date
        WHEN MATCHED THEN
            UPDATE SET Rate = source.Rate
        WHEN NOT MATCHED THEN
            INSERT (Currency, Rate, Date) VALUES (source.Currency, source.Rate, source.Date);";

		var valueStrings = new List<string>(); // Stores the dynamically generated VALUES portion of the SQL query.
		var parameters = new List<SqlParameter>(); // Stores the SQL parameters to prevent SQL injection.

		int index = 0; // Counter for uniquely naming SQL parameters in the loop.
		foreach (var rate in rates.Rates) // Iterates through each currency exchange rate.
		{
			var currencyParam = new SqlParameter($"@Currency{index}", SqlDbType.VarChar) { Value = rate.Currency };
			var rateParam = new SqlParameter($"@Rate{index}", SqlDbType.Decimal) { Value = rate.Rate };
			var dateParam = new SqlParameter($"@Date{index}", SqlDbType.Date) { Value = rates.Date };

			valueStrings.Add($"(@Currency{index}, @Rate{index}, @Date{index})");
			parameters.AddRange(new[] { currencyParam, rateParam, dateParam });

			index++;
		}

		if (valueStrings.Count > 0)
		{
			_logger.LogInformation($"🔍Preparing to insert/update {valueStrings.Count} currency rates into the database.");

			var finalCommandText = string.Format(commandText, string.Join(", ", valueStrings));
			using var command = new SqlCommand(finalCommandText, connection);
			command.Parameters.AddRange(parameters.ToArray());
			await command.ExecuteNonQueryAsync();

			// Invalidate Cache After Updating Database**
			_cache.Remove("ECB_ExchangeRates");

			_logger.LogInformation("🔍Cache invalidated after updating exchange rates.");
		}
		else
		{
			_logger.LogInformation("🔍No valid exchange rate data to insert/update.");
		}
	}
}







//What is a SQL Parameter?
//A SQL Parameter is a placeholder used in SQL queries to safely pass values into a statement.It prevents SQL injection, 
//improves performance, and ensures type safety by allowing the database to process the data efficiently.