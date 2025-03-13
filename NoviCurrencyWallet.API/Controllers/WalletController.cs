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
    private readonly IWalletsRepository _walletsRepository;
    public WalletController(IWalletsRepository walletsRepository)
    {
        _walletsRepository = walletsRepository;
    }


	//Why ActionResult?
	[HttpGet("{walletId}")]
	public async Task<ActionResult<GetWalletBalanceDto>> GetWalletBalance(int walletId, string? currency = null)
	{
		//IF I CALLED _walletsRepository.GetAsync(id, null);then the overloaded method would still be used
		
		var walletBalanceDto = string.IsNullOrEmpty(currency)
			? await _walletsRepository.GetAsync<GetWalletBalanceDto>(walletId)
			: await _walletsRepository.GetAsync(walletId, currency);

		if (walletBalanceDto == null)
		{
			return NotFound("Wallet not found.");
		}

		return Ok(walletBalanceDto);
	}


	[HttpPost]
	public async Task<ActionResult<GetWalletBalanceDto>> PostWallet(CreateWalletDto createWalletDto)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var createdWalletDto = await _walletsRepository.CreateWalletAsync(createWalletDto);

		return CreatedAtAction(nameof(GetWalletBalance), new { walletId = createdWalletDto.Id }, createdWalletDto);
	}



	[HttpPost("{walletId}/adjustbalance")]
	public async Task<IActionResult> AdjustWalletBalance(UpdateWalletBalanceDto updateWalletBalanceDto)
	{
		await _walletsRepository.AdjustBalance(updateWalletBalanceDto);

		return Ok();
	}




}
