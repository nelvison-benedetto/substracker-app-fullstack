using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Core.Domain.Exceptions;

public abstract class NotFoundException : Exception
{
    protected NotFoundException(string message) : base(message) { }
}

// esempio specifico per User, see .api\middleware\exceptionhandling\ExceptionMiddlewareExtensions.cs x more info!!!
public sealed class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(UserId userId)
        : base($"User with id {userId} was not found.") { }
}

