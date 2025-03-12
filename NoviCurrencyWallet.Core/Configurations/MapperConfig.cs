using AutoMapper;
using NoviCurrencyWallet.Core.Models.Wallet;
using NoviCurrencyWallet.Data.Entities;

namespace NoviCurrencyWallet.Core.Configurations;

public class MapperConfig : Profile
{

    public MapperConfig()
    {
        CreateMap<Wallet, CreateWalletDto>().ReverseMap();
		CreateMap<Wallet, UpdateWalletBalanceDto>().ReverseMap();
		CreateMap<Wallet, GetWalletBalanceDto>().ReverseMap();
	}
}
