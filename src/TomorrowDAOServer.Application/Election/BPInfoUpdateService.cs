using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Chains;
using TomorrowDAOServer.Common.Provider;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.Proposal;
using TomorrowDAOServer.Proposal.Index;
using TomorrowDAOServer.Proposal.Provider;
using TomorrowDAOServer.Vote.Provider;
using Volo.Abp.Caching;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Election;

public class BPInfoUpdateService : ScheduleSyncDataService
{
    private readonly ILogger<BPInfoUpdateService> _logger;
    private readonly IChainAppService _chainAppService;
    private readonly IAElfClientProvider _aElfClientProvider;
    
    public BPInfoUpdateService(ILogger<BPInfoUpdateService> logger, IGraphQLProvider graphQlProvider,
        IChainAppService chainAppService, IAElfClientProvider aElfClientProvider)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _chainAppService = chainAppService;
        _aElfClientProvider = aElfClientProvider;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        
        return newIndexHeight;
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
}