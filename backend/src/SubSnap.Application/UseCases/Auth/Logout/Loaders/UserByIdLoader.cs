using SubSnap.Application.Ports.Persistence;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.UseCases.Auth.Logout.Loaders;

public class UserByIdLoader
{
    private readonly IUserRepository _userRepository;
    public UserByIdLoader(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<User?> Load(UserId userId, CancellationToken ct) 
        => _userRepository.FindByIdAsync(userId, ct);
        

}
