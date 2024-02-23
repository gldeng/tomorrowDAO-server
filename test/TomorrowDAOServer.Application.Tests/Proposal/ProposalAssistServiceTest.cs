using System.Collections.Generic;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Organization.Index;
using TomorrowDAOServer.Vote.Index;
using Xunit;
using Xunit.Abstractions;

namespace TomorrowDAOServer.Proposal;

public sealed class ProposalAssistServiceTest : TomorrowDAOServerApplicationTestBase
{
    private readonly IProposalAssistService _proposalAssistService;

    public ProposalAssistServiceTest(ITestOutputHelper output) : base(output)
    {
        _proposalAssistService = GetRequiredService<ProposalAssistService>();
    }


    private static Dictionary<GovernanceMechanism, ProposalIndex> MockGovernanceProposal()
    {
        return new Dictionary<GovernanceMechanism, ProposalIndex>
        {
            [GovernanceMechanism.Parliament] = new()
            {
                GovernanceMechanism = GovernanceMechanism.Parliament,
                MinimalRequiredThreshold = 7500,
                MinimalVoteThreshold = 0,
                MinimalApproveThreshold = 6700,
                MaximalRejectionThreshold = 2000,
                MaximalAbstentionThreshold = 2000
            },
            [GovernanceMechanism.Association] = new()
            {
                GovernanceMechanism = GovernanceMechanism.Association,
                MinimalRequiredThreshold = 5000,
                MinimalVoteThreshold = 0,
                MinimalApproveThreshold = 6000,
                MaximalRejectionThreshold = 4000,
                MaximalAbstentionThreshold = 4000
            },
            [GovernanceMechanism.Customize] = new()
            {
                GovernanceMechanism = GovernanceMechanism.Customize,
                MinimalRequiredThreshold = 0,
                MinimalVoteThreshold = 0,
                MinimalApproveThreshold = 0,
                MaximalRejectionThreshold = 0,
                MaximalAbstentionThreshold = 0
            },
            [GovernanceMechanism.Referendum] = new()
            {
                GovernanceMechanism = GovernanceMechanism.Referendum,
                MinimalRequiredThreshold = 1,
                MinimalVoteThreshold = 0,
                MinimalApproveThreshold = 5000,
                MaximalRejectionThreshold = 2000,
                MaximalAbstentionThreshold = 2000
            }
        };
    }

    [Theory]
    [InlineData(ProposalStatus.Expired, 100, 10, 60, 30, 20, 50)]
    [InlineData(ProposalStatus.Rejected, 100, 21, 20, 59, 25, 30)]
    [InlineData(ProposalStatus.Abstained, 100, 20, 21, 59, 19, 20)]
    [InlineData(ProposalStatus.Approved, 100, 20, 10, 70, 50, 60)]
    public void ToProposalResult_Parliament_Returns_Expected_Status(ProposalStatus expectedStatus, int totalVotes, int rejectionCount, int abstentionCount, int approvedCount, int voterCount, int organizationMemberCount)
    {
        // Arrange
        var proposalDict = MockGovernanceProposal();
        var proposal = proposalDict[GovernanceMechanism.Parliament];
        var voteInfo = new IndexerVote
        {
            VoterCount = voterCount,
            VotesAmount = totalVotes,
            RejectionCount = rejectionCount,
            AbstentionCount = abstentionCount,
            ApprovedCount = approvedCount
        };

        var organizationInfo = new IndexerOrganizationInfo
        {
            OrganizationMemberCount = organizationMemberCount
        };

        // Act
        var result = _proposalAssistService.ToProposalResult(proposal, voteInfo, organizationInfo);

        // Assert
        Assert.Equal(expectedStatus, result);
    }
    
    [Theory]
    [InlineData(ProposalStatus.Expired, 100, 10, 60, 30, 20, 50)]
    [InlineData(ProposalStatus.Rejected, 100, 41, 20, 39, 25, 30)]
    [InlineData(ProposalStatus.Abstained, 100, 20, 41, 39, 19, 20)]
    [InlineData(ProposalStatus.Approved, 100, 40, 0, 60, 50, 60)]
    public void ToProposalResult_Association_Returns_Expected_Status(ProposalStatus expectedStatus, int totalVotes, int rejectionCount, int abstentionCount, int approvedCount, int voterCount, int organizationMemberCount)
    {
        // Arrange
        var proposalDict = MockGovernanceProposal();
        var proposal = proposalDict[GovernanceMechanism.Association];
        var voteInfo = new IndexerVote
        {
            VoterCount = voterCount,
            VotesAmount = totalVotes,
            RejectionCount = rejectionCount,
            AbstentionCount = abstentionCount,
            ApprovedCount = approvedCount
        };

        var organizationInfo = new IndexerOrganizationInfo
        {
            OrganizationMemberCount = organizationMemberCount
        };

        // Act
        var result = _proposalAssistService.ToProposalResult(proposal, voteInfo, organizationInfo);

        // Assert
        Assert.Equal(expectedStatus, result);
    }
    
    [Theory]
    [InlineData(ProposalStatus.Expired, 100, 10, 60, 30, 0, 50)]
    [InlineData(ProposalStatus.Rejected, 100, 21, 20, 59, 25, 30)]
    [InlineData(ProposalStatus.Abstained, 100, 20, 21, 59, 19, 20)]
    [InlineData(ProposalStatus.Approved, 100, 20, 20, 60, 50, 60)]
    public void ToProposalResult_Referendum_Returns_Expected_Status(ProposalStatus expectedStatus, int totalVotes, int rejectionCount, int abstentionCount, int approvedCount, int voterCount, int organizationMemberCount)
    {
        // Arrange
        var proposalDict = MockGovernanceProposal();
        var proposal = proposalDict[GovernanceMechanism.Referendum];
        var voteInfo = new IndexerVote
        {
            VoterCount = voterCount,
            VotesAmount = totalVotes,
            RejectionCount = rejectionCount,
            AbstentionCount = abstentionCount,
            ApprovedCount = approvedCount
        };

        var organizationInfo = new IndexerOrganizationInfo
        {
            OrganizationMemberCount = organizationMemberCount
        };

        // Act
        var result = _proposalAssistService.ToProposalResult(proposal, voteInfo, organizationInfo);

        // Assert
        Assert.Equal(expectedStatus, result);
    }

}