using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubSnap.API.Contracts.Responses;
using SubSnap.API.Contracts.Users.Requests;
using SubSnap.API.Contracts.Users.Responses;
using SubSnap.Application.UseCases.Users.DeleteUser;
using SubSnap.Application.UseCases.Users.RegisterUser;

namespace SubSnap.API.Controllers.V1;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IMapper _mapper;
    //private readonly IValidator<RUCommand> _validator; non serve, intanto il flow della pipeline passa sempre anche da validationbehavior.cs
    //private readonly IRUHandler _ruHandler;

    //private readonly IMediator _mediator;
    private readonly RUOrchestrator _ruOrchestrator;
    private readonly DeleteUserOrchestrator _deleteUserOrchestrator;
    public UsersController( 
        IMapper mapper, 
        //IValidator<RUCommand> validator,
        //IRUHandler ruHandler
        RUOrchestrator ruOrchestrator,
        DeleteUserOrchestrator deleteUserOrchestrator
    )
    {
        _mapper = mapper;
        //_mediator = mediator;
        _ruOrchestrator = ruOrchestrator;
        _deleteUserOrchestrator = deleteUserOrchestrator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResult<RegisterUserResponse>), StatusCodes.Status200OK)] //questi servono agli sviluppatori per capire / OpenAPI / Swagger x status code http x this method. SE INVECE USI IL RETURN Task<IActionResult> funziona sempre ma è meno leggibile da .net e da SwaggerApi
    [ProducesResponseType(typeof(ApiResult<object>), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiResult<object>), StatusCodes.Status500InternalServerError)] questo intanto è globale lo da auto via middleware
    public async Task<ActionResult<ApiResult<RegisterUserResponse>>> RegisterUserAsync([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<RUCommand>(request);
        //await ValidatorHelper.ValidateCommandAsync(_validator, command); E' POCO CLEAN (il controller non dovrebbe conoscere la validation) ORA INVECE USO VALIDAZIONE AUTOMATICA w
        //PLUGIN MediatR (x validazione automatica, per non dover ogni volta esplicitare nel code) + plugin fluentvalidation. see more validationbehaviour.cs dependencyinjection.cs
        //var result = await _ruHandler.HandleAsync(command, ct);  OLD w IRUHandler
        //var result = await _mediator.Send(command, ct);  //x plugin MediatR
        var result = await _ruOrchestrator.Execute(command, ct);  //Controller → Orchestrator → MediatR pipelibe behviors → Handler

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

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteUserAsync([FromBody] DeleteUserRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<DeleteUserCommand>(request);
        await _deleteUserOrchestrator.Execute(command,ct);
        return NoContent();
    }


}
