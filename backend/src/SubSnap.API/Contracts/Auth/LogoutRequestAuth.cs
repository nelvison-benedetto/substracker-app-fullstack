namespace SubSnap.API.Contracts.Auth;

public sealed class LogoutRequestAuth
{
    public required string RefreshToken { get; init; }
}
