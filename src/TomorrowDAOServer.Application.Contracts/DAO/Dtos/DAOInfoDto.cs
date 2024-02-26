using System;
using System.Collections.Generic;
using TomorrowDAOServer.Common;

namespace TomorrowDAOServer.DAO.Dtos;

public class DAOInfoDto
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    public long BlockHeight { get; set; }
    public string Creator { get; set; }
    public MetadataDto Metadata { get; set; }
    public string GovernanceToken { get; set; }
    public string GovernanceSchemeId { get; set; }
    public bool IsHighCouncilEnabled { get; set; }
    public HighCouncilConfigDto HighCouncilConfig { get; set; }
    public long HighCouncilTermNumber { get; set; }
    public long MemberCount { get; set; }
    public long CandidateCount { get; set; }
    public List<FileInfoDto> FileInfoList { get; set; }
    public bool IsTreasuryContractNeeded { get; set; }
    public bool SubsistStatus { get; set; }
    public string TreasuryContractAddress { get; set; }
    public string TreasuryAccountAddress { get; set; }
    public bool IsTreasuryPause { get; set; }
    public string TreasuryPauseExecutor { get; set; }
    public string VoteContractAddress { get; set; }
    public string ElectionContractAddress { get; set; }
    public string GovernanceContractAddress { get; set; }
    public string TimelockContractAddress { get; set; }
    public string PermissionAddress { get; set; }
    public List<PermissionInfoDto> PermissionInfoList { get; set; }
    public DateTime CreateTime { get; set; }
}

public class MetadataDto
{
    public string Name { get; set; }
    public string LogoUrl { get; set; }
    public string Description { get; set; }
    public Dictionary<string, string> SocialMedia { get; set; }
}

public class FileInfoDto
{
    public string Name { get; set; }
    public string Cid { get; set; }
    public string Url { get; set; }
}

public class GovernanceSchemeThresholdDto
{
    public int MinimalRequiredThreshold { get; set; }
    public int MinimalVoteThreshold { get; set; }
    public int MinimalApproveThreshold { get; set; }
    public int MaximalRejectionThreshold { get; set; }
    public int MaximalAbstentionThreshold { get; set; }
}

public class HighCouncilConfigDto
{
    public int MaxHighCouncilMemberCount { get; set; }
    public int MaxHighCouncilCandidateCount { get; set; }
    public int ElectionPeriod { get; set; }
    public bool IsRequireHighCouncilForExecution { get; set; }
}

public class PermissionInfoDto
{
    public string Where { get; set; }
    public string What { get; set; }
    public PermissionType PermissionType { get; set; }
    public string Who { get; set; }
}