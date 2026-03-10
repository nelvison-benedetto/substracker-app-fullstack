using SubSnap.API.StartupExtensions.Authentication;
using SubSnap.API.StartupExtensions.Authorization;
using SubSnap.API.StartupExtensions.CorrelationId;
using SubSnap.API.StartupExtensions.Cors;
using SubSnap.API.StartupExtensions.HealthChecks;
using SubSnap.API.StartupExtensions.Swagger;
using SubSnap.API.StartupExtensions.Validation;
using SubSnap.Application.DependencyInjection;
//using Microsoft.OpenApi.Models;
using SubSnap.Infrastructure.DependencyInjection;
using SubSnap.Infrastructure.Persistence.Context;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

//custom startup configs
builder.Services.AddValidationConfiguration();
builder.Services.AddCorsConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddAuthorizationConfiguration();
builder.Services.AddHealthChecksConfiguration();

//validators. OLD now in .application uso plugin MediatR + plugin FluentValidation per validazione automatica, per non dover ogni volta esplicitare nel code. see more validationbehaviour.cs dependencyinjection.cs
//builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);  //here uso plugin Automapper, non plugin AutomapperExtension.ect altrimenti x c# c'è ambiguita x 'AddAutoMapper()'.

//.Application layer (BEFORE .infrastructure layer!)
builder.Services.AddApplication();

//.Infrastructure layer
builder.Services.AddInfrastructure(builder.Configuration);  //servicecollectionextensions.cs
    //qua accade DbContext, repositories concreti, unit of work, connection str

var app = builder.Build();  //crei l'istanza finale dell'app. ora elenchi i middlewares (http chain)

// Middleware pipeline
app.UseCorrelationId();  //1° nella chain!! x ogni richiesta HTTP, se il client non fornisce un header "X-Correlation-ID", ne genera uno nuovo e lo aggiunge alla richiesta. Se invece il client fornisce già un "X-Correlation-ID", lo lascia intatto. In questo modo ogni richiesta ha un identificatore unico che può essere usato per tracciare la richiesta attraverso i log e i sistemi di monitoraggio (e.g.Telemetery..)
app.UseSwaggerConfiguration();
app.UseCorsConfiguration();
app.UseHttpsRedirection();
app.UseAuthentication(); //legge JWT dal header, valida token, popola HttpContext.User
app.UseAuthorization();  //applica [Authorize]
app.UseHealthChecksConfiguration();

app.MapControllers();  //collega routing -> controller

app.MapGet("/health/db", async (ApplicationDbContext db) =>
{
    await db.Database.CanConnectAsync();
    return Results.Ok("DB OK");
});  //non è CleanArchitecture, ma ok mi serve solo x check connessione su server backend<-->db

app.Run();
