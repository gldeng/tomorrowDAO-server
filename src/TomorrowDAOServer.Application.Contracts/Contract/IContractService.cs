using System.Collections.Generic;
using TomorrowDAOServer.Contract.Dto;

namespace TomorrowDAOServer.Contract;

public interface IContractService
{
    List<FunctionInfoDto> GetFunctionList(string chainId, string contractAddress);
    List<ContractInfoDto> GetContractInfo(string chainId);
}