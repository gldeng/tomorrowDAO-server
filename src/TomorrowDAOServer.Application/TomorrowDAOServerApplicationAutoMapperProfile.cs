using System;
using AutoMapper;
using TomorrowDAOServer.DAO;
using TomorrowDAOServer.Dtos.DAO;
using TomorrowDAOServer.Common;
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
                => opt.MapFrom(source => source.TokenName));
        CreateMap<DAOIndex, DAODto>().ReverseMap();
        CreateMap<DAOMetadata, DAOMetadataDto>().ReverseMap();
        CreateMap<GovernanceSchemeThreshold, GovernanceSchemeThresholdDto>().ReverseMap();
        CreateMap<HighCouncilConfig, HighCouncilConfigDto>().ReverseMap();
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
            .ForMember(des => des.Transaction, opt => opt.MapFrom(src => new ProposalListResponse.TransactionDto
            {
                ContractMethodName = src.ContractMethod,
                ToAddress = src.ContractAddress
            }))
            .ForMember(des => des.MinimalRequiredThreshold, opt => opt.MapFrom(src => src.OrganizationInfo.ReleaseThreshold.MinimalApprovalThreshold))
            .ForMember(des => des.MinimalApproveThreshold, opt => opt.MapFrom(src => src.OrganizationInfo.ReleaseThreshold.MinimalApprovalThreshold))
            .ForMember(des => des.MinimalVoteThreshold, opt => opt.MapFrom(src => src.OrganizationInfo.ReleaseThreshold.MinimalVoteThreshold))
            .ForMember(des => des.MaximalRejectionThreshold, opt => opt.MapFrom(src => src.OrganizationInfo.ReleaseThreshold.MaximalRejectionThreshold))
            .ForMember(des => des.MaximalAbstentionThreshold, opt => opt.MapFrom(src => src.OrganizationInfo.ReleaseThreshold.MaximalAbstentionThreshold))
            ;
    }
}