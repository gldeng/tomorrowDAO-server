using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Enums;

namespace TomorrowDAOServer.Governance;

public class IndexerGovernanceMechanismResult : IndexerCommonResult<IndexerGovernanceMechanismResult>
{
    public List<IndexerGovernanceMechanism> DataList { get; set; } = new ();
}

public class IndexerGovernanceMechanism
{
    public string Id { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public GovernanceMechanism GovernanceMechanism  { get; set; }
}