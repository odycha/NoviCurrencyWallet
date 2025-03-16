using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Exceptions;
using NoviCurrencyWallet.Core.Models.Wallet;
using NoviCurrencyWallet.Core.Models.Wallet.Enums;
using NoviCurrencyWallet.Data;
using NoviCurrencyWallet.Data.Entities;
using NoviCurrencyWallet.Gateway.Contracts;

namespace NoviCurrencyWallet.Core.Repository;

public class WalletRepository : GenericRepository<Wallet>, IWalletsRepository
{
	private readonly NoviCurrencyWalletDbContext _context;
	private readonly IMapper _mapper;
	private readonly IEcbGatewayService _ecbGateway;
	private readonly ILogger<WalletRepository> _logger;
	public WalletRepository(
		NoviCurrencyWalletDbContext context,
		IMapper mapper, IEcbGatewayService ecbGateway,
		ILogger<WalletRepository> logger) : base(context, mapper)
	{
		_context = context;
		_mapper = mapper;
		_ecbGateway = ecbGateway;
		_logger = logger;
	}


	public async Task<GetWalletBalanceDto> GetAsync(long id, string targetCurrency)
	{
		var wallet = await _context.Wallets.FindAsync(id);

		if (wallet == null)
		{
			throw new NotFoundException(nameof(Wallet), id);
		}

		decimal convertedBalance = await ConvertCurrency(wallet.Currency, targetCurrency, wallet.Balance);

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
		if (createWalletDto == null)
		{
			throw new BadRequestException("Wallet creation data cannot be null.");
		}

		var newWallet = _mapper.Map<Wallet>(createWalletDto);

		var wallet = await AddAsync(newWallet);

		var getWalletBalanceDto = _mapper.Map<GetWalletBalanceDto>(wallet);

		return getWalletBalanceDto;
	}

	public async Task AdjustBalance(UpdateWalletBalanceDto updateWalletBalanceDto)
	{
		var wallet = await _context.Wallets.FindAsync(updateWalletBalanceDto.Id);

		if (wallet == null)
		{
			throw new NotFoundException(nameof(Wallet), updateWalletBalanceDto.Id);
		}

		decimal amount = updateWalletBalanceDto.Amount;

		if (updateWalletBalanceDto.Currency != wallet.Currency)
		{
			//convert ammount to the same currency
			amount = await ConvertCurrency(updateWalletBalanceDto.Currency, wallet.Currency, amount);
		}


		if (updateWalletBalanceDto.Strategy == AdjustmentStrategy.AddFundsStrategy)
		{
			wallet.Balance = Math.Round(wallet.Balance + amount, 2);
		}
		else if (updateWalletBalanceDto.Strategy == AdjustmentStrategy.SubtractFundsStrategy)
		{
			if (amount > wallet.Balance)
			{
				throw new BadRequestException("Insufficient funds for operation");
			}
			else
			{
				wallet.Balance = Math.Round(wallet.Balance - amount, 2);
			}
		}
		else if (updateWalletBalanceDto.Strategy == AdjustmentStrategy.ForceSubtractFundsStrategy)
		{
			wallet.Balance = Math.Round(wallet.Balance - amount, 2);
		}

		await UpdateAsync(wallet);
	}


	public async Task<decimal> ConvertCurrency(string initialCurrency, string targetCurrency, decimal ammount)
	{
		if (initialCurrency == targetCurrency)
		{
			return ammount;
		}

		decimal initialRate = await GetCurrencyRate(initialCurrency);
		decimal targetRate = await GetCurrencyRate(targetCurrency);

		decimal ammountInEur = ammount / initialRate;
		decimal convertedAmmount = ammountInEur * targetRate;

		return Math.Round(convertedAmmount, 2);
	}

	public async Task<decimal> GetCurrencyRate(string currency)
	{
		if (currency == "EUR") return 1m;

		var cachedRates = _ecbGateway.GetCachedExchangeRates();

		if (cachedRates == null)
		{
			_logger.LogWarning("Cache is empty, falling back to database.");

			var dbRate = await _context.CurrencyRates
				.Where(r => r.Currency == currency)
				.OrderByDescending(r => r.Date)
				.FirstOrDefaultAsync();

			if (dbRate == null)
			{
				throw new NotFoundException("CurrencyRate", currency);
			}

			return dbRate.Rate;
		}

		var currencyRate = cachedRates?.Rates?.FirstOrDefault(r => r.Currency == currency);

		if (currencyRate == null)
		{
			throw new NotFoundException("CurrencyRate", currency);
		}
		return currencyRate.Rate;
	}
}




//Why Not Use IMemoryCache Directly in WalletRepository?
//Using IMemoryCache inside WalletRepository is possible, but it violates separation of concerns and makes the repository responsible for caching logic, which isn't its job.