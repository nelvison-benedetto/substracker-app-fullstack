using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SubSnap.API.StartupExtensions.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");  //prende la section
        var issuer = jwtSettings["Issuer"]!;  //chi ha emesso il token
        var audience = jwtSettings["Audience"]!;  //a chi è destinato
        var secretKey = jwtSettings["SecretKey"]!;  //firmare e verificare il token

        //e poi la connection str la prendi in .Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs

        services
            .AddAuthentication(options =>  //"Questo progetto usa JWT Bearer Authentication"
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    //quando aspnet deve auntenticare una richiesta -> usa JWT
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    //quando serve risposndere w 401 unauthorized -> usa JWT
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters //definisci le regole di validazione dei token
                {
                    ValidateIssuer = true,  //activate (il token deve essere emesso da chi dici tu)
                    ValidateAudience = true,  //activate
                    ValidateLifetime = true,  //non deve essere un token scaduto
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer, //jwtSettings["Issuer"], //devono combaciare perfettamente
                    ValidAudience = audience, //jwtSettings["Audience"], //devono combaciare perfettamente
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),
                    //key simmetrica, deve essere la stessa usata quando generi il token
                    ClockSkew = TimeSpan.Zero, //xk di default asp.net ha margine di tolleranza 5 min, ma noi vogliamo che anche 1 sec dopo la scadenza token sia scaduto!
                    ValidateTokenReplay = false //!! future-ready,  xk ORA NON HO ANCORA blacklist access token
                };
            });
        return services;
    }
}
