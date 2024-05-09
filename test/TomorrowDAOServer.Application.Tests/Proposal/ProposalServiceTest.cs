// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Microsoft.Extensions.DependencyInjection;
// using Moq;
// using Newtonsoft.Json;
// using Shouldly;
// using TomorrowDAOServer.Entities;
// using TomorrowDAOServer.Enums;
// using TomorrowDAOServer.Organization.Index;
// using TomorrowDAOServer.Organization.Provider;
// using TomorrowDAOServer.Proposal.Dto;
// using TomorrowDAOServer.Proposal.Provider;
// using TomorrowDAOServer.Vote.Dto;
// using TomorrowDAOServer.Vote.Index;
// using TomorrowDAOServer.Vote.Provider;
// using Xunit;
// using Xunit.Abstractions;
//
// namespace TomorrowDAOServer.Proposal;
//
// public class ProposalServiceTest : TomorrowDAOServerApplicationTestBase
// {
//     private const string ProposalIdNotExist = "ProposalId-Not-Exist";
//     private const string Voter = "voter1";
//     private readonly IProposalService _proposalService;
//
//     public ProposalServiceTest(ITestOutputHelper output) : base(output)
//     {
//         _proposalService = GetRequiredService<ProposalService>();
//     }
//
//     protected override void AfterAddApplication(IServiceCollection services)
//     {
//         services.AddSingleton(MockProposalProvider());
//         services.AddSingleton(MockVoteProvider());
//         services.AddSingleton(MockOrganizationInfoProvider());
//     }
//
//     private IProposalProvider MockProposalProvider()
//     {
//         var mock = new Mock<IProposalProvider>();
//
//         mock.Setup(p =>
//             p.GetProposalListAsync(It.Is<QueryProposalListInput>(x => x.ChainId.Equals(ChainIdTDVV) && x.DaoId.Equals(DAOId)))).ReturnsAsync(MockProposalList());
//         
//         // use default
//         mock.Setup(p => p.GetProposalListAsync(It.Is<QueryProposalListInput>(x => !x.ChainId.Equals(ChainIdTDVV) || !x.DaoId.Equals(DAOId))))
//             .ReturnsAsync(new Tuple<long, List<ProposalIndex>>(0, new List<ProposalIndex>()));
//         
//         mock.Setup(p => p.GetProposalByIdAsync(It.IsAny<string>(),
//                 It.Is<string>(x => x.Equals(ProposalId))))
//             .ReturnsAsync(MockProposalList().Item2.Where(item => item.ProposalId.Equals(ProposalId))
//                 .Select(item => item).FirstOrDefault);
//
//         return mock.Object;
//     }
//
//     private IVoteProvider MockVoteProvider()
//     {
//         var mock = new Mock<IVoteProvider>();
//
//         mock.Setup(p =>
//             p.GetVoteItemsAsync(It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(MockVoteInfos());
//
//         mock.Setup(p => p.GetVoteRecordAsync(It.Is<GetVoteRecordInput>(x => x.VotingItemId.Equals(ProposalId))))
//             .ReturnsAsync(MockVoteRecord());
//         
//         mock.Setup(p => p.GetVoteStakeAsync(It.IsAny<string>(),It.Is<string>(x => x.Equals(ProposalId)), 
//                 It.IsAny<string>()))
//             .ReturnsAsync(MockVoteStake());
//
//         return mock.Object;
//     }
//
//     private IOrganizationInfoProvider MockOrganizationInfoProvider()
//     {
//         var mock = new Mock<IOrganizationInfoProvider>();
//
//         mock.Setup(p =>
//                 p.GetOrganizationInfosMemoryAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
//             .ReturnsAsync(MockOrganizationInfos());
//
//         return mock.Object;
//     }
//
//     private static Dictionary<string, IndexerVote> MockVoteInfos()
//     {
//         return new Dictionary<string, IndexerVote>
//         {
//             ["99df86594a989227b8e6259f70b08976812537c20486717a3d0158788155b1f0"] = new()
//             {
//                 AcceptedCurrency = ELF,
//                 ApprovedCount = 2,
//                 RejectionCount = 1,
//                 AbstentionCount = 1,
//                 VotesAmount = 4,
//                 VoterCount = 4
//             },
//             ["b97db4a9f43296157fb1a5d38cebdac478d0e91ed7b8dc1ae2effe1e29e64354"] = new()
//             {
//                 AcceptedCurrency = ELF,
//                 ApprovedCount = 3,
//                 RejectionCount = 2,
//                 AbstentionCount = 2,
//                 VotesAmount = 7,
//                 VoterCount = 7
//             }
//         };
//     }
//     
//     private static IndexerVoteStake MockVoteStake()
//     {
//         return new IndexerVoteStake
//         {
//             AcceptedCurrency = ELF,
//             Amount = 99
//         };
//     }
//
//         
//     private static List<IndexerVoteRecord> MockVoteRecord()
//     {
//         return new List<IndexerVoteRecord>
//         { 
//             new ()
//             {
//                 Voter = "voter1",
//                 Amount = 5,
//                 Option = VoteOption.Approved
//             },
//             new()
//             {
//                 Voter = "voter2",
//                 Amount = 4,
//                 Option = VoteOption.Abstained
//             },
//             new()
//             {
//                 Voter = "voter3",
//                 Amount = 3,
//                 Option = VoteOption.Rejected
//             }
//         };
//     }
//
//     private static Dictionary<string, IndexerOrganizationInfo> MockOrganizationInfos()
//     {
//         return new Dictionary<string, IndexerOrganizationInfo>
//         {
//             ["UE6mcinaCFJZmGNgY9fpMnyzwMETJUhqwbnvtjRgX1f12rBQj"] = new()
//             {
//                 OrganizationName = "Organization Test",
//                 OrganizationAddress = "UE6mcinaCFJZmGNgY9fpMnyzwMETJUhqwbnvtjRgX1f12rBQj",
//                 OrganizationMemberCount = 3
//             }
//         };
//     }
//
//     [Fact]
//     public async void QueryProposalListAsync_Null_Test()
//     {
//         // Arrange
//         var input = new QueryProposalListInput()
//         {
//             ChainId = "AELF",
//             DaoId = DAOId
//         };
//         // Act
//         var result = await _proposalService.QueryProposalListAsync(input);
//
//         // Assert
//         result.ShouldNotBeNull();
//         result.TotalCount.ShouldBe(0);
//         result.Items.ShouldBeEmpty();
//     }
//     
//     [Fact]
//     public async void QueryProposalListAsync_Test()
//     {
//         // Arrange
//         var input = new QueryProposalListInput()
//         {
//             ChainId = ChainIdTDVV,
//             DaoId = DAOId
//         };
//         var tuple = MockProposalList();
//         // Act
//         var result = await _proposalService.QueryProposalListAsync(input);
//
//         // Assert
//         result.ShouldNotBeNull();
//         result.Items.ShouldNotBeEmpty();
//         result.TotalCount.ShouldBe(tuple.Item1);
//
//         foreach (var item in result.Items)
//         {
//             item.TagList.Count.ShouldBe(2);
//             item.VoterCount.ShouldBeGreaterThan(1);
//             item.VotesAmount.ShouldBeGreaterThan(1);
//         }
//     }
//     
//     [Fact]
//     public async void QueryProposalDetailAsync_Null_Test()
//     {
//         // Arrange
//         var input = new QueryProposalDetailInput
//         { 
//             ChainId = ChainIdTDVV,
//             ProposalId = ProposalIdNotExist
//         };
//         // Act
//         var result = await _proposalService.QueryProposalDetailAsync(input);
//
//         // Assert
//         result.ShouldNotBeNull();
//         result.ProposalId.ShouldBeNull();
//     }
//     
//     [Fact]
//     public async void QueryProposalDetailAsync_Test()
//     {
//         // Arrange
//         var input = new QueryProposalDetailInput
//         { 
//             ChainId = ChainIdTDVV,
//             ProposalId = ProposalId
//         };
//         // Act
//         var result = await _proposalService.QueryProposalDetailAsync(input);
//
//         // Assert
//         result.ShouldNotBeNull();
//         result.OrganizationInfo.ShouldNotBeNull();
//         result.OrganizationInfo.OrganizationName.ShouldNotBeNull();
//         result.VoteTopList.ShouldNotBeNull();
//         // Check VoteTopList
//         VoteRecordDto? lastRecord = null;
//         foreach (var record in result.VoteTopList)
//         {
//             lastRecord?.Amount.ShouldBeGreaterThanOrEqualTo(record.Amount);
//             lastRecord = record;
//         }
//     }
//     
//     [Fact]
//     public async void QueryMyInfoAsync_Null_Test()
//     {
//         // Arrange
//         var input = new QueryMyProposalInput
//         { 
//             ChainId = ChainIdTDVV,
//             ProposalId = ProposalIdNotExist,
//             Address = Voter
//         };
//         // Act
//         var result = await _proposalService.QueryMyInfoAsync(input);
//
//         // Assert
//         result.ShouldNotBeNull();
//         result.Symbol.ShouldBeNull();
//         result.StakeAmount.ShouldBe(0);
//     }
//     
//     [Fact]
//     public async void QueryMyInfoAsync_Test()
//     {
//         // Arrange
//         var input = new QueryMyProposalInput
//         { 
//             ChainId = ChainIdTDVV,
//             ProposalId = ProposalId,
//             Address = Voter
//         };
//         var voteStake = MockVoteStake();
//         // Act
//         var result = await _proposalService.QueryMyInfoAsync(input);
//
//         // Assert
//         result.ShouldNotBeNull();
//         result.CanVote.ShouldBeFalse();
//         result.StakeAmount.ShouldBe(voteStake.Amount);
//         result.VotesAmount.ShouldBe(voteStake.Amount);
//         result.AvailableUnStakeAmount.ShouldBe(result.StakeAmount);
//         result.Symbol.ShouldBe(ELF);
//     }
// }