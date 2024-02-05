using AutoMapper;
using TomorrowDAOServer.DAO;
using TomorrowDAOServer.Dtos.DAO;
using TomorrowDAOServer.Entities;
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
        CreateMap<DAOIndex, DAODto>().ReverseMap();
        CreateMap<DAOMetadata, DAOMetadataDto>().ReverseMap();
        CreateMap<GovernanceSchemeThreshold, GovernanceSchemeThresholdDto>().ReverseMap();
        CreateMap<HighCouncilConfig, HighCouncilConfigDto>().ReverseMap();
        CreateMap<FileInfo, FileInfoDto>().ReverseMap();
        CreateMap<PermissionInfo, PermissionInfoDto>().ReverseMap();
    }
}