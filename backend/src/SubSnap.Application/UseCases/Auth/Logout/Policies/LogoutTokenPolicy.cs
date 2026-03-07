using SubSnap.Application.Ports.Auth;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.UseCases.Auth.Logout.Policies;

public class LogoutTokenPolicy
{
    private readonly IPasswordHasherService _hasher;

    public LogoutTokenPolicy(IPasswordHasherService hasher)
    {
        _hasher = hasher;
    }
    public Core.Domain.Entities.RefreshToken EnsureValid(string providedToken, User user) 
    {  //uso tutto il path xk ho anche folder che si chiama RefreshToken e VS si confonde
        var token = user.FindActiveRefreshToken(
            storedToken =>  //rappresenta il token salvato sul db, cioe RefreshToken.Token !
                _hasher.Verify( providedToken, new PasswordHash(storedToken)));

        return token ?? throw new UnauthorizedAccessException();
    }
    //lambda expression. è come dire bool Match(string storedToken){return _hasher.Verify(providedToken, new PasswordHash(storedToken));}
}
