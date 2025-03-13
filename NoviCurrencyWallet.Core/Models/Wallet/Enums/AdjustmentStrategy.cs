using System.Text.Json.Serialization;

namespace NoviCurrencyWallet.Core.Models.Wallet.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdjustmentStrategy
{
	AddFundsStrategy = 0,
	SubtractFundsStrategy = 1,
	ForceSubtractFundsStrategy = 2
}