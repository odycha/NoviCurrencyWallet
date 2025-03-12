using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Models.Wallet;
using NoviCurrencyWallet.Data.Entities;

namespace NoviCurrencyWallet.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WalletController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IWalletsRepository _walletsRepository;
    public WalletController(IMapper mapper, IWalletsRepository walletsRepository)
    {
        _mapper = mapper;
        _walletsRepository = walletsRepository;
    }


	//Why ActionResult?
	[HttpGet]
	public async Task<ActionResult<GetWalletBalanceDto>> GetWalletBalance(int id, string? currency = null)
	{
		Wallet walletBalance;

		//IF I CALLED _walletsRepository.GetAsync(id, null);then the overloaded method would still be used

		if (string.IsNullOrEmpty(currency))
		{
			walletBalance = await _walletsRepository.GetAsync(id); // Call single-parameter method
		}
		else
		{
			walletBalance = await _walletsRepository.GetAsync(id, currency); // Call overloaded method
		}

		if (walletBalance == null)
		{
			return NotFound("Wallet not found.");
		}

		var walletBalanceDto = _mapper.Map<GetWalletBalanceDto>(walletBalance);
		return Ok(walletBalanceDto);
	}


	[HttpPost]
	public async Task<ActionResult<Wallet>> PostWallet(CreateWalletDto createWalletDto)
	{
		var wallet = _mapper.Map<Wallet>(createWalletDto);

        await _walletsRepository.AddAsync(wallet);

        return CreatedAtAction("GetWallet", new { id = wallet, Id }, wallet);
	}



	[HttpPost]
	public async Task AdjustWalletBalance()
	{

	} 






















}
