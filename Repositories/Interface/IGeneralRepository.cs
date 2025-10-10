using Microsoft.EntityFrameworkCore.Storage;

public interface IGeneralRepository<T> where T : class
{
    public Task<IDbContextTransaction> BeginTransactionAsync();
    public Task SaveChangesAsync();
    public Task AddAsync(T entity);

    public TEntity MapDto<TEntity, TDto>(TDto dto);
}