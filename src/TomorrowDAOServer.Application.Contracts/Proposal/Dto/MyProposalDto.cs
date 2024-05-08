using System;
using System.Collections.Generic;

namespace TomorrowDAOServer.Proposal.Dto;

public class MyProposalDto
{
    public string ChainId { get; set; }

    public string Symbol { get; set; }
    
    public long AvailableUnStakeAmount { get; set; }

    public long StakeAmount { get; set; }
    
    public long VotesAmount { get; set; }
    
    public bool CanVote { get; set; }
    public List<string> ProposalIdList { get; set; }
}