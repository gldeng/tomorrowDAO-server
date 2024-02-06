using System.Collections.Generic;
using TomorrowDAOServer.Entities;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalDto : ProposalBase
{
    //TagList
    public List<string> TagList { get; set; }
    //vote count info
    public int VotesAmount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectionCount { get; set; }
    public int AbstentionCount { get; set; }
}