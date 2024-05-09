using System.Collections.Generic;

namespace TomorrowDAOServer.Vote.Index;

public class IndexerVotes
{
    public List<IndexerVote> Data { get; set; } = new ();
}
public class IndexerVote
{
    // The voting activity id.(proposal id/customize)
    public string VotingItemId { get; set; }
    public string Executer { get; set; }

    public string VoteSchemeId { get; set; }

    public string DAOId { get; set; }

    public string AcceptedCurrency { get; set; }

    public int ApprovedCount { get; set; }

    public int RejectionCount { get; set; }

    public int AbstentionCount { get; set; }

    public int VotesAmount { get; set; }
    
    public int VoterCount { get; set; }
}