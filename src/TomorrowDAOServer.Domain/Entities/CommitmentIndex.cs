using System;
using AElf.Indexing.Elasticsearch;
using Nest;

namespace TomorrowDAOServer.Entities;

public class CommitmentIndex : AbstractEntity<string>, IIndexBuild
{
    [Keyword] public string TransactionId { get; set; }
    public long BlockHeight { get; set; }
    [Keyword] public string DaoId { get; set; }
    [Keyword] public string ProposalId { get; set; }
    [Keyword] public string Voter { get; set; }
    public string Commitment { get; set; }
    public long LeafIndex { get; set; }
    public DateTime Timestamp { get; set; }
}