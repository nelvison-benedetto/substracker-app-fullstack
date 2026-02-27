
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SubSnap.Application.Common.Behaviors;
using System.Reflection;

namespace SubSnap.Application.DependencyInjection;

/*
 SEPARATA DA .infrastructure.dependencyinjection.servicecollectionextensions bc .infrastructure NON DEVE CONOSCERE implementazioni in .application!  va bene se conosce le sue ports e.g.IUserRepository.cs perche intanto poi la vera implementazione rimane dentro .infrastructure (e.g.UserRepository.cs). 
 */

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MEDIATR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(assembly));

        // FLUENT VALIDATION
        services.AddValidatorsFromAssembly(assembly);

        // PIPELINE
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}