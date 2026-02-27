//using FluentValidation;
//using FluentValidation.Results;

//namespace SubSnap.API.Validators;

//public static class ValidatorHelper
//{
//    //Serve da wrapper generico per validare qualsiasi Command o DTO, valida usando plugin FluentValidation. se ci sono errori lancia ValidationException
//    //!!lo chiami da dove vuoi nel controller w e.g.
//    //await ValidatorHelper.ValidateCommandAsync(new RegisterUserValidator(), command);
//    //gestione di fluentvalidation centralizzata cool!!
//    public static async Task ValidateCommandAsync<T>(IValidator<T> validator, T command)
//    {
//        ValidationResult result = await validator.ValidateAsync(command);
//        if (!result.IsValid)
//        {
//            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();
//            throw new FluentValidation.ValidationException(result.Errors); //questo verra poi catturato da tuo custom in ExceptionMiddlewareExtensions.cs e formattato in una ApiError correttamenet da restituire al client
//        }
//    }
//    public static void ValidateCommand<T>(IValidator<T> validator, T command) //IValidator<T> è il validator da usare e.g.RegisterUserValidator, T command l'obj da validare!
//    {
//        ValidationResult result = validator.Validate(command);
//        if (!result.IsValid)
//        {
//            //Puoi formattare gli errori come vuoi
//            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();
//            //Lancia una ValidationException FluentValidation oppure usa la tua ApiError
//            throw new FluentValidation.ValidationException(result.Errors);
//        }
//    }
//}

//ORA INVECE USO PLUGIN MediatR (x validazione automatica, per non dover ogni volta esplicitare nel code) + plugin fluentvalidation. see more validationbehaviour.cs dependencyinjection.cs