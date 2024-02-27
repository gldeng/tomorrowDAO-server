using System.Collections.Generic;
using NSubstitute;
using Shouldly;
using TomorrowDAOServer.Vote.Dto;
using TomorrowDAOServer.Vote.Provider;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace TomorrowDAOServer.Vote;

public class VoteServiceTest
{ 
    private readonly VoteService _voteService = new(Substitute.For<IVoteProvider>(), Substitute.For<IObjectMapper>());

    [Fact]
    public async void GetVoteSchemeAsync_Test()
    {
        var result = await _voteService.GetVoteSchemeAsync(new GetVoteSchemeInput
        {
            ChainId = "AELF",
            Types = new List<int>{1}
        });
        result.ShouldNotBeNull();
    }
}