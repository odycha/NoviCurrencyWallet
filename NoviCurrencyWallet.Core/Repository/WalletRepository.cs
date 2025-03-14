﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Exceptions;
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

		decimal ammount = updateWalletBalanceDto.Amount;

		if(updateWalletBalanceDto.Currency != wallet.Currency)
		{
			//convert ammount to the same currency
			ammount = await ConvertCurrency(updateWalletBalanceDto.Currency, wallet.Currency, ammount);
		}


		if(updateWalletBalanceDto.Strategy == AdjustmentStrategy.AddFundsStrategy)
		{
			wallet.Balance = wallet.Balance + ammount;
		}
		else if(updateWalletBalanceDto.Strategy == AdjustmentStrategy.SubtractFundsStrategy)
		{
			if(ammount > wallet.Balance)
			{
				throw new BadRequestException("Insufficient funds for operation");
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


	public async Task<decimal> ConvertCurrency(string initialCurrency, string targetCurrency, decimal ammount)
	{
		if(initialCurrency == targetCurrency)
		{
			return ammount;
		}

		decimal initialRate = await GetCurrencyRate(initialCurrency);
		decimal targetRate = await GetCurrencyRate(targetCurrency);

		decimal ammountInEur = ammount / initialRate;
		decimal convertedAmmount = ammountInEur * targetRate;

		return convertedAmmount;
	}

	private async Task<decimal> GetCurrencyRate(string currency)
	{
		if (currency == "EUR") return 1m;

		var currencyRate = await _context.CurrencyRates.FirstOrDefaultAsync(r => r.Currency == currency);
		if (currencyRate == null)
		{
			throw new NotFoundException("CurrencyRate", currency);
		}

		return currencyRate.Rate;
	}

}

