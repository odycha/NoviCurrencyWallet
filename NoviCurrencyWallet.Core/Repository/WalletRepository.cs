using AutoMapper;
using NoviCurrencyWallet.Core.Contracts;
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



}
