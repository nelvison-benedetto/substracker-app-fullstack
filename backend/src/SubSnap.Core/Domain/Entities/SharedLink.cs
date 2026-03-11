using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Core.Domain.Entities;

public class SharedLink
{
    public SharedLinkId Id { get; }
    public string Link { get; }
    public DateTime? ExpireAt { get; }
    public int Views { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public SharedLink() { }

    public SharedLink(
        string link,
        DateTime? expireAt
    )
    {
        Id = SharedLinkId.New();
        Link = link;
        ExpireAt = expireAt;
        Views = 0;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastUpdateAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }

}
