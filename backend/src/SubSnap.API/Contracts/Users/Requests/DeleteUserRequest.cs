namespace SubSnap.API.Contracts.Users.Requests;

public class DeleteUserRequest
{
    public required Guid UserId { get; init; }
}
