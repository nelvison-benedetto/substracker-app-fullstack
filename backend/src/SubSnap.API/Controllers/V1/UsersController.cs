using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SubSnap.API.Contracts.Responses;
using SubSnap.API.Contracts.Users.Requests;
using SubSnap.API.Contracts.Users.Responses;
using SubSnap.API.Validators;
using SubSnap.Application.Ports.Users;
using SubSnap.Application.UseCases.Users.RegisterUser;

namespace SubSnap.API.Controllers.V1;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IValidator<RUCommand> _validator;
    private readonly IRUHandler _ruHandler;

    public UsersController( 
        IMapper mapper, 
        IValidator<RUCommand> validator,
        IRUHandler ruHandler
    )
    {
        _mapper = mapper;
        _validator = validator;
        _ruHandler = ruHandler;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResult<RegisterUserResponse>), StatusCodes.Status200OK)] //questi servono agli sviluppatori per capire / OpenAPI / Swagger x status code http x this method. SE INVECE USI IL RETURN Task<IActionResult> funziona sempre ma è meno leggibile da .net e da SwaggerApi
    [ProducesResponseType(typeof(ApiResult<object>), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiResult<object>), StatusCodes.Status500InternalServerError)] questo intanto è globale lo da auto via middleware
    public async Task<ActionResult<ApiResult<RegisterUserResponse>>> RegisterUserAsync(RegisterUserRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<RUCommand>(request);
        //await ValidatorHelper.ValidateCommandAsync(_validator, command); E' POCO CLEAN, il controller non dovrebbe conoscere la validation. QUINDI USO INVECE
        //PLUGIN MediatR (x validazione automatica, per non dover ogni volta esplicitare nel code)
        var result = await _ruHandler.HandleAsync(command, ct);
        var response = _mapper.Map<RegisterUserResponse>(result);  //see .api/mapping/resulttoresponseprofile.cs
        return Ok(ApiResult<RegisterUserResponse>.Ok(response));
        //qualsiasi cosa tu metta dentro Ok(...) verrà serializzata in JSON come body della risposta HTTP
    }

    //[HttpPost("register")]
    //[ProducesResponseType(typeof(ApiResult<UserResponse>), StatusCodes.Status200OK)] //questi servono agli sviluppatori per capire / OpenAPI / Swagger x status code http x this method.
    //[ProducesResponseType(typeof(ApiResult<object>), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiResult<object>), StatusCodes.Status500InternalServerError)]
    //public async Task<ActionResult<ApiResult<UserResponse>>> Register( RegisterUserRequest request , CancellationToken ct)
    //{
    //    // Request -> Command mapping
    //    var command = _mapper.Map<RUCommand>(request);
    //    // Validazione centralizzata
    //    await ValidatorHelper.ValidateCommandAsync(_validator, command);
    //    // Application Layer
    //    var result = await _userService.RegisterAsync(command, ct);
    //    // Result -> Response
    //    var response = _mapper.Map<UserResponse>(result);  //see .api/mapping/resulttoresponseprofile.cs
    //    return Ok(ApiResult<UserResponse>.Ok(response));
    //    //qualsiasi cosa tu metta dentro Ok(...) verrà serializzata in JSON come body della risposta HTTP
    //    //ApiResult<T>.Ok(data)  my custom envelope standard per tutte le risposte
    //}
    /*
     Client -> HTTP Request -> DTO esterno (RegisterUserRequest)
      -> AutoMapper -> Command applicativo (RegisterUserCommand)
      -> Middleware valida Command (FluentValidation)
          -> Se fallisce: ApiResult.Fail 400
      -> Controller chiama Service
          -> Service orchestration
              -> Domain logic, Repositories
          -> Restituisce Result (UserResult)
      -> AutoMapper -> Response DTO (UserResponse)
      -> Envelope ApiResult.Ok
      -> JSON Response
     */

}
