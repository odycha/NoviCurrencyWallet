using NoviCurrencyWallet.Gateway.Models;
using System.Reflection.PortableExecutable;

namespace NoviCurrencyWallet.Gateway.Contracts
{
	public interface IEcbGatewayService
	{
		//IN INTERFACES METHODS NO ACCESS MODIFIER
		Task<EcbCube> GetExchangeRatesAsync();
		EcbCube DeserializeXmlStringToObject(string xmlData);
	}
}
