using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Organization.Index;
using TomorrowDAOServer.Organization.Provider;
using TomorrowDAOServer.Proposal.Dto;
using TomorrowDAOServer.Proposal.Provider;
using TomorrowDAOServer.Vote.Index;
using TomorrowDAOServer.Vote.Provider;
using Xunit;
using Xunit.Abstractions;

namespace TomorrowDAOServer.Proposal;

public class ProposalServiceTest : TomorrowDAOServerApplicationTestBase
{
    private readonly IProposalService _proposalService;

    public ProposalServiceTest(ITestOutputHelper output) : base(output)
    {
        _proposalService = GetRequiredService<ProposalService>();
    }

    protected override void AfterAddApplication(IServiceCollection services)
    {
        services.AddSingleton(MockProposalProvider());
        services.AddSingleton(MockVoteProvider());
        services.AddSingleton(MockOrganizationInfoProvider());
    }

    private IProposalProvider MockProposalProvider()
    {
        var mock = new Mock<IProposalProvider>();

        mock.Setup(p =>
            p.GetProposalListAsync(It.IsAny<QueryProposalListInput>())).ReturnsAsync(MockProposalList());

        return mock.Object;
    }

    private IVoteProvider MockVoteProvider()
    {
        var mock = new Mock<IVoteProvider>();

        mock.Setup(p =>
            p.GetVoteInfosAsync(It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(MockVoteInfos());

        return mock.Object;
    }

    private IOrganizationInfoProvider MockOrganizationInfoProvider()
    {
        var mock = new Mock<IOrganizationInfoProvider>();

        mock.Setup(p =>
                p.GetOrganizationInfosMemoryAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
            .ReturnsAsync(MockOrganizationInfos());

        return mock.Object;
    }

    //mock data
    private static Tuple<long, List<ProposalIndex>> MockProposalList()
    {
        var jsonString = @"[
        {
            ""id"": ""99df86594a989227b8e6259f70b08976812537c20486717a3d0158788155b1f0"",
            ""DAOId"": ""a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3"",
            ""proposalId"": ""99df86594a989227b8e6259f70b08976812537c20486717a3d0158788155b1f0"",
            ""proposalTitle"": ""Proposal Title test"",
            ""proposalType"": 1,
            ""governanceMechanism"": 3,
            ""proposalStatus"": 1,
            ""startTime"": ""2024-02-07T10:10:27.3577550Z"",
            ""endTime"": ""2024-02-09T10:10:27.3580530Z"",
            ""expiredTime"": ""2024-02-10T10:10:27.3580960Z"",
            ""executeAddress"": ""aLyxCJvWMQH6UEykTyeWAcYss9baPyXkrMQ37BHnUicxD2LL3"",
            ""proposalDescription"": ""f5bc4667d8cb512113dc140163c5b3bc4829468f49c01483aa46b21298221774"",
            ""transactionInfo"": {
                ""toAddress"": ""YeCqKprLBGbZZeRTkN1FaBLXsetY8QFotmVKqo98w9K6jK2PY"",
                ""contractMethodName"": ""ChangeName"",
                ""params"": {}
            },
            ""governanceSchemeId"": ""f16f5443dbfc30be571104872d88101705834ffeea6632858bc8e70608be5e50"",
            ""executeByHighCouncil"": false,
            ""deployTime"": ""2024-02-07T10:10:27.3691230Z"",
            ""voteFinished"": false,
            ""voteSchemeId"": ""1"",
            ""organizationAddress"": ""UE6mcinaCFJZmGNgY9fpMnyzwMETJUhqwbnvtjRgX1f12rBQj"",
            ""minimalRequiredThreshold"": 11,
            ""minimalVoteThreshold"": 13,
            ""minimalApproveThreshold"": 50,
            ""maximalRejectionThreshold"": 30,
            ""maximalAbstentionThreshold"": 20,
            ""chainId"": ""tDVV"",
            ""blockHash"": ""dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1"",
            ""blockHeight"": 120,
            ""previousBlockHash"": ""e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e"",
            ""isDeleted"": false
        },
        {
            ""id"": ""b97db4a9f43296157fb1a5d38cebdac478d0e91ed7b8dc1ae2effe1e29e64354"",
            ""DAOId"": ""a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3"",
            ""proposalId"": ""b97db4a9f43296157fb1a5d38cebdac478d0e91ed7b8dc1ae2effe1e29e64354"",
            ""proposalTitle"": ""Proposal Title test 2"",
            ""proposalType"": 2,
            ""governanceMechanism"": 1,
            ""proposalStatus"": 1,
            ""startTime"": ""2024-02-07T10:03:03.8204790Z"",
            ""endTime"": ""2024-02-09T10:03:03.8207190Z"",
            ""expiredTime"": ""2024-02-10T10:03:03.8207570Z"",
            ""executeAddress"": ""aLyxCJvWMQH6UEykTyeWAcYss9baPyXkrMQ37BHnUicxD2LL3"",
            ""proposalDescription"": ""f5bc4667d8cb512113dc140163c5b3bc4829468f49c01483aa46b21298221774"",
            ""transactionInfo"": {
                ""toAddress"": ""YeCqKprLBGbZZeRTkN1FaBLXsetY8QFotmVKqo98w9K6jK2PY"",
                ""contractMethodName"": ""ForWord"",
                ""params"": {}
            },
            ""governanceSchemeId"": ""f16f5443dbfc30be571104872d88101705834ffeea6632858bc8e70608be5e50"",
            ""executeByHighCouncil"": false,
            ""deployTime"": ""2024-02-07T10:03:03.8310160Z"",
            ""voteFinished"": false,
            ""voteSchemeId"": ""2"",
            ""organizationAddress"": ""UE6mcinaCFJZmGNgY9fpMnyzwMETJUhqwbnvtjRgX1f12rBQj"",
            ""minimalRequiredThreshold"": 11,
            ""minimalVoteThreshold"": 13,
            ""minimalApproveThreshold"": 50,
            ""maximalRejectionThreshold"": 30,
            ""maximalAbstentionThreshold"": 20,
            ""chainId"": ""tDVV"",
            ""blockHash"": ""dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1"",
            ""blockHeight"": 120,
            ""previousBlockHash"": ""e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e"",
            ""isDeleted"": false
        }   
    ]";
        return new Tuple<long, List<ProposalIndex>>(10, JsonConvert.DeserializeObject<List<ProposalIndex>>(jsonString));
    }

    private static Dictionary<string, IndexerVote> MockVoteInfos()
    {
        return new Dictionary<string, IndexerVote>
        {
            ["99df86594a989227b8e6259f70b08976812537c20486717a3d0158788155b1f0"] = new()
            {
                AcceptedCurrency = "ELF",
                ApproveCount = 2,
                RejectCount = 1,
                AbstainCount = 1,
                VotesAmount = 4,
                VoterCount = 4
            },
            ["b97db4a9f43296157fb1a5d38cebdac478d0e91ed7b8dc1ae2effe1e29e64354"] = new()
            {
                AcceptedCurrency = "ELF",
                ApproveCount = 3,
                RejectCount = 2,
                AbstainCount = 2,
                VotesAmount = 7,
                VoterCount = 7
            }
        };
    }

    private static Dictionary<string, IndexerOrganizationInfo> MockOrganizationInfos()
    {
        return new Dictionary<string, IndexerOrganizationInfo>
        {
            ["UE6mcinaCFJZmGNgY9fpMnyzwMETJUhqwbnvtjRgX1f12rBQj"] = new()
            {
                OrganizationName = "Organization Test",
                OrganizationAddress = "UE6mcinaCFJZmGNgY9fpMnyzwMETJUhqwbnvtjRgX1f12rBQj",
                OrganizationMemberCount = 3
            }
        };
    }

    [Fact]
    public async void QueryProposalListAsync_Test()
    {
        // Arrange
        var input = new QueryProposalListInput();
        var tuple = MockProposalList();

        // Act
        var result = await _proposalService.QueryProposalListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeEmpty();
        result.TotalCount.ShouldBe(tuple.Item1);
    }
}