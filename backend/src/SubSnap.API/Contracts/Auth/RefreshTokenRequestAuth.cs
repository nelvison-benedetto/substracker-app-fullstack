namespace SubSnap.API.Contracts.Auth;

public sealed class RefreshTokenRequestAuth
{
    public required string RefreshToken { get; init; }
}
