namespace NoviCurrencyWallet.Gateway.Configurations;

public class EcbGatewayOptions
{
	public string BaseUrl { get; set; } = string.Empty;
	public int CacheDurationMinutes { get; set; }
	public string CacheKey { get; set; }
	public int TimeoutSeconds { get; set; } = 30;
}
