using Microsoft.EntityFrameworkCore;
using SubSnap.Core.Domain.Entities;
using SubSnap.Infrastructure.Persistence.Outbox;

namespace SubSnap.Infrastructure.Persistence.Context;

//see applicationdbcontext.cs  servicecollectionextensions.cs  dependencyinjection.cs

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    //SOLO AGGREGATE ROOTS
    public DbSet<User> Users => Set<User>();  //solo la ROOT DELL'AGGREGATE deve essere esposto al dbset. ora EF puo PUO TRACCIARE questa aggregate root. 
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();  //x dire a ef di creare tab OutboxMessages
    //public DbSet<UserMedia> userMedias => Set<UserMedia>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        //prende assembly corrente -> trova tutte le classi IEntityTypeConfiguration<T> -> le esegue. trova tutte le tue xxxConfiguration.
    }
    //scansiona l'assembly, e dici come mappare domain->db. altrimenti dovresti fare a mano modelBuilder.ApplyConfiguration(new UserConfiguration());...(anche per tutte le altri xxxConfiguration.cs)

}
