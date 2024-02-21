using TomorrowDAOServer.Entities;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalDto : ProposalBase
{
    //vote count info
    public int VoterCount { get; set; }
    
    public int VotesAmount { get; set; }
    
    public int ApprovedCount { get; set; }
    
    public int RejectionCount { get; set; }
    
    public int AbstentionCount { get; set; }
}