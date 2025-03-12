using NoviCurrencyWallet.Core.Configurations.Options;
using NoviCurrencyWallet.Gateway.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Quartz;
using System.Data;

namespace NoviCurrencyWallet.Jobs;


[DisallowConcurrentExecution]  //If the time to complete the job takes more than the Interval, we dont want another instance of the job to be created
public class RetrieveRatesBackgroundJob : IJob
{
	private readonly IEcbGatewayService _gatewayService; // Interface for fetching currency exchange rates from ECB.
	private readonly CurrencyRateJobOptions _options; // Provides access to appsettings.json configuration.

	// Constructor for dependency injection of the ECB Gateway Service and configuration settings.
	public RetrieveRatesBackgroundJob(IEcbGatewayService gatewayService, IOptions<CurrencyRateJobOptions> options)
	{
		_gatewayService = gatewayService;
		_options = options.Value;
	}

	// The Execute method is triggered by Quartz.NET to run the job.
	public async Task Execute(IJobExecutionContext context)
	{
		// Fetches the latest currency exchange rates asynchronously from the ECB Gateway Service.
		var rates = await _gatewayService.GetExchangeRatesAsync();

		// Retrieves the database connection string from appsettings.json.
		var connectionString = _options.ConnectionString;

		
		using var connection = new SqlConnection(connectionString);           // Establishes a new SQL database connection using the retrieved connection string.
		await connection.OpenAsync();                                           // Opens the database connection asynchronously.

		// SQL MERGE statement template for inserting or updating currency rates.
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
		foreach (var rate in rates) // Iterates through each currency exchange rate.
		{
			
			var currencyParam = new SqlParameter($"@Currency{index}", SqlDbType.VarChar) { Value = rate.Currency };    // Creates a SQL parameter for the currency code.


			var rateParam = new SqlParameter($"@Rate{index}", SqlDbType.Decimal) { Value = rate.Rate };    // Creates a SQL parameter for the exchange rate value.


			var dateParam = new SqlParameter($"@Date{index}", SqlDbType.Date) { Value = rate.Date };     // Creates a SQL parameter for the date associated with the exchange rate.


			valueStrings.Add($"(@Currency{index}, @Rate{index}, @Date{index})");     // Adds a placeholder for this set of values in the SQL query.


			parameters.AddRange(new[] { currencyParam, rateParam, dateParam });     // Adds the parameters to the parameter list for safe execution.

			index++; // Increments the index for unique parameter naming.
		}

		// Executes the SQL command only if there are values to insert/update.
		if (valueStrings.Count > 0)
		{
			
			var finalCommandText = string.Format(commandText, string.Join(", ", valueStrings));     // Formats the SQL command by replacing `{0}` with the actual parameterized values.

			
			using var command = new SqlCommand(finalCommandText, connection);    // Creates a SQL command using the final SQL query and the established database connection.

			
			command.Parameters.AddRange(parameters.ToArray());     // Adds all the dynamically generated parameters to the SQL command.

			
			await command.ExecuteNonQueryAsync();    // Executes the SQL MERGE statement asynchronously, performing bulk insert/update.
		}
	}
}







//What is a SQL Parameter?
//A SQL Parameter is a placeholder used in SQL queries to safely pass values into a statement.It prevents SQL injection, 
//improves performance, and ensures type safety by allowing the database to process the data efficiently.