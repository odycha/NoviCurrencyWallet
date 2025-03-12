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




    [HttpPost]
	public async Task<ActionResult<Wallet>> PostWallet(CreateWalletDto createWalletDto)
	{
		var wallet = _mapper.Map<Wallet>(createWalletDto);

        await _walletsRepository.AddAsync(wallet);

        return CreatedAtAction("GetWallet", new { id = wallet, Id }, wallet);
	}



























}
