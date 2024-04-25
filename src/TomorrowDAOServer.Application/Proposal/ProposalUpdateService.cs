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
    private readonly IProposalAssistService _proposalAssistService;

    public ProposalUpdateService(ILogger<ProposalSyncDataService> logger,
        IGraphQLProvider graphQlProvider,
        IProposalProvider proposalProvider,
        IChainAppService chainAppService,
        IProposalAssistService proposalAssistService)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _proposalProvider = proposalProvider;
        _chainAppService = chainAppService;
        _proposalAssistService = proposalAssistService;
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
            var resultList = await _proposalAssistService.ConvertProposalList(chainId, queryList);
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