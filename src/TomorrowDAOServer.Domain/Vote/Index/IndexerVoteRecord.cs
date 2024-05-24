using System;
using System.Collections.Generic;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Enums;

namespace TomorrowDAOServer.Vote.Index;

public class IndexerVoteRecords
{
    public List<IndexerVoteRecord> DataList { get; set; } = new ();
}
public class IndexerVoteRecord
{
    public string Voter { get; set; }
    public string TransactionId { get; set; }
    public VoteMechanism VoteMechanism { get; set; }
    public int Amount { get; set; }
    public string VotingItemId {get; set; }

    public VoteOption Option { get; set; }
    public DateTime VoteTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}