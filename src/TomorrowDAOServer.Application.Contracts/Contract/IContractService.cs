using System.Collections.Generic;
using TomorrowDAOServer.Contract.Dto;

namespace TomorrowDAOServer.Contract;

public interface IContractService
{
    FunctionDetail GetFunctionList(string chainId, string contractAddress);
    ContractDetail GetContractInfo(string chainId);
}