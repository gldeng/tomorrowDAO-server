using System.Collections.Generic;
using TomorrowDAOServer.Common;

namespace TomorrowDAOServer.Vote.Index;

public class IndexerVotes : IndexerCommonResult<IndexerVotes>
{
    public List<IndexerVote> DataList { get; set; } = new ();
}
public class IndexerVote : IndexerCommonResult<IndexerVote>
{
    // The voting activity id.(proposal id/customize)
    public string VotingItemId { get; set; }

    public string VoteSchemeId { get; set; }

    public string DAOId { get; set; }

    public string AcceptedCurrency { get; set; }

    public int ApproveCount { get; set; }

    public int RejectCount { get; set; }

    public int AbstainCount { get; set; }

    public int VotesAmount { get; set; }
    
    public int VoterCount { get; set; }
}