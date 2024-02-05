using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.Dtos.NetworkDao;

public class ProposalListRequest : PagedResultRequestDto
{
    public ProposalListRequest()
    {
    }
    
    public ProposalListRequest(int skipCount, int maxResultCount)
    {
        base.SkipCount = skipCount;
        base.MaxResultCount = maxResultCount;
    }

    public string ChainId { get; set; }
    public string GovernanceType { get; set; }
    public string ProposalStatus { get; set; }
    public string Content { get; set; }
}


public class ProposalListResponse
{
    
    public string ChainId { get; set; }
    public string ProposalId { get; set; }
    public string Proposer { get; set; }
    public string DeployTime { get; set; }
    public string ProposalTitle { get; set; }
    public string GovernanceType { get; set; }
    public string ProposalStatus { get; set; }
    public string ProposalDescription { get; set; }
    public string ProposalType { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string ExpiredTime { get; set; }
    public TransactionDto Transaction { get; set; }
    public string TotalVoteCount { get; set; }
    public string ApprovedCount { get; set; }
    public string RejectionCount { get; set; }
    public string AbstentionCount { get; set; }
    public int MinimalRequiredThreshold { get; set; }
    public int MinimalVoteThreshold { get; set; }
    public int MinimalApproveThreshold { get; set; }
    public int MinimalRejectionThreshold { get; set; }
    public int MinimalAbstentionThreshold { get; set; }
    public List<string> TagList { get; set; }
    
    public class TransactionDto
    {
        public string ContractMethodName { get; set; }
        public string ToAddress { get; set; }
    }
    
}