
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SubSnap.Application.Common.Behaviors;
using System.Reflection;

namespace SubSnap.Application.DependencyInjection;

/*
 SEPARATA DA .infrastructure.dependencyinjection.servicecollectionextensions bc .infrastructure NON DEVE CONOSCERE implementazioni in .application!  va bene se conosce le sue ports e.g.IUserRepository.cs perche intanto poi la vera implementazione rimane dentro .infrastructure (e.g.UserRepository.cs). 

questo è per .Application level, quindi puoi registrare qui tutti i servizi che riguardano l'application layer, come ad esempio MediatR, FluentValidation e i pipeline behaviors. 
Poi lo aggiungi direttamente nel Program.cs.
 */

public static class DependencyInjection
{
    public static IServiceCollection AddApplication( this IServiceCollection services )
    {
        var assembly = Assembly.GetExecutingAssembly();

        //MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(assembly));

        //FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        //pipeline BEHAVIORS (order is important!) x MediatR!
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
        //AddTransient() xk ogni .Send() nell'hadler, crea nuova istanza di X behavior. quindi NO scoped bc quella creerebbe solo 1 istanza x http req (però poi all'interno magari devi fare multipli Send() !!)

        return services;
    }
}