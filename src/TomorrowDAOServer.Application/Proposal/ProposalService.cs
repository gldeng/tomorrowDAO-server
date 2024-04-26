using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.Proposal.Dto;
using TomorrowDAOServer.Proposal.Provider;
using TomorrowDAOServer.Vote.Dto;
using TomorrowDAOServer.Vote.Index;
using TomorrowDAOServer.Vote.Provider;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Proposal;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class ProposalService : TomorrowDAOServerAppService, IProposalService
{
    private const string VoteTopSorting = "Amount DESC";
    private readonly IObjectMapper _objectMapper;
    private readonly IOptionsMonitor<ProposalTagOptions> _proposalTagOptionsMonitor;
    private readonly IProposalProvider _proposalProvider;
    private readonly IVoteProvider _voteProvider;


    public ProposalService(IObjectMapper objectMapper, IProposalProvider proposalProvider, IVoteProvider voteProvider,
        IOptionsMonitor<ProposalTagOptions> proposalTagOptionsMonitor)
    {
        _objectMapper = objectMapper;
        _proposalProvider = proposalProvider;
        _voteProvider = voteProvider;
        _proposalTagOptionsMonitor = proposalTagOptionsMonitor;
    }

    public async Task<PagedResultDto<ProposalListDto>> QueryProposalListAsync(QueryProposalListInput input)
    {
        var tuple = await _proposalProvider.GetProposalListAsync(input);

        if (tuple.Item2.IsNullOrEmpty())
        {
            return new PagedResultDto<ProposalListDto>();
        }

        //query proposal vote infos
        var proposalIds = tuple.Item2.Select(item => item.ProposalId).ToList();
        //todo query real vote result, mock now
        // var voteInfos = await _voteProvider.GetVoteInfosAsync(input.ChainId, proposalIds);
        var voteInfos = new Dictionary<string, IndexerVote>();
        var resultList = new List<ProposalListDto>();
        foreach (var proposal in tuple.Item2)
        {
            var proposalDto = _objectMapper.Map<ProposalIndex, ProposalListDto>(proposal);

            if (voteInfos.TryGetValue(proposal.ProposalId, out var voteInfo))
            {
                //of vote info
                _objectMapper.Map(voteInfo, proposalDto);
            }

            proposalDto.OfTagList(_proposalTagOptionsMonitor.CurrentValue);
            resultList.Add(proposalDto);
        }

        return new PagedResultDto<ProposalListDto>
        {
            Items = resultList,
            TotalCount = tuple.Item1
        };
    }

    public async Task<ProposalDetailDto> QueryProposalDetailAsync(QueryProposalDetailInput input)
    {
        var proposalIndex = await _proposalProvider.GetProposalByIdAsync(input.ChainId, input.ProposalId);
        if (proposalIndex == null)
        {
            return new ProposalDetailDto();
        }

        var proposalDetailDto = _objectMapper.Map<ProposalIndex, ProposalDetailDto>(proposalIndex);

        //todo query graphql later
        // var voteInfos = await _voteProvider.GetVoteInfosAsync(input.ChainId,
        //     new List<string> { input.ProposalId });

        var voteInfos = new Dictionary<string, IndexerVote>
        {
            [input.ProposalId] = new()
        };

        if (voteInfos.TryGetValue(input.ProposalId, out var voteInfo))
        {
            //of vote info
            _objectMapper.Map(voteInfo, proposalDetailDto);
        }

        //todo query graphql later
        // var voteRecords = await _voteProvider.GetVoteRecordAsync(new GetVoteRecordInput
        // {
        //     ChainId = input.ChainId,
        //     VotingItemId = input.ProposalId,
        //     Sorting = VoteTopSorting
        // });
        // proposalDetailDto.VoteTopList = _objectMapper.Map<List<IndexerVoteRecord>, List<VoteRecordDto>>(voteRecords);
        proposalDetailDto.VoteTopList = new List<VoteRecordDto>();
        return proposalDetailDto;
    }

    public async Task<MyProposalDto> QueryMyInfoAsync(QueryMyProposalInput input)
    {
        var proposalIndex = await _proposalProvider.GetProposalByIdAsync(input.ChainId, input.ProposalId);
        if (proposalIndex == null)
        {
            return new MyProposalDto();
        }
        var myProposalDto = _objectMapper.Map<ProposalIndex, MyProposalDto>(proposalIndex);
        var voteStake = await _voteProvider.GetVoteStakeAsync(input.ChainId, input.ProposalId, input.Address);
        myProposalDto.StakeAmount = voteStake.Amount;
        myProposalDto.VotesAmount = myProposalDto.StakeAmount;
        myProposalDto.Symbol = voteStake.AcceptedCurrency;
        myProposalDto.CanVote = !proposalIndex.IsFinalStatus();
        if (proposalIndex.IsFinalStatus())
        {
            myProposalDto.AvailableUnStakeAmount = myProposalDto.StakeAmount;
        }
        return myProposalDto;
    }
}