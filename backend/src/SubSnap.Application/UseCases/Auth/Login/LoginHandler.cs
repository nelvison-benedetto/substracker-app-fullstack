using MediatR;
using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Application.UseCases.Auth.Login.Loaders;
using SubSnap.Application.UseCases.Auth.Login.Policies;

namespace SubSnap.Application.UseCases.Auth.Login;

/*
 * quando fai nel usercontroller.cs
await _mediator.Send(command) la pipeline (grazie a method Handle) è
 Controller
   ↓
ValidationBehavior
   ↓
LoggingBehavior
   ↓
PerformanceBehavior
   ↓
TransactionBehavior
   ↓
ExceptionBehavior
   ↓
Handler
//plugin MediatR costruisce dinamicamente la pipeline usando reflrection, quindi cerca sermpe Handle(...) !!
COME FUNZIONA CON NEXT/RETURN RESPONSE NELLA PIPELINE
TransactionBehavior entra
↓
await next()
    ↓
    RUHandler.Handle()
        AddAsync(user)
        (NO SAVE)
    ↑ ritorna
↓
SaveChangesAsync()   ← QUI
↓
response
//quindi transactionbehavior(che lancierà EFunitofwork!) circonda exceptionbehavior che circonda a sua volta handler!! cipolla.
 */

//public sealed class LoginHandler : ILoginHandler

public sealed class LoginHandler : IRequestHandler<LoginCommand, LoginResult> //x plugin MediatR(validazione automatica!) (works w fluentvalidation) see validationbehaviour.cs  dependencyinjection.cs

{

    //code without policies/ e loaders/   ORIGINAL CODE in AuthHandler.cs
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

    public async Task<LoginResult> Handle(LoginCommand cmd, CancellationToken ct)  //chiamalo proprio 'Handle' altrimenti mediatr da error
    {
        var user = await _loader.Load(cmd.Email, ct)
            ?? throw new UnauthorizedAccessException();
        _passwordPolicy.EnsureValid(cmd.Password, user);  //plain psw
        var access = _jwt.GenerateAccessToken(user);
        var refreshRaw = _jwt.GenerateRefreshToken();
        var refreshHash = _hasher.Hash(refreshRaw);
        var expiry = DateTime.UtcNow.AddDays(30);
        user.AddRefreshToken(refreshHash.Value, expiry);  //!top!
        //await _uow.SaveChangesAsync(ct);  //lo faccio automaticamente gia nel transactionbehavior.cs, durante la risalita verso in controller. ct ce l'hoha gia il transactionbehavior.
        return new(access, refreshRaw);
    }
    
}
