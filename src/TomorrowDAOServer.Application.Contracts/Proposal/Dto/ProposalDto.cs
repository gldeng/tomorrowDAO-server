using System;
using TomorrowDAOServer.Entities;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalDto
{
    public string ChainId { get; set; }
    public long BlockHeight { get; set; }
    public string Id { get; set; }
    
    public string DAOId { get; set; }

    public string ProposalId { get; set; }

    public string ProposalTitle { get; set; }
    
    public string ProposalDescription { get; set; }
    
    public string ForumUrl { get; set; }
    
    public string ProposalType { get; set; }
    
    public DateTime ActiveStartTime { get; set; }
   
    public DateTime ActiveEndTime { get; set; }
    
    public DateTime? ExecuteStartTime { get; set; }

    public DateTime? ExecuteEndTime { get; set; }
    
    public string ProposalStatus { get; set; }
    
    public string ProposalStage { get; set; }
    
    public string Proposer { get; set; }
    
    public string SchemeAddress { get; set; }
    
    public ExecuteTransaction Transaction { get; set; }
    
    public string VoteSchemeId { get; set; }
    
    public string VetoProposalId { get; set; }
    
    public DateTime DeployTime { get; set; }

    public DateTime? ExecuteTime { get; set; }   
    
    
    public string GovernanceMechanism { get; set; }
    
    public int MinimalRequiredThreshold { get; set; }
    
    public int MinimalVoteThreshold { get; set; }
    
    //percentage            
    public int MinimalApproveThreshold { get; set; }
    
    //percentage    
    public int MaximalRejectionThreshold { get; set; }
    
    //percentage    
    public int MaximalAbstentionThreshold { get; set; }
    
    public long ActiveTimePeriod { get; set; }
    
    public long VetoActiveTimePeriod { get; set; }
    
    public long PendingTimePeriod { get; set; }
    
    public long ExecuteTimePeriod { get; set; }
    
    public long VetoExecuteTimePeriod { get; set; }

    //vote count info
    public int VoterCount { get; set; }
    
    public int VotesAmount { get; set; }
    
    public int ApprovedCount { get; set; }
    
    public int RejectionCount { get; set; }
    
    public int AbstentionCount { get; set; }
}