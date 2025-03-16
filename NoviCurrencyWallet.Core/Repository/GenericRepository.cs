using AutoMapper;
using NoviCurrencyWallet.Core.Contracts;
using NoviCurrencyWallet.Core.Exceptions;
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
		if (entity == null)
		{
			throw new BadRequestException("Cannot add a null entity.");
		}

		await _context.Set<T>().AddAsync(entity);

		var changes = await _context.SaveChangesAsync(); //returns int

		if (changes == 0)
		{
			throw new Exception("Database operation failed: Entity was not saved.");
		}

		return entity;
	}

	public async Task<TResult> GetAsync<TResult>(long? id)
	{
		if (!id.HasValue)
		{
			throw new BadRequestException("ID parameter is required.");
		}

		var entity = await _context.Set<T>().FindAsync(id); 

		if (entity is null)
		{
			throw new NotFoundException(typeof(T).Name, id.Value);
		}

		var resultDto = _mapper.Map<TResult>(entity);

		return resultDto;
	}

	public async Task UpdateAsync(T entity)
	{
		if (entity == null)
		{
			throw new BadRequestException("Cannot update a null entity.");
		}

		_context.Update(entity);

		var changes = await _context.SaveChangesAsync();
		if (changes == 0)
		{
			throw new Exception("Database operation failed: Entity was not updated.");
		}
	}



}
