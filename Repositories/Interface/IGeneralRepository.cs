using Microsoft.EntityFrameworkCore.Storage;

public interface IGeneralRepository
{
    public Task<IDbContextTransaction> BeginTransactionAsync();
    public Task SaveChangesAsync();
    public TDestination MapDto<TSource, TDestination>(TSource source);
    public TDestination MapDtoUpdate<TSource, TDestination>(TSource source, TDestination destination);
}