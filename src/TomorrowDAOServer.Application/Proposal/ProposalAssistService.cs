using System;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Organization.Index;
using TomorrowDAOServer.Vote.Index;

namespace TomorrowDAOServer.Proposal;

public interface IProposalAssistService
{
    public ProposalStatus ToProposalResult(ProposalIndex proposal, IndexerVote voteInfo,
        IndexerOrganizationInfo organizationInfo);
}

public class ProposalAssistService : TomorrowDAOServerAppService, IProposalAssistService
{
    private readonly ILogger<ProposalAssistService> _logger;

    public ProposalAssistService(ILogger<ProposalAssistService> logger)
    {
        _logger = logger;
    }
    
    public ProposalStatus ToProposalResult(ProposalIndex proposal, IndexerVote voteInfo, IndexerOrganizationInfo organizationInfo)
    {
        var targetStatus = proposal.ProposalStatus;
        // VotesAmount not enough
        if (proposal.MinimalVoteThreshold > 0 && voteInfo.VotesAmount < proposal.MinimalVoteThreshold)
        {
            _logger.LogInformation(
                "[VoteFinishedStatus] proposalId:{proposalId}, VotesAmount: {VotesAmount} < MinimalVoteThreshold:{MinimalVoteThreshold}",
                proposal.ProposalId, voteInfo.VotesAmount, proposal.MinimalVoteThreshold);
            return ProposalStatus.Expired;
        }

        if (proposal.GovernanceMechanism is GovernanceMechanism.Customize or GovernanceMechanism.Referendum)
        {
            _logger.LogInformation(
                "[VoteFinishedStatus] proposalId:{proposalId}, GovernanceMechanism:{GovernanceMechanism} " +
                "VoterCount: {VoterCount} MinimalRequiredThreshold:{MinimalRequiredThreshold}",
                proposal.ProposalId, proposal.GovernanceMechanism, voteInfo.VoterCount, proposal.MinimalRequiredThreshold);
            if (proposal.MinimalRequiredThreshold > 0 && voteInfo.VoterCount < proposal.MinimalRequiredThreshold)
            {
                return ProposalStatus.Expired;
            }
        }

        if (proposal.GovernanceMechanism is GovernanceMechanism.Parliament or GovernanceMechanism.Association)
        {
            double voterPercentage = GetPercentage(voteInfo.VoterCount, organizationInfo.OrganizationMemberCount);
            _logger.LogInformation(
                "[VoteFinishedStatus] proposalId:{proposalId}, GovernanceMechanism:{GovernanceMechanism} " +
                "voterPercentage: {voterPercentage} MinimalRequiredThreshold:{MinimalRequiredThreshold}",
                proposal.ProposalId, proposal.GovernanceMechanism, voterPercentage, proposal.MinimalRequiredThreshold);
            if (voterPercentage < proposal.MinimalRequiredThreshold)
            {
                return ProposalStatus.Expired;
            }
        }

        double rejectionPercentage = GetPercentage(voteInfo.RejectCounts, voteInfo.VotesAmount);
        double abstentionPercentage = GetPercentage(voteInfo.AbstainCounts, voteInfo.VotesAmount);
        double approvalPercentage = GetPercentage(voteInfo.ApproveCounts, voteInfo.VotesAmount);
        _logger.LogInformation(
            "[VoteFinishedStatus] proposalId:{proposalId}, GovernanceMechanism:{GovernanceMechanism} " +
            "rejectionPercentage: {rejectionPercentage} MinimalRejectionThreshold: {MinimalRejectionThreshold} " +
            "abstentionPercentage:{abstentionPercentage} MinimalAbstentionThreshold:{MinimalAbstentionThreshold} " +
            "approvalPercentage:{approvalPercentage} MinimalApproveThreshold:{MinimalApproveThreshold}",
            proposal.ProposalId, proposal.GovernanceMechanism, rejectionPercentage, proposal.MinimalRejectionThreshold,
            abstentionPercentage, proposal.MinimalAbstentionThreshold, approvalPercentage, proposal.MinimalApproveThreshold);
        if (rejectionPercentage >= proposal.MinimalRejectionThreshold)
        {
            targetStatus = ProposalStatus.Rejected;
        }
        else if (abstentionPercentage >= proposal.MinimalAbstentionThreshold)
        {
            targetStatus = ProposalStatus.Abstained;
        }
        else if (approvalPercentage >= proposal.MinimalApproveThreshold)
        {
            targetStatus = ProposalStatus.Approved;
        }
        _logger.LogInformation("[VoteFinishedStatus] end targetStatus:{}", targetStatus);
        return targetStatus;
    }
    
    private double GetPercentage(int count, int totalCount)
    {
        return Math.Round((double)count / totalCount * 100, 2);
    }
}