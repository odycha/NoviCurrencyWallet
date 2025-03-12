
namespace NoviCurrencyWallet.Data.Entities;

public class CurrencyRate
{
    public long Id { get; set; }
    public string Currency { get; set; }
	public decimal Rate { get; set; }
	public DateTime Date { get; set; }

}
