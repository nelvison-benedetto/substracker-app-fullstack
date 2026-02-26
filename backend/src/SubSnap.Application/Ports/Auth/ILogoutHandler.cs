using SubSnap.Application.UseCases.Auth.Logout;

namespace SubSnap.Application.Ports.Auth;

public interface ILogoutHandler
{
    Task Handle(LogoutCommand command, CancellationToken ct);
}
