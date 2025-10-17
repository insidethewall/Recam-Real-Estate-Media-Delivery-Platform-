using Microsoft.EntityFrameworkCore.Storage;

public interface IGeneralRepository
{
    public Task<IDbContextTransaction> BeginTransactionAsync();
    public Task SaveChangesAsync();
    public TDestination MapDto<TSource, TDestination>(TSource source);
}