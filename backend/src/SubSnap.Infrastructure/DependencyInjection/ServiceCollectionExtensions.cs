using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.DataLoadersorQueries;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Infrastructure.Background;
using SubSnap.Infrastructure.DataLoaders;
using SubSnap.Infrastructure.Identity.Services;
using SubSnap.Infrastructure.Persistence.Context;
using SubSnap.Infrastructure.Persistence.UnitOfWork;
using SubSnap.Infrastructure.Repositories.Implementations;

namespace SubSnap.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(  // custom extension x IServiceCollection (la collezione di servizi della DI di ASP.NET Core)
        this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer( configuration.GetConnectionString("sqlConnection")));
        services.AddDbContext<ApplicationDbContext>( options => options.UseNpgsql( configuration.GetConnectionString("sqlConnection") )
        );  //postgreSQL x EF core. SOLO WRITE(sempre usando UnitOfWork)

        services.AddDbContextFactory<ApplicationDbContext>(options =>  options.UseNpgsql(configuration.GetConnectionString("sqlConnection")));  //SOLO READ(cosi puoi fare QUERY PARALLELE!!)

        services.AddHostedService<OutboxProcessor>(); //va bene dovunque here nella chain, viene avviato auto quando l'host .net parte.

        //registrazione dei repositories!!
        //quando qualcuno chiede IUserRepository, la DI darà un’istanza concreta di UserRepository
        services.AddScoped<IUserRepository, UserRepository>();  //!!!repositories
        services.AddScoped<IUnitOfWork, EFUnitOfWork>();     //!!!unit of work
        services.AddScoped<IPasswordHasherService, AspNetPasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserAggregateLoader, UserAggregateLoader>();

        //services.AddScoped<AuthHandlerOldService>(); //non serve interfaccia x utilizzarlo xk non attravera i BOUNDARIES, è un servizio che rimane interno a .Infrastructure, non lo chiama nessun altro prj.
        //Auth

        //services.AddScoped<ILoginHandler, LoginHandler>();
        //services.AddScoped<ILogoutHandler, LogoutHandler>();
        //services.AddScoped<IRTHandler, RTHandler>();
        //services.AddScoped<IRUHandler, RUHandler>();
        //User
        //services.AddScoped<IUserHandlerOldService, UserHandlerOldService>();     //OLD now all in .application.usecases
        //services.AddScoped< IRUHandler,RUHandler>();

        //!!!NON PIU NECESSARI GLI Ixxx degli handlers bc ora PLUGIN MEDIATR w 'services.AddMediatR(...)' (.application.dependencyinjection.dependencyinjection.cs) FA AUTO SCAN ASSEMBLY -> trova Ixxxhandler ->  registra xxxhandler.!!!  uso PLUGIN MediatR

        services.AddScoped<ISubscriptionBatchLoader, SubscriptionBatchLoader>(); //addscoped bc 1 http req = 1 batch window

        return services;
        //scoped: una nuova istanza per ogni richiesta HTTP, condivisa all’interno della stessa richiesta. Perfetto x DbContext e servizi che lavorano con esso.
        //!!!in questo modo quando chiami l'interfaccia, utilizzi invece la classe concrete. ottimo xk rispetti i BOUNDARIES(confini) tra prj, un proj non conosce le implementazioni dell'altro, ma SOLO LE INTERFACCIE. e poi xk ti permette di fare facilmente il mock dei servizi nelle unit test!!
    }

}