using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NoviCurrencyWallet.Gateway.Contracts;
using NoviCurrencyWallet.Gateway.Models;

namespace NoviCurrencyWallet.Gateway.Services;

public class CachedEcbGatewayService : IEcbGatewayService
{
	private readonly IEcbGatewayService _innerService;
	private readonly IMemoryCache _cache;
	private readonly ILogger<CachedEcbGatewayService> _logger;
	private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10); // Cache duration

	public CachedEcbGatewayService(IEcbGatewayService innerService, IMemoryCache cache, ILogger<CachedEcbGatewayService> logger)
	{
		_innerService = innerService;
		_cache = cache;
		_logger = logger;
	}

	public async Task<EcbCube> GetExchangeRatesAsync()
	{
		string cacheKey = "ECB_ExchangeRates";

		// Check if cache contains exchange rates
		if (_cache.TryGetValue(cacheKey, out EcbCube cachedRates))
		{
			_logger.LogInformation("🔍Returning exchange rates from cache.");

			return cachedRates;
		}

		// Fetch exchange rates from the original service
		var rates = await _innerService.GetExchangeRatesAsync();

		// Store in cache for future use
		_cache.Set(cacheKey, rates, _cacheDuration);

		_logger.LogInformation("🔍Exchange rates cached successfully.");

		return rates;
	}

	public EcbCube DeserializeXmlStringToObject(string xmlData)
	{
		return _innerService.DeserializeXmlStringToObject(xmlData);
	}
}
