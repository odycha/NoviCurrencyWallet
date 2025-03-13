namespace NoviCurrencyWallet.Core.Contracts;

public interface IGenericRepository<T> where T : class
{
	public Task AddAsync<TCreateDto>(TCreateDto createDto);
	public Task<TResult> GetAsync<TResult>(int? id);
	public Task UpdateAsync(T entity);
}
