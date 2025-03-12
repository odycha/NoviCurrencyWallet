using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using NoviCurrencyWallet.Data.Entities;

namespace NoviCurrencyWallet.Data;

public class NoviCurrencyWalletDbContext : DbContext
{

	public NoviCurrencyWalletDbContext(DbContextOptions<NoviCurrencyWalletDbContext> options)
		: base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<CurrencyRate>()
			.Property(c => c.Rate)
			.HasPrecision(18, 6); // Adjust scale as needed

		modelBuilder.Entity<Wallet>()
			.Property(w => w.Balance)
			.HasPrecision(18, 2); // Standard money format

		base.OnModelCreating(modelBuilder);
	}


	public DbSet <CurrencyRate> CurrencyRates { get; set; }
	public DbSet<Wallet> Wallets { get; set; }

}



//Without precision, large decimal values (e.g., 1.1234567) might get truncated to 1.12.
//This can cause rounding errors in financial calculations.
//✅ Setting precision ensures accurate storage and retrieval of currency values.
//Let me know if you need more help! 🚀