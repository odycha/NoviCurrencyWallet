using NoviCurrencyWallet.Core.Models.Wallet;
using NoviCurrencyWallet.Data.Entities;

namespace NoviCurrencyWallet.Core.Contracts;

public interface IWalletsRepository : IGenericRepository<Wallet>
{
	public Task<GetWalletBalanceDto> GetAsync(long id, string targetCurrency);
	public Task<GetWalletBalanceDto> CreateWalletAsync(CreateWalletDto createWalletDto);

	public Task AdjustBalance(UpdateWalletBalanceDto updateWalletBalanceDto);

	Task<decimal> ConvertCurrency(string initialCurrency, string targetCurrency, decimal ammount);


}
