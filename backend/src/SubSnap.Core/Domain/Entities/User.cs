using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Core.Domain.Entities;

//user domain semplice (no ICollection no EF no navigation props), w no list of SharedLink e Subscription 
//rappresentare concetti di business, contenere logica (es. ChangeEmail, Login()), essere indipendente da EF, SQL, Docker, VPS
public class User
{
    private readonly List<RefreshToken> _refreshTokens = new();  //private xk SOLO user puo modificcarli, quindi i refreshtokens fanno parte dell'aggregate user!!(non hanno un lifecycle indipendente, non hanno un Repository proprio, non possono vivere senza user!!)
    
    public UserId Id { get; private set; }  //type other obj (readonly struct)(./ValueObjects/), COSI FAI LA VALIDAZIONE
        //nullable. verrà creato da DB. nessuns 'private setter' sull'id, domain puro
    public Email Email { get; private set; }   //type other obj  (readonly struct)(./ValueObjects/)
        //private set; perche in futuro voglio poterlo cambiare here only w e.g. method ChangeEmail()
    public PasswordHash PasswordHash { get; private set; }   //type other obj  (readonly struct)(./ValueObjects/)

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastLogin { get; private set; }  //private set; xk editabile solo here da method UpdateLastLogin()
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;
    

    protected User() { }  //constructor!! x ORM only

    public User(   //constructor
        //UserId? id,  //nullable
        Email email,
        PasswordHash passwordHash
        //DateTime createdAt,
        //DateTime updatedAt,
        //DateTime? lastLogin
    )
    {
        //Id = id?? UserId.New();  //fallback genero dal backend!!
        Id = UserId.New();
        Email = email;
        PasswordHash = passwordHash;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    //internal void SetId(UserId id)  //IMPORTANTISSISMO! xk ti serve x obj entity-->domain obj
    //     //internal, solo .Infrastructure(stesso prj, assembly) puo usarlo!
    //{
    //    if (Id != null)
    //        throw new InvalidOperationException("Id already set");
    //    Id = id;
    //    //utilizza anche file.Core / AssemblyInfo.cs
    //}

    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    public void AddRefreshToken(string token, DateTime expiresAt)
    {
        _refreshTokens.Add(new RefreshToken(token, expiresAt));
    }
    public RefreshToken? FindActiveRefreshToken( string providedToken, IPasswordHasherService hasher)
    {
        return _refreshTokens
            .FirstOrDefault(rt =>
                rt.IsActive() &&
                hasher.Verify(
                    providedToken,
                    new PasswordHash(rt.Token)));
    }
    public void RevokeRefreshToken(RefreshToken token)
    {
        if (!token.IsActive())
            throw new InvalidOperationException("Invalid refresh token");

        token.Revoke();
    }

}
