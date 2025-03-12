namespace NoviCurrencyWallet.Core.Contracts;

public interface IGenericRepository<T> where T : class
{
	public Task AddAsync(T entity);

	public Task<T> GetAsync(int id);
	public Task<T> GetAsync(int id, string currency);



}
