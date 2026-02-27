using AutoMapper;
using SubSnap.API.Contracts.Auth.Responses;
using SubSnap.API.Contracts.Users.Responses;
using SubSnap.Application.UseCases.Auth.Login;
using SubSnap.Application.UseCases.Auth.RefreshToken;
using SubSnap.Application.UseCases.Users.RegisterUser;

namespace SubSnap.API.Mapping;

public sealed class ResultToResponseProfile : Profile  //Profile è classe di AutoMapper che contiene tutte le regole di mapping 
{
    public ResultToResponseProfile()
    {
        //CreateMap<From, To>()
        CreateMap<LoginResult, LoginResponseAuth>();

        CreateMap<RTResult, RefreshTokenResponseAuth>();
        //non necessario fare .ConstructUsing() xk properties matchano stesso nome & no value objects (e.g.no Email Email)

        CreateMap<RUResult, RegisterUserResponse>();

    }
    //CreateMap<UserResult, UserResponse>();
    //e nel controller puoi fare e.g. var response = _mapper.Map<UserResponse>(result); (result è di type UserResult)
    //ora puoi spedire il 'response' pulito nel web

}
