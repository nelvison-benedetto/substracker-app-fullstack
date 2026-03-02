using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SubSnap.API.Validators;

namespace SubSnap.API.StartupExtensions.Validation;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidationConfiguration( this IServiceCollection services )
    {
        // here DISABILITO VALIDAZIONE AUTOMATICA ASP.NET x ModelState!! (xk di default [ApiController] (nel controller) fa if (!ModelState.IsValid){return BadRequest(ModelState);}  quindi non ti da possibilita di customizzazione!!) . ora lo hai solo disattivato, fa invece la fluent validation manualmente in API/Validators/xx.cs
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
        //SCANSIONA TUTTO L'ASSEMBLY(e.g. .API) dove si trova RegisterUserValidator.cs, E REGISTRA TUTTI I validators trovati!!! necessario xk validationbehavior.cs DOVRA INIETTARE I VALIDATORS (e.g.)

        return services;
    }

}