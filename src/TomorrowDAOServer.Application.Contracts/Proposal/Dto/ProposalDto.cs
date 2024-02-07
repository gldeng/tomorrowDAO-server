using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Entities;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalDto : ProposalBase
{
    //TagList
    public List<string> TagList { get; set; }
    
    //vote count info
    public int VoterCount { get; set; }
    
    public int VotesAmount { get; set; }
    
    public int ApprovedCount { get; set; }
    
    public int RejectionCount { get; set; }
    
    public int AbstentionCount { get; set; }
}