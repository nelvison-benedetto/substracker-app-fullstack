using AutoMapper;

namespace SubSnap.API.Mapping;

public sealed class ResultToResponseProfile : Profile  //Profile è classe di AutoMapper che contiene tutte le regole di mapping 
{
    public ResultToResponseProfile()
    {
        //CreateMap<From, To>()
        CreateMap<UserResult, UserResponse>();
    }
    //e nel controller puoi fare e.g. var response = _mapper.Map<UserResponse>(result); (result è di type UserResult)
    //ora puoi spedire il 'response' pulito nel web

}
