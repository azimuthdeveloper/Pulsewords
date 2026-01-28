using Microsoft.AspNetCore.Mvc;
using PulseWord.Api.Models;
using PulseWord.Core.Services;

namespace PulseWord.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("anonymous")]
    public async Task<ActionResult<AuthResponse>> Anonymous([FromBody] AnonymousSessionRequest request)
    {
        var result = await _authService.CreateAnonymousSessionAsync(request.DisplayName);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new AuthResponse(result.AccessToken, result.RefreshToken, result.User));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Username, request.Password);
        if (!result.Success)
        {
            return Unauthorized(new { message = result.Error });
        }

        return Ok(new AuthResponse(result.AccessToken, result.RefreshToken, result.User));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new AuthResponse(result.AccessToken, result.RefreshToken, result.User));
    }
}
