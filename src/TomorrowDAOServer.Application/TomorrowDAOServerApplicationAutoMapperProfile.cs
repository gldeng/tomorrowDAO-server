using AutoMapper;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Proposal.Index;
using TomorrowDAOServer.Token;
using TomorrowDAOServer.Token.Index;
using TomorrowDAOServer.User.Dtos;

namespace TomorrowDAOServer;

public class TomorrowDAOServerApplicationAutoMapperProfile : Profile
{
    public TomorrowDAOServerApplicationAutoMapperProfile()
    {
        CreateMap<UserIndex, UserDto>().ReverseMap();
        CreateMap<AddressInfo, UserAddressInfo>().ReverseMap();
        CreateMap<IndexerUserToken, UserTokenDto>();
        CreateMap<TokenGrainDto, TokenBasicInfo>()
            .ForMember(des => des.Name, opt
                => opt.MapFrom(source => source.TokenName));
        CreateMap<IndexerProposal, ProposalIndex>();
    }
}