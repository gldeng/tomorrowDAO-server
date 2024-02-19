using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.Organization.Dto;
using TomorrowDAOServer.Organization.Index;
using TomorrowDAOServer.Organization.Provider;
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
    private readonly IOrganizationInfoProvider _organizationInfoProvider;


    public ProposalService(IObjectMapper objectMapper, IProposalProvider proposalProvider, IVoteProvider voteProvider,
        IOptionsMonitor<ProposalTagOptions> proposalTagOptionsMonitor,
        IOrganizationInfoProvider organizationInfoProvider)
    {
        _objectMapper = objectMapper;
        _proposalProvider = proposalProvider;
        _voteProvider = voteProvider;
        _proposalTagOptionsMonitor = proposalTagOptionsMonitor;
        _organizationInfoProvider = organizationInfoProvider;
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
        var voteInfos = await _voteProvider.GetVoteInfosAsync(input.ChainId, proposalIds);
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

        var voteInfos = await _voteProvider.GetVoteInfosAsync(input.ChainId,
            new List<string> { input.ProposalId });

        if (voteInfos.TryGetValue(input.ProposalId, out var voteInfo))
        {
            //of vote info
            _objectMapper.Map(voteInfo, proposalDetailDto);
        }

        var organizationInfoDict = await _organizationInfoProvider
            .GetOrganizationInfosMemoryAsync(input.ChainId, new List<string> { proposalIndex.OrganizationAddress });
        if (organizationInfoDict.TryGetValue(proposalIndex.OrganizationAddress, out var organizationInfo))
        {
            proposalDetailDto.OrganizationInfo =
                _objectMapper.Map<IndexerOrganizationInfo, OrganizationDto>(organizationInfo);
        }

        var voteRecords = await _voteProvider.GetVoteRecordAsync(new GetVoteRecordInput
        {
            ChainId = input.ChainId,
            VotingItemId = input.ProposalId,
            Sorting = VoteTopSorting
        });
        proposalDetailDto.VoteTopList = _objectMapper.Map<List<IndexerVoteRecord>, List<VoteRecordDto>>(voteRecords);
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
        if (proposalIndex.ProposalStatus == ProposalStatus.Active)
        {
            myProposalDto.CanVote = true;
        }
        var voteStake = await _voteProvider.GetVoteStakeAsync(input.ChainId, input.ProposalId, input.Address);
        myProposalDto.StakeAmount = voteStake.Amount;
        myProposalDto.VotesAmount = myProposalDto.StakeAmount;
        myProposalDto.Symbol = voteStake.AcceptedCurrency;
        return myProposalDto;
    }
}