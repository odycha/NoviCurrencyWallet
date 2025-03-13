using Microsoft.AspNetCore.Mvc;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Models.Wallet;
using Microsoft.AspNetCore.RateLimiting;



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
	public async Task<IActionResult> AdjustWalletBalance(long walletId, [FromBody] UpdateWalletBalanceDto updateWalletBalanceDto)
	{
		await _walletsRepository.AdjustBalance(updateWalletBalanceDto);

		return Ok();
	}
}
