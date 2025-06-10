using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemAPI.Services;

namespace RecamSystemApi.Services;

 public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<User> _roleManager;

        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        

        /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="userManager"></param>
    public AuthService(UserManager<User> userManager, RoleManager<User> roleManager,IMapper mapper, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _configuration = configuration;
    }

        public async Task<string>  Register(RegisterRequestDto registerRequest)
        {
            User user = _mapper.Map<User>(registerRequest);
           
            IdentityResult result = await _userManager.CreateAsync(user, registerRequest.Password);
            bool roleExists = await _roleManager.RoleExistsAsync(registerRequest.Role.ToString());
            if (result.Succeeded && roleExists)
            {
                //Create JWT token
                string token = GenerateJwtToken(user);
                return token;
            }

            throw new System.Exception("Create User failed");
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],
                        audience: _configuration["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(30),
                        signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }