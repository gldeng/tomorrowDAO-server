using System;
using System.Collections.Generic;
using TomorrowDAOServer.Common;

namespace TomorrowDAOServer.Dtos.DAO;

public class DAODto
{
    public string ChainId { get; set; }
    public string DaoId { get; set; }
    public string Creator { get; set; }
    public string MetadataAdmin { get; set; }
    public DAOMetadataDto Metadata { get; set; }
    public string GovernanceToken { get; set; }
    public string GovernanceSchemeId { get; set; }
    public GovernanceSchemeThresholdDto GovernanceSchemeThreshold { get; set; }
    public bool IsHighCouncilEnabled { get; set; }
    public bool HighCouncilExecutionConfig { get; set; }
    public HighCouncilConfigDto HighCouncilConfig { get; set; }
    public long TermNumber { get; set; }
    public List<string> MemberList { get; set; }
    public List<string> CandidateList { get; set; }
    public List<FileInfoDto> FileInfoList { get; set; }
    public bool IsTreasuryContractNeeded { get; set; }
    public bool IsVoteContractNeeded { get; set; }
    public bool SubsistStatus { get; set; }
    public string TreasuryContractAddress { get; set; }
    public string VoteContractAddress { get; set; }
    public string PermissionAddress { get; set; }
    public List<PermissionInfoDto> PermissionInfoList { get; set; }
    public DateTime CreateTime { get; set; }
}

public class DAOMetadataDto
{
    public string Name { get; set; }
    public string LogoUrl { get; set; }
    public string Description { get; set; }
    public Dictionary<string, string> SocialMedia { get; set; }
}

public class FileInfoDto
{
    public string Name { get; set; }
    public string Hash { get; set; }
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