using Microsoft.EntityFrameworkCore;
using SubSnap.Core.Abstractions.Identity;
using SubSnap.Core.Contracts.Repositories;
using SubSnap.Core.Domain.Aggregates;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
//using SubSnap.Infrastructure.Mapping;
using SubSnap.Infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Infrastructure.Repositories.Implementations;

//repository = composizione query
//here NO savechangesasync !!, because it's done in unit of work!!

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasherService _passwordHasherService;
    public UserRepository(ApplicationDbContext context, IPasswordHasherService passwordHasherService)
    {
        _context = context;
        _passwordHasherService = passwordHasherService;
    }

    public async Task<Core.Domain.Entities.User?> GetByIdAsync(UserId id)  //Task<User?> because it can return null, return type is domain User
    {
        return await _context.Users
            //.AsNoTracking()  //NO TRACKING xk ef non deve tracciare nulla, EF here è sol x data reading.
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(x => x.Id == id);
        //ora entity è di type 'Infrastructure.Persistence.Scaffold.User?'
        //return entity is null ? null : UserMapper.ToDomain(entity);  //mapping entity found to domain!!CLEAN ARCHITECTURE
    }

    public async Task<User?> GetByEmailAsync(Email email)
    {
        return await _context.Users
            //.AsNoTracking() non usarlo, xk altrimenti refresh token non viene salvato
            .FirstOrDefaultAsync(x => x.Email == email);
        //return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        var users = await _context.Users.Include(u => u.RefreshTokens).ToListAsync();
        return users.FirstOrDefault(u => u.RefreshTokens.Any(rt => _passwordHasherService.Verify(refreshToken, new PasswordHash(rt.Token))));
    }


    //---COMMANDS---
    public Task AddAsync(User user)  //Repository + SaveChanges (DDD puro)
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

    public async Task<UserAggregate?> GetUserAggregateAsync(UserId userId)
    {
        var userEntity = await _context.Users
            .Include(u => u.Subscriptions)
            .Include(u => u.SharedLinks)
            .FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (userEntity is null) return null;
        var userDomain = new User(
            new Email(userEntity.Email),
            new PasswordHash(userEntity.PasswordHash)
        );
        var subscriptionsDomain = userEntity.Subscriptions
            .Select(s => new Subscription(
                new SubscriptionId(s.Id),
                s.Amount,
                s.CreatedAt
            ))
            .ToList();
        var sharedLinksDomain = userEntity.SharedLinks
            .Select(sl => new SharedLink(
                new SharedLinkId(sl.Id),
                sl.Url,
                sl.CreatedAt
            ))
            .ToList();
        return new UserAggregate(userDomain, subscriptionsDomain, sharedLinksDomain);
    }

    public async Task<UserSubscriptionsAggregate?> GetUserWithSubscriptionsAsync(UserId userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return null;
        var subscriptions = await _context.Set<Subscription>()
            .Where(s => EF.Property<Guid>(s, "UserId") == userId.Value)  //UserId è una shadow property, questo è l'unico modo x usarla!!
            .ToListAsync();
        return new UserSubscriptionsAggregate(user, subscriptions);
    }

}
