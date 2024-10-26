using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Chains;
using TomorrowDAOServer.Commitment.Provider;
using TomorrowDAOServer.Common.Provider;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;

namespace TomorrowDAOServer.Commitment;

public class CommitmentSyncDataService : ScheduleSyncDataService
{
    private readonly ICommitmentProvider _commitmentProvider;
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IChainAppService _chainAppService;
    private const int MaxResultCount = 1000;

    public CommitmentSyncDataService(ICommitmentProvider commitmentProvider, ILogger<ScheduleSyncDataService> logger,
        IGraphQLProvider graphQlProvider, IChainAppService chainAppService) : base(
        logger, graphQlProvider)
    {
        _commitmentProvider = commitmentProvider;
        _logger = logger;
        _chainAppService = chainAppService;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var skipCount = 0;
        var blockHeight = -1L;
        // lastEndHeight = 0;
        List<CommitmentIndex> queryList;
        do
        {
            queryList = await _commitmentProvider.GetSyncCommitmentDataAsync(skipCount, chainId, lastEndHeight, newIndexHeight,
                MaxResultCount);
            _logger.LogInformation(
                "CommitmentData queryList skipCount {skipCount} startBlockHeight: {lastEndHeight} endBlockHeight: {newIndexHeight} count: {count}",
                skipCount, lastEndHeight, newIndexHeight, queryList?.Count);
            if (queryList == null || queryList.IsNullOrEmpty())
            {
                break;
            }

            blockHeight = Math.Max(blockHeight, queryList.Select(t => t.BlockHeight).Max());
            await _commitmentProvider.BulkAddOrUpdateAsync(queryList);
            skipCount += queryList.Count;
        } while (!queryList.IsNullOrEmpty());

        return blockHeight+1;
    }

    public override async Task<List<string>> GetChainIdsAsync()
    {
        var chainIds = await _chainAppService.GetListAsync();
        return chainIds.ToList();
    }

    public override WorkerBusinessType GetBusinessType()
    {
        return WorkerBusinessType.CommitmentSync;
    }
}