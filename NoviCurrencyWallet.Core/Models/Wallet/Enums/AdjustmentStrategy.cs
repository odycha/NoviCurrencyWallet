using System.Text.Json.Serialization;

namespace NoviCurrencyWallet.Core.Models.Wallet.Enums;

//ensures that when this enum is serialized to JSON (e.g., for API responses), it uses string values instead of numeric ones.
[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum AdjustmentStrategy
{
	AddFundsStrategy = 0,
	SubtractFundsStrategy = 1,
	ForceSubtractFundsStrategy = 2
}