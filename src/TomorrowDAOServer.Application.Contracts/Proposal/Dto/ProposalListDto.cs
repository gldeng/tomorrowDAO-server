using System.Collections.Generic;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalListDto : ProposalDto
{
    //TagList
    public List<string> TagList { get; set; }
}