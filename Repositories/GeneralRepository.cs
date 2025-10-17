using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RecamSystemApi.Data;

public class GeneralRepository : IGeneralRepository
{
    private readonly ReacmDbContext _context;


    protected readonly IMapper _mapper;
    public GeneralRepository(ReacmDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

 

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    public TDestination MapDto<TSource, TDestination>(TSource source)
    {
        return _mapper.Map<TDestination>(source);
    }


}