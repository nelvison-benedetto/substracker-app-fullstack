using SubSnap.Application.Ports.Persistence;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.UseCases.Auth.Login.Loaders;

public sealed class UserByEmailLoader
{
    private readonly IUserRepository _userRepository;

    public UserByEmailLoader(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<User?> Load(Email email, CancellationToken ct)
        => _userRepository.FindByEmailAsync(email, ct);
}
