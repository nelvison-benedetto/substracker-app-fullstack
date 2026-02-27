namespace SubSnap.API.Contracts.Users.Requests;

public class RegisterUserRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
