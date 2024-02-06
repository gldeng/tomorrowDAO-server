using System.Collections.Generic;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Entities;

namespace TomorrowDAOServer.Proposal.Index;

public class IndexerProposalSync : IndexerCommonResult<IndexerProposalSync>
{
    public long TotalRecordCount { get; set; }
    public List<IndexerProposal> DataList { get; set; }
}

public class IndexerProposal : ProposalBase
{ 
    
}