using System;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Proposal;
namespace TomorrowDAOServer.Entities;

public class ProposalBase : BlockInfoBase
{
    [Keyword] public override string Id { get; set; }

    [Keyword] public string DaoId { get; set; }

    [Keyword] public string ProposalId { get; set; }

    [Keyword] public string ProposalTitle { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ProposalType ProposalType { get; set; }

    //get from GovernanceSchemeId
    [JsonConverter(typeof(StringEnumConverter))]
    public GovernanceMechanism? GovernanceMechanism { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ProposalStatus ProposalStatus { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public DateTime ExpiredTime { get; set; }
    
    [Keyword] public string ReleaseAddress { get; set; }
    
    [Keyword] public string ProposalDescription { get; set; }
    
    public CallTransactionInfo TransactionInfo { get; set; }

    //sub_scheme_id
    [Keyword] public string GovernanceSchemeId { get; set; }

    [Keyword] public string VoteSchemeId { get; set; }

    public bool ExecuteByHighCouncil { get; set; }

    public DateTime DeployTime { get; set; }
    
    public bool VoteFinished  { get; set; } 
    
    //--------Organization info-------
    [Keyword] public string OrganizationAddress { get; set; }

    public int OrganizationMemberCount { get; set; }

    //--------Governance Threshold param-------
    public int MinimalRequiredThreshold { get; set; }
        
    public int MinimalVoteThreshold { get; set; }
    
    //percentage            
    public int MinimalApproveThreshold { get; set; }
    
    //percentage    
    public int MinimalRejectionThreshold { get; set; }
    
    //percentage    
    public int MinimalAbstentionThreshold { get; set; }
    
    //--------Vote Result-------
    [Keyword] public string AcceptedCurrency { get; set; }

    public int ApproveCounts { get; set; }

    public int RejectCounts { get; set; }

    public int AbstainCounts { get; set; }

    public int VotesAmount { get; set; }
    
    public int VoterCount { get; set; }
    
    public void OfProposalStatus()
    {
        if (IsFinalStatus())
        {
            return;
        }

        if (EndTime > DateTime.UtcNow)
        {
            ProposalStatus = ProposalStatus.Active;
        }
        else if (VoteFinished)
        {
            ProposalStatus = VoteFinishedResult();
        }
    }
    
    public bool IsFinalStatus()
    {
        return ProposalStatus is ProposalStatus.Rejected or ProposalStatus.Abstained or ProposalStatus.Expired
            or ProposalStatus.Executed;
    }
    
    public ProposalStatus VoteFinishedResult()
    {
        var targetStatus = ProposalStatus;
    
        // VotesAmount not enough
        if (MinimalVoteThreshold > 0 && VotesAmount < MinimalVoteThreshold)
        {
            return ProposalStatus.Expired;
        } 
    
        if (GovernanceMechanism == Enums.GovernanceMechanism.Customize
            || GovernanceMechanism == Enums.GovernanceMechanism.Referendum) 
        {
            if (MinimalRequiredThreshold > 0 && VoterCount < MinimalRequiredThreshold)
            {
                return ProposalStatus.Expired;
            }
        } 
    
        if (GovernanceMechanism == Enums.GovernanceMechanism.Parliament
            || GovernanceMechanism == Enums.GovernanceMechanism.Association)
        {
            double voterPercentage = Math.Round((double)(VoterCount * 100 / OrganizationMemberCount), 2);
            if (voterPercentage < MinimalRequiredThreshold)
            {
                return ProposalStatus.Expired;
            }
        }
        double rejectionPercentage = Math.Round((double)(RejectCounts * 100 / VotesAmount), 2);
        double abstentionPercentage = Math.Round((double)(AbstainCounts * 100 / VotesAmount), 2);
        double approvalPercentage = Math.Round((double)(ApproveCounts * 100 / VotesAmount), 2);
    
        if (rejectionPercentage >= MinimalRejectionThreshold)
        {
            targetStatus = ProposalStatus.Rejected;
        }
        else if (abstentionPercentage >= MinimalAbstentionThreshold)
        {
            targetStatus = ProposalStatus.Abstained;
        }
        else if (approvalPercentage >= MinimalApproveThreshold)
        {
            targetStatus = ProposalStatus.Approved;
        }
    
        return targetStatus;
    }
}