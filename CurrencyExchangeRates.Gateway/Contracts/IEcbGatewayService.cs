using CurrencyExchangeRates.Gateway.Models;
using System.Reflection.PortableExecutable;

namespace CurrencyExchangeRates.Gateway.Contracts
{
	public interface IEcbGatewayService
	{
		//IN INTERFACES METHODS NO ACCESS MODIFIER
		Task<EcbCube> GetExchangeRatesAsync();
		EcbCube DeserializeXmlStringToObject(string xmlData);
	}
}
