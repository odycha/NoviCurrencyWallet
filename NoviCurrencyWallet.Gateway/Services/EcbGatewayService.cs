using NoviCurrencyWallet.Gateway.Configurations;
using NoviCurrencyWallet.Gateway.Contracts;
using NoviCurrencyWallet.Gateway.Models;
using Microsoft.Extensions.Options;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace NoviCurrencyWallet.Gateway.Services;

public class EcbGatewayService : IEcbGatewayService
{

	private readonly HttpClient _httpClient;
	private readonly EcbGatewayOptions _options;
	private readonly ILogger<EcbGatewayService> _logger;
	private readonly IMemoryCache _cache;
	private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
	private readonly string _cacheKey = "ECB_ExchangeRates";

	public EcbGatewayService(IHttpClientFactory httpClientFactory, IOptions<EcbGatewayOptions> options, ILogger<EcbGatewayService> logger, IMemoryCache cache)
	{
		_httpClient = httpClientFactory.CreateClient("EcbClient");
		_options = options.Value;
		_logger = logger;
		_cache = cache;
	}

	public async Task<EcbCube> GetExchangeRatesAsync()
	{
		var url = _options.BaseUrl;
		var response = await _httpClient.GetAsync(url);

		if (!response.IsSuccessStatusCode)
		{
			throw new HttpRequestException($"EcbGatewayService: Failed to fetch exchange rates. Status Code: {response.StatusCode}");
		}

		string xmlData = await response.Content.ReadAsStringAsync();

		_logger.LogInformation("🔍EcbGatewayService: Raw XML Response from ECB:\n{XmlData}", xmlData);

		var objectRates = DeserializeXmlStringToObject(xmlData);

		// Store in cache for future use
		_cache.Set(_cacheKey, objectRates, _cacheDuration);

		_logger.LogInformation("🔍EcbGatewayService: Exchange rates cached successfully.");

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

	//does this have to be async?
	public EcbCube GetCachedExchangeRates()
	{
		// Check if cache contains exchange rates
		if (_cache.TryGetValue(_cacheKey, out EcbCube cachedRates))
		{
			_logger.LogInformation("🔍EcbGatewayService: Returning exchange rates from cache.");

			return cachedRates;
		}
		else
		{
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