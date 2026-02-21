using SubSnap.Core.Abstractions.Identity;
using SubSnap.Core.Contracts.Repositories;
using SubSnap.Core.Contracts.UnitOfWork;
using SubSnap.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Core.Services.Application;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _uow;

    public AuthService(
        IUserRepository userRepo,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService,
        IUnitOfWork uow)
    {
        _userRepository = userRepo;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
        _uow = uow;
    }

    public async Task<(string accessToken, string refreshToken)> LoginAsync(Email email, string plainPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email)
            ?? throw new UnauthorizedAccessException();

        if (!_passwordHasherService.Verify(plainPassword, user.PasswordHash))
            throw new UnauthorizedAccessException();

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var hashed = _passwordHasherService.Hash(refreshToken);
        var expiry = DateTime.UtcNow.AddDays(30);
        user.AddRefreshToken(hashed.ToString(), expiry);
        await _uow.SaveChangesAsync();
        return (accessToken, refreshToken);
    }
    public async Task LogoutAsync(UserId userId, string refreshToken)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new Exception("User not found");
        user.RevokeRefreshToken(refreshToken);
        await _uow.SaveChangesAsync();
    }

}
