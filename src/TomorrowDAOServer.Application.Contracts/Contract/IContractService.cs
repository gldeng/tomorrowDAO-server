using System.Collections.Generic;
using TomorrowDAOServer.Contract.Dto;

namespace TomorrowDAOServer.Contract;

public interface IContractService
{
    FunctionDetailDto GetFunctionList(string chainId, string contractAddress);
    ContractDetailDto GetContractInfo(string chainId);
}