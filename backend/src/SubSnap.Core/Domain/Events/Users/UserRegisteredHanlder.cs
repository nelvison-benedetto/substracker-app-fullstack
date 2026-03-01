using MediatR;
using Microsoft.Extensions.Logging;

namespace SubSnap.Core.Domain.Events.Users;

public sealed class UserRegisteredHandler
    : INotificationHandler<UserRegisteredEvent>  //subscriber
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
            notification.UserId.Value);  //NON CONCATENARE W '+', ALTRIMENTI ILLEGGIBILE xk diventa pura str, ora ok leggibile da plugin OpenTelemetry!!
        //internamente diventa {"message": "User registered", "UserId": "123","level": "Information"}
        //e quindi nei logs puoi fare query e.g. UserId=123 per filtrare

        // future:
        // send email
        // analytics
        // webhook

        return Task.CompletedTask;
    }
}
