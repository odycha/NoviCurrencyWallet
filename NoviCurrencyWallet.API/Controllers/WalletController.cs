using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Models.Wallet;
using NoviCurrencyWallet.Core.Models.Wallet.Enums;
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
	[HttpGet("{walletId}")]
	public async Task<ActionResult<GetWalletBalanceDto>> GetWalletBalance(int walletId, string? currency = null)
	{
		GetWalletBalanceDto walletBalanceDto;

		//IF I CALLED _walletsRepository.GetAsync(id, null);then the overloaded method would still be used

		if (string.IsNullOrEmpty(currency))
		{
			walletBalanceDto = await _walletsRepository.GetAsync<GetWalletBalanceDto>(walletId); // Call single-parameter method
		}
		else
		{
			walletBalanceDto = await _walletsRepository.GetAsync(walletId, currency); // Call overloaded method
		}

		if (walletBalanceDto == null)
		{
			return NotFound("Wallet not found.");
		}

		return Ok(walletBalanceDto);
	}


	[HttpPost]
	public async Task<ActionResult<Wallet>> PostWallet(CreateWalletDto createWalletDto)
	{
        await _walletsRepository.AddAsync<CreateWalletDto>(createWalletDto);

        return CreatedAtAction("GetWalletBalance", new { id = wallet, Id }, wallet);
	}



	[HttpPost("{walletId}/adjustbalance")]
	public async Task<IActionResult> AdjustWalletBalance(UpdateWalletBalanceDto updateWalletBalanceDto)
	{
		await _walletsRepository.AdjustBalance(updateWalletBalanceDto);

		return Ok();
	}




}
