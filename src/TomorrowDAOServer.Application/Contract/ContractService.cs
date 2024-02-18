using System.Collections.Generic;
using System.Linq;
using TomorrowDAOServer.Contract.Dto;
using TomorrowDAOServer.Contract.Provider;
using TomorrowDAOServer.Options;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Contract;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class ContractService : TomorrowDAOServerAppService, IContractService
{
    private readonly IObjectMapper _objectMapper;
    private readonly IContractProvider _contractProvider;

    public ContractService(IObjectMapper objectMapper, IContractProvider contractProvider)
    {
        _objectMapper = objectMapper;
        _contractProvider = contractProvider;
    }

    public List<FunctionInfoDto> GetFunctionList(string chainId, string contractAddress)
    {
        var contractInfo = _contractProvider.GetContractInfo(chainId, contractAddress).First();
        return _objectMapper.Map<List<FunctionInfo>, List<FunctionInfoDto>>(contractInfo.FunctionList);
    }

    public List<ContractInfoDto> GetContractInfo(string chainId)
    {
        var contractInfos = _contractProvider.GetContractInfo(chainId, string.Empty);
        return _objectMapper.Map<List<ContractInfo>, List<ContractInfoDto>>(contractInfos);
    }
}