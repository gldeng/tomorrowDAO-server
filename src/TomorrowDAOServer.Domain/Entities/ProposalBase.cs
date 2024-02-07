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

    [PropertyName("DAOId")]
    [Keyword] public string DAOId { get; set; }

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

    [Keyword] public string ExecuteAddress { get; set; }

    [Keyword] public string ProposalDescription { get; set; }

    public CallTransactionInfo TransactionInfo { get; set; }

    //sub_scheme_id
    [Keyword] public string GovernanceSchemeId { get; set; }

    [Keyword] public string VoteSchemeId { get; set; }

    public bool ExecuteByHighCouncil { get; set; }

    public DateTime DeployTime { get; set; }

    public DateTime? ExecuteTime { get; set; }

    public bool VoteFinished { get; set; }

    [Keyword] public string OrganizationAddress { get; set; }

    //--------Governance Threshold param-------

    public int MinimalRequiredThreshold { get; set; }

    public int MinimalVoteThreshold { get; set; }

    //percentage            
    public int MinimalApproveThreshold { get; set; }

    //percentage    
    public int MaximalRejectionThreshold { get; set; }
    
    //percentage    
    public int MaximalAbstentionThreshold { get; set; }
    
    public bool IsFinalStatus()
    {
        return ProposalStatus is ProposalStatus.Rejected or ProposalStatus.Abstained or ProposalStatus.Expired
            or ProposalStatus.Executed;
    }
}