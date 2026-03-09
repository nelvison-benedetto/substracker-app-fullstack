using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.Ports.Persistence;

public interface IUserRepository
{
    Task<Core.Domain.Entities.User?> FindByIdAsync(UserId id, CancellationToken ct);  //validazione tramite type UserId
    Task<User?> FindByEmailAsync(Email email, CancellationToken ct);
    Task<User?> FindByRefreshTokenAsync(string refreshToken, CancellationToken ct);

    //COMMANDS
    Task AddAsync(User user, CancellationToken ct);

    Task DeleteAsync(User user, CancellationToken ct);

}
