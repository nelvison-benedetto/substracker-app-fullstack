using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.Ports.Auth;

public interface IPasswordHasherService   //non lo chiamo IAspNetPasswordHAsher xk x astrazione non devo citare nessuna tecnologia
{
    PasswordHash Hash(string plainPassword);
    bool Verify(string plainPassword, PasswordHash passwordHash);
}
