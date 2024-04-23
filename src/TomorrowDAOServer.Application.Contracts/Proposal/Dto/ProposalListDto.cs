using System;
using System.Collections.Generic;
using TomorrowDAOServer.Options;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalListDto : ProposalDto
{
    public List<string> TagList { get; set; } = new();
    
    public void OfTagList(ProposalTagOptions tagOptions)
    {
        AddTag(tagOptions.MatchTag(Transaction.ContractMethodName));
        AddTag(tagOptions.MatchTag(GovernanceMechanism?.ToString()));
    }

    private void AddTag(string tag)
    {
        if (!tag.IsNullOrWhiteSpace())
        {
            TagList.Add(tag);
        }
    }
}