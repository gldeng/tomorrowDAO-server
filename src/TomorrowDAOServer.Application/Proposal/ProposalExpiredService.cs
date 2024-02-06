using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Chains;
using TomorrowDAOServer.Common.Provider;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.Proposal.Provider;
using Volo.Abp.Caching;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Proposal;

public class ProposalExpiredService : ScheduleSyncDataService
{
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly IProposalProvider _proposalProvider;
    private readonly IChainAppService _chainAppService;
    private readonly IDistributedCache<List<string>> _distributedCache;
    private readonly IOptionsMonitor<SyncDataOptions> _syncDataOptionsMonitor;

    public ProposalExpiredService(ILogger<ProposalSyncDataService> logger,
        IGraphQLProvider graphQlProvider,
        IProposalProvider proposalProvider,
        IChainAppService chainAppService,
        IDistributedCache<List<string>> distributedCache,
        IOptionsMonitor<SyncDataOptions> syncDataOptionsMonitor, 
        IObjectMapper objectMapper)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _proposalProvider = proposalProvider;
        _chainAppService = chainAppService;
        _distributedCache = distributedCache;
        _syncDataOptionsMonitor = syncDataOptionsMonitor;
        _objectMapper = objectMapper;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var skipCount = 0;
        var blockHeight = -1L;
        List<ProposalIndex> queryList;
        do
        {
            queryList = await _proposalProvider.GetExpiredProposalListAsync(new List<ProposalStatus> { ProposalStatus.Approved });
            _logger.LogInformation(
                "ExpiredProposal queryList skipCount {skipCount} startBlockHeight: {lastEndHeight} endBlockHeight: {newIndexHeight} count: {count}",
                skipCount, lastEndHeight, newIndexHeight, queryList?.Count);
            if (queryList.IsNullOrEmpty())
            {
                break;
            }
            var resultList = new List<ProposalIndex>();
            foreach (var info in queryList)
            {
                blockHeight = Math.Max(blockHeight, info.BlockHeight);
                info.ProposalStatus = ProposalStatus.Expired;
                resultList.Add(info);
            }
            await _proposalProvider.BulkAddOrUpdateAsync(resultList);
            skipCount += queryList.Count;
        } while (!queryList.IsNullOrEmpty());

        return blockHeight;
    }

    public override async Task<List<string>> GetChainIdsAsync()
    {
        //add multiple chains
        var chainIds = await _chainAppService.GetListAsync();
        return chainIds.ToList();
    }

    public override WorkerBusinessType GetBusinessType()
    {
        return WorkerBusinessType.ProposalExpired;
    }
}