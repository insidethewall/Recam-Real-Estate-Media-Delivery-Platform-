using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RecamSystemApi.Data;

public class GeneralRepository<T> : IGeneralRepository<T> where T : class
{
    private readonly ReacmDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected readonly IMapper _mapper;
    public GeneralRepository(ReacmDbContext context, IMapper mapper)
    {
        _context = context;
        _dbSet = _context.Set<T>();
        _mapper = mapper;
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    public TEntity MapDto<TEntity, TDto>(TDto dto)
    {
        return _mapper.Map<TEntity>(dto);
    }


}