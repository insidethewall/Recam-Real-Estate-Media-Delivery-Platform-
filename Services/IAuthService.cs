
using RecamSystemApi.DTOs;

namespace RecamSystemAPI.Services
{
    public interface IAuthService
    {
        Task<string> Register(RegisterRequestDto registerRequest);
    }
}