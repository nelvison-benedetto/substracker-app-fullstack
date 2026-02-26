using SubSnap.Application.Ports.Persistence;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Application.UseCases.Auth.Login.Loaders;

public sealed class UserByEmailLoader
{
    private readonly IUserRepository _userRepository;

    public UserByEmailLoader(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<User?> Load(Email email, CancellationToken ct)
        => _userRepository.FindByEmailAsync(email, ct);
}
