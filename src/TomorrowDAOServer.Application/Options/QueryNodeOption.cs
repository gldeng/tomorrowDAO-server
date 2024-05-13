using System.Collections.Generic;

namespace TomorrowDAOServer.Options;

public class QueryNodeOption
{
    public List<QueryNodeInfo> EwellInfoList { get; set; }

}

public class QueryNodeInfo
{
    public string ChainName { get; set; }
    public string AdminKey { get; set; }
    public string ContractAddress { get; set; }
}