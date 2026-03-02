using MediatR;
using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.Exceptions;
using SubSnap.Core.Domain.ValueObjects;
using MediatR;

namespace SubSnap.Application.UseCases.Users.RegisterUser;

//no EF, no DBO 
//transazione controllata, orchestration pulita

//public class RUHandler : IRUHandler 
public sealed class RUHandler : IRequestHandler<RUCommand, RUResult>  //x plugin MediatR(validazione automatica!) (works w fluentvalidation) see validationbehaviour.cs  dependencyinjection.cs validationextensions.cs

{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasherService;

    public RUHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasherService passwordHasherService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasherService = passwordHasherService;
    }

    //public async Task<UserResult> RegisterAsync(RegisterUserCommand command)
    //{
    //    // 1 Controllo se esiste già l'email
    //    var existing = await _userRepository.GetByEmailAsync(new Email(command.Email));
    //    if (existing != null)
    //        throw new EmailAlreadyRegisteredException(command.Email);
    //    // 2️ Creo il domain entity
    //    var user = new User(
    //        //id: null,  
    //        email: new Email(command.Email),
    //        passwordHash: new PasswordHash(command.Password)
    //        //createdAt: DateTime.UtcNow,
    //        //updatedAt: DateTime.UtcNow,
    //        //lastLogin: null
    //    );
    //    await _userRepository.AddAsync(user);  // 2. Aggiungo al repository (senza SaveChanges!)
    //    await _unitOfWork.SaveChangesAsync();  // 3. Commit tramite UnitOfWork
    //    //ora hai 2 opzioni per ottenere l'id della nuova row aggiunta su db:
    //    //1.   ricarichi dal DB (DDD pulito e sicuro!), ma ti costa un'ulteriore query, ma ok
    //    var saved = await _userRepository.GetByEmailAsync(user.Email);
    //    //2.   un po piu violento sul DDD e puo essere overkill, ma eviti di fare nuova query
    //    //user.SetId(new UserId(entity.UserId)); // entity = EF tracked entity che ora ha l'ID
    //    return new UserResult(
    //        user.Id.Value,
    //        user.Email.Value
    //    );
    //}

    public async Task<RUResult> Handle(RUCommand command, CancellationToken ct)  
        //plugin MediatR vuole che si chiami 'Handle' non 'HandleAsync'
    {
        // 1️⃣ Email unique
        var existing =
            await _userRepository.FindByEmailAsync(new Email(command.Email), ct);

        if (existing != null)
            throw new EmailAlreadyRegisteredException(command.Email);
        // 2️⃣ HASH PASSWORD (IMPORTANTISSIMO)
        var passwordHash = _passwordHasherService.Hash(command.Password);
        // 3️⃣ Create domain entity
        var user = new User(
            new Email(command.Email),
            passwordHash
        );
        // 4️⃣ Persist
        await _userRepository.AddAsync(user, ct);
        //await _unitOfWork.SaveChangesAsync(ct);  //sempre propagare il token!!serve e.g.se utente spegne il cellulare! OLD, NOW CENTRALIZZATO IN transactionbehaviour.cs !!!
        return new RUResult(
            user.Id.Value,
            user.Email.Value
        );
    }

}
