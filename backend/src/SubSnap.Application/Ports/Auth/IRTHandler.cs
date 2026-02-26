using SubSnap.Application.UseCases.Auth.RefreshToken;

namespace SubSnap.Application.Ports.Auth;

public interface IRTHandler
{
    Task<RTResult> Handle(RTCommand command, CancellationToken ct);
}
