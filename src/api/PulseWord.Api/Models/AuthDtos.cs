using PulseWord.Core.Services;

namespace PulseWord.Api.Models;

public record AnonymousSessionRequest(string? DisplayName);

public record LoginRequest(string Username, string Password);

public record RefreshRequest(string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);
