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

    public string DaoId { get; set; }

    public string AcceptedCurrency { get; set; }

    public int ApproveCounts { get; set; }

    public int RejectCounts { get; set; }

    public int AbstainCounts { get; set; }

    public int VotesAmount { get; set; }
}