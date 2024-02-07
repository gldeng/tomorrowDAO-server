using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.GraphQL;
using TomorrowDAOServer.Vote.Index;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.Vote.Provider;

public interface IVoteProvider
{
    Task<Dictionary<string, IndexerVote>> GetVoteInfosMemory(string chainId, List<string> votingItemIds);
    
    Task<Dictionary<string, IndexerVote>> GetVoteInfos(string chainId, List<string> votingItemIds);
}

public class VoteProvider : IVoteProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;

    public VoteProvider(IGraphQlHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
    }

    public async Task<Dictionary<string, IndexerVote>> GetVoteInfosMemory(string chainId, List<string> votingItemIds)
    {
        if (votingItemIds.IsNullOrEmpty())
        {
            return new();
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
                        approveCounts,
                        rejectCounts,
                        abstainCounts,
                        votesAmount
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

    public async Task<Dictionary<string, IndexerVote>> GetVoteInfos(string chainId, List<string> votingItemIds)
    {
        if (votingItemIds.IsNullOrEmpty())
        {
            return new();
        }
        
        var result = await _graphQlHelper.QueryAsync<IndexerCommonResult<IndexerVotes>>(new GraphQLRequest
        {
            Query = @"
			    query($chainId: String!,$votingItemIds: [votingItemId!]!) {
                    dataList:getVoteInfos(input:{chainId:$chainId,votingItemIds:$votingItemIds}) {
                        votingItemId,
                        voteSchemeId,
                        daoId,
                        acceptedCurrency,
                        approveCounts,
                        rejectCounts,
                        abstainCounts,
                        votesAmount
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
}