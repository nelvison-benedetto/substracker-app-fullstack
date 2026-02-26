using FluentValidation;
using SubSnap.Application.UseCases.Users.RegisterUser;

namespace SubSnap.API.Validators;

//Validator vive solo nell’API, non tocca Domain, Repository o DB!
public class RegisterUserValidator : AbstractValidator<RUCommand>  //abstractValidator<T> è FluentValidation.
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