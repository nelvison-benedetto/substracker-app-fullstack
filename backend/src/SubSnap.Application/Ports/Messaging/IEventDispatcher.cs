using SubSnap.Core.Domain.Common;

namespace SubSnap.Application.Ports.Messaging;

public interface IEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events,CancellationToken ct = default);
}
