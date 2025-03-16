namespace NoviCurrencyWallet.Core.Exceptions;

public class BadRequestException : ApplicationException
{
	public BadRequestException(string message) : base(message)
	{

	}
}


//alternative : ArgumentException or InvalidOperationException