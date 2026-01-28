using PulseWord.Core.Entities;

namespace PulseWord.Core.Services;

public interface IAuthService
{
    Task<AuthResult> CreateAnonymousSessionAsync(string? displayName = null);
    Task<AuthResult> LoginAsync(string username, string password);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    string GenerateJwtToken(User user);
}

public record AuthResult(string AccessToken, string RefreshToken, UserDto User, bool Success, string? Error = null);

public record UserDto(Guid Id, string UserName, string? DisplayName, bool IsAnonymous);
