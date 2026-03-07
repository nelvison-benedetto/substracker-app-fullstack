using MediatR;
using SubSnap.Application.UseCases.Users.DeleteUser.Loaders;

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
        
    }

}
