namespace SubSnap.Application.UseCases.Users.RegisterUser;

//i DTOs/Application parlano linguaggio applicativo (sono i/o dei servizi), non sanno nulla di HTTP! 
//public sealed record RegisterUserCommand(
//    string Email,
//    string Password
//);
public sealed class RUCommand
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}