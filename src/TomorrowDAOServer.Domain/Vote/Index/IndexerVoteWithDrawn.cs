using System;
using System.Collections.Generic;
using TomorrowDAOServer.Common;

namespace TomorrowDAOServer.Vote.Index;

public class IndexerVoteWithdrawnStakes : IndexerCommonResult<IndexerVoteWithdrawnStakes>
{
    public List<IndexerVoteWithdrawnStake> DataList { get; set; } = new ();
}

public class IndexerVoteWithdrawnStake : IndexerCommonResult<IndexerVoteWithdrawnStake>
{
    public string Id { get; set; }
    public string DaoId { get; set; }
    public string Voter { get; set; }
    public long WithdrawAmount { get; set; }
    public DateTime WithdrawTimestamp { get; set; }
    public List<string> VotingItemIdList { get; set; }
    public DateTime CreateTime { get; set; }
}