using NoviCurrencyWallet.Gateway.Models;

namespace NoviCurrencyWallet.Gateway.Contracts;

public interface IEcbGatewayService
{
	//IN INTERFACES METHODS NO ACCESS MODIFIER
	Task<EcbCube> GetExchangeRatesAsync();
	EcbCube DeserializeXmlStringToObject(string xmlData);
	EcbCube GetCachedExchangeRates();
}
