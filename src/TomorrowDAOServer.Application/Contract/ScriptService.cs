using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf;
using AElf.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Contract.Dto;
using TomorrowDAOServer.Options;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.Contract;

public interface IScriptService
{
    public Task<List<string>> GetCurrentBPAsync(string chainId);
}

public class ScriptService : IScriptService, ITransientDependency
{
    private readonly ITransactionService _transactionService;
    private readonly List<QueryContractInfo> _queryContractInfos;
    private readonly ILogger<ScriptService> _logger;

    public ScriptService(ITransactionService transactionService, IOptionsSnapshot<QueryContractOption> queryContractOption, 
        ILogger<ScriptService> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
        _queryContractInfos = queryContractOption.Value.QueryContractInfoList?.ToList() ?? new List<QueryContractInfo>();
    }

    public async Task<List<string>> GetCurrentBPAsync(string chainId)
    {
        var queryContractInfo = _queryContractInfos.First(x => x.ChainId == chainId);
        var result = await _transactionService.CallTransactionAsync<GetCurrentMinerPubkeyListDto>(chainId, queryContractInfo.PrivateKey, queryContractInfo.ConsensusContractAddress, ContractConstants.GetCurrentMinerPubkeyList);
        return result?.Pubkeys?.Select(x => Address.FromPublicKey(ByteArrayHelper.HexStringToByteArray(x)).ToBase58()).ToList() ?? new List<string>();
    }
}