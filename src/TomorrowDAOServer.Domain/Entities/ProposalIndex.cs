using System;
using AElf.Indexing.Elasticsearch;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TomorrowDAOServer.Enums;

namespace TomorrowDAOServer.Entities;

public class ProposalIndex : BlockInfoBase, IIndexBuild
{
    [Keyword] public override string Id { get; set; }

    [Keyword] public string DaoId { get; set; }

    [Keyword] public string ProposalId { get; set; }

    [Keyword] public string ProposalTitle { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ProposalType ProposalType { get; set; }

    //get from GovernanceSchemeId
    [JsonConverter(typeof(StringEnumConverter))]
    public GovernanceType GovernanceType { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public DateTime ExpiredTime { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ProposalStatus ProposalStatus { get; set; }

    [Keyword] public string Proposer { get; set; }

    [Keyword] public string OrganizationAddress { get; set; }

    [Keyword] public string ReleaseAddress { get; set; }

    [Keyword] public string ProposalDescriptionUrl { get; set; }

    public CallTransaction Transaction { get; set; }

    //sub_scheme_id
    [Keyword] public string GovernanceSchemeId { get; set; }

    [Keyword] public string VoteSchemeId { get; set; }

    public bool ExecuteByHighCouncil { get; set; }

    public DateTime DeployTime { get; set; }
}

public class CallTransaction
{
    // The address of the target contract.
    [Keyword] public string ToAddress { get; set; }

    // The method that this proposal will call when being released.
    [Keyword] public string ContractMethodName { get; set; }

    public object[] Params { get; set; }
}