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
        var refreshRaw = _jwtTokenService.GenerateRefreshToken();
        var refreshHash = _passwordHasherService.Hash(refreshRaw);
        var expiry = DateTime.UtcNow.AddDays(30);
        user.AddRefreshToken(refreshHash.Value, expiry);
        await _uow.SaveChangesAsync();
        return (accessToken, refreshRaw);
    }
    public async Task LogoutAsync(UserId userId, string refreshToken)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new UnauthorizedAccessException();
        var token = user.FindActiveRefreshToken(refreshToken,_passwordHasherService)
            ?? throw new UnauthorizedAccessException();
        user.RevokeRefreshToken(token);
        await _uow.SaveChangesAsync();
    }

    public async Task<(string access, string refresh)> RefreshAsync(string refreshToken)
    {
        // 1️⃣ trova utente con token attivo
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException();
        // 2️⃣ trova token domain
        var token = user.FindActiveRefreshToken(
            refreshToken,
            _passwordHasherService)
            ?? throw new UnauthorizedAccessException();
        // 3️⃣ revoke old token (ROTATION)
        user.RevokeRefreshToken(token);
        // 4️⃣ crea nuovi token
        var newAccess = _jwtTokenService.GenerateAccessToken(user);

        var newRefreshRaw = _jwtTokenService.GenerateRefreshToken();
        var newRefreshHash = _passwordHasherService.Hash(newRefreshRaw);

        user.AddRefreshToken(
            newRefreshHash.Value,
            DateTime.UtcNow.AddDays(30));

        await _uow.SaveChangesAsync();
        return (newAccess, newRefreshRaw);
    }
}
