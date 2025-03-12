namespace NoviCurrencyWallet.Data.Entities;

public class Wallet
{
	public long Id { get; set; }
	public decimal Balance { get; set; }
	public string Currency { get; set; }
}
