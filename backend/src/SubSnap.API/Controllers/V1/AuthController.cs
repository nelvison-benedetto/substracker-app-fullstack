using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubSnap.API.Contracts.Auth.Requests;
using SubSnap.API.Contracts.Auth.Responses;
using SubSnap.API.Contracts.Responses;
using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.Users;
using SubSnap.Application.UseCases.Auth.Login;
using SubSnap.Application.UseCases.Auth.Logout;
using SubSnap.Application.UseCases.Auth.RefreshToken;
using SubSnap.Core.Domain.ValueObjects;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SubSnap.API.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    //private readonly AuthHandler _authHandler;
    private readonly IMapper _mapper;
    private readonly ILoginHandler _loginHandler;
    private readonly ILogoutHandler _logoutHandler;
    private readonly IRTHandler _rtHandler;
    public AuthController(
        IMapper mapper,
        ILoginHandler loginHandler,
        ILogoutHandler logoutHandler,
        IRTHandler rtHandler,
        IRUHandler ruHandler
    )
    {
        //_authHandler = authHandler;
        _mapper = mapper;
        _loginHandler = loginHandler;
        _logoutHandler = logoutHandler;
        _rtHandler = rtHandler;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestAuth request, CancellationToken ct)
    {
        var email = new Email(request.Email);  //validation
        //var (access, refresh) = await _authHandler.LoginAsync(email, request.Password,ct); OLD
        //var command = new LoginCommand( new Email(request.Email), request.Password );  oppure meglio usi il mapper per essere pulito .API.mapping/
        var command = _mapper.Map<LoginCommand>(request);  //more info in .API.mapping/
        var result = await _loginHandler.HandleAsync(command, ct);
        var response = _mapper.Map<LoginResponseAuth>(result);
        //return Ok(ApiResult<object>.Ok(new
        //{
        //    accessToken = result.AccessToken,
        //    refreshToken = result.RefreshToken
        //}));
        return Ok(ApiResult<LoginResponseAuth>.Ok(response));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestAuth request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userIdClaim is null)
            return Unauthorized();

        //var userId = new UserId(Guid.Parse(userIdClaim));
        //await _authHandler.LogoutAsync(userId, request.RefreshToken, ct);
        var command = new LogoutCommand( 
            new UserId(Guid.Parse(userIdClaim)),
            request.RefreshToken
        );  //qui non uso il mapper xk non hanno un match diretto (in LogoutCommand ho anche Userid che lo trovo here non mi arriva direttamente da http)
        await _logoutHandler.HandleAsync(command, ct);
        return NoContent();  //PERFECT REST!
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
    [FromBody] RefreshTokenRequestAuth request, CancellationToken ct)
    {
        //var (access, refresh) =
        //    await _authHandler.RefreshAsync(request.RefreshToken, ct);
        var command = _mapper.Map<RTCommand>(request);
        var result = await _rtHandler.HandleAsync(command, ct);
        var response = _mapper.Map<RefreshTokenResponseAuth>(result);
        //return Ok(ApiResult<object>.Ok(new
        //{
        //    accessToken = result.AccessToken,
        //    refreshToken = result.RefreshToken
        //}));
        return Ok(ApiResult<RefreshTokenResponseAuth>.Ok(response));
    }

}