using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Application.UseCases.Auth.Login;

public sealed class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _uow;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService,
        IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
        _uow = uow;
    }

    public async Task<LoginResult> Handle( LoginCommand command, CancellationToken ct)
    {
        var user = await _userRepository.FindByEmailAsync(command.Email, ct)
            ?? throw new UnauthorizedAccessException();

        if (!_passwordHasherService.Verify(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException();

        var access = _jwtTokenService.GenerateAccessToken(user);

        var refreshRaw = _jwtTokenService.GenerateRefreshToken();
        var refreshHash = _passwordHasherService.Hash(refreshRaw);

        user.AddRefreshToken(
            refreshHash.Value,
            DateTime.UtcNow.AddDays(30));

        await _uow.SaveChangesAsync(ct);

        return new LoginResult(access, refreshRaw);
    }

}
