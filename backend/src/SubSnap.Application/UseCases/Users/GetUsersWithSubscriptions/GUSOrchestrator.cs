using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Application.UseCases.Users.GetUsersWithSubscriptions;

/*
 Controller
   ↓
MediatR pipelin (behavior, transaction, ect...)
   ↓
GUSHandler   ← 🧠 orchestration
   ↓
BatchLoader  ← ⚡ performance
   ↓
EF Core
   ↓
DB
 */

public sealed class GUSOrchestrator
{
    private readonly IMediator _mediator;

    public GUSOrchestrator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<List<GUSResult>> Execute(
        CancellationToken ct = default)
    {
        return _mediator.Send(new GUSCommand(), ct);
    }
}
