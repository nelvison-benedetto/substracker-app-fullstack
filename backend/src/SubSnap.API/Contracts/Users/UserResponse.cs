//namespace SubSnap.Core.DTOs.External.Responses.Users;

namespace SubSnap.API.Contracts.Users;


//quello che esponi al world (xk non esponi mai domain o entity)
public sealed class UserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = null!;  //null! zittisce il compilatore, dice 'So che sembra null, ma fidati di me: non lo sarà a runtime.'. xk di default string non puo essere null.
}
