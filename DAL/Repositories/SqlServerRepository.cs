
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

//Generic Repository class towards SqlServer
//Works on Models with id as either int or string
//U get free CRUD here

public class SqlServerRepository<T> : IRepository<T> where T : class
{
    protected AppDbContext _context;

    public SqlServerRepository(AppDbContext dbContext)
    {
        _context = dbContext;
    }

    public async Task<T> Add(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T?> GetById(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T?> GetById(string id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T> Update(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> Delete(int id)
    {
        var entity = await GetById(id);
        if (entity == null) return false;

        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(string id)
    {
        var entity = await GetById(id);
        if (entity == null) return false;

        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
