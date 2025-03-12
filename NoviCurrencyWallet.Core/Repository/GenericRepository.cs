using AutoMapper;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Data;

namespace NoviCurrencyWallet.Core.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly NoviCurrencyWalletDbContext _context;
    private readonly IMapper _mapper;
    public GenericRepository(NoviCurrencyWalletDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }





    public async Task AddAsync(T entity)
	{
        _context.Add(entity); //equivalent to _context.Set<T>().Add(entity); EF auto detects entity type
        await _context.SaveChangesAsync();
	}
}
