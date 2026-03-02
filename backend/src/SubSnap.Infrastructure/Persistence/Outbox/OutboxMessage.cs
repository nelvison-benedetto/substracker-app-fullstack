namespace SubSnap.Infrastructure.Persistence.Outbox;

/*
tabella su db. serve per persistence.
 */
//see User.cs  transactionbehavior.cs efunitofwork.cs  outboxprocessor.cs outboxmessage.cs

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;  //sara '[Namespace.EventName], [XAssembly]'. serve per ricostruire il tipo via reflection. e.g. SubSnap.Core.Domain.Events.Users.UserRegisteredEvent, SubSnap.Core
    public string Payload { get; set; } = default!;  //domain event serializzato json
    public DateTime OccurredOnUtc { get; set; }  //quando è accaduto nel domain
    public DateTime? ProcessedOnUtc { get; set; }  //se è null -> è da processare! se exists allora gia processato.
}