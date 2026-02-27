using MediatR;
namespace SubSnap.Application.UseCases.Users.RegisterUser;

//i DTOs/Application parlano linguaggio applicativo (sono i/o dei servizi), non sanno nulla di HTTP! 
//public sealed record RegisterUserCommand(
//    string Email,
//    string Password
//);

//public sealed class RUCommand
//{
//    public required string Email { get; init; }
//    public required string Password { get; init; }
//}

//public sealed record RUCommand(string Email, string Password);

//uso MediatR x validazione auto!! 
public sealed record RUCommand(string Email, string Password) : IRequest<RUResult>;  // MediatR, specifica che questo comando richiede un risultato di tipo RUResult. In questo modo, quando lo invii al MediatR, sa che deve aspettarsi un RUResult come risposta!
