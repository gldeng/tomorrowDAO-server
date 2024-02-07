using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.Proposal.Dto;
using TomorrowDAOServer.Proposal.Provider;
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

    public async Task<PagedResultDto<ProposalDto>> QueryProposalListAsync(QueryProposalListInput input)
    {
        var tuple = await _proposalProvider.GetProposalListAsync(input);

        if (tuple.Item2.IsNullOrEmpty())
        {
            return new PagedResultDto<ProposalDto>();
        }

        //query proposal vote infos
        var voteInfos = await _voteProvider.GetVoteInfos(input.ChainId, 
            tuple.Item2.Select(item => item.ProposalId).ToList());
        var resultList = new List<ProposalDto>();
        foreach (var proposal in tuple.Item2)
        {
            var proposalDto = _objectMapper.Map<ProposalIndex, ProposalDto>(proposal);

            if (voteInfos.TryGetValue(proposal.ProposalId, out var voteInfo))
            {
                //of vote info
                _objectMapper.Map(voteInfo, proposalDto);
            }

            proposalDto.TagList =
                _proposalTagOptionsMonitor.CurrentValue.MatchTagList(proposal.TransactionInfo.ContractMethodName);
            resultList.Add(proposalDto);
        }

        return new PagedResultDto<ProposalDto>
        {
            Items = resultList,
            TotalCount = tuple.Item1
        };
    }
}