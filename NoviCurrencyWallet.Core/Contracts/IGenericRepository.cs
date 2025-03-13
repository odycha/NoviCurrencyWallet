namespace NoviCurrencyWallet.Core.Contracts;

public interface IGenericRepository<T> where T : class
{
	public Task<T> AddAsync(T entity);
	public Task<TResult> GetAsync<TResult>(long? id);
	public Task UpdateAsync(T entity);
}
