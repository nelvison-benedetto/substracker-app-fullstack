using MediatR;
using SubSnap.Application.UseCases.Auth.Logout.Loaders;
using SubSnap.Application.UseCases.Auth.Logout.Policies;

namespace SubSnap.Application.UseCases.Auth.Logout;

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

//public sealed class LogoutHandler : ILogoutHandler

public sealed class LogoutHandler : IRequestHandler<LogoutCommand> //x plugin MediatR(validazione automatica!) (works w fluentvalidation) see validationbehaviour.cs  dependencyinjection.cs    

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
    //private readonly IUnitOfWork _uow;

    public LogoutHandler(
        UserByIdLoader loader,
        LogoutTokenPolicy policy)
    {
        _loader = loader;
        _policy = policy;
    }
    public async Task Handle( LogoutCommand command, CancellationToken ct)
    {
        var user = await _loader.Load(command.UserId, ct)
            ?? throw new UnauthorizedAccessException();

        var token = _policy.EnsureValid( command.RefreshToken, user);

        user.RevokeRefreshToken(token);
        //await _uow.SaveChangesAsync(ct); lo faccio nel transactionbehavior.cs durante la risalita verso il controller
    }

}
