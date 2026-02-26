using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Application.UseCases.Auth.Logout.Loaders;
using SubSnap.Application.UseCases.Auth.Logout.Policies;

namespace SubSnap.Application.UseCases.Auth.Logout;

public sealed class LogoutHandler : ILogoutHandler
{
    //code without policies/ e loaders/, ORIGINAL CODE IN AuthHandler.cs
    //private readonly UserByIdLoader _loader;
    //private readonly LogoutTokenPolicy _passwordPolicy;
    //private readonly IUserRepository _userRepository;
    //private readonly IPasswordHasherService _passwordHasherService;
    //private readonly IUnitOfWork _uow;
    //public LogoutHandler(
    //    IUserRepository userRepository,
    //    IPasswordHasherService passwordHasherService,
    //    IUnitOfWork uow)
    //{
    //    _userRepository = userRepository;
    //    _passwordHasherService = passwordHasherService;
    //    _uow = uow;
    //}
    //public async Task Handle(LogoutCommand command, CancellationToken ct)
    //{
    //    var user = await _userRepository.FindByIdAsync(command.UserId, ct)
    //        ?? throw new UnauthorizedAccessException();
    //    var token = user.FindActiveRefreshToken(
    //        storedToken =>
    //            _passwordHasherService.Verify(
    //                command.RefreshToken,
    //                new PasswordHash(storedToken)))
    //        ?? throw new UnauthorizedAccessException();
    //    user.RevokeRefreshToken(token);
    //    await _uow.SaveChangesAsync(ct);
    //}

    private readonly UserByIdLoader _loader;
    private readonly LogoutTokenPolicy _policy;
    private readonly IUnitOfWork _uow;

    public LogoutHandler(
        UserByIdLoader loader,
        LogoutTokenPolicy policy,
        IUnitOfWork uow)
    {
        _loader = loader;
        _policy = policy;
        _uow = uow;
    }
    public async Task Handle( LogoutCommand command, CancellationToken ct)
    {
        var user = await _loader.Load(command.UserId, ct)
            ?? throw new UnauthorizedAccessException();

        var token = _policy.EnsureValid(
            command.RefreshToken,
            user);

        user.RevokeRefreshToken(token);
        await _uow.SaveChangesAsync(ct);
    }

}
