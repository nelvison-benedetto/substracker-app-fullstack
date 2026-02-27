namespace SubSnap.API.Contracts.Users.Responses;

public class RegisterUserResponse
{
    public Guid Id { get; init; }
    public required string Email { get; init; }  //better than =null!; safety e c# moderno ok!
    //public string Email { get; init; } = null!;  //null! zittisce il compilatore, dice 'So che sembra null, ma fidati di me: non lo sarà a runtime.'. xk di default string non puo essere null.
}
