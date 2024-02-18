using System.Collections.Generic;

namespace TomorrowDAOServer.Contract.Dto;

public class ContractInfoDto
{
    public string ContractName { get; set; }
    public List<FunctionInfoDto> FunctionList { get; set; }
}

public class FunctionInfoDto
{
    public string FunctionName { get; set; }
    public List<string> FunctionParams { get; set; }
}