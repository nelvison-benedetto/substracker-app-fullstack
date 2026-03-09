using MediatR;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Application.UseCases.Users.DeleteUser.Loaders;
using SubSnap.Core.Domain.Exceptions;

namespace SubSnap.Application.UseCases.Users.DeleteUser;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly UserByIdLoader _loader;
    private readonly IUserRepository _userRepository;

    public DeleteUserHandler(
        UserByIdLoader loader,
        IUserRepository userRepository
    )
    {
        _loader = loader;
        _userRepository = userRepository;
    }

    public async Task Handle(DeleteUserCommand command, CancellationToken ct)
    {
        var user = await _loader.Load( command.userId, ct)
            ?? throw new UserNotFoundException(command.userId);
        await _userRepository.DeleteAsync(user, ct);

    }
    

}
