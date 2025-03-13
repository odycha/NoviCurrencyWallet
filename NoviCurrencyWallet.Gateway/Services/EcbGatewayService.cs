using NoviCurrencyWallet.Gateway.Configurations;
using NoviCurrencyWallet.Gateway.Contracts;
using NoviCurrencyWallet.Gateway.Models;
using Microsoft.Extensions.Options;
using System.Xml.Serialization;
using System.Xml;

namespace NoviCurrencyWallet.Gateway.Services
{
	public class EcbGatewayService : IEcbGatewayService
	{

		private readonly HttpClient _httpClient;
		private readonly EcbGatewayOptions _options;
		public EcbGatewayService(IHttpClientFactory httpClientFactory, IOptions<EcbGatewayOptions> options)
		{
			_httpClient = httpClientFactory.CreateClient("EcbClient");
			_options = options.Value;
		}

		public async Task<EcbCube> GetExchangeRatesAsync()
		{
			var url = _options.BaseUrl;
			var response = await _httpClient.GetAsync(url);

			if (!response.IsSuccessStatusCode)
			{
				throw new HttpRequestException($"Failed to fetch exchange rates. Status Code: {response.StatusCode}");
			}


			string xmlData = await response.Content.ReadAsStringAsync();

			// 🚨 Log the XML response for debugging
			Console.WriteLine("🔍 Raw XML Response from ECB:");
			Console.WriteLine(xmlData);

			return DeserializeXmlStringToObject(xmlData);
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

	}
}






//if i create an instance of http client then every time a connection is established a new instance is going to be used
//this will lead to the socket exhaustion problem
//1 so i can use a static constructor for an application singleton
//2 or http client factory
//3 or wrap the whole getting of the api inside a using statement so it terminates afterwards


//1 To use IHttpClientFactory i must install Microsoft.Extensions.Http in the Gateway project
//2 Also register  the Gateway in the API Project