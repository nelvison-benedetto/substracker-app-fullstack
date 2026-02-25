using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubSnap.API.Contracts.Responses;
using SubSnap.Core.Domain.ValueObjects;
using SubSnap.Core.DTOs.Auth;
using SubSnap.Core.Services.Application;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using SubSnap.API.Contracts.Auth;

namespace SubSnap.API.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestAuth request)
    {
        var email = new Email(request.Email);  //validation
        var (access, refresh) = await _authService.LoginAsync(email, request.Password);
        return Ok(ApiResult<object>.Ok(new
        {
            accessToken = access,
            refreshToken = refresh
        }));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestAuth request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userIdClaim is null)
            return Unauthorized();
        var userId = new UserId(Guid.Parse(userIdClaim));
        await _authService.LogoutAsync(userId, request.RefreshToken);
        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
    [FromBody] RefreshTokenRequestAuth request)
    {
        var (access, refresh) =
            await _authService.RefreshAsync(request.RefreshToken);

        return Ok(ApiResult<object>.Ok(new
        {
            accessToken = access,
            refreshToken = refresh
        }));
    }

}