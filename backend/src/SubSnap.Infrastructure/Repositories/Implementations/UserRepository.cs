using Microsoft.EntityFrameworkCore;
using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.Persistence;
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
        //non sarebbe correttissimo x DDD xk sto usando un service APPLICATIVO dentro un repository!! pero here è accettabile xk la query db dipende dal hash verification. cmnq ok per ora.
        if (token is null)
            return null;
        //ora hai un token valido, devi trovare il user collegato
        return await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == EF.Property<UserId>(token, "UserId"),  //legge la SHADOW KEY(la trovi nei xxxconfiguration e serve a legare le entitites e.g. subscriptions->user)!
                  //significa User.Id == RefreshToken.UserId solo che RefreshToken.UserId è la shadowkey
                ct);
    }
    public async Task<User?> FindByIdWithSharedLinksAsync(
        UserId id,
        CancellationToken ct
    )
    {
        return await _context.Users
            .Include("_sharedLinks")  //raro caso in cui usi .Include().here stai edit aggregate.
            .FirstOrDefaultAsync(x => x.Id == id, ct);
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
        //savechangesasync sarà fatto in transactionbehavior.cs durante la risalita verso il controller
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken ct)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }

    //repository CLEAN, no aggregates & loaders (quelli sono i folders dedicati).
}
