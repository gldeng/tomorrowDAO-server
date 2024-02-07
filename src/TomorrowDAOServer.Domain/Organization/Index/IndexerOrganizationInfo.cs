using System.Collections.Generic;
using TomorrowDAOServer.Common;

namespace TomorrowDAOServer.Organization.Index;


public class IndexerOrganizationInfos : IndexerCommonResult<IndexerOrganizationInfos>
{
    public List<IndexerOrganizationInfo> DataList { get; set; } = new ();
}

public class IndexerOrganizationInfo : IndexerCommonResult<IndexerOrganizationInfo>
{
    public string OrganizationAddress { get; set; }
    
    public int OrganizationMemberCount { get; set; }
}