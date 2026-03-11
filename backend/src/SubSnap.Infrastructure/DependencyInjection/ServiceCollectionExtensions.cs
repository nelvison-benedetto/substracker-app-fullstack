using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubSnap.Application.Ports.Auth;
using SubSnap.Application.Ports.DataLoadersorQueries;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Application.Ports.Storage;
using SubSnap.Infrastructure.Background;
using SubSnap.Infrastructure.DataLoaders.Aggregates;
using SubSnap.Infrastructure.DataLoaders.Batch;
using SubSnap.Infrastructure.External.Storage;
using SubSnap.Infrastructure.Identity.Services;
using SubSnap.Infrastructure.Persistence.Context;
using SubSnap.Infrastructure.Persistence.UnitOfWork;
using SubSnap.Infrastructure.Repositories.Implementations;

namespace SubSnap.Infrastructure.DependencyInjection;

//see applicationdbcontext.cs  servicecollectionextensions.cs  dependencyinjection.cs

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(  // custom extension x IServiceCollection (la collezione di servizi della DI di ASP.NET Core)
        this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer( configuration.GetConnectionString("sqlConnection")));
        services.AddDbContext<ApplicationDbContext>( options => options.UseNpgsql( configuration.GetConnectionString("sqlConnection"),
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,                   // numero massimo di tentativi
                    maxRetryDelay: TimeSpan.FromSeconds(2), // ritardo massimo tra retry
                    errorCodesToAdd: null                // puoi aggiungere codici specifici se vuoi
                );
            }  //questi aiutano in production, se una query fallisce temporaneamente (e.g.per il db non è ancora pronto dopo il deploy) allora ritenta X volte, senza crashare al 1° tentativo.
            )
        );  //postgreSQL x EF core. SOLO WRITE(sempre usando UnitOfWork)

        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("sqlConnection"),
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null);
            }
            ),
            ServiceLifetime.Scoped);   //SOLO READ(cosi puoi fare QUERY PARALLELE!!)

        services.AddHostedService<OutboxProcessor>(); //va bene dovunque here nella chain, viene avviato auto quando l'host .net parte.
        services.AddHttpContextAccessor();  //cosi subscriptionbatchloader.cs ha accesso al correlationid x logging(mex di debug)

        //registrazione dei repositories!!
        //quando qualcuno chiede IUserRepository, la DI darà un’istanza concreta di UserRepository
        services.AddScoped<IUserRepository, UserRepository>();  //!!!repositories
        services.AddScoped<IUnitOfWork, EFUnitOfWork>();     //!!!unit of work
        services.AddScoped<IPasswordHasherService, AspNetPasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        //services.AddScoped<AuthHandlerOldService>(); //non serve interfaccia x utilizzarlo xk non attravera i BOUNDARIES, è un servizio che rimane interno a .Infrastructure, non lo chiama nessun altro prj.
        //Auth

        //services.AddScoped<ILoginHandler, LoginHandler>();
        //services.AddScoped<ILogoutHandler, LogoutHandler>();
        //services.AddScoped<IRTHandler, RTHandler>();
        //services.AddScoped<IRUHandler, RUHandler>();
        //User
        //services.AddScoped<IUserHandlerOldService, UserHandlerOldService>();     //OLD now all in .application/usecases

        //services.AddScoped< IRUHandler,RUHandler>();
        //non piu necessario IRUhandler perche ora usi RUCommand : IRequest<RUResult> e RUHandler : IRequestHandler<RUCommand, RUResult> usando plugin MediatR (mediatr diventa il dispatcher)

        //services.AddScoped<IEventDispatcher, MediatREventDispatcher>(); //ora uso outboxprocessor x dispatcher outbox pattern.


        //x Loaders & Bathchers, x Projections non necessario xk è interno.
        services.AddScoped<IUserAggregateLoader, UserAggregateLoader>();
        services.AddScoped<ISubscriptionAggregateLoader, SubscriptionAggregateLoader>();
            //addscoped bc 1 http req = 1 batch window
        services.AddScoped<ISubscriptionBatchLoader, SubscriptionBatchLoader>();
        //services.AddScoped<ISharedLinkBatchLoader, SharedLinkBatchLoader>();


        services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = configuration["ObjectStorage:Endpoint"],
                ForcePathStyle = true
            };
            return new AmazonS3Client(
                configuration["ObjectStorage:AccessKey"],
                configuration["ObjectStorage:SecretKey"],
                config);
        });
        services.AddScoped<IObjectStorageService,HetznerObjectStorageService>();
        //see IObjectStorageService.cs HetznerObjectStorageService.cs UserMedia.cs xxxhandler.cs

        return services;
        //scoped: una nuova istanza per ogni richiesta HTTP, condivisa all’interno della stessa richiesta. Perfetto x DbContext e servizi che lavorano con esso.
        //!!!in questo modo quando chiami l'interfaccia, utilizzi invece la classe concrete. ottimo xk rispetti i BOUNDARIES(confini) tra prj, un proj non conosce le implementazioni dell'altro, ma SOLO LE INTERFACCIE. e poi xk ti permette di fare facilmente il mock dei servizi nelle unit test!!
    }

}