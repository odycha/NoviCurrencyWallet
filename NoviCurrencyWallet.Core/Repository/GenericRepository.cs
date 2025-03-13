using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

	public async Task<T> AddAsync(T entity)
	{
		await _context.Set<T>().AddAsync(entity);
		await _context.SaveChangesAsync();
		return entity;
	}

	public async Task<TResult> GetAsync<TResult>(long? id)
	{
		var entity = await _context.Set<T>().FindAsync(id);    //??? what is Set<T>

		if (entity is null)
		{
			//TODO: throw new NotFoundException(typeof(T).Name, id.HasValue ? id : "No Key Provided");
		}

		var resultDto = _mapper.Map<TResult>(entity);

		return resultDto;
	}

	public async Task UpdateAsync(T entity)
	{
		_context.Update(entity);
		await _context.SaveChangesAsync();
	}



}
