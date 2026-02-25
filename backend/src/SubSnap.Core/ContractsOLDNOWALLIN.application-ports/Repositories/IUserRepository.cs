//using SubSnap.Core.Domain.Aggregates;
//using SubSnap.Core.Domain.Entities;
//using SubSnap.Core.Domain.ValueObjects;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SubSnap.Core.Contracts.Repositories;

//public interface IUserRepository
//{
//    Task<Core.Domain.Entities.User?> FindByIdAsync(UserId id, CancellationToken ct);  //validazione tramite type UserId
//    Task<User?> FindByEmailAsync(Email email, CancellationToken ct);
//    Task<User?> FindByRefreshTokenAsync(string refreshToken, CancellationToken ct);

//    //COMMANDS
//    Task AddAsync(User user, CancellationToken ct);

//}
