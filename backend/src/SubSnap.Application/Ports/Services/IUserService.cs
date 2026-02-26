using SubSnap.Application.UseCases.Users;
using SubSnap.Core.DTOs.Application.Commands.Users;

namespace SubSnap.Application.Ports.Services;

public interface IUserService
{
    Task<UserResult> RegisterAsync(Command command);
}
