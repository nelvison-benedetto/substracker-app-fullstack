
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SubSnap.Application.Common.Behaviors;
using SubSnap.Application.UseCases.Auth.Login;
using SubSnap.Application.UseCases.Auth.Login.Loaders;
using SubSnap.Application.UseCases.Users.RegisterUser;
using System.Reflection;

namespace SubSnap.Application.DependencyInjection;

//see applicationdbcontext.cs  servicecollectionextensions.cs  dependencyinjection.cs

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

        
        //.Application layer, carichi(registri) tutti i file che finiscono con '..Loader','..Policy','..Orchestrator', xk altrimenti DI non li conosce. 
        services.Scan(scan =>
            scan.FromAssemblyOf<LoginCommand>()
                .AddClasses(classes => classes.Where(t =>
                    t.Name.EndsWith("Policy") ||
                    t.Name.EndsWith("Loader") ||
                    t.Name.EndsWith("Orchestrator")
                    ))
                .AsSelf()
                .WithScopedLifetime());
        

        //mediatr conosce solo automaticamenete i suoi hanldlers e le sue validazioni, ma se nell'handler metti delle dipendenze esterne (e.g.UserByEmailLoader) allora le devi dichiarare!
        //pero io ho multipli 'UserByEmailLoader' in diversi slices,e se li aggiungo tutti a questo file, c'è confusione con i nomi visto che sono identici! quindi ok here il mio code Scan(...)
        //!cmnq meglio NON usare plugin esterno Scrutor (anche difficile poi risalire i bugs), ma e.g. chiamali 'Login_UserByEmailLoader'  'Logout_UserByEmailLoader' e aggiungili manualmente services.AddScoped<Login_UserByEmailLoader>(); ...(tutti!!)

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