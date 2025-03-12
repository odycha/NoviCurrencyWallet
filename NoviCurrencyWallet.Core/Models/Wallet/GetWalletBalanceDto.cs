﻿using System.ComponentModel.DataAnnotations;

namespace NoviCurrencyWallet.Core.Models.Wallet;

public class GetWalletBalanceDto
{
	public int Id { get; set; }

	[Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative.")]
	public decimal Balance { get; set; }

	[Required]
	[StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters.")]
	[RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be in uppercase.")]
	public string Currency { get; set; }

	public decimal? ConvertedBalance { get; set; } // Optional, for currency conversion results
	public string? ConvertedCurrency { get; set; } // Optional, for currency conversion results
}
