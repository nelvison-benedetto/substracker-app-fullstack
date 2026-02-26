using SubSnap.Application.Ports.Persistence;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Application.UseCases.Auth.Logout.Loaders;

public class UserByIdLoader
{
    private readonly IUserRepository _userRepository;
    public UserByIdLoader(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<User?> Load(UserId userId, CancellationToken ct) 
        => _userRepository.FindByIdAsync(userId, ct);
        

}
