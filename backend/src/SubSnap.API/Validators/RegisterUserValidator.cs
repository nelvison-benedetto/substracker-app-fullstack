using FluentValidation;
using SubSnap.Application.UseCases.Users.RegisterUser;

namespace SubSnap.API.Validators;

//Validator vive solo nell’API, non tocca Domain, Repository o DB!
//perfect che valida il Command
//PLUGIN FLUENTVALIDATION è libreria che ti evita di scrivere a mano e.g. if (string.IsNullOrEmpty(request.Email)){return BadRequest("Email required")};
/*
 HTTP Request → mapped to RUCommand → validator controlla RUCommand, .Application ricevera solo dati gia validati!!

!!!!QUESTO VALIDATOR VIENE CATTURATO DA ValidatorBehvior.cs!!!
 */
public class RegisterUserValidator : AbstractValidator<RUCommand>  //abstractValidator<T> è FluentValidation. DEVI VALIDARE IL COMMAND, NON IL DTO, PERCHE IL COMMAND è L'OGGETTO CHE ARRIVA ALL'APPLICATION LAYER, DEVE ESSERE GIA VALIDATO PRIMA DI ARRIVARCI!
{
    public RegisterUserValidator() 
    {
        RuleFor(x => x.Email)  //rules di FluentValidation
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email must be valid");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");
    }

}