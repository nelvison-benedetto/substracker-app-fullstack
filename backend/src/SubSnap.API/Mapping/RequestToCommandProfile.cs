using AutoMapper;
using SubSnap.API.Contracts.Auth;
using SubSnap.API.Contracts.Users;
using SubSnap.Application.UseCases.Auth.Login;
using SubSnap.Application.UseCases.Auth.Logout;
using SubSnap.Application.UseCases.Auth.RefreshToken;
using SubSnap.Application.UseCases.Users.RegisterUser;
namespace SubSnap.API.Mapping;

public sealed class RequestToCommandProfile : Profile  //Profile è classe di AutoMapper che contiene tutte le regole di mapping 
{
    public RequestToCommandProfile()
    {
        //CreateMap<From, To>()
        CreateMap<LoginRequestAuth, LoginCommand>()
            .ConstructUsing(src => new LoginCommand(
                new Core.Domain.ValueObjects.Email(src.Email),
                src.Password
                ));
        //devi esplicitare come convertire xk Email di LoginCommand è un value object!

        CreateMap<RefreshTokenRequestAuth, RTCommand>();

    }
    //CreateMap<RegisterUserRequest, RUCommand>();
    //e nel controller puoi fare e.g. var command = _mapper.Map<RegisterUserCommand>(request); (request è di type RegisterUserRequest)
    //ora puoi passare il 'command' pulito nei tuoi services

}
