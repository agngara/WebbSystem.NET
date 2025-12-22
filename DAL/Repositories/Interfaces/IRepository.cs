namespace DAL.Repositories.Interfaces;

public interface IRepository<T>
{
    public Task<T> Add(T entity);

    public Task<T?> GetById(int id);

    public Task<T?> GetById(string id);

    public Task<T> Update(T entity);

    public Task<bool> Delete(int id);

    public Task<bool> Delete(string id);
}
