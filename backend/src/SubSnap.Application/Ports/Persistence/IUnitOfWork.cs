namespace SubSnap.Application.Ports.Persistence;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}



