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

        //FluentValidation - Registra tutti i validator dell'assembly API!! necessario xk poi in validationbehavior.cs dovrai iniettare i validators(e.g.registeruservalidator.cs)
        services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();

        return services;
    }
    //se also API/Validators/ValidatorHelper.cs, e nel controller usi  'await ValidatorHelper.ValidateCommandAsync(_validator, command);'
}