using SubSnap.Application.Ports.Auth;
using SubSnap.Core.Domain.Entities;

namespace SubSnap.Application.UseCases.Auth.Login.Policies;

public sealed class PasswordPolicy
{
    private readonly IPasswordHasherService _hasher;

    public PasswordPolicy(IPasswordHasherService hasher)
    {
        _hasher = hasher;
    }

    public void EnsureValid(string password, User user)
    {
        if (!_hasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException();
    }
}
