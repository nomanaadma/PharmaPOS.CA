namespace PharmaPOS.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
	Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
	Task<List<T>> GetAllAsync(CancellationToken ct = default);
	void Add(T entity);
	void Update(T entity);
	void Remove(T entity);
}
