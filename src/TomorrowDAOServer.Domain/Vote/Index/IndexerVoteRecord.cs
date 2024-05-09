using System;
using System.Collections.Generic;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Enums;

namespace TomorrowDAOServer.Vote.Index;

public class IndexerVoteRecords : IndexerCommonResult<IndexerVoteRecords>
{
    public List<IndexerVoteRecord> DataList { get; set; } = new ();
}
public class IndexerVoteRecord : IndexerCommonResult<IndexerVoteRecord>
{
    public string Voter { get; set; }
    
    public int Amount { get; set; }

    public VoteOption Option { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}