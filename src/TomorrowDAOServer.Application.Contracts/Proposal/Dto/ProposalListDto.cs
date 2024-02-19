using System.Collections.Generic;
using TomorrowDAOServer.Options;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalListDto : ProposalDto
{
    public List<string> TagList { get; set; }
    
    public void OfTagList(ProposalTagOptions tagOptions)
    {
        TagList = new List<string>
        {
            tagOptions.MatchTag(TransactionInfo.ContractMethodName),
            tagOptions.MatchTag(GovernanceMechanism?.ToString())
        };
    }
}