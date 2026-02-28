using MediatR;
using Microsoft.Extensions.Logging;

namespace SubSnap.Core.Domain.Events.Users;

public sealed class UserRegisteredHandler
    : INotificationHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredHandler> _logger;

    public UserRegisteredHandler(
        ILogger<UserRegisteredHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        UserRegisteredEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "User {UserId} registered",
            notification.UserId.Value);

        // future:
        // send email
        // analytics
        // webhook

        return Task.CompletedTask;
    }
}
