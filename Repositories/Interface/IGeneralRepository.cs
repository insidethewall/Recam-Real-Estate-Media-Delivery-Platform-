using Microsoft.EntityFrameworkCore.Storage;

public interface IGeneralRepository
{
    public Task<IDbContextTransaction> BeginTransactionAsync();
    public Task SaveChangesAsync();
    public TEntity MapDto<TEntity, TDto>(TDto dto);
}