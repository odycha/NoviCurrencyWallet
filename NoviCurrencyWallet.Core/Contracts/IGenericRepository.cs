namespace NoviCurrencyWallet.Core.Contracts;

public interface IGenericRepository<T> where T : class
{
	public Task AddAsync(T entity);





}
