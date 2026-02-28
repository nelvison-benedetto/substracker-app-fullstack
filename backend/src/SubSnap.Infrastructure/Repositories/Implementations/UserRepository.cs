using Microsoft.EntityFrameworkCore;
using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Core.Domain.Aggregates;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
//using SubSnap.Infrastructure.Mapping;
using SubSnap.Infrastructure.Persistence.Context;

namespace SubSnap.Infrastructure.Repositories.Implementations;

//repository = composizione query
//here NO savechangesasync !!, because it's done in unit of work!!

//Cancellation token: serve per interrompere la query! e.g.la quesry sta ancora girando ma utente esce dal sito.

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasherService _passwordHasherService;
    public UserRepository(ApplicationDbContext context, IPasswordHasherService passwordHasherService)
    {
        _context = context;
        _passwordHasherService = passwordHasherService;
    }
    
    public async Task<Core.Domain.Entities.User?> FindByIdAsync(UserId id, CancellationToken ct)  //Task<User?> because it can return null, return type is domain User
    {
        return await _context.Users
            //.AsNoTracking()  //NO TRACKING xk ef non deve tracciare nulla, EF here è sol x data reading.
            //.Include(u => u.RefreshTokens)  per Uber-style il methodo deve essere minimal 0 include() inutili 
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        //ora entity è di type 'Infrastructure.Persistence.Scaffold.User?'
        //return entity is null ? null : UserMapper.ToDomain(entity);  //mapping entity found to domain!!CLEAN ARCHITECTURE
    }

    public async Task<User?> FindByEmailAsync(Email email, CancellationToken ct)
    {
        return await _context.Users
            //.AsNoTracking() non usarlo, xk altrimenti refresh token non viene salvato
            .FirstOrDefaultAsync(x => x.Email == email, ct);
        //return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> FindByRefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        //var users = await _context.Users.Include(u => u.RefreshTokens).ToListAsync();
        //return users.FirstOrDefault(u => u.RefreshTokens.Any(rt => _passwordHasherService.Verify(refreshToken, new PasswordHash(rt.Token))));
        var token = await _context.Set<RefreshToken>()
            .AsNoTracking()
            .FirstOrDefaultAsync(rt =>
                _passwordHasherService.Verify(
                    refreshToken,
                    new PasswordHash(rt.Token)),
                ct);

        if (token is null)
            return null;

        return await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == EF.Property<UserId>(token, "UserId"),
                ct);
    }

    //---COMMANDS---
    public Task AddAsync(User user, CancellationToken ct)  //Repository + SaveChanges (DDD puro)
    {
        //var entity = UserMapper.ToEntity(user);
        //_context.User.Add(entity);
        //await _context.SaveChangesAsync();
        ////user.Id = new UserId(entity.UserId); //domain aggiornato, da fare DOPO il savechanges!!
        //return new User(  //ritorno new w id
        //    new UserId(entity.UserId),
        //    user.Email,
        //    user.PasswordHash,
        //    user.CreatedAt,
        //    user.UpdatedAt,
        //    user.LastLogin
        //);
        //var entity = UserMapper.ToEntity(user);
        _context.Users.Add(user);
        // ❗ NON SaveChanges qui
        // l'ID verrà valorizzato DOPO dal UnitOfWork!!
        return Task.CompletedTask;
    }

    //---AGGREGATES---

    //old approach
    //public async Task<UserAggregate?> GetAggregateAsync(UserId id)
    //{
    //    var entity = await _context.Users
    //        .Include(u => u.Subscription)  //ok! gli aggregate vanno caricati insieme!
    //        .Include(u => u.SharedLink)
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(u => u.UserId == id.Value);

    //    return entity is null ? null : UserAggregateMapper.ToDomain(entity);
    //}

    public async Task<UserAggregate?> LoadUserAggregateAsync(UserId userId, CancellationToken ct)
    {
        // no navigation properties fra aggregates quindi NO .Include()!!, usa invece query separate. style usato anche da Uber & Stripe

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);  //add ct cosi da propagarlo fino al db driver.

        if (user is null)
            return null;

        var subscriptionsTask =  _context.Set<Subscription>()  //here non usare 'await'
            .AsNoTracking()
            .Where(s => EF.Property<Guid>(s, "UserId") == userId.Value)
            .ToListAsync(ct); //add ct cosi da propagarlo fino al db driver.

        var sharedLinksTask =  _context.Set<SharedLink>()  //here non usare 'await'
            .AsNoTracking()
            .Where(sl => EF.Property<Guid>(sl, "UserId") == userId.Value)
            .ToListAsync(ct);

        await Task.WhenAll(subscriptionsTask, sharedLinksTask);  //EF work using parallel queries!! pero EF di default non puo fare parallel queries, quindi devi settare IDbContextFactory<ApplicationDbContext>...INFO

        return new UserAggregate(
            user,
            subscriptionsTask.Result,
            sharedLinksTask.Result);
    }
    
    public async Task<UserSubscriptionsAggregate?> LoadUserWithSubscriptionsAsync(UserId userId, CancellationToken ct)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return null;

        var subscriptions = await _context.Set<Subscription>()
            .AsNoTracking()
            .Where(s => EF.Property<Guid>(s, "UserId") == userId.Value)  //usi la shadow property
            .ToListAsync(ct);

        return new UserSubscriptionsAggregate(user, subscriptions);
    }

}
