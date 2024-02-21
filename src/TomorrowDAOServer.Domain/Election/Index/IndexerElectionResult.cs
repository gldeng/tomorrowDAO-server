using System.Collections.Generic;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Enums;

namespace TomorrowDAOServer.Election.Index;


public class IndexerElectionResult : IndexerCommonResult<IndexerElectionResult>
{
    public long TotalCount { get; set; }
    public List<IndexerElection> DataList { get; set; } = new ();
}

public class IndexerElection
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    public string DAOId { get; set; }
    public long TermNumber { get; set; }
    public HighCouncilType HighCouncilType { get; set; }
    public string Address { get; set; }
}