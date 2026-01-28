using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PulseWord.Core.Entities;
using PulseWord.Core.Services;
using PulseWord.Infrastructure.Data;
using BCrypt.Net;

namespace PulseWord.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly PulseWordContext _context;
    private readonly IConfiguration _config;

    public AuthService(PulseWordContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<AuthResult> CreateAnonymousSessionAsync(string? displayName = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = $"anon_{Guid.NewGuid():N}",
            DisplayName = displayName ?? "Anonymous Player",
            IsAnonymous = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await GenerateAuthResult(user);
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (user == null || user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return new AuthResult("", "", null!, false, "Invalid username or password");
        }

        return await GenerateAuthResult(user);
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new AuthResult("", "", null!, false, "Invalid or expired refresh token");
        }

        return await GenerateAuthResult(user);
    }

    public string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("is_anonymous", user.IsAnonymous.ToString().ToLower())
        };

        var keyString = _config["JwtSettings:Key"];
        if (string.IsNullOrEmpty(keyString))
        {
            throw new InvalidOperationException("JWT Key is not configured.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:DurationInMinutes"] ?? "60"));

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<AuthResult> GenerateAuthResult(User user)
    {
        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRandomToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        var userDto = new UserDto(user.Id, user.UserName, user.DisplayName, user.IsAnonymous);
        return new AuthResult(accessToken, refreshToken, userDto, true);
    }

    private string GenerateRandomToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
