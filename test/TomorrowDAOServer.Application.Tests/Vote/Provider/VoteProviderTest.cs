using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using NSubstitute;
using Shouldly;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.GraphQL;
using TomorrowDAOServer.Vote.Dto;
using TomorrowDAOServer.Vote.Index;
using Xunit;

namespace TomorrowDAOServer.Vote.Provider;

public class VoteProviderTest
{ 
    private static readonly IGraphQlHelper GraphQlHelper = Substitute.For<IGraphQlHelper>();
    private readonly VoteProvider _voteProvider = new(GraphQlHelper);

    // [Fact]
    // public async void GetVoteInfosMemoryAsync_Test()
    // {
    //     var result = await _voteProvider.GetVoteInfosMemoryAsync("AELF", new List<string>());
    //     result.ShouldNotBeNull();
    //
    //     GraphQlHelper.QueryAsync<IndexerCommonResult<IndexerVotes>>(Arg.Any<GraphQLRequest>())
    //         .Returns(Task.FromResult(new IndexerCommonResult<IndexerVotes>()));
    //     result = await _voteProvider.GetVoteInfosMemoryAsync("AELF", new List<string>{"votingItemId"});
    //     result.ShouldNotBeNull();
    // }
    
    [Fact]
    public async void GetVoteInfosAsync_Test()
    {
        var result = await _voteProvider.GetVoteInfosAsync("AELF", new List<string>());
        result.ShouldNotBeNull();

        GraphQlHelper.QueryAsync<IndexerCommonResult<IndexerVotes>>(Arg.Any<GraphQLRequest>())
            .Returns(Task.FromResult(new IndexerCommonResult<IndexerVotes>()));
        result = await _voteProvider.GetVoteInfosAsync("AELF", new List<string>{"votingItemId"});
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async void GetVoteStakeAsync_Test()
    {
        var result = await _voteProvider.GetVoteStakeAsync("AELF", string.Empty, "voter");
        result.ShouldNotBeNull();

        GraphQlHelper.QueryAsync<IndexerCommonResult<IndexerVoteStake>>(Arg.Any<GraphQLRequest>())
            .Returns(Task.FromResult(new IndexerCommonResult<IndexerVoteStake>()));
        result = await _voteProvider.GetVoteStakeAsync("AELF", "votingItemId", "voter");
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async void GetVoteRecordAsync_Test()
    {
        var result = await _voteProvider.GetVoteRecordAsync(new GetVoteRecordInput
        {
            ChainId = "AELF",
            VotingItemId = "VotingItemId",
            Voter = "Voter",
            Sorting = "DESC"
        });
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async void GetGovernanceMechanismAsync_Test()
    {
        var result = await _voteProvider.GetVoteSchemeAsync(new GetVoteSchemeInput
        {
            ChainId = "AELF",
            Types = new List<int>{1}
        });
        result.ShouldNotBeNull();
    }
}