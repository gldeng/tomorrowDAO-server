using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.GraphQL;
using TomorrowDAOServer.Vote.Dto;
using TomorrowDAOServer.Vote.Index;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.Vote.Provider;

public interface IVoteProvider
{
    Task<Dictionary<string, IndexerVote>> GetVoteInfosMemoryAsync(string chainId, List<string> votingItemIds);
    
    Task<Dictionary<string, IndexerVote>> GetVoteInfosAsync(string chainId, List<string> votingItemIds);
    
    Task<IndexerVoteStake> GetVoteStakeAsync(string chainId, string votingItemId, string voter);
    
    Task<List<IndexerVoteRecord>> GetVoteRecordAsync(GetVoteRecordInput input);
    
    Task<List<IndexerVoteSchemeInfo>> GetVoteSchemeAsync(GetVoteSchemeInput input);
}

public class VoteProvider : IVoteProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;
    private readonly ILogger<VoteProvider> _logger;

    public VoteProvider(IGraphQlHelper graphQlHelper, ILogger<VoteProvider> logger)
    {
        _graphQlHelper = graphQlHelper;
        _logger = logger;
    }

    public async Task<Dictionary<string, IndexerVote>> GetVoteInfosMemoryAsync(string chainId, List<string> votingItemIds)
    {
        if (votingItemIds.IsNullOrEmpty())
        {
            return new Dictionary<string, IndexerVote>();
        }

        var result = await _graphQlHelper.QueryAsync<IndexerCommonResult<IndexerVotes>>(new GraphQLRequest
        {
            Query = @"
			    query($chainId: String!,$votingItemIds: [votingItemId!]!) {
                    dataList:getVoteInfosMemory(input:{chainId:$chainId,votingItemIds:$votingItemIds}) {
                        votingItemId,
                        voteSchemeId,
                        daoId,
                        acceptedCurrency,
                        approvedCount,
                        rejectionCount,
                        AbstentionCount,
                        votesAmount,
                        voterCount
                    }
                  }",
            Variables = new
            {
                chainId, votingItemIds
            }
        });
        var voteInfos = result.Data?.Data ?? new IndexerVotes();
        return voteInfos.DataList.ToDictionary(vote => vote.VotingItemId, vote => vote);
    }

    public async Task<Dictionary<string, IndexerVote>> GetVoteInfosAsync(string chainId, List<string> votingItemIds)
    {
        if (votingItemIds.IsNullOrEmpty())
        {
            return new Dictionary<string, IndexerVote>();
        }

        try
        {
            var result = await _graphQlHelper.QueryAsync<IndexerCommonResult<IndexerVotes>>(new GraphQLRequest
            {
                Query = @"
			    query($chainId: String!,$votingItemIds: [votingItemId!]!) {
                    dataList:getVoteInfos(input:{chainId:$chainId,votingItemIds:$votingItemIds}) {
                        votingItemId,
                        voteSchemeId,
                        daoId,
                        acceptedCurrency,
                        approvedCount,
                        rejectionCount,
                        AbstentionCount,
                        votesAmount,
                        voterCount
                    }
                  }",
                Variables = new
                {
                    chainId, votingItemIds
                }
            });
            var voteInfos = result.Data?.Data ?? new IndexerVotes();
            return voteInfos.DataList.ToDictionary(vote => vote.VotingItemId, vote => vote);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetVoteInfosAsync Exception chainId {chainId}, votingItemIds {votingItemIds}", chainId, votingItemIds);
            return new Dictionary<string, IndexerVote>();
        }
    }

    public async Task<IndexerVoteStake> GetVoteStakeAsync(string chainId, string votingItemId, string voter)
    {
        if (votingItemId.IsNullOrEmpty() || voter.IsNullOrEmpty())
        {
            return new IndexerVoteStake();
        }
        
        var result = await _graphQlHelper.QueryAsync<IndexerCommonResult<IndexerVoteStake>>(new GraphQLRequest
        {
            Query = @"
			    query($chainId: String!,$votingItemId: String!,$voter: String!) {
                    getVoteStake(input:{chainId:$chainId,votingItemId:$votingItemId,voter:$voter}) {
                        votingItemId,
                        voter,
                        acceptedCurrency
                        amount,
                        createTime
                    }
                  }",
            Variables = new
            {
                chainId, votingItemId, voter
            }
        });
        return result.Data?.Data ?? new IndexerVoteStake();
    }

    public async Task<List<IndexerVoteRecord>> GetVoteRecordAsync(GetVoteRecordInput input)
    {
        try
        {
            var result = await _graphQlHelper.QueryAsync<IndexerCommonResult<IndexerVoteRecords>>(new GraphQLRequest
            {
                Query = @"
			    query($chainId: String!,$votingItemId: String!,$voter: String,$sorting: String) {
                    dataList:getVoteRecord(input:{chainId:$chainId,votingItemId:$votingItemId,voter:$voter,sorting:$sorting}) {
                        voter,
                        amount,
                        option,
                        voteTime
                    }
                  }",
                Variables = new
                {
                    chainId = input.ChainId,
                    votingItemId = input.VotingItemId, 
                    voter = input.Voter,
                    sorting = input.Sorting
                }
            });
            return result?.Data?.DataList ?? new List<IndexerVoteRecord>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetVoteRecordAsync Exception chainId {chainId}, votingItemId {votingItemId}, voter {voter}, sorting {sorting}", 
                input.ChainId, input.VotingItemId, input.Voter, input.Sorting);
            return new List<IndexerVoteRecord>();
        }
    }

    public async Task<List<IndexerVoteSchemeInfo>> GetVoteSchemeAsync(GetVoteSchemeInput input)
    {
        var graphQlResponse = await _graphQlHelper.QueryAsync<IndexerVoteSchemeResult>(new GraphQLRequest
        {
            Query =
                @"query($chainId:String,$types:[Int!]){
            data:getVoteSchemeInfo(input: {chainId:$chainId})
            {
                id,chainId,voteSchemeId,voteMechanism
            }}",
            Variables = new
            {
                input.ChainId,
            }
        });
        return graphQlResponse?.DataList ?? new List<IndexerVoteSchemeInfo>();
    }
}