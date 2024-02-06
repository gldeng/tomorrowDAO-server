using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Chains;
using TomorrowDAOServer.Common.Provider;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.Proposal.Index;
using TomorrowDAOServer.Proposal.Provider;
using Volo.Abp.Caching;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Proposal;

public class ProposalSyncDataService : ScheduleSyncDataService
{
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly IProposalProvider _proposalProvider;
    private readonly IChainAppService _chainAppService;
    private readonly IDistributedCache<List<string>> _distributedCache;
    private readonly IOptionsMonitor<SyncDataOptions> _syncDataOptionsMonitor;

    public ProposalSyncDataService(ILogger<ProposalSyncDataService> logger,
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
        List<IndexerProposal> queryList;
        do
        {
            queryList = await _proposalProvider.GetSyncProposalDataAsync(skipCount, chainId, lastEndHeight, 0);
            _logger.LogInformation(
                "SyncProposalData queryList skipCount {skipCount} startBlockHeight: {lastEndHeight} endBlockHeight: {newIndexHeight} count: {count}",
                skipCount, lastEndHeight, newIndexHeight, queryList?.Count);
            if (queryList.IsNullOrEmpty())
            {
                break;
            }
            List<string> proposalList = await _distributedCache.GetAsync(GetProposalSyncHeightCacheKey(lastEndHeight));
            var filterProposals = new List<IndexerProposal>();
            foreach (var info in queryList)
            {
                if (proposalList != null && proposalList.Contains(info.ProposalId))
                {
                    continue;
                }
                blockHeight = Math.Max(blockHeight, info.BlockHeight);
                filterProposals.Add(info);
            }
            //get server index
            var proposalDict = await _proposalProvider
                .GetProposalListByIds(filterProposals.Select(p => p.ProposalId).ToList());

            var resultList = ToProposalList(proposalDict, filterProposals);
            
            await _proposalProvider.BulkAddOrUpdateAsync(resultList);
            
            proposalList = queryList.Where(obj => obj.BlockHeight == lastEndHeight)
                .Select(obj => obj.ProposalId)
                .ToList();
            
            await _distributedCache.SetAsync(GetProposalSyncHeightCacheKey(lastEndHeight), proposalList,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration =
                        DateTimeOffset.Now.AddSeconds(_syncDataOptionsMonitor.CurrentValue.CacheSeconds)
                });

            skipCount += queryList.Count;
        } while (!queryList.IsNullOrEmpty());

        return blockHeight;
    }

    private List<ProposalIndex> ToProposalList(Dictionary<string, ProposalIndex> proposalDict, List<IndexerProposal> indexers)
    {
        return indexers.Select(indexer =>
        {
            var proposal = _objectMapper.Map<IndexerProposal, ProposalIndex>(indexer);

            if (indexer.ProposalStatus == ProposalStatus.Executed)
            {
                return proposal;
            }

            if (proposalDict.TryGetValue(proposal.ProposalId, out var preProposal))
            {
                if (preProposal.IsFinalStatus())
                {
                    proposal.ProposalStatus = preProposal.ProposalStatus;
                }
                else if (preProposal.ProposalStatus == ProposalStatus.Approved &&
                         proposal.ExpiredTime <= DateTime.UtcNow)
                {
                    proposal.ProposalStatus = ProposalStatus.Expired;
                }
            }

            proposal.OfProposalStatus();
            return proposal;
        }).ToList();
    }

    public override async Task<List<string>> GetChainIdsAsync()
    {
        //add multiple chains
        var chainIds = await _chainAppService.GetListAsync();
        return chainIds.ToList();
    }

    public override WorkerBusinessType GetBusinessType()
    {
        return WorkerBusinessType.ProposalSync;
    }

    private string GetProposalSyncHeightCacheKey(long blockHeight)
    {
        return $"ProposalSync:{blockHeight}";
    }
}