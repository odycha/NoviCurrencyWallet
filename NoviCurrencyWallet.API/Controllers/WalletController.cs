using Microsoft.AspNetCore.Mvc;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Models.Wallet;
using Microsoft.AspNetCore.RateLimiting;
using NoviCurrencyWallet.Core.Exceptions;

namespace NoviCurrencyWallet.API.Controllers;

[Route("api/wallets")]
[ApiController]
[EnableRateLimiting("fixed")]
public class WalletController : ControllerBase
{
	private readonly IWalletsRepository _walletsRepository;
	public WalletController(IWalletsRepository walletsRepository)
	{
		_walletsRepository = walletsRepository;
	}


	//Why ActionResult?
	[HttpGet("{walletId}")]
	public async Task<ActionResult<GetWalletBalanceDto>> GetWalletBalance(long walletId, string? currency = null)
	{
		//IF I CALLED _walletsRepository.GetAsync(id, null);then the overloaded method would still be used
		GetWalletBalanceDto walletBalanceDto;

		if (string.IsNullOrEmpty(currency))
		{
			walletBalanceDto = await _walletsRepository.GetAsync<GetWalletBalanceDto>(walletId);
		}
		else
		{
			walletBalanceDto = await _walletsRepository.GetAsync(walletId, currency);
		}

		return Ok(walletBalanceDto);
	}


	[HttpPost]
	public async Task<ActionResult<GetWalletBalanceDto>> PostWallet(CreateWalletDto createWalletDto)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		};

		var createdWalletDto = await _walletsRepository.CreateWalletAsync(createWalletDto);

		return CreatedAtAction(nameof(GetWalletBalance), new { walletId = createdWalletDto.Id }, createdWalletDto);
	}


	[HttpPost("{walletId}/adjustbalance")]
	public async Task<IActionResult> AdjustWalletBalance(long walletId, [FromBody] UpdateWalletBalanceDto updateWalletBalanceDto)
	{
		if (walletId != updateWalletBalanceDto.Id)
		{
			return BadRequest("Invalid Record Id");
		}

		await _walletsRepository.AdjustBalance(updateWalletBalanceDto);

		return Ok();
	}



}
