using SubSnap.Application.UseCases.Users;
using SubSnap.Application.UseCases.Users.RegisterUser;

namespace SubSnap.Application.Ports.Services;

public interface IUserService
{
    Task<UserResult> RegisterAsync(RegisterUserCommand command);
}
