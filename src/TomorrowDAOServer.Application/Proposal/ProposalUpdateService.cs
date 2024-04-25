using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Chains;
using TomorrowDAOServer.Common.Provider;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Proposal.Provider;

namespace TomorrowDAOServer.Proposal;

public class ProposalUpdateService : ScheduleSyncDataService
{
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IProposalProvider _proposalProvider;
    private readonly IChainAppService _chainAppService;

    public ProposalUpdateService(ILogger<ProposalSyncDataService> logger,
        IGraphQLProvider graphQlProvider,
        IProposalProvider proposalProvider,
        IChainAppService chainAppService)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _proposalProvider = proposalProvider;
        _chainAppService = chainAppService;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var skipCount = 0;
        var blockHeight = -1L;
        List<ProposalIndex> queryList;
        do
        {
            queryList = await _proposalProvider.GetNonFinishedProposalListAsync(skipCount, new List<ProposalStage> { ProposalStage.Finished });
            _logger.LogInformation(
                "ExpiredProposal queryList skipCount {skipCount} count: {count}", skipCount, queryList?.Count);
            if (queryList== null || queryList.IsNullOrEmpty())
            {
                break;
            }
            var resultList = await ConvertProposalList(queryList);
            await _proposalProvider.BulkAddOrUpdateAsync(resultList);
            skipCount += queryList.Count;
        } while (!queryList.IsNullOrEmpty());

        return blockHeight;
    }

    private async Task<List<ProposalIndex>> ConvertProposalList(List<ProposalIndex> proposalList)
    {
        //todo code later
        foreach (var proposalIndex in proposalList)
        {
            proposalIndex.ProposalStage = proposalIndex.ActiveEndTime > DateTime.UtcNow
                ? ProposalStage.Finished
                : ProposalStage.Active;
            proposalIndex.ProposalStatus = proposalIndex.ActiveEndTime > DateTime.UtcNow
                ? ProposalStatus.Abstained
                : ProposalStatus.PendingVote;
        }

        return proposalList;
        // foreach (var proposalIndex in proposalList)
        // {
        //     var proposalType = proposalIndex.ProposalType;
        //     var proposalStage = proposalIndex.ProposalStage;
        //     var proposalStatus = proposalIndex.ProposalStatus;
        //     var activeStartTime = proposalIndex.ActiveStartTime;
        //     var activeEndTime = proposalIndex.ActiveEndTime;
        //     var executeStartTime = proposalIndex.ExecuteStartTime;
        //     var executeEndTime = proposalIndex.ExecuteEndTime;
        //     switch (proposalType)
        //     {
        //         case ProposalType.Governance:
        //             switch (proposalStage)
        //             {
        //                 case ProposalStage.Active:
        //                     
        //             }
        //         case ProposalType.Veto:
        //             switch (proposalStage)
        //             {
        //                 case ProposalStage.Active:
        //             }
        //         case ProposalType.Advisory:
        //             switch (proposalStage)
        //             {
        //                 case ProposalStage.Active:
        //             }
        //     }
        // }
        //
        // return null;
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