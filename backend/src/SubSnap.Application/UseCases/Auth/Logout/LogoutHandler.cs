using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Application.UseCases.Auth.Logout.Loaders;
using SubSnap.Application.UseCases.Auth.Logout.Policies;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.UseCases.Auth.Logout;

public sealed class LogoutHandler
{
    private readonly UserByIdLoader _loader;
    private readonly LogoutTokenPolicy _passwordPolicy;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IUnitOfWork _uow;

    public LogoutHandler(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasherService,
        IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _uow = uow;
    }

    public async Task Handle(LogoutCommand command, CancellationToken ct)
    {
        var user = await _userRepository.FindByIdAsync(command.UserId, ct)
            ?? throw new UnauthorizedAccessException();

        var token = user.FindActiveRefreshToken(
            storedToken =>
                _passwordHasherService.Verify(
                    command.RefreshToken,
                    new PasswordHash(storedToken)))
            ?? throw new UnauthorizedAccessException();

        user.RevokeRefreshToken(token);
        await _uow.SaveChangesAsync(ct);
    }

}
