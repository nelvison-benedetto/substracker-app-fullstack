namespace SubSnap.Core.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; }  //questo meglio salvarlo Hashato(come un psw), non in plain chiaro!!TODO
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private RefreshToken() { }  //x ORM

    internal RefreshToken(string token, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        Token = token;
        ExpiresAt = expiresAt;
        IsRevoked = false;
    }

    public void Revoke() => IsRevoked = true;
    public bool IsActive() => !IsRevoked && DateTime.UtcNow < ExpiresAt;

}
