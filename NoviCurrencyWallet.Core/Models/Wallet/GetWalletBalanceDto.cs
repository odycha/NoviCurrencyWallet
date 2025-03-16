using System.ComponentModel.DataAnnotations;

namespace NoviCurrencyWallet.Core.Models.Wallet;

public class GetWalletBalanceDto
{
	public long Id { get; set; }

	[Range(0, double.MaxValue, ErrorMessage = "Balance must be greater than zero.")]
	public decimal Balance { get; set; }

	[Required]
	[StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters.")]
	[RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be in uppercase.")]
	public string Currency { get; set; }

	[StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters.")]
	[RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be in uppercase.")]
	public string? ConvertedCurrency { get; set; } // Optional, for currency conversion results

	[Range(0, double.MaxValue, ErrorMessage = "Balance must be greater than zero.")]
	public decimal? ConvertedBalance { get; set; } // Optional, for currency conversion results
}
