using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Chains;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.Provider;
using TomorrowDAOServer.DAO.Indexer;
using TomorrowDAOServer.DAO.Provider;
using TomorrowDAOServer.Enums;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.DAO;

public class DAOSyncDataService : ScheduleSyncDataService
{
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly IDAOProvider _daoProvider;
    private readonly IChainAppService _chainAppService;
    private readonly INESTRepository<DAOIndex, string> _daoIndexRepository;
    private const int MaxResultCount = 500;
    
    public DAOSyncDataService(ILogger<DAOSyncDataService> logger,
        IObjectMapper objectMapper, 
        IGraphQLProvider graphQlProvider,
        IDAOProvider daoProvider,
        IChainAppService chainAppService,
        INESTRepository<DAOIndex, string> daoIndexRepository)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _daoProvider = daoProvider;
        _chainAppService = chainAppService;
        _daoIndexRepository = daoIndexRepository;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var skipCount = 0;
        var blockHeight = -1l;
        List<IndexerDAOInfo> queryList;
        do
        {
            var input = new GetChainBlockHeightInput()
            {
                ChainId = chainId,
                SkipCount = skipCount,
                MaxResultCount = MaxResultCount,
                StartBlockHeight = lastEndHeight,
                EndBlockHeight = newIndexHeight
            };
            queryList = await _daoProvider.GetDAOListAsync(input);
            _logger.LogInformation(
                "SyncDAOInfos queryList chainId: {chainId} skipCount: {skipCount} startBlockHeight: {lastEndHeight} endBlockHeight: {newIndexHeight} count: {count}",
                chainId, skipCount, lastEndHeight, newIndexHeight, queryList?.Count);
            if (queryList.IsNullOrEmpty())
            {
                break;
            }

            blockHeight = Math.Max(blockHeight, queryList.Select(t => t.BlockHeight).Max());

            await _daoIndexRepository.BulkAddOrUpdateAsync(
                _objectMapper.Map<List<IndexerDAOInfo>, List<DAOIndex>>(queryList));

            skipCount += queryList.Count;
        } while (!queryList.IsNullOrEmpty());

        return blockHeight;
    }

    public override async Task<List<string>> GetChainIdsAsync()
    {
        var chainIds = await _chainAppService.GetListAsync();
        return chainIds.ToList();
    }

    public override WorkerBusinessType GetBusinessType()
    {
        return WorkerBusinessType.DAOSync;
    }
}