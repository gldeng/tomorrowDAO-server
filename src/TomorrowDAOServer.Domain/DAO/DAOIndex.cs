using System;
using System.Collections.Generic;
using AElf.Indexing.Elasticsearch;
using Nest;
using TomorrowDAOServer.Entities;

namespace TomorrowDAOServer.DAO;

public class DAOIndex : AbstractEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string ChainId { get; set; }
    public long BlockHeight { get; set; }
    [Keyword] public string Creator { get; set; }
    [Keyword] public string MetadataAdmin { get; set; }
    public Metadata Metadata { get; set; }
    [Keyword] public string GovernanceToken { get; set; }
    [Keyword] public string GovernanceSchemeId { get; set; }
    public GovernanceSchemeThreshold GovernanceSchemeThreshold { get; set; }
    public bool IsHighCouncilEnabled { get; set; }
    public bool HighCouncilExecutionConfig { get; set; }
    public HighCouncilConfig HighCouncilConfig { get; set; }
    public long HighCouncilTermNumber { get; set; }
    public List<string> MemberList { get; set; }
    public List<string> CandidateList { get; set; }
    public List<FileInfo> FileInfoList { get; set; }
    public bool IsTreasuryContractNeeded { get; set; }
    public bool IsVoteContractNeeded { get; set; }
    public bool SubsistStatus { get; set; }
    [Keyword] public string TreasuryContractAddress { get; set; }
    [Keyword] public string TreasuryAccountAddress { get; set; }
    public bool IsTreasuryPause { get; set; }
    [Keyword] public string TreasuryPauseExecutor { get; set; }
    [Keyword] public string VoteContractAddress { get; set; }
    [Keyword] public string PermissionAddress { get; set; }
    public List<PermissionInfo> PermissionInfoList { get; set; }
    public DateTime CreateTime { get; set; }
}