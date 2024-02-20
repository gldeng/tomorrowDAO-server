using System;

namespace TomorrowDAOServer.Proposal.Dto;

public class MyProposalDto
{
    public string ChainId { get; set; }

    public string Symbol { get; set; }
    
    public long AvailableUnStakeAmount { get; set; }

    public long StakeAmount { get; set; }
    
    public long VotesAmount { get; set; }
    
    public bool CanVote { get; set; }
    
    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public DateTime ExpiredTime { get; set; }
    
    public DateTime? ExecuteTime { get; set; }
}