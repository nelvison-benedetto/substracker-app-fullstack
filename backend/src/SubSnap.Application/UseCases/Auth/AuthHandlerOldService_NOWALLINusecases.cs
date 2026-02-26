//using SubSnap.Application.Ports.Auth;
//using SubSnap.Application.Ports.Persistence;
//using SubSnap.Core.Domain.ValueObjects;

//namespace SubSnap.Application.UseCases.Auth;

////not connected in SubSnap.Infrastructure.DependencyInjection.ServiceCollectionExtensions bc intanto QUESTO FILE NON LO USO now all in usecases!!

//public class AuthHandler
//{
//    private readonly IUserRepository _userRepository;
//    private readonly IPasswordHasherService _passwordHasherService;
//    private readonly IJwtTokenService _jwtTokenService;
//    private readonly IUnitOfWork _uow;

//    public AuthHandler(
//        IUserRepository userRepo,
//        IPasswordHasherService passwordHasherService,
//        IJwtTokenService jwtTokenService,
//        IUnitOfWork uow)
//    {
//        _userRepository = userRepo;
//        _passwordHasherService = passwordHasherService;
//        _jwtTokenService = jwtTokenService;
//        _uow = uow;
//    }

//    public async Task<(string accessToken, string refreshToken)> LoginAsync(Email email, string plainPassword, CancellationToken ct)
//    {
//        var user = await _userRepository.FindByEmailAsync(email, ct)
//            ?? throw new UnauthorizedAccessException();

//        if (!_passwordHasherService.Verify(plainPassword, user.PasswordHash))
//            throw new UnauthorizedAccessException();
        
//        var accessToken = _jwtTokenService.GenerateAccessToken(user);
//        var refreshRaw = _jwtTokenService.GenerateRefreshToken();
//        var refreshHash = _passwordHasherService.Hash(refreshRaw);
//        var expiry = DateTime.UtcNow.AddDays(30);
//        user.AddRefreshToken(refreshHash.Value, expiry);
//        await _uow.SaveChangesAsync();
//        return (accessToken, refreshRaw);
//    }
//    public async Task LogoutAsync(UserId userId, string refreshToken, CancellationToken ct)
//    {
//        var user = await _userRepository.FindByIdAsync(userId, ct)
//            ?? throw new UnauthorizedAccessException();
//        //var token = user.FindActiveRefreshToken(refreshToken, _passwordHasherService)
//        //    ?? throw new UnauthorizedAccessException();
//        var token = user.FindActiveRefreshToken(
//            storedToken => _passwordHasherService.Verify(
//                    refreshToken,
//                    new PasswordHash(storedToken)))  //ora vero DDD (user.cs Domain non consce esterni ma gli arriva solo una funct)
//            ?? throw new UnauthorizedAccessException();
//        user.RevokeRefreshToken(token);
//        await _uow.SaveChangesAsync();
//    }
    
//    public async Task<(string access, string refresh)> RefreshAsync(string refreshToken, CancellationToken ct)
//    {
//        // 1️⃣ trova utente con token attivo
//        var user = await _userRepository.FindByRefreshTokenAsync(refreshToken, ct)
//            ?? throw new UnauthorizedAccessException();
//        // 2️⃣ trova token domain
//        //var token = user.FindActiveRefreshToken(refreshToken,_passwordHasherService)  //WRONG non vedo DDD x User.cs
//        var token = user.FindActiveRefreshToken(
//            storedToken => _passwordHasherService.Verify(
//                    refreshToken,
//                    new PasswordHash(storedToken)))  //ora vero DDD (user.cs Domain non consce esterni ma gli arriva solo una funct)
//            ?? throw new UnauthorizedAccessException();
//        //    ?? throw new UnauthorizedAccessException();
//        // 3️⃣ revoke old token (ROTATION)
//        user.RevokeRefreshToken(token);
//        // 4️⃣ crea nuovi token
//        var newAccess = _jwtTokenService.GenerateAccessToken(user);

//        var newRefreshRaw = _jwtTokenService.GenerateRefreshToken();
//        var newRefreshHash = _passwordHasherService.Hash(newRefreshRaw);

//        user.AddRefreshToken(
//            newRefreshHash.Value,
//            DateTime.UtcNow.AddDays(30));

//        await _uow.SaveChangesAsync();
//        return (newAccess, newRefreshRaw);
//    }
//}
