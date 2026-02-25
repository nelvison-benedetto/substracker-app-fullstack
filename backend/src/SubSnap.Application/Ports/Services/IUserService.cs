namespace SubSnap.Application.Ports.Services;

public interface IUserService
{
    Task<UserResult> RegisterAsync(RegisterUserCommand command);
}
