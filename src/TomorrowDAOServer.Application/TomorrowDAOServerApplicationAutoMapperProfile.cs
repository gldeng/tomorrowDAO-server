using AutoMapper;
using MongoDB.Bson;
using TomorrowDAOServer.DAO;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.DAO.Dtos;
using TomorrowDAOServer.DAO.Indexer;
using TomorrowDAOServer.Dtos.Explorer;
using TomorrowDAOServer.Dtos.NetworkDao;
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
                => opt.MapFrom(source => source.TokenName))
            .ReverseMap();
        
        CreateMap<DAOIndex, DAOInfoDto>().ReverseMap();
        CreateMap<IndexerDAOInfo, DAOIndex>().ReverseMap();
        CreateMap<Metadata, MetadataDto>().ReverseMap();
        CreateMap<IndexerMetadata, Metadata>().ReverseMap();
        
        CreateMap<GovernanceSchemeThreshold, GovernanceSchemeThresholdDto>().ReverseMap();
        CreateMap<HighCouncilConfig, HighCouncilConfigDto>().ReverseMap();
        CreateMap<IndexerHighCouncilConfig, HighCouncilConfig>().ReverseMap();
        CreateMap<FileInfo, FileInfoDto>().ReverseMap();
        CreateMap<PermissionInfo, PermissionInfoDto>().ReverseMap();

        CreateMap<ExplorerProposalResult, ProposalListResponse>()
            .ForMember(des => des.DeployTime, opt => opt.MapFrom(src => src.ReleasedTime.DefaultIfEmpty(src.ReleasedTime).ToUtcMilliSeconds()))
            .ForMember(des => des.GovernanceType, opt => opt.MapFrom(src => src.ProposalType))
            .ForMember(des => des.ProposalType, opt => opt.MapFrom(src => src.ProposalType))
            .ForMember(des => des.ProposalStatus, opt => opt.MapFrom(src => src.Status))
            .ForMember(des => des.StartTime, opt => opt.MapFrom(src => src.CreateAt.ToUtcMilliSeconds()))
            .ForMember(des => des.ExpiredTime, opt => opt.MapFrom(src => src.ExpiredTime.ToUtcMilliSeconds()))
            .ForMember(des => des.EndTime, opt => opt.MapFrom(src => src.ReleasedTime.DefaultIfEmpty(src.ExpiredTime).ToUtcMilliSeconds()))
            .ForMember(des => des.ApprovedCount, opt => opt.MapFrom(src => src.Approvals))
            .ForMember(des => des.RejectionCount, opt => opt.MapFrom(src => src.Rejections))
            .ForMember(des => des.AbstentionCount, opt => opt.MapFrom(src => src.Abstentions))
            .ForMember(des => des.TotalVoteCount, opt => opt.MapFrom(src => src.Approvals + src.Rejections + src.Abstentions))
            .ForMember(des => des.MinimalRequiredThreshold, opt => opt.MapFrom(src => src.OrganizationInfo.ReleaseThreshold.MinimalApprovalThreshold))
            .ForMember(des => des.MinimalApproveThreshold, opt => opt.MapFrom(src => src.OrganizationInfo.ReleaseThreshold.MinimalApprovalThreshold))
            .ForMember(des => des.MinimalVoteThreshold, opt => opt.MapFrom(src => src.OrganizationInfo.ReleaseThreshold.MinimalVoteThreshold))
            .ForMember(des => des.MaximalRejectionThreshold, opt => opt.MapFrom(src => src.OrganizationInfo.ReleaseThreshold.MaximalRejectionThreshold))
            .ForMember(des => des.MaximalAbstentionThreshold, opt => opt.MapFrom(src => src.OrganizationInfo.ReleaseThreshold.MaximalAbstentionThreshold))
            .ForMember(des => des.Transaction, opt => opt.MapFrom(src => new ProposalListResponse.TransactionDto
            {
                ContractMethodName = src.ContractMethod,
                ToAddress = src.ContractAddress
            }))
            .ReverseMap();
        
        CreateMap<ExplorerTransactionResponse, TreasuryTransactionDto>()
            .ForMember(des => des.TransactionHash, opt => opt.MapFrom(src => src.TxId))
            .ForMember(des => des.MethodName, opt => opt.MapFrom(src => src.Method))
            .ReverseMap();

        CreateMap<ExplorerTransferResult, TreasuryTransactionDto>()
            .ForMember(des => des.TransactionHash, opt => opt.MapFrom(src => src.TxId))
            .ForMember(des => des.MethodName, opt => opt.MapFrom(src => src.Method))
            .ForMember(des => des.From, opt => opt.MapFrom(src => src.AddressFrom))
            .ForMember(des => des.To, opt => opt.MapFrom(src => src.AddressTo))
            .ForMember(des => des.TransactionTime, opt => opt.MapFrom(src => src.Time))
            .ReverseMap();
    }
}