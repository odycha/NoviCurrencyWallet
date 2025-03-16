using NoviCurrencyWallet.Core.Models.Wallet.Enums;
using System.ComponentModel.DataAnnotations;

namespace NoviCurrencyWallet.Core.Models.Wallet;

public class UpdateWalletBalanceDto
{
	[Required]
	public long Id { get; set; }

	[Required]
	[Range(0.01, double.MaxValue, ErrorMessage = "Adjustment amount must be greater than zero.")]
	public decimal Amount { get; set; } 

	[Required]
	[StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters.")]
	[RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be in uppercase")]
	public string Currency { get; set; }

	[Required]
	[EnumDataType(typeof(AdjustmentStrategy), ErrorMessage = "Invalid adjustment strategy.")]
	public AdjustmentStrategy Strategy { get; set; } 
}


//Using double.MaxValue in [Range] has no real impact in a financial app because currency transactions will never approach that magnitude.
//double.MaxValue is used because the [Range] attribute requires double, but it is not the best choice for a decimal field.