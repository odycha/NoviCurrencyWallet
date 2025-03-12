using System.ComponentModel.DataAnnotations;

namespace NoviCurrencyWallet.Core.Models.Wallet;

public class CreateWalletDto
{
	[Required]
	[Range(0.01, double.MaxValue, ErrorMessage = "Balance must be greater than zero.")]
	public decimal Balance { get; set; }

	[Required]
	[StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters.")]
	[RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be in uppercase.")]
	public string Currency { get; set; }

}
