using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Application.UseCases.Auth.Login.Loaders;
using SubSnap.Application.UseCases.Auth.Login.Policies;

namespace SubSnap.Application.UseCases.Auth.Login;

public sealed class LoginHandler
{

    //version without policies/ e loaders/
    //ORIGINAL CODE in AuthHandler.cs
    //private readonly IUserRepository _userRepository;
    //private readonly IPasswordHasherService _passwordHasherService;
    //private readonly IJwtTokenService _jwtTokenService;
    //private readonly IUnitOfWork _uow;
    //public LoginHandler(
    //    IUserRepository userRepository,
    //    IPasswordHasherService passwordHasherService,
    //    IJwtTokenService jwtTokenService,
    //    IUnitOfWork uow)
    //{
    //    _userRepository = userRepository;
    //    _passwordHasherService = passwordHasherService;
    //    _jwtTokenService = jwtTokenService;
    //    _uow = uow;
    //}
    //public async Task<LoginResult> Handle( LoginCommand command, CancellationToken ct)
    //{
    //    var user = await _userRepository.FindByEmailAsync(command.Email, ct)
    //        ?? throw new UnauthorizedAccessException();

    //    if (!_passwordHasherService.Verify(command.Password, user.PasswordHash))
    //        throw new UnauthorizedAccessException();

    //    var access = _jwtTokenService.GenerateAccessToken(user);

    //    var refreshRaw = _jwtTokenService.GenerateRefreshToken();
    //    var refreshHash = _passwordHasherService.Hash(refreshRaw);

    //    user.AddRefreshToken(
    //        refreshHash.Value,
    //        DateTime.UtcNow.AddDays(30));

    //    await _uow.SaveChangesAsync(ct);

    //    return new LoginResult(access, refreshRaw);
    //}

    private readonly UserByEmailLoader _loader;
    private readonly PasswordPolicy _passwordPolicy;
    private readonly IJwtTokenService _jwt;
    private readonly IPasswordHasherService _hasher;
    private readonly IUnitOfWork _uow;

    public LoginHandler(
        UserByEmailLoader loader,
        PasswordPolicy passwordPolicy,
        IJwtTokenService jwt,
        IPasswordHasherService hasher,
        IUnitOfWork uow)
    {
        _loader = loader;
        _passwordPolicy = passwordPolicy;
        _jwt = jwt;
        _hasher = hasher;
        _uow = uow;
    }

    public async Task<LoginResult> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await _loader.Load(cmd.Email, ct)
            ?? throw new UnauthorizedAccessException();
        _passwordPolicy.EnsureValid(cmd.PlainPassword, user);
        var access = _jwt.GenerateAccessToken(user);
        var refreshRaw = _jwt.GenerateRefreshToken();
        var refreshHash = _hasher.Hash(refreshRaw);
        var expiry = DateTime.UtcNow.AddDays(30);
        user.AddRefreshToken(refreshHash.Value, expiry);
        await _uow.SaveChangesAsync(ct);
        return new(access, refreshRaw);
    }

}
