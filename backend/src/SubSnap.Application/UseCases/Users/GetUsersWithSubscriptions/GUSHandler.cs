using MediatR;
using SubSnap.Application.Ports.DataLoadersorQueries;
using SubSnap.Core.Domain.Aggregates;

namespace SubSnap.Application.UseCases.Users.GetUsersWithSubscriptions;
/*
HTTP
 ↓
Controller
 ↓
GUSOrchestrator   ⭐ entrypoint application
 ↓
IMediator.Send
 ↓
Pipeline Behaviors
 ↓
GUSHandler        ⭐ orchestration tecnica
 ↓
BatchLoader
 ↓
EF Core
 ↓
Database
 */
//CODE VALIDO, DA DECOMMENTARE QUANDO LO VORRAI USARE!!

//public sealed class GUSHandler
//    : IRequestHandler<GUSCommand, List<GUSResult>>
//{
//    private readonly ApplicationDbContext _context;
//    private readonly ISubscriptionBatchLoader _batchLoader;

//    public GUSHandler(
//        ApplicationDbContext context,
//        ISubscriptionBatchLoader batchLoader)
//    {
//        _context = context;
//        _batchLoader = batchLoader;
//    }

//    public async Task<List<GUSResult>> Handle(
//        GUSCommand request,
//        CancellationToken ct)
//    {
//        // 1️⃣ load base entities
//        var users = await _context.Users
//            .AsNoTracking()
//            .ToListAsync(ct);

//        if (users.Count == 0)
//            return [];

//        // 2️⃣ queue batch requests (NESSUNA QUERY ANCORA!)
//        var subscriptionTasks = users
//            .Select(u => _batchLoader.Load(u.Id, ct))
//            .ToList();

//        // 3️⃣ SINGLE SQL QUERY EXECUTED HERE
//        var subscriptionsPerUser =
//            await Task.WhenAll(subscriptionTasks);

//        // 4️⃣ compose aggregates runtime
//        var aggregates = users
//            .Zip(subscriptionsPerUser,
//                (user, subs) =>
//                    new UserSubscriptionsAggregate(user, subs))
//            .ToList();

//        // 5️⃣ map to result
//        return aggregates
//            .Select(a =>
//                new GUSResult(
//                    a.User.Id.Value,
//                    a.User.Email.Value,
//                    a.Subscriptions.Count))
//            .ToList();
//    }

//}