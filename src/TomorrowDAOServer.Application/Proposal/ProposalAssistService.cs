using System;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Organization.Index;
using TomorrowDAOServer.Vote.Index;

namespace TomorrowDAOServer.Proposal;

public interface IProposalAssistService
{
    public ProposalStatus ToProposalResult(ProposalIndex proposal, IndexerVote voteInfo);
}

public class ProposalAssistService : TomorrowDAOServerAppService, IProposalAssistService
{
    private readonly ILogger<ProposalAssistService> _logger;

    public ProposalAssistService(ILogger<ProposalAssistService> logger)
    {
        _logger = logger;
    }
    
    public ProposalStatus ToProposalResult(ProposalIndex proposal, IndexerVote voteInfo)
    {
        //todo change later
        return proposal.ActiveEndTime < DateTime.UtcNow ? ProposalStatus.Expired : ProposalStatus.PendingVote;

        var targetStatus = proposal.ProposalStatus;
        // VotesAmount not enough
        if (proposal.MinimalVoteThreshold > 0 && voteInfo.VotesAmount < proposal.MinimalVoteThreshold)
        {
            _logger.LogInformation(
                "[VoteFinishedStatus] proposalId:{proposalId}, VotesAmount: {VotesAmount} < MinimalVoteThreshold:{MinimalVoteThreshold}",
                proposal.ProposalId, voteInfo.VotesAmount, proposal.MinimalVoteThreshold);
            return ProposalStatus.Expired;
        }

        // if (proposal.GovernanceMechanism is GovernanceMechanism.Customize or GovernanceMechanism.Referendum)
        // {
        //     _logger.LogInformation(
        //         "[VoteFinishedStatus] proposalId:{proposalId}, GovernanceMechanism:{GovernanceMechanism} " +
        //         "VoterCount: {VoterCount} MinimalRequiredThreshold:{MinimalRequiredThreshold}",
        //         proposal.ProposalId, proposal.GovernanceMechanism, voteInfo.VoterCount, proposal.MinimalRequiredThreshold);
        //     if (proposal.MinimalRequiredThreshold > 0 && voteInfo.VoterCount < proposal.MinimalRequiredThreshold)
        //     {
        //         return ProposalStatus.Expired;
        //     }
        // }
        //
        // if (proposal.GovernanceMechanism is GovernanceMechanism.Parliament or GovernanceMechanism.Association)
        // {
        //     double voterPercentage = GetPercentage(voteInfo.VoterCount, organizationInfo.OrganizationMemberCount);
        //     _logger.LogInformation(
        //         "[VoteFinishedStatus] proposalId:{proposalId}, GovernanceMechanism:{GovernanceMechanism} " +
        //         "voterPercentage: {voterPercentage} MinimalRequiredThreshold:{MinimalRequiredThreshold}",
        //         proposal.ProposalId, proposal.GovernanceMechanism, voterPercentage, proposal.MinimalRequiredThreshold);
        //     if (voterPercentage < proposal.MinimalRequiredThreshold)
        //     {
        //         return ProposalStatus.Expired;
        //     }
        // }

        double rejectionPercentage = GetPercentage(voteInfo.RejectionCount, voteInfo.VotesAmount);
        double abstentionPercentage = GetPercentage(voteInfo.AbstentionCount, voteInfo.VotesAmount);
        double approvalPercentage = GetPercentage(voteInfo.ApprovedCount, voteInfo.VotesAmount);
        _logger.LogInformation(
            "[VoteFinishedStatus] proposalId:{proposalId}, GovernanceMechanism:{GovernanceMechanism} " +
            "rejectionPercentage: {rejectionPercentage} MaximalRejectionThreshold: {MaximalRejectionThreshold} " +
            "abstentionPercentage:{abstentionPercentage} MaximalAbstentionThreshold:{MaximalAbstentionThreshold} " +
            "approvalPercentage:{approvalPercentage} MinimalApproveThreshold:{MinimalApproveThreshold}",
            proposal.ProposalId, proposal.GovernanceMechanism, rejectionPercentage, proposal.MaximalRejectionThreshold,
            abstentionPercentage, proposal.MaximalAbstentionThreshold, approvalPercentage, proposal.MinimalApproveThreshold);
        if (rejectionPercentage > proposal.MaximalRejectionThreshold)
        {
            targetStatus = ProposalStatus.Rejected;
        }
        else if (abstentionPercentage > proposal.MaximalAbstentionThreshold)
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
        return Math.Round((double)count / totalCount * 10000, 2);
    }
}