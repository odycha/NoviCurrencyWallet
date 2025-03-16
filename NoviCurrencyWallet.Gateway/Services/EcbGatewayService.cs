using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NoviCurrencyWallet.Gateway.Configurations;
using NoviCurrencyWallet.Gateway.Contracts;
using NoviCurrencyWallet.Gateway.Models;
using System.Xml;
using System.Xml.Serialization;

namespace NoviCurrencyWallet.Gateway.Services;

public class EcbGatewayService : IEcbGatewayService
{
	private readonly HttpClient _httpClient;
	private readonly IOptionsSnapshot<EcbGatewayOptions> _optionsSnapshot;
	private readonly ILogger<EcbGatewayService> _logger;
	private readonly IMemoryCache _cache;

	public EcbGatewayService(
		IHttpClientFactory httpClientFactory,
		IOptionsSnapshot<EcbGatewayOptions> optionsSnapshot,
		ILogger<EcbGatewayService> logger,
		IMemoryCache cache)
	{
		_httpClient = httpClientFactory.CreateClient("EcbClient");
		_optionsSnapshot = optionsSnapshot;
		_logger = logger;
		_cache = cache;
	}

	public async Task<EcbCube> GetExchangeRatesAsync()
	{
		var options = _optionsSnapshot.Value; // Fetch latest settings dynamically
		_httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
		var cacheKey = options.CacheKey;
		var cacheDuration = TimeSpan.FromMinutes(options.CacheDurationMinutes);
		var url = options.BaseUrl;

		_logger.LogInformation("🔍 EcbGatewayService: Fetching exchange rates from {Url}", url);

		var response = await _httpClient.GetAsync(url);

		//No need for manual status code check—this will throw on failure
		response.EnsureSuccessStatusCode();

		string xmlData = await response.Content.ReadAsStringAsync();

		_logger.LogInformation("🔍EcbGatewayService: Raw XML Response from ECB:\n{XmlData}", xmlData);

		var objectRates = DeserializeXmlStringToObject(xmlData);

		// Store in cache
		if (objectRates != null)
		{
			_cache.Set(cacheKey, objectRates, cacheDuration);

			_logger.LogInformation("🔍EcbGatewayService: Exchange rates cached successfully.");
		}

		return objectRates;
	}

	public EcbCube DeserializeXmlStringToObject(string xmlData)
	{
		var serializer = new XmlSerializer(typeof(EcbEnvelope));

		var xmlReaderSettings = new XmlReaderSettings
		{
			IgnoreWhitespace = true,
			IgnoreComments = true
		};

		using (var stringReader = new StringReader(xmlData))
		using (var xmlReader = XmlReader.Create(stringReader, xmlReaderSettings))
		{
			var envelope = (EcbEnvelope)serializer.Deserialize(xmlReader);
			return envelope?.CubeContainer?.DateCube;
		}
	}

	public EcbCube GetCachedExchangeRates()
	{
		var cacheKey = _optionsSnapshot.Value.CacheKey;

		// Check if cache contains exchange rates
		if (_cache.TryGetValue(cacheKey, out EcbCube cachedRates))
		{
			_logger.LogInformation("🔍EcbGatewayService: Returning exchange rates from cache.");

			return cachedRates;
		}
		else
		{
			_logger.LogWarning("🔍 EcbGatewayService: No exchange rates found in cache.");

			return null;
		}
	}
}






//if i create an instance of http client then every time a connection is established a new instance is going to be used
//this will lead to the socket exhaustion problem
//1 so i can use a static constructor for an application singleton
//2 or http client factory
//3 or wrap the whole getting of the api inside a using statement so it terminates afterwards


//1 To use IHttpClientFactory i must install Microsoft.Extensions.Http in the Gateway project
//2 Also register  the Gateway in the API Project





//What do we mean by "ensures requests to the ECB API do not hang indefinitely"?
//When your application makes a request to an external API(like the European Central Bank (ECB) API) using HttpClient,
//it waits for a response.However, if the ECB server is slow, unresponsive, or experiencing issues, your request might
//take an unpredictable amount of time to return or might never return at all.
//This is called "hanging"—your application is stuck waiting forever for a response that may never come.

//How does a Timeout prevent this?
//A timeout is a time limit that forces your request to fail fast if the ECB API takes too long to respond.

//If the ECB API doesn’t respond within the set time (e.g., 30 seconds), your request is automatically canceled.
//Your application doesn’t hang forever and can handle the failure properly (e.g., retry, fallback, log the error).




//Why Not Use private readonly Fields?
//1️IOptionsSnapshot<T> is Designed for Dynamic Configurations
//IOptionsSnapshot<T> fetches the latest configuration on every request.
//If you store _cacheKey, _cacheDuration, and _url as readonly fields in the constructor,
//they will be set once at startup and never update dynamically.