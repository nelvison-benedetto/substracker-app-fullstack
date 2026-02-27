namespace SubSnap.API.Contracts.Auth.Requests;

public sealed class RefreshTokenRequestAuth
{
    public required string RefreshToken { get; init; }
}
