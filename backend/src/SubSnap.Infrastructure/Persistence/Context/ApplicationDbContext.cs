using Microsoft.EntityFrameworkCore;
using SubSnap.Core.Domain.Entities;
using SubSnap.Infrastructure.Persistence.Outbox;

namespace SubSnap.Infrastructure.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();  //solo la root dell'aggregate deve essere esposto al dbset.
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();  //x outbox pattern!!

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
