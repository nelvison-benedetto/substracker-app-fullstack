namespace SubSnap.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<DependencyInjection>()
            .AddClasses(c => c.Where(t => t.Name.EndsWith("Handler")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
