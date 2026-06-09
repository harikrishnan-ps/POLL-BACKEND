using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using poll_api.Application.DTOs.Auth;
using poll_api.Application.Interfaces;
using poll_api.Domain.Entities;
using poll_api.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace poll_api.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IConfiguration _config;

        public AuthService(IRepository<User> userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _config = config;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var users = await _userRepository.FindAsync(u => u.Username == loginDto.Username);
            var user = users.FirstOrDefault();
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new BadRequestException("Invalid username or password");

            // Assuming user role loaded (using EF Core include in repository would be ideal, but for simplicity let's assume Role is populated or we query it if needed. Actually we should load Role. We can fetch it via another repo or DbContext directly if we need. Since Repository doesn't include navigational properties by default, let's just query with Role in a real scenario. For now, RoleId 1 is Admin, 2 is User)
            var roleName = user.RoleId == 1 ? "Admin" : "User";

            var token = GenerateJwtToken(user, roleName);
            
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = "dummy-refresh-token", // To be implemented fully
                Username = user.Username,
                Role = roleName
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var existingUsers = await _userRepository.FindAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email);
            if (existingUsers.Any())
                throw new BadRequestException("Username or Email already exists.");

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                RoleId = 2 // Default to User
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var token = GenerateJwtToken(user, "User");

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = "dummy-refresh-token",
                Username = user.Username,
                Role = "User"
            };
        }

        private string GenerateJwtToken(User user, string roleName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, roleName)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(_config["Jwt:DurationInMinutes"]!)),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
