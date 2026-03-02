using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using SubSnap.API.Contracts.Errors;
using SubSnap.API.Contracts.Responses;
using SubSnap.Core.Domain.Exceptions;

namespace SubSnap.API.Middleware.ExceptionHandling;

//Cattura tutte le eccezioni NON GESTITE NELLA PIPELINE, mappa eccezioni di dominio o validation → 400, mappa altre eccezioni → 500. Restituisce sempre ApiResult.Fail al client.

//lo chiami w  app.UseGlobalExceptionHandler(); in program.cs o un extension starter.

//Se il service lancia EmailAlreadyRegisteredException, il middleware (ExceptionMiddlewareExtensions.cs) lo cattura come DomainException!! e restituisce 400 con il messaggio che hai definito in EmailAlreadyRegisteredException
//analogamente se il service lancia UserNotFoundException (lo lanci tu), il client riceverà:
/*
{
  "success": false,
  "data": null,
  "error": {
    "statusCode": 404,
    "message": "User with id 42 was not found."
  }
}
 */
public static class ExceptionMiddlewareExtensions
{
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                var exception = context.Features
                    .Get<IExceptionHandlerFeature>()?.Error; //.NET TI PASSA L'EXCEPTION NON GESTITA (CHE PROVIENE DA ExceptionBehvior.cs)
                if (exception is null) return;
                var (statusCode, message) = exception switch
                {
                    NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                    DomainException => (  //my custom!!
                        StatusCodes.Status400BadRequest,
                        exception.Message
                    ),
                    ValidationException => ( StatusCodes.Status400BadRequest, exception.Message ),
                        _ => ( StatusCodes.Status500InternalServerError, "Unexpected server error"),
                };
                context.Response.StatusCode = statusCode;
                var error = new ApiError(statusCode, message);
                var result = ApiResult<object>.Fail(error);
                await context.Response.WriteAsJsonAsync(result);
            });
        });
    }
}
