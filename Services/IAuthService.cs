
using RecamSystemApi.DTOs;

namespace RecamSystemApi.Services;

    public interface IAuthService
    {
        Task<string> Register(RegisterRequestDto registerRequest);
    }
