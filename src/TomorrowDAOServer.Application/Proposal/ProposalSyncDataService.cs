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
using TomorrowDAOServer.Vote.Provider;
using Volo.Abp.Caching;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Proposal;

public class ProposalSyncDataService : ScheduleSyncDataService
{
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly IProposalProvider _proposalProvider;
    private readonly IVoteProvider _voteProvider;
    private readonly IChainAppService _chainAppService;
    private readonly IDistributedCache<List<string>> _distributedCache;
    private readonly IOptionsMonitor<SyncDataOptions> _syncDataOptionsMonitor;
    private readonly IProposalAssistService _proposalAssistService;

    public ProposalSyncDataService(ILogger<ProposalSyncDataService> logger,
        IGraphQLProvider graphQlProvider,
        IProposalProvider proposalProvider,
        IChainAppService chainAppService,
        IDistributedCache<List<string>> distributedCache,
        IOptionsMonitor<SyncDataOptions> syncDataOptionsMonitor, 
        IObjectMapper objectMapper, IVoteProvider voteProvider, 
        IProposalAssistService proposalAssistService)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _proposalProvider = proposalProvider;
        _chainAppService = chainAppService;
        _distributedCache = distributedCache;
        _syncDataOptionsMonitor = syncDataOptionsMonitor;
        _objectMapper = objectMapper;
        _voteProvider = voteProvider;
        _proposalAssistService = proposalAssistService;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var skipCount = 0;
        var blockHeight = -1L;
        List<IndexerProposal> queryList;
        do
        {
            queryList = await _proposalProvider.GetSyncProposalDataAsync(skipCount, chainId, lastEndHeight, 0);
            _logger.LogInformation("SyncProposalData queryList skipCount {skipCount} startBlockHeight: {lastEndHeight} endBlockHeight: {newIndexHeight} count: {count}",
                skipCount, lastEndHeight, newIndexHeight, queryList?.Count);
            if (queryList == null || queryList.IsNullOrEmpty())
            {
                break;
            }
            blockHeight = Math.Max(blockHeight, queryList.Select(t => t.BlockHeight).Max());
            await _proposalProvider.BulkAddOrUpdateAsync(await ConvertProposalList(queryList));
            skipCount += queryList.Count;
        } while (!queryList.IsNullOrEmpty());

        return blockHeight;
    }

    private async Task<List<ProposalIndex>> ConvertProposalList(List<IndexerProposal> proposalList)
    {
        //todo code later
        var list = _objectMapper.Map<List<IndexerProposal>, List<ProposalIndex>>(proposalList);
        foreach (var proposalIndex in list)
        {
            proposalIndex.ProposalStage = proposalIndex.ActiveEndTime > DateTime.UtcNow
                ? ProposalStage.Finished
                : ProposalStage.Active;
            proposalIndex.ProposalStatus = proposalIndex.ActiveEndTime > DateTime.UtcNow
                ? ProposalStatus.Abstained
                : ProposalStatus.PendingVote;
        }

        return list;
    }

    // private async Task<List<ProposalIndex>> ConvertProposalList(string chainId, List<IndexerProposal> indexers)
    // {
    //     //get server index before
    //     var preProposalDict = await _proposalProvider
    //         .GetProposalListByIds(chainId, indexers.Select(p => p.ProposalId).ToList());
    //     var voteFinishedProposalIds = indexers.Where(index => index.VoteFinished)
    //         .Select(index => index.ProposalId).ToList();
    //     var voteDict = await _voteProvider.GetVoteInfosMemoryAsync(chainId, voteFinishedProposalIds);
    //     return indexers.Select(indexer =>
    //     {
    //         var proposal = _objectMapper.Map<IndexerProposal, ProposalIndex>(indexer);
    //         if (proposal.ProposalStatus == ProposalStatus.Executed)
    //         {
    //             return proposal;
    //         }
    //
    //         if (preProposalDict.TryGetValue(proposal.ProposalId, out var preProposal) && preProposal.IsFinalStatus())
    //         {
    //             proposal.ProposalStatus = preProposal.ProposalStatus;
    //         }
    //         else if (preProposal?.ProposalStatus == ProposalStatus.Approved && proposal.ExecuteEndTime <= DateTime.UtcNow)
    //         {
    //             proposal.ProposalStatus = ProposalStatus.Expired;
    //         }
    //         else if (!proposal.IsFinalStatus())
    //         {
    //             if (proposal.ActiveEndTime > DateTime.UtcNow)
    //             {
    //                 proposal.ProposalStatus = ProposalStatus.PendingVote;
    //             }
    //             else if (proposal.VoteFinished && voteDict.TryGetValue(proposal.ProposalId, out var voteInfo))
    //             {
    //                 _logger.LogInformation(
    //                     "[VoteFinishedStatus] start proposalId:{proposalId} proposalStatus:{proposalStatus}",
    //                     proposal.ProposalId, proposal.ProposalStatus);
    //                 proposal.ProposalStatus = _proposalAssistService.ToProposalResult(proposal, voteInfo);
    //                 _logger.LogInformation(
    //                     "[VoteFinishedStatus] end proposalId:{proposalId} proposalStatus:{proposalStatus}",
    //                     proposal.ProposalId, proposal.ProposalStatus);
    //             }
    //         }
    //
    //         return proposal;
    //     }).ToList();
    // }

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