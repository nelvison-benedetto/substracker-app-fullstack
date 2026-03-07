using MediatR;

namespace SubSnap.Application.UseCases.Users.DeleteUser;

public sealed record DeleteUserCommand(string userId) : IRequest;