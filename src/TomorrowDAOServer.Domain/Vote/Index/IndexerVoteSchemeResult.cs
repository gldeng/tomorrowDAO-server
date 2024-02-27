using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Enums;

namespace TomorrowDAOServer.Vote.Index;

public class IndexerVoteSchemeResult : IndexerCommonResult<IndexerVoteSchemeResult>
{
    public List<IndexerVoteSchemeInfo> DataList { get; set; } = new ();
}

public class IndexerVoteSchemeInfo
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    public long BlockHeight { get; set; }
    public string VoteSchemeId { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public VoteMechanism VoteMechanism { get; set; }
    public bool IsLockToken { get; set; }
    public bool IsQuadratic { get; set; }
    public long TicketCost { get; set; }
    public DateTime CreateTime { get; set; }
}

