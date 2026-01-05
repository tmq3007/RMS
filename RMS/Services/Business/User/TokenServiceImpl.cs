using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RMS.Data.Entities;
using RMS.DTOs.Requests;
using RMS.Shared.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace RMS.Services.Business.User
{
    public class TokenServiceImpl : ITokenService
    {
        private readonly SymmetricSecurityKey _secretKey;
        private readonly string _validIssuer;
        private readonly string _validAudience;
        private readonly double _expires;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TokenServiceImpl> _logger;

        public TokenServiceImpl(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
            {
                throw new ArgumentNullException("JWT secret key are not properly configured.");
            }
            _secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.Key));
            _validIssuer = jwtSettings.ValidIssuer;
            _validAudience = jwtSettings.ValidAudience;
            _expires = jwtSettings.Expires;

        }

        public async Task<string> GenerateToken(ApplicationUser user)
        {
            var signingCredentials = new SigningCredentials(_secretKey, SecurityAlgorithms.HmacSha256);
            var claims = await GetClaimsAsync(user);
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }
        private async Task<List<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("Gender", user.Gender),

            };
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(
            SigningCredentials signingCredentials,
            List<Claim> claims)
        {
           return new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_expires),
                signingCredentials: signingCredentials);

        }

        

        public string GenerateRefreshToken()
        {
           var randomNumber = new byte[64];
           using var rng = RandomNumberGenerator.Create();
              rng.GetBytes(randomNumber);
              return Convert.ToBase64String(randomNumber);
        }
    }
}
