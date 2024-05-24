using System.Collections.Generic;

namespace TomorrowDAOServer.Governance.Dto;

public class GovernanceMechanismDto
{
    public string ChainId { get; set; }
    public List<GovernanceMechanismInfo> GovernanceMechanismList { get; set; }
}

public class GovernanceMechanismInfo
{
    public string GovernanceSchemeId { get; set; }
    public string Name  { get; set; }
}