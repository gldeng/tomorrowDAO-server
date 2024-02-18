using System.Collections.Generic;

namespace TomorrowDAOServer.Options;

public class ContractInfoOptions
{
    public Dictionary<string, Dictionary<string, ContractInfo>> ContractInfos { get; set; }
}

public class ContractInfo
{
    public string ContractName { get; set; }
    public List<FunctionInfo> FunctionList { get; set; }
}

public class FunctionInfo
{
    public string FunctionName { get; set; }
    public List<string> FunctionParams { get; set; }
}