using RecamSystemApi.Enums;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IAuthRepository
{
    public Task CreatePhotographerAsync(Photographer photographer);
  
}