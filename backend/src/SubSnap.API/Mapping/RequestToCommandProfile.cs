using AutoMapper;
using SubSnap.API.Contracts.Users;
using SubSnap.Application.UseCases.Users.RegisterUser;
namespace SubSnap.API.Mapping;

public sealed class RequestToCommandProfile : Profile  //Profile è classe di AutoMapper che contiene tutte le regole di mapping 
{
    public RequestToCommandProfile()
    {
        //CreateMap<From, To>()
        CreateMap<RegisterUserRequest, RUCommand>();
    }
    //e nel controller puoi fare e.g. var command = _mapper.Map<RegisterUserCommand>(request); (request è di type RegisterUserRequest)
    //ora puoi passare il 'command' pulito nei tuoi services

}
