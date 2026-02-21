using Microsoft.EntityFrameworkCore;
using SubSnap.Core.Contracts.Repositories;
using SubSnap.Core.Domain.Aggregates;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
using SubSnap.Infrastructure.Mapping;
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
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Core.Domain.Entities.User?> GetByIdAsync(UserId id)  //Task<User?> because it can return null, return type is domain User
    {
        return await _context.Users
            .AsNoTracking()  //NO TRACKING xk ef non deve tracciare nulla, EF here è sol x data reading.
            .FirstOrDefaultAsync(x => x.Id == id);
        //ora entity è di type 'Infrastructure.Persistence.Scaffold.User?'
        //return entity is null ? null : UserMapper.ToDomain(entity);  //mapping entity found to domain!!CLEAN ARCHITECTURE
    }

    public async Task<User?> GetByEmailAsync(Email email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email);
        //return entity is null ? null : UserMapper.ToDomain(entity);
    }

    //AGGREGATES  //TO SET CORRECT SOLO DOPO CHE FACCIO I CRUD DI BASE X ALL
    //public async Task<UserAggregate?> GetAggregateAsync(UserId id)
    //{
    //    var entity = await _context.Users
    //        .Include(u => u.Subscription)  //ok! gli aggregate vanno caricati insieme!
    //        .Include(u => u.SharedLink)
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(u => u.UserId == id.Value);

    //    return entity is null ? null : UserAggregateMapper.ToDomain(entity);
    //}

    //COMMANDS
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

}
