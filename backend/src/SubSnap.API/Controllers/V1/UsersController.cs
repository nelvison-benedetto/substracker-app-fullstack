using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SubSnap.API.Contracts.Responses;
using SubSnap.API.Validators;
using SubSnap.Core.Contracts.Services;
using SubSnap.Core.DTOs.Application.Commands.Users;
using SubSnap.Core.DTOs.External.Requests.Users;
using SubSnap.Core.DTOs.External.Responses.Users;

namespace SubSnap.API.Controllers.V1;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<RegisterUserCommand> _validator;

    public UsersController( IMapper mapper, IUserService userService, IValidator<RegisterUserCommand> validator)
    {
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResult<UserResponse>), StatusCodes.Status200OK)] //questi servono agli sviluppatori per capire / OpenAPI / Swagger x status code http x this method.
    [ProducesResponseType(typeof(ApiResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResult<UserResponse>>> Register( RegisterUserRequest request )
    {
        // Request -> Command mapping
        var command = _mapper.Map<RegisterUserCommand>(request);
        // Validazione centralizzata
        await ValidatorHelper.ValidateCommandAsync(_validator, command);
        // Application Layer
        var result = await _userService.RegisterAsync(command);
        // Result -> Response
        var response = _mapper.Map<UserResponse>(result);
        return Ok(ApiResult<UserResponse>.Ok(response));
        //qualsiasi cosa tu metta dentro Ok(...) verrà serializzata in JSON come body della risposta HTTP
        //ApiResult<T>.Ok(data)  my custom envelope standard per tutte le risposte
    }
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
