using MediatR;
using SubSnap.Application.UseCases.Users.DeleteUser.Loaders;
using SubSnap.Core.Domain.Exceptions;

namespace SubSnap.Application.UseCases.Users.DeleteUser;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly UserByIdLoader _loader;

    public DeleteUserHandler(
        UserByIdLoader loader
    )
    {
        _loader = loader;
    }

    public async Task Handle(DeleteUserCommand command, CancellationToken ct)
    {
        var user = await _loader.Load(command.userId, ct)
            ?? throw new UserNotFoundException(command.userId);


    }

}
