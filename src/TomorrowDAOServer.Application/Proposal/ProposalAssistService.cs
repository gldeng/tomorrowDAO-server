using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Proposal.Dto;
using TomorrowDAOServer.Proposal.Index;
using TomorrowDAOServer.Proposal.Provider;
using TomorrowDAOServer.Vote.Index;
using TomorrowDAOServer.Vote.Provider;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Proposal;

public interface IProposalAssistService
{
    public Task<List<ProposalIndex>> ConvertProposalList(string chainId, List<IndexerProposal> list);
    public Task<List<ProposalIndex>> ConvertProposalList(string chainId, List<ProposalIndex> list);
    public List<ProposalLifeDto> ConvertProposalLifeList(ProposalIndex proposalIndex);
}

public class ProposalAssistService : TomorrowDAOServerAppService, IProposalAssistService
{
    private readonly ILogger<ProposalAssistService> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly IVoteProvider _voteProvider;
    private readonly IProposalProvider _proposalProvider;
    private const int AbstractVoteTotal = 10000;

    public ProposalAssistService(ILogger<ProposalAssistService> logger, IObjectMapper objectMapper, IVoteProvider voteProvider,
        IProposalProvider proposalProvider)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _voteProvider = voteProvider;
        _proposalProvider = proposalProvider;
    }

    public async Task<List<ProposalIndex>> ConvertProposalList(string chainId, List<IndexerProposal> list)
    {
        return await ConvertProposalList(chainId, _objectMapper.Map<List<IndexerProposal>, List<ProposalIndex>>(list));
    }

    public async Task<List<ProposalIndex>> ConvertProposalList(string chainId, List<ProposalIndex> list)
    {
        var proposalIds = list.Select(x => x.ProposalId).ToList();
        var voteInfos = await _voteProvider.GetVoteItemsAsync(chainId, proposalIds);
        
        foreach (var proposal in list)
        {
            var proposalId = proposal.ProposalId;
            var vetoProposalId = proposal.VetoProposalId;
            var proposalType = proposal.ProposalType;
            var proposalStage = proposal.ProposalStage;
            var executeStartTime = proposal.ExecuteStartTime;

            voteInfos.TryGetValue(proposalId, out var voteInfo);
            
            switch (proposalType)
            {
                case ProposalType.Governance:
                    switch (proposalStage)
                    {
                        case ProposalStage.Active:
                            _objectMapper.Map(ProcessActiveProposalStage(proposal, voteInfo), proposal);
                            break;
                        case ProposalStage.Pending:
                            if (!vetoProposalId.IsNullOrEmpty())
                            {
                                var vetoProposal = await _proposalProvider.GetProposalByIdAsync(chainId, vetoProposalId);
                                var vetoProposalStatus = vetoProposal?.ProposalStatus?? ProposalStatus.Empty;
                                proposal.ProposalStatus = vetoProposalStatus == ProposalStatus.Executed ? ProposalStatus.Vetoed : ProposalStatus.Challenged;
                                proposal.ProposalStage = vetoProposalStatus == ProposalStatus.Executed ? ProposalStage.Finished : ProposalStage.Execute;
                            }
                            else if (TimeEnd(executeStartTime))
                            {
                                proposal.ProposalStatus = ProposalStatus.Approved;
                                proposal.ProposalStage = ProposalStage.Execute;
                            }
                            break;
                        case ProposalStage.Execute:
                            _objectMapper.Map(ProcessExecuteProposalStage(proposal), proposal);
                            break;
                    }
                    break;
                case ProposalType.Veto:
                    switch (proposalStage)
                    {
                        case ProposalStage.Active:
                            _objectMapper.Map(ProcessActiveProposalStage(proposal, voteInfo), proposal);
                            break;
                        case ProposalStage.Execute:
                            _objectMapper.Map(ProcessExecuteProposalStage(proposal), proposal);
                            break;
                    }
                    break;
                case ProposalType.Advisory:
                    switch (proposalStage)
                    {
                        case ProposalStage.Active:
                            _objectMapper.Map(ProcessActiveProposalStage(proposal, voteInfo), proposal);
                            break;
                    }
                    break;
            }
        }
        
        return list;
    }

    public List<ProposalLifeDto> ConvertProposalLifeList(ProposalIndex proposalIndex)
    {
        var result = new List<ProposalLifeDto>();
        AddProposalLife(ref result, ProposalStage.Active, ProposalStatus.PendingVote);

        var proposalStatus = proposalIndex.ProposalStatus;
        var isVetoed = proposalIndex.VetoProposalId.IsNullOrEmpty();
        switch (proposalIndex.ProposalStage)
            {
                case ProposalStage.Active:
                    break;
                case ProposalStage.Pending:
                    AddProposalLife(ref result, ProposalStage.Pending, proposalStatus);
                    break;
                case ProposalStage.Execute:
                    if (GovernanceMechanism.HighCouncil == proposalIndex.GovernanceMechanism)
                    {
                        AddProposalLife(ref result, ProposalStage.Pending, isVetoed ? ProposalStatus.Challenged : ProposalStatus.Approved);
                    }
                    AddProposalLife(ref result, ProposalStage.Execute, ProposalStatus.Approved);
                    break;
                case ProposalStage.Finished:
                    switch (proposalStatus)
                    {
                        case ProposalStatus.Rejected:
                        case ProposalStatus.Abstained:
                        case ProposalStatus.BelowThreshold:    
                            break;
                        case ProposalStatus.Vetoed:
                            AddProposalLife(ref result, ProposalStage.Pending, ProposalStatus.Challenged);
                            break;
                        case ProposalStatus.Executed:
                        case ProposalStatus.Expired:
                            if (GovernanceMechanism.HighCouncil == proposalIndex.GovernanceMechanism)
                            {
                                AddProposalLife(ref result, ProposalStage.Pending, isVetoed ? ProposalStatus.Challenged : ProposalStatus.Approved);
                            }
                            AddProposalLife(ref result, ProposalStage.Execute, ProposalStatus.Approved);
                            break;
                    }
                    
                    AddProposalLife(ref result, ProposalStage.Finished, proposalStatus);
                    break;
            }

        return result;
    }

    private static void AddProposalLife(ref List<ProposalLifeDto> result, ProposalStage proposalStage, ProposalStatus proposalStatus)
    {
        result.AddLast(new ProposalLifeDto
        {
            ProposalStage = proposalStage.ToString(),
            ProposalStatus = proposalStatus.ToString()
        });
    }

    private ProposalIndex ProcessActiveProposalStage(ProposalIndex proposal, IndexerVote voteInfo)
    {
        var governanceMechanism = proposal.GovernanceMechanism;
        var activeEndTime = proposal.ActiveEndTime;
        var totalVote = voteInfo?.VotesAmount ?? 0;
        var totalVoter = voteInfo?.VoterCount ?? 0;
        var rejectVote = voteInfo?.RejectionCount ?? 0;
        var abstainVote = voteInfo?.AbstentionCount ?? 0;
        var approveVote = voteInfo?.ApprovedCount ?? 0;

        var enoughVoter = totalVoter >= proposal.MinimalApproveThreshold;
        var enoughVote = rejectVote + abstainVote + approveVote >= proposal.MinimalVoteThreshold;
        var isReject = rejectVote / (double)totalVote * AbstractVoteTotal > proposal.MaximalRejectionThreshold;
        var isAbstained = abstainVote / (double)totalVote * AbstractVoteTotal > proposal.MaximalAbstentionThreshold;
        var isApproved = approveVote / (double)totalVote * AbstractVoteTotal > proposal.MaximalAbstentionThreshold;

        if (!TimeEnd(activeEndTime))
        {
            return proposal;
        }

        if (enoughVoter && enoughVote)
        {
            if (isApproved)
            {
                proposal.ProposalStage = governanceMechanism == GovernanceMechanism.Referendum ? ProposalStage.Execute : ProposalStage.Pending;
                proposal.ProposalStatus = ProposalStatus.Approved;
            }
            else
            {
                proposal.ProposalStage = ProposalStage.Finished;
                proposal.ProposalStatus = isReject ? ProposalStatus.Rejected : ProposalStatus.Abstained;
            }
        }
        else
        {
            proposal.ProposalStatus = ProposalStatus.BelowThreshold;
            proposal.ProposalStage = ProposalStage.Finished;
        }

        return proposal;
    }

    private ProposalIndex ProcessExecuteProposalStage(ProposalIndex proposal)
    {
        var executeEndTime = proposal.ExecuteEndTime;
        var executeTime = proposal.ExecuteTime;
        if (!TimeEnd(executeEndTime))
        {
            return proposal;
        }

        proposal.ProposalStatus = executeTime == null ? ProposalStatus.Expired : ProposalStatus.Executed;
        proposal.ProposalStage = ProposalStage.Finished;
        return proposal;
    }

    private bool TimeEnd(DateTime time) => DateTime.UtcNow > time;
}