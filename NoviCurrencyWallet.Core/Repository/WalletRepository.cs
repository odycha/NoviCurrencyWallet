using AutoMapper;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Models.Wallet;
using NoviCurrencyWallet.Core.Models.Wallet.Enums;
using NoviCurrencyWallet.Data;
using NoviCurrencyWallet.Data.Entities;

namespace NoviCurrencyWallet.Core.Repository;

public class WalletRepository : GenericRepository<Wallet>, IWalletsRepository
{
	private readonly NoviCurrencyWalletDbContext _context;
	private readonly IMapper _mapper;
	public WalletRepository(NoviCurrencyWalletDbContext context, IMapper mapper) : base(context, mapper)
	{
		_context = context;
		_mapper = mapper;
	}


	public async Task<GetWalletBalanceDto> GetAsync(long id, string targetCurrency)
	{
		var wallet = await _context.Wallets.FindAsync(id);

		if (wallet == null)
		{
			//TODO
		}

		decimal convertedBalance = ConvertCurrency(wallet.Currency, targetCurrency, wallet.Balance);

		var resultDto = new GetWalletBalanceDto
		{
			Id = wallet.Id,
			Balance = wallet.Balance,
			Currency = wallet.Currency,
			ConvertedCurrency = targetCurrency,
			ConvertedBalance = convertedBalance
		};

		return resultDto;
	}


	public async Task<GetWalletBalanceDto> CreateWalletAsync(CreateWalletDto createWalletDto)
	{
		var newWallet = _mapper.Map<Wallet>(createWalletDto);

		var wallet = await AddAsync(newWallet);

		if (wallet == null)
		{
			//TODO: throw new Exception("Failed to create wallet");
		}


		var getWalletBalanceDto = _mapper.Map<GetWalletBalanceDto>(wallet);

		return getWalletBalanceDto;
	}





	public async Task AdjustBalance(UpdateWalletBalanceDto updateWalletBalanceDto)
	{
		var wallet = await _context.Wallets.FindAsync(updateWalletBalanceDto.Id);

		if (wallet == null)
		{
			//TODO
		}

		decimal ammount = updateWalletBalanceDto.Amount;

		if(updateWalletBalanceDto.Currency != wallet.Currency)
		{
			//convert ammount to the same currency
			ammount = ConvertCurrency(updateWalletBalanceDto.Currency, wallet.Currency, ammount);
		}


		if(updateWalletBalanceDto.Strategy == AdjustmentStrategy.AddFundsStrategy)
		{
			wallet.Balance = wallet.Balance + ammount;
		}
		else if(updateWalletBalanceDto.Strategy == AdjustmentStrategy.SubtractFundsStrategy)
		{
			if(ammount > wallet.Balance)
			{
				//TODO THROW EXCEPTION
			}
			else
			{
				wallet.Balance = wallet.Balance - ammount;
			}
		}
		else if(updateWalletBalanceDto.Strategy == AdjustmentStrategy.ForceSubtractFundsStrategy)
		{
			wallet.Balance = wallet.Balance - ammount;
		}

		await UpdateAsync(wallet);
	}


	public decimal ConvertCurrency(string initialCurrency, string targetCurrency, decimal ammount)
	{
		decimal convertedBalance;
		var targetCurrencyRate = _context.CurrencyRates.FirstOrDefault(r => r.Currency == targetCurrency);

		if (targetCurrencyRate == null)
		{
			//TODO
		}

		if (initialCurrency == "EUR")
		{
			if (targetCurrency == "EUR")
			{
				convertedBalance = ammount;
			}
			else
			{
				convertedBalance = (ammount) * (targetCurrencyRate.Rate);
			}
		}
		else
		{
			var walletCurrencyRate = _context.CurrencyRates.FirstOrDefault(r => r.Currency == initialCurrency);
			if (walletCurrencyRate == null)
			{
				//TODO
			}


			if (targetCurrency == "EUR")
			{
				convertedBalance = (ammount) / (walletCurrencyRate.Rate);
			}
			else
			{
				decimal convertedToEur = (ammount) / (walletCurrencyRate.Rate);

				convertedBalance = convertedToEur * (targetCurrencyRate.Rate);
			}
		}

		return convertedBalance;
	}

}
