using RecamSystemApi.Models;
using RecamSystemApi.Services;

public class ListingCasesService : IListingCasesService
{
    private readonly IListingCasesRepository _repository;

    public ListingCasesService(IListingCasesRepository repository)
    {
        _repository = repository;

    }

    public async Task<ICollection<ListingCase>> GetAllListingCasesAsync()
    {
        return await _repository.GetAllListingCasesAsync();
    }

}