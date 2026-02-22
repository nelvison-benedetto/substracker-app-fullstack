using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubSnap.Core.Abstractions.Identity;
using SubSnap.Core.Contracts.Repositories;
using SubSnap.Core.Contracts.Services;
using SubSnap.Core.Contracts.UnitOfWork;
using SubSnap.Core.Services.Application;
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

        services.AddDbContextFactory<ApplicationDbContext>(options =>  options.UseNpgsql(configuration.GetConnectionString("sqlConnection")));  //SOLO READ(cosi puoi fare query parallele!!)

        //registrazione dei repositories!!
        //quando qualcuno chiede IUserRepository, la DI darà un’istanza concreta di UserRepository
        services.AddScoped<IUserRepository, UserRepository>();  //!!!repositories
        services.AddScoped<IUnitOfWork, EFUnitOfWork>();     //!!!unit of work
        services.AddScoped<IUserService, UserService>();     //!!!application services
        services.AddScoped<IPasswordHasherService, AspNetPasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<AuthService>(); //non serve interfaccia x utilizzarlo xk non attravera i BOUNDARIES, è un servizio che rimane interno a .Infrastructure, non lo chiama nessun altro prj.

        return services;
        //scoped: una nuova istanza per ogni richiesta HTTP, condivisa all’interno della stessa richiesta. Perfetto x DbContext e servizi che lavorano con esso.
        //!!!in questo modo quando chiami l'interfaccia, utilizzi invece la classe concrete. ottimo xk rispetti i BOUNDARIES(confini) tra prj, un proj non conosce le implementazioni dell'altro, ma SOLO LE INTERFACCIE. e poi xk ti permette di fare facilmente il mock dei servizi nelle unit test!!
    }

} 