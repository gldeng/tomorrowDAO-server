using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Commitment.Provider;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.GraphQL;
using TomorrowDAOServer.Common.Provider;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Proposal.Index;

namespace TomorrowDAOServer.Commitment;

public class CommitmentSyncDataService : ScheduleSyncDataService
{
    private readonly ICommitmentProvider _commitmentProvider;
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private const int MaxResultCount = 1000;

    public CommitmentSyncDataService(ICommitmentProvider commitmentProvider, ILogger<ScheduleSyncDataService> logger,
        IGraphQLProvider graphQlProvider) : base(
        logger, graphQlProvider)
    {
        _commitmentProvider = commitmentProvider;
        _logger = logger;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var skipCount = 0;
        var blockHeight = -1L;
        // lastEndHeight = 0;
        List<CommitmentIndex> queryList;
        do
        {
            queryList = await _commitmentProvider.GetSyncCommitmentDataAsync(skipCount, chainId, lastEndHeight, 0,
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

        return blockHeight;
    }

    public override Task<List<string>> GetChainIdsAsync()
    {
        throw new System.NotImplementedException();
    }

    public override WorkerBusinessType GetBusinessType()
    {
        throw new System.NotImplementedException();
    }
}