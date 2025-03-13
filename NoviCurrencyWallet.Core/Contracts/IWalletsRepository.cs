using NoviCurrencyWallet.Core.Models.Wallet;
using NoviCurrencyWallet.Data.Entities;

namespace NoviCurrencyWallet.Core.Contracts;

public interface IWalletsRepository : IGenericRepository<Wallet>
{
	public Task<GetWalletBalanceDto> GetAsync(int id, string targetCurrency);

	public Task AdjustBalance(UpdateWalletBalanceDto updateWalletBalanceDto);

	public decimal ConvertCurrency(string initialCurrency, string targetCurrency, decimal ammount);


}
