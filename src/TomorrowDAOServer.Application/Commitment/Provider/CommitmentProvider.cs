using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using GraphQL;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.GraphQL;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Proposal.Index;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.Commitment.Provider;

public class CommitmentIndexSync : IndexerCommonResult<CommitmentIndexSync>
{
    public long TotalRecordCount { get; set; }
    public List<CommitmentIndex>? DataList { get; set; }
}

public interface ICommitmentProvider
{
    Task<List<CommitmentIndex>> GetSyncCommitmentDataAsync(int skipCount, string chainId,
        long startBlockHeight, long endBlockHeight, int maxResultCount);

    Task BulkAddOrUpdateAsync(List<CommitmentIndex> list);
}

public class CommitmentProvider : ICommitmentProvider, ISingletonDependency
{
    private readonly ILogger<CommitmentProvider> _logger;
    private readonly IGraphQlHelper _graphQlHelper;
    private readonly INESTRepository<CommitmentIndex, string> _commitmentIndexRepository;

    public CommitmentProvider(INESTRepository<CommitmentIndex, string> commitmentIndexRepository,
        IGraphQlHelper graphQlHelper, ILogger<CommitmentProvider> logger)
    {
        _commitmentIndexRepository = commitmentIndexRepository;
        _graphQlHelper = graphQlHelper;
        _logger = logger;
    }

    public async Task<List<CommitmentIndex>> GetSyncCommitmentDataAsync(int skipCount, string chainId,
        long startBlockHeight, long endBlockHeight, int maxResultCount)
    {
        var graphQlResponse = await _graphQlHelper.QueryAsync<CommitmentIndexSync>(new GraphQLRequest
        {
            Query =
                @"query GetCommitments(
                  $skipCount: Int!,
                  $chainId: String,
                  $startBlockHeight: Long!,
                  $endBlockHeight: Long!,
                  $maxResultCount: Int!
                ) {
                  dataList:getCommitments(input: {
                    skipCount: $skipCount,
                    maxResultCount: $maxResultCount,
                    chainId: $chainId,
                    startBlockHeight: $startBlockHeight,
                    endBlockHeight: $endBlockHeight
                  }) {
                    chainId
                    transactionId
                    blockHeight
                    daoId
                    votingItemId
                    proposalId
                    voter
                    commitment
                    leafIndex
                    timestamp
                  }
                }",
            Variables = new
            {
                skipCount,
                chainId,
                startBlockHeight,
                endBlockHeight,
                maxResultCount
            }
        });
        return graphQlResponse?.DataList ?? new List<CommitmentIndex>();
    }

    public async Task BulkAddOrUpdateAsync(List<CommitmentIndex> list)
    {
        await _commitmentIndexRepository.BulkAddOrUpdateAsync(list);
    }
}