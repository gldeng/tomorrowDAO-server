using System;
using TomorrowDAOServer.Common.Dtos;

namespace TomorrowDAOServer.Dtos.Explorer;

public class ProposalListRequest : ExplorerPagerRequest
{
    
    public ProposalListRequest(int pageNum, int pageSize) : base(pageNum, pageSize)
    {
    }
    
    /// <see cref=" TomorrowDAOServer.Common.Enum.ProposalType"/>
    public string ProposalType { get; set; }
    
    /// <see cref=" TomorrowDAOServer.Common.Enum.ProposalStatusEnum"/>
    public string Status { get; set; }

    public int IsContract { get; set; }
    
    public string Address { get; set; }
    
    public string Search { get; set; }
    
}

public class ProposalResponse : ExplorerPagerResult<ProposalResult>
{
    public int BpCount { get; set; }
}

public class ProposalResult
{
    public int Abstentions { get; set; }
    public int Approvals { get; set; }
    public bool CanVote { get; set; }
    public string ContractAddress { get; set; }
    public string ContractMethod { get; set; }
    public DateTime CreateAt { get; set; }
    public string CreateTxId { get; set; }
    public string CreatedBy { get; set; }
    public DateTime ExpiredTime { get; set; }
    public int Id { get; set; }
    public bool IsContractDeployed { get; set; }
    public LeftInfoDto LeftInfo { get; set; }
    public string OrganizationAddress { get; set; }
    public string OrgAddress { get; set; }
    public OrganizationInfoDto OrganizationInfo { get; set; }
    public string ProposalType { get; set; }
    public ReleaseThresholdDto ReleaseThreshold { get; set; }
    public string TxId { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string ProposalId { get; set; }
    public string Proposer { get; set; }
    public int Rejections { get; set; }
    public DateTime ReleasedTime { get; set; }
    public string ReleasedTxId { get; set; }
    public string Status { get; set; }
    public string VotedStatus { get; set; }
    
    public class LeftInfoDto
    {
        public string OrganizationAddress { get; set; }
    }

    public class OrganizationInfoDto
    {
        public DateTime CreatedAt { get; set; }
        public string Creator { get; set; }
        public LeftOrgInfo LeftOrgInfo { get; set; }
        public string OrgAddress { get; set; }
        public string OrgHash { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class LeftOrgInfo
    {
        public bool ProposerAuthorityRequired { get; set; }
        public bool ParliamentMemberProposingAllowed { get; set; }
        public object CreationToken { get; set; }
    }

    public class ReleaseThresholdDto
    {
        public string MinimalApprovalThreshold { get; set; }
        public string MaximalRejectionThreshold { get; set; }
    }
}