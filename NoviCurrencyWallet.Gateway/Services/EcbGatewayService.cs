using NoviCurrencyWallet.Gateway.Configurations;
using NoviCurrencyWallet.Gateway.Contracts;
using NoviCurrencyWallet.Gateway.Models;
using Microsoft.Extensions.Options;
using System.Xml.Serialization;

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
			return DeserializeXmlStringToObject(xmlData);
		}

		public EcbCube DeserializeXmlStringToObject(string xmlData)
		{
			var serializer = new XmlSerializer(typeof(EcbEnvelope));              //It is used to convert XML into a C# object (deserialization) or vice versa (serialization). typeof(EcbCurrencyRateDto) tells the serializer which class the XML should be converted into.

			using (var reader = new StringReader(xmlData))                               //It treats a string as a readable stream, which means it allows XMLSerializer to read the XML data like a file. This is required because XmlSerializer.Deserialize() expects a stream or reader as input, not a plain string.
			{
				var envelope = (EcbEnvelope)serializer.Deserialize(reader);				//The Deserialize method reads from reader, processes the XML, and creates an instance of EcbCurrencyRateDto
				return envelope?.CubeContainer?.DateCube;                               //ach ? ensures that if any of the preceding objects are null, the expression short-circuits and returns null instead of throwing an exception.
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