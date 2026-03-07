using MediatR;
using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.UseCases.Auth.RefreshToken;

//public sealed class RTHandler : IRTHandler
public sealed class RTHandler : IRequestHandler<RTCommand, RTResult>

//x plugin MediatR(validazione automatica!) (works w fluentvalidation) see validationbehaviour.cs  dependencyinjection.cs
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;

    public RTHandler(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<RTResult> Handle( RTCommand command, CancellationToken ct )  //chiamato 'Handle' obbligatorio x plugin MediatR
    {
        var user = await _userRepository.FindByRefreshTokenAsync(
            command.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException();
           //oppure metti questo code in Loaders/

        var token = user.FindActiveRefreshToken(
            stored =>
                _passwordHasherService.Verify(
                    command.RefreshToken,
                    new PasswordHash(stored)))
            ?? throw new UnauthorizedAccessException();
           //oppure metti questo code in Policies/

        user.RevokeRefreshToken(token);

        var newAccess = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshRaw = _jwtTokenService.GenerateRefreshToken();
        var newRefreshHash = _passwordHasherService.Hash(newRefreshRaw);

        user.AddRefreshToken(
            newRefreshHash.Value,
            DateTime.UtcNow.AddDays(30));

        //await _uow.SaveChangesAsync(ct); ora lo faccio nel transactionbehavior.cs durante la risalita verso il controller.

        return new RTResult(newAccess, newRefreshRaw);
    }
}
