using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Models.Wallet;

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
	public async Task<ActionResult<GetWalletBalanceDto>> PostWallet([FromBody] CreateWalletDto createWalletDto)
	{
		if (!ModelState.IsValid) // ensures the incoming request data adheres to the defined model constraints
		{
			return BadRequest(ModelState);
		};

		var createdWalletDto = await _walletsRepository.CreateWalletAsync(createWalletDto);

		//201 Created
		return CreatedAtAction(nameof(GetWalletBalance), new { walletId = createdWalletDto.Id }, createdWalletDto);
	}


	[HttpPost("{walletId}/adjustbalance")]
	public async Task<IActionResult> AdjustWalletBalance(long walletId, [FromBody] UpdateWalletBalanceDto updateWalletBalanceDto)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		if (walletId != updateWalletBalanceDto.Id)
		{
			return BadRequest("Invalid Record Id");
		}

		await _walletsRepository.AdjustBalance(updateWalletBalanceDto);

		return Ok();
	}
}
