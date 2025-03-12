using NoviCurrencyWallet.Core.Models.Wallet.Enums;
using System.ComponentModel.DataAnnotations;

namespace NoviCurrencyWallet.Core.Models.Wallet;

public class UpdateWalletBalanceDto
{
	[Required]
	[Range(0.01, double.MaxValue, ErrorMessage = "Adjustment amount must be greater than zero.")]
	public decimal Amount { get; set; } 

	[Required]
	[StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters.")]
	[RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be in uppercase (ISO 4217).")]
	public string Currency { get; set; }

	[Required]
	[EnumDataType(typeof(AdjustmentStrategy), ErrorMessage = "Invalid adjustment strategy.")]
	public AdjustmentStrategy Strategy { get; set; } 




}
