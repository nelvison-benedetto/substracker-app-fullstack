namespace SubSnap.API.Contracts.Auth.Requests;

public sealed class LogoutRequestAuth
{
    public required string RefreshToken { get; init; }
}
